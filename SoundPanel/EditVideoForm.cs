using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;

namespace SoundPanel
{
    public partial class EditVideoForm : Form
    {
        private LibVLC libVLC;
        private MediaPlayer mediaPlayer;
        private Media currentMedia;
        private string currentVideoPath = "";
        private long videoDurationMs = 0;
        private long currentPositionMs = 0;
        private long startTrimMs = 0;
        private long endTrimMs = 0;
        private bool isDraggingStart = false;
        private bool isDraggingEnd = false;
        private bool isDraggingPosition = false;
        private System.Windows.Forms.Timer positionTimer;
        private bool isVideoLoaded = false;
        private int currentVolume = 100;

        // Overlay sistemleri
        private List<OverlayItem> overlayItems = new List<OverlayItem>();
        private List<string> videoFiles = new List<string>();
        private int selectedOverlayIndex = -1;

        // Visual overlay editörü - KATMAN SİSTEMİ
        private bool isOverlayEditMode = false;
        private OverlayItem draggedOverlay = null;
        private bool isDraggingOverlay = false;
        private bool isResizingOverlay = false;
        private Point lastMousePosition;
        private Point overlayOffset;
        private System.Windows.Forms.Timer overlayUpdateTimer;

        // Overlay preview panel - TRANSPARAN KATMAN
        private Panel overlayCanvas;
        private Dictionary<OverlayItem, Image> cachedImages = new Dictionary<OverlayItem, Image>();
        private int resizeHandleIndex = -1;
        private bool updatingControls = false;

        // Video ölçekleri
        private float videoAspectRatio = 16f / 9f;
        private Rectangle actualVideoRect;

        public EditVideoForm()
        {
            InitializeComponent();
            InitializeVLC();
            SetupTimer();
            SetupKeyboardShortcuts();
            SetupVolumeControl();
            SetupOverlayControls();
            SetupVisualOverlayEditor();
            // 'tabPageBasic' nesnesini tanımlayın (tasarımda eklediyseniz zaten var)
            tabPageOverlays = tabControlFeatures.TabPages["tabPageOverlays"];

            // TabControl'dan kaldırarak gizleyin
            tabControlFeatures.TabPages.Remove(tabPageOverlays);
        }
        // 'overlayCanvas' için bir alt sınıf oluşturun ve SetStyle metodunu burada kullanın
        
        
        // 'overlayCanvas' için bir alt sınıf oluşturun ve SetStyle metodunu burada kullanın
        private class TransparentPanel : Panel
        {
            public TransparentPanel()
            {
                SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                SetStyle(ControlStyles.Opaque, false);
                SetStyle(ControlStyles.UserPaint, true);
                SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                SetStyle(ControlStyles.ResizeRedraw, true);
                BackColor = Color.Transparent;
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                // Hiçbir şey çizme, böylece altındaki kontrol görünür
            }
        }
        private void AddOverlayControl(Control control)
        {
            overlayCanvas.Controls.Add(control);
            // VLC'yi en alta al
            vlcControl.SendToBack();
            // Yeni eklenen kontrolü en üste al
            control.BringToFront();
        }
        //private void SetupVisualOverlayEditor()
        //{
        //    overlayCanvas = new TransparentPanel();
        //    overlayCanvas.BackColor = Color.Transparent;
        //    overlayCanvas.Size = panelMain.Size;
        //    overlayCanvas.Location = new Point(0, 0);
        //    overlayCanvas.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

        //    // VLC'yi panelMain'e ekle
        //    panelMain.Controls.Add(vlcControl);
        //    vlcControl.Location = new Point(0, 0);
        //    vlcControl.Size = panelMain.Size;
        //    vlcControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

        //    // overlayCanvas'ı panelMain'e ekle ve en üste al
        //    panelMain.Controls.Add(overlayCanvas);
        //    overlayCanvas.BringToFront();

        //    // Event handler'lar
        //    overlayCanvas.Paint += OverlayCanvas_Paint;
        //    overlayCanvas.MouseDown += OverlayCanvas_MouseDown;
        //    overlayCanvas.MouseMove += OverlayCanvas_MouseMove;
        //    overlayCanvas.MouseUp += OverlayCanvas_MouseUp;
        //    overlayCanvas.MouseLeave += OverlayCanvas_MouseLeave;

        //    vlcControl.Resize += VlcControl_Resize;
        //    vlcControl.LocationChanged += VlcControl_LocationChanged;

        //    overlayUpdateTimer = new System.Windows.Forms.Timer();
        //    overlayUpdateTimer.Interval = 33;
        //    overlayUpdateTimer.Tick += OverlayUpdateTimer_Tick;
        //    overlayUpdateTimer.Start();
        //}

        private void SetupVisualOverlayEditor()
        {
            // Tamamen transparan overlay katmanı oluştur
            overlayCanvas = new TransparentPanel();
            overlayCanvas.BackColor = Color.Transparent;
            overlayCanvas.Size = panelMain.Size;
            overlayCanvas.Location = new Point(0, 0);
            overlayCanvas.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // VLC kontrolünü overlayCanvas'ın içine ekle
            overlayCanvas.Controls.Add(vlcControl);
            vlcControl.Location = new Point(0, 0);
            vlcControl.Size = overlayCanvas.Size;
            vlcControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            vlcControl.SendToBack(); // VLC en altta

            // overlayCanvas event handler'ları
            overlayCanvas.Paint += OverlayCanvas_Paint;
            overlayCanvas.MouseDown += OverlayCanvas_MouseDown;
            overlayCanvas.MouseMove += OverlayCanvas_MouseMove;
            overlayCanvas.MouseUp += OverlayCanvas_MouseUp;
            overlayCanvas.MouseLeave += OverlayCanvas_MouseLeave;

            // VLC resize event'ini dinle
            vlcControl.Resize += VlcControl_Resize;
            vlcControl.LocationChanged += VlcControl_LocationChanged;

            // overlayCanvas'ı ana panel'e ekle
            panelMain.Controls.Add(overlayCanvas);
            overlayCanvas.BringToFront(); // Her zaman en üstte

            // Overlay güncelleme timer'ı
            overlayUpdateTimer = new System.Windows.Forms.Timer();
            overlayUpdateTimer.Interval = 33; // ~30 FPS
            overlayUpdateTimer.Tick += OverlayUpdateTimer_Tick;
            overlayUpdateTimer.Start();
        }

        private void OverlayCanvas_Paint(object sender, PaintEventArgs e)
        {
            if (!isVideoLoaded || overlayItems.Count == 0)
                return;

            Graphics g = e.Graphics;

            // TAMAMEN TRANSPARAN ARKA PLAN - VLC gözükür
            g.Clear(Color.Transparent);

            // High quality rendering
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

            double currentSeconds = currentPositionMs / 1000.0;

            // Z-order'a göre overlay'leri çiz
            for (int i = 0; i < overlayItems.Count; i++)
            {
                var overlay = overlayItems[i];

                // Overlay'in aktif olma zamanında mı?
                if (currentSeconds >= overlay.StartTime && currentSeconds <= overlay.EndTime)
                {
                    Rectangle overlayRect = GetScreenRect(overlay);

                    // Overlay içeriğini çiz
                    DrawOverlayContent(g, overlay, overlayRect);

                    // Edit modunda seçim çerçevesi
                    if (isOverlayEditMode && selectedOverlayIndex == i)
                    {
                        DrawSelectionFrame(g, overlayRect);
                        DrawResizeHandles(g, overlayRect);
                        DrawOverlayInfo(g, overlay, overlayRect);
                    }
                }
            }
        }

        private void OverlayCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            // Edit modu kapalıysa mouse event'lerini VLC'ye geçir
            if (!isOverlayEditMode)
            {
                // Mouse event'ini VLC'ye forward et
                ForwardMouseEventToVLC(e, "DOWN");
                return;
            }

            // Edit modundaysa overlay işlemleri
            if (!isVideoLoaded) return;

            double currentSeconds = currentPositionMs / 1000.0;
            OverlayItem clickedOverlay = null;
            int clickedIndex = -1;
            bool hitOverlay = false;

            // Aktif overlay'leri kontrol et
            for (int i = overlayItems.Count - 1; i >= 0; i--)
            {
                var overlay = overlayItems[i];
                if (currentSeconds >= overlay.StartTime && currentSeconds <= overlay.EndTime)
                {
                    Rectangle overlayRect = GetScreenRect(overlay);

                    // Resize handle kontrolü
                    if (selectedOverlayIndex == i)
                    {
                        var handles = GetResizeHandles(overlayRect, 12);
                        for (int h = 0; h < handles.Length; h++)
                        {
                            if (handles[h].Contains(e.Location))
                            {
                                resizeHandleIndex = h;
                                isResizingOverlay = true;
                                draggedOverlay = overlay;
                                lastMousePosition = e.Location;
                                overlayCanvas.Cursor = GetResizeCursor(h);
                                overlayCanvas.Invalidate();
                                hitOverlay = true;
                                return;
                            }
                        }
                    }

                    // Overlay gövde kontrolü
                    if (overlayRect.Contains(e.Location))
                    {
                        clickedOverlay = overlay;
                        clickedIndex = i;
                        hitOverlay = true;
                        break;
                    }
                }
            }

            if (hitOverlay && clickedOverlay != null)
            {
                // Overlay seçildi - event'i yakalayıp işle
                selectedOverlayIndex = clickedIndex;
                draggedOverlay = clickedOverlay;
                isDraggingOverlay = true;
                lastMousePosition = e.Location;

                Rectangle overlayRect = GetScreenRect(clickedOverlay);
                overlayOffset = new Point(e.X - overlayRect.X, e.Y - overlayRect.Y);

                overlayCanvas.Cursor = Cursors.SizeAll;
                UpdateOverlayControls();
                overlayCanvas.Invalidate();

                listBoxOverlays.SelectedIndex = clickedIndex;

                if (e.Button == MouseButtons.Right)
                {
                    ShowOverlayContextMenu(e.Location, clickedOverlay);
                }
            }
            else
            {
                // Overlay dışına tıklandı - VLC'ye geçir
                ForwardMouseEventToVLC(e, "DOWN");

                // Overlay seçimini temizle
                selectedOverlayIndex = -1;
                draggedOverlay = null;
                resizeHandleIndex = -1;
                listBoxOverlays.SelectedIndex = -1;
                UpdateOverlayControls();
                overlayCanvas.Invalidate();
            }
        }

        private void OverlayCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isOverlayEditMode)
            {
                // Edit modu kapalıysa VLC'ye geçir
                ForwardMouseEventToVLC(e, "MOVE");
                overlayCanvas.Cursor = Cursors.Default;
                return;
            }

            if (draggedOverlay == null)
            {
                // Hover kontrolü
                UpdateMouseCursor(e.Location);

                // Overlay üzerinde değilse mouse event'i VLC'ye geçir
                bool overOverlay = IsMouseOverAnyOverlay(e.Location);
                if (!overOverlay)
                {
                    ForwardMouseEventToVLC(e, "MOVE");
                }
                return;
            }

            // Overlay drag/resize işlemleri
            if (isResizingOverlay)
            {
                float scaleX = 1920f / actualVideoRect.Width;
                float scaleY = 1080f / actualVideoRect.Height;

                int deltaX = (int)((e.X - lastMousePosition.X) * scaleX);
                int deltaY = (int)((e.Y - lastMousePosition.Y) * scaleY);

                ResizeOverlay(draggedOverlay, resizeHandleIndex, deltaX, deltaY);

                lastMousePosition = e.Location;
                UpdateOverlayControls();
                overlayCanvas.Invalidate();
            }
            else if (isDraggingOverlay)
            {
                float scaleX = 1920f / actualVideoRect.Width;
                float scaleY = 1080f / actualVideoRect.Height;

                Point videoRelativePoint = new Point(
                    e.X - actualVideoRect.X,
                    e.Y - actualVideoRect.Y
                );

                int newX = (int)((videoRelativePoint.X - overlayOffset.X + actualVideoRect.X - actualVideoRect.X) * scaleX);
                int newY = (int)((videoRelativePoint.Y - overlayOffset.Y + actualVideoRect.Y - actualVideoRect.Y) * scaleY);

                draggedOverlay.X = Math.Max(0, Math.Min(newX, 1920 - draggedOverlay.Width));
                draggedOverlay.Y = Math.Max(0, Math.Min(newY, 1080 - draggedOverlay.Height));

                UpdateOverlayControls();
                overlayCanvas.Invalidate();
            }
        }

        private void OverlayCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (!isOverlayEditMode)
            {
                ForwardMouseEventToVLC(e, "UP");
                return;
            }

            if (draggedOverlay == null)
            {
                // Overlay işlemi yoksa VLC'ye geçir
                ForwardMouseEventToVLC(e, "UP");
            }

            isDraggingOverlay = false;
            isResizingOverlay = false;
            draggedOverlay = null;
            resizeHandleIndex = -1;
            overlayCanvas.Cursor = Cursors.Default;
        }

        private void OverlayCanvas_MouseLeave(object sender, EventArgs e)
        {
            overlayCanvas.Cursor = Cursors.Default;
        }

        // VLC'ye mouse event forward etme
        private void ForwardMouseEventToVLC(MouseEventArgs e, string eventType)
        {
            try
            {
                // Overlay canvas koordinatlarını VLC koordinatlarına dönüştür
                Point vlcPoint = new Point(e.X, e.Y);

                // VLC'ye focus ver
                vlcControl.Focus();

                // Mouse event simülasyonu
                switch (eventType.ToUpper())
                {
                    case "DOWN":
                        SendMessage(vlcControl.Handle, WM_LBUTTONDOWN,
                                   (IntPtr)MK_LBUTTON, MakeLParam(vlcPoint.X, vlcPoint.Y));
                        break;
                    case "UP":
                        SendMessage(vlcControl.Handle, WM_LBUTTONUP,
                                   IntPtr.Zero, MakeLParam(vlcPoint.X, vlcPoint.Y));
                        break;
                    case "MOVE":
                        SendMessage(vlcControl.Handle, WM_MOUSEMOVE,
                                   IntPtr.Zero, MakeLParam(vlcPoint.X, vlcPoint.Y));
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"VLC Mouse Forward Error: {ex.Message}");
            }
        }

        // Mouse overlay kontrolü
        private bool IsMouseOverAnyOverlay(Point location)
        {
            if (!isVideoLoaded || overlayItems.Count == 0) return false;

            double currentSeconds = currentPositionMs / 1000.0;

            foreach (var overlay in overlayItems)
            {
                if (currentSeconds >= overlay.StartTime && currentSeconds <= overlay.EndTime)
                {
                    Rectangle overlayRect = GetScreenRect(overlay);
                    if (overlayRect.Contains(location))
                        return true;

                    // Seçili overlay'in handle'larını da kontrol et
                    if (isOverlayEditMode && overlayItems.IndexOf(overlay) == selectedOverlayIndex)
                    {
                        var handles = GetResizeHandles(overlayRect, 12);
                        foreach (var handle in handles)
                        {
                            if (handle.Contains(location))
                                return true;
                        }
                    }
                }
            }

            return false;
        }

        // Win32 API
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const uint WM_LBUTTONDOWN = 0x0201;
        private const uint WM_LBUTTONUP = 0x0202;
        private const uint WM_MOUSEMOVE = 0x0200;
        private const uint MK_LBUTTON = 0x0001;

        private static IntPtr MakeLParam(int loWord, int hiWord)
        {
            return (IntPtr)((hiWord << 16) | (loWord & 0xFFFF));
        }

        // Button toggle güncelleme
        private void btnToggleOverlayEdit_Click(object sender, EventArgs e)
        {
            isOverlayEditMode = !isOverlayEditMode;

            btnToggleOverlayEdit.Text = isOverlayEditMode ? "🎯 Edit Modu: AÇIK" : "🎯 Edit Modu: KAPALI";
            btnToggleOverlayEdit.ButtonBackColor = isOverlayEditMode ? Color.FromArgb(0, 204, 0) : Color.FromArgb(128, 128, 128);

            // Canvas durumu - HER ZAMAN AKTİF KALIR
            overlayCanvas.Visible = true;
            overlayCanvas.Enabled = true;

            if (isOverlayEditMode)
            {
                // Edit modu: Overlay etkileşimli
                overlayCanvas.BringToFront();

                if (overlayItems.Count == 0)
                {
                    MessageBox.Show("Edit modu aktif!\n\n" +
                                  "✅ VLC video her zaman görünür (arka plan)\n" +
                                  "✅ Overlay'ler transparan katman olarak üstte\n" +
                                  "✅ Edit modunda overlay'leri düzenleyebilirsiniz\n" +
                                  "✅ Edit modu kapalıyken VLC tam kontrol\n\n" +
                                  "🎯 Transparan katman sistemi aktif!\n" +
                                  "👤 CaYatur | 📅 2025-07-15 00:57:06 UTC",
                                  "Transparan Katman Sistemi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                // Normal mod: VLC tam kontrol, overlay'ler sadece görünür
                vlcControl.Focus();

                selectedOverlayIndex = -1;
                listBoxOverlays.SelectedIndex = -1;
                UpdateOverlayControls();
            }

            overlayCanvas.Invalidate();
        }

        // Video yükleme güncelleme
        private void LoadVideo(string videoPath)
        {
            try
            {
                currentVideoPath = videoPath;
                lblVideoPath.Text = Path.GetFileName(videoPath);

                FileInfo fileInfo = new FileInfo(videoPath);
                lblFileInfo.Text = $"📁 Dosya: {FormatFileSize(fileInfo.Length)} | " +
                                 $"📅 Oluşturma: {fileInfo.CreationTime:dd.MM.yyyy HH:mm}";

                currentMedia?.Dispose();
                currentMedia = new Media(libVLC, videoPath);
                mediaPlayer.Media = currentMedia;

                startTrimMs = 0;
                currentPositionMs = 0;
                isVideoLoaded = false;

                // KATMAN SIRASI - VLC altında, overlay transparan üstte
                vlcControl.SendToBack();
                overlayCanvas.BringToFront();

                mediaPlayer.Play();

                Task.Delay(500).ContinueWith(t =>
                {
                    this.Invoke((System.Windows.Forms.MethodInvoker)delegate
                    {
                        mediaPlayer.Pause();
                    });
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Video yüklenirken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }






        private void VlcControl_Resize(object sender, EventArgs e)
        {
            UpdateOverlayCanvasPosition();
        }

        private void VlcControl_LocationChanged(object sender, EventArgs e)
        {
            UpdateOverlayCanvasPosition();
        }

        private void UpdateOverlayCanvasPosition()
        {
            if (overlayCanvas == null || vlcControl == null) return;

            // Overlay canvas'ı VLC ile tam sync tut
            overlayCanvas.Size = vlcControl.Size;
            overlayCanvas.Location = vlcControl.Location;

            // Gerçek video render alanını hesapla
            CalculateActualVideoRect();

            // Canvas'ı her zaman en üstte tut
            overlayCanvas.BringToFront();
        }

        private void CalculateActualVideoRect()
        {
            if (!isVideoLoaded)
            {
                actualVideoRect = new Rectangle(0, 0, vlcControl.Width, vlcControl.Height);
                return;
            }

            // Video aspect ratio'yu koru
            Rectangle clientRect = new Rectangle(0, 0, vlcControl.Width, vlcControl.Height);
            float clientAspect = (float)clientRect.Width / clientRect.Height;

            if (clientAspect > videoAspectRatio)
            {
                // Client daha geniş - video yüksekliği client'ı doldurur
                int videoWidth = (int)(clientRect.Height * videoAspectRatio);
                int offsetX = (clientRect.Width - videoWidth) / 2;
                actualVideoRect = new Rectangle(offsetX, 0, videoWidth, clientRect.Height);
            }
            else
            {
                // Client daha yüksek - video genişliği client'ı doldurur
                int videoHeight = (int)(clientRect.Width / videoAspectRatio);
                int offsetY = (clientRect.Height - videoHeight) / 2;
                actualVideoRect = new Rectangle(0, offsetY, clientRect.Width, videoHeight);
            }
        }

        private void OverlayUpdateTimer_Tick(object sender, EventArgs e)
        {
            // Overlay'ler varsa sürekli güncelle
            if (isVideoLoaded && overlayItems.Count > 0)
            {
                overlayCanvas.Invalidate();
            }

            // Canvas pozisyonunu sürekli kontrol et
            if (overlayCanvas.Size != vlcControl.Size || overlayCanvas.Location != vlcControl.Location)
            {
                UpdateOverlayCanvasPosition();
            }
        }

       

        private void DrawOverlayContent(Graphics g, OverlayItem overlay, Rectangle rect)
        {
            try
            {
                if (overlay.Type == OverlayType.Image && !string.IsNullOrEmpty(overlay.FilePath))
                {
                    if (!cachedImages.ContainsKey(overlay))
                    {
                        if (File.Exists(overlay.FilePath))
                        {
                            try
                            {
                                cachedImages[overlay] = Image.FromFile(overlay.FilePath);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Resim yükleme hatası: {ex.Message}");
                                return;
                            }
                        }
                        else
                        {
                            return;
                        }
                    }

                    var image = cachedImages[overlay];

                    // Alpha blending - Edit modunda daha belirgin
                    float alpha = isOverlayEditMode && selectedOverlayIndex >= 0 && overlayItems[selectedOverlayIndex] == overlay ? 1.0f : 0.85f;

                    using (var attributes = new System.Drawing.Imaging.ImageAttributes())
                    {
                        var colorMatrix = new System.Drawing.Imaging.ColorMatrix();
                        colorMatrix.Matrix33 = alpha;
                        attributes.SetColorMatrix(colorMatrix);

                        g.DrawImage(image, rect, 0, 0, image.Width, image.Height,
                                  GraphicsUnit.Pixel, attributes);
                    }
                }
                else if (overlay.Type == OverlayType.Text && !string.IsNullOrEmpty(overlay.Text))
                {
                    float fontSize = GetScaledFontSize(overlay.FontSize);

                    using (var font = new Font(overlay.FontName, fontSize, FontStyle.Regular))
                    using (var brush = new SolidBrush(overlay.TextColor))
                    using (var shadowBrush = new SolidBrush(Color.FromArgb(128, Color.Black)))
                    {
                        var format = new StringFormat
                        {
                            Alignment = StringAlignment.Near,
                            LineAlignment = StringAlignment.Near,
                            FormatFlags = StringFormatFlags.NoWrap
                        };

                        // Gölge efekti
                        var shadowRect = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width, rect.Height);
                        g.DrawString(overlay.Text, font, shadowBrush, shadowRect, format);

                        // Ana metin
                        g.DrawString(overlay.Text, font, brush, rect, format);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Overlay çizim hatası: {ex.Message}");
            }
        }

        private void DrawSelectionFrame(Graphics g, Rectangle rect)
        {
            using (var pen = new Pen(Color.Yellow, 3))
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                g.DrawRectangle(pen, rect);
            }

            using (var pen = new Pen(Color.FromArgb(180, Color.Black), 2))
            {
                rect.Inflate(2, 2);
                g.DrawRectangle(pen, rect);
            }
        }

        private void DrawResizeHandles(Graphics g, Rectangle rect)
        {
            int handleSize = 12;
            var handleBrush = new SolidBrush(Color.Yellow);
            var handlePen = new Pen(Color.Black, 2);

            var handles = GetResizeHandles(rect, handleSize);

            for (int i = 0; i < handles.Length; i++)
            {
                g.FillEllipse(handleBrush, handles[i]);
                g.DrawEllipse(handlePen, handles[i]);

                if (resizeHandleIndex == i)
                {
                    using (var activePen = new Pen(Color.Red, 4))
                    {
                        g.DrawEllipse(activePen, handles[i]);
                    }
                }
            }

            handleBrush.Dispose();
            handlePen.Dispose();
        }

        private Rectangle[] GetResizeHandles(Rectangle rect, int handleSize)
        {
            int half = handleSize / 2;
            return new Rectangle[]
            {
                new Rectangle(rect.Left - half, rect.Top - half, handleSize, handleSize),
                new Rectangle(rect.Left + rect.Width/2 - half, rect.Top - half, handleSize, handleSize),
                new Rectangle(rect.Right - half, rect.Top - half, handleSize, handleSize),
                new Rectangle(rect.Right - half, rect.Top + rect.Height/2 - half, handleSize, handleSize),
                new Rectangle(rect.Right - half, rect.Bottom - half, handleSize, handleSize),
                new Rectangle(rect.Left + rect.Width/2 - half, rect.Bottom - half, handleSize, handleSize),
                new Rectangle(rect.Left - half, rect.Bottom - half, handleSize, handleSize),
                new Rectangle(rect.Left - half, rect.Top + rect.Height/2 - half, handleSize, handleSize)
            };
        }

        private void DrawOverlayInfo(Graphics g, OverlayItem overlay, Rectangle rect)
        {
            string info = overlay.Type == OverlayType.Text
                ? $"📝 \"{overlay.Text}\" ({overlay.Width}x{overlay.Height})"
                : $"🖼️ {Path.GetFileName(overlay.FilePath)} ({overlay.Width}x{overlay.Height})";

            if (info.Length > 50) info = info.Substring(0, 50) + "...";

            using (var font = new Font("Segoe UI", 10, FontStyle.Bold))
            {
                var textSize = g.MeasureString(info, font);
                var infoRect = new Rectangle(
                    rect.X,
                    rect.Y - (int)textSize.Height - 8,
                    (int)textSize.Width + 12,
                    (int)textSize.Height + 6
                );

                // Arka plan - daha belirgin
                using (var brush = new SolidBrush(Color.FromArgb(240, Color.Black)))
                {
                    g.FillRectangle(brush, infoRect);
                }

                // Çerçeve
                using (var pen = new Pen(Color.Yellow, 2))
                {
                    g.DrawRectangle(pen, infoRect);
                }

                // Metin
                g.DrawString(info, font, Brushes.Yellow, infoRect.X + 6, infoRect.Y + 3);
            }
        }

        private Rectangle GetScreenRect(OverlayItem overlay)
        {
            // 1920x1080 koordinatlarını gerçek video alanına dönüştür
            float scaleX = (float)actualVideoRect.Width / 1920f;
            float scaleY = (float)actualVideoRect.Height / 1080f;

            return new Rectangle(
                actualVideoRect.X + (int)(overlay.X * scaleX),
                actualVideoRect.Y + (int)(overlay.Y * scaleY),
                (int)(overlay.Width * scaleX),
                (int)(overlay.Height * scaleY)
            );
        }

        private float GetScaledFontSize(int originalSize)
        {
            float scale = Math.Min(actualVideoRect.Width / 1920f, actualVideoRect.Height / 1080f);
            return Math.Max(8, originalSize * scale);
        }

        

        private Cursor GetResizeCursor(int handleIndex)
        {
            switch (handleIndex)
            {
                case 0: case 4: return Cursors.SizeNWSE;
                case 1: case 5: return Cursors.SizeNS;
                case 2: case 6: return Cursors.SizeNESW;
                case 3: case 7: return Cursors.SizeWE;
                default: return Cursors.SizeAll;
            }
        }

       

        private void ResizeOverlay(OverlayItem overlay, int handleIndex, int deltaX, int deltaY)
        {
            int minSize = 20;

            switch (handleIndex)
            {
                case 0: // Top-Left
                    overlay.X += deltaX;
                    overlay.Y += deltaY;
                    overlay.Width -= deltaX;
                    overlay.Height -= deltaY;
                    break;
                case 1: // Top-Center
                    overlay.Y += deltaY;
                    overlay.Height -= deltaY;
                    break;
                case 2: // Top-Right
                    overlay.Y += deltaY;
                    overlay.Width += deltaX;
                    overlay.Height -= deltaY;
                    break;
                case 3: // Middle-Right
                    overlay.Width += deltaX;
                    break;
                case 4: // Bottom-Right
                    overlay.Width += deltaX;
                    overlay.Height += deltaY;
                    break;
                case 5: // Bottom-Center
                    overlay.Height += deltaY;
                    break;
                case 6: // Bottom-Left
                    overlay.X += deltaX;
                    overlay.Width -= deltaX;
                    overlay.Height += deltaY;
                    break;
                case 7: // Middle-Left
                    overlay.X += deltaX;
                    overlay.Width -= deltaX;
                    break;
            }

            // Minimum boyut kontrolü
            if (overlay.Width < minSize)
            {
                if (handleIndex == 0 || handleIndex == 6 || handleIndex == 7)
                    overlay.X -= (minSize - overlay.Width);
                overlay.Width = minSize;
            }

            if (overlay.Height < minSize)
            {
                if (handleIndex == 0 || handleIndex == 1 || handleIndex == 2)
                    overlay.Y -= (minSize - overlay.Height);
                overlay.Height = minSize;
            }

            // Video sınırları içinde tut
            overlay.X = Math.Max(0, Math.Min(overlay.X, 1920 - overlay.Width));
            overlay.Y = Math.Max(0, Math.Min(overlay.Y, 1080 - overlay.Height));
            overlay.Width = Math.Min(overlay.Width, 1920 - overlay.X);
            overlay.Height = Math.Min(overlay.Height, 1080 - overlay.Y);
        }

        private void UpdateMouseCursor(Point location)
        {
            if (!isOverlayEditMode)
            {
                overlayCanvas.Cursor = Cursors.Default;
                return;
            }

            if (selectedOverlayIndex >= 0)
            {
                double currentSeconds = currentPositionMs / 1000.0;
                var overlay = overlayItems[selectedOverlayIndex];

                if (currentSeconds >= overlay.StartTime && currentSeconds <= overlay.EndTime)
                {
                    Rectangle overlayRect = GetScreenRect(overlay);
                    var handles = GetResizeHandles(overlayRect, 12);

                    for (int i = 0; i < handles.Length; i++)
                    {
                        if (handles[i].Contains(location))
                        {
                            overlayCanvas.Cursor = GetResizeCursor(i);
                            return;
                        }
                    }

                    if (overlayRect.Contains(location))
                    {
                        overlayCanvas.Cursor = Cursors.SizeAll;
                        return;
                    }
                }
            }

            overlayCanvas.Cursor = Cursors.Default;
        }

        

        // Geri kalan metodlar aynı kalacak... (ShowOverlayContextMenu, SetupOverlayControls, vs.)
        // Sadece gerekli metodları ekleyeceğim:

        private void ShowOverlayContextMenu(Point location, OverlayItem overlay)
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            if (overlay.Type == OverlayType.Text)
            {
                var editItem = new ToolStripMenuItem("📝 Metni Düzenle");
                editItem.Click += (s, e) => EditOverlayText(overlay);
                menu.Items.Add(editItem);
            }

            var timeItem = new ToolStripMenuItem("⏰ Zamanları Ayarla");
            timeItem.Click += (s, e) => EditOverlayTiming(overlay);
            menu.Items.Add(timeItem);

            var duplicateItem = new ToolStripMenuItem("📋 Kopyala");
            duplicateItem.Click += (s, e) => DuplicateOverlay(overlay);
            menu.Items.Add(duplicateItem);

            menu.Items.Add(new ToolStripSeparator());

            var deleteItem = new ToolStripMenuItem("🗑️ Sil");
            deleteItem.Click += (s, e) => DeleteOverlay(overlay);
            menu.Items.Add(deleteItem);

            menu.Show(overlayCanvas, location);
        }

        private void DuplicateOverlay(OverlayItem original)
        {
            var duplicate = new OverlayItem
            {
                Type = original.Type,
                FilePath = original.FilePath,
                Text = original.Text,
                StartTime = original.StartTime,
                EndTime = original.EndTime,
                X = Math.Min(original.X + 20, 1920 - original.Width),
                Y = Math.Min(original.Y + 20, 1080 - original.Height),
                Width = original.Width,
                Height = original.Height,
                FontSize = original.FontSize,
                FontName = original.FontName,
                TextColor = original.TextColor
            };

            overlayItems.Add(duplicate);
            RefreshOverlayList();
            selectedOverlayIndex = overlayItems.Count - 1;
            listBoxOverlays.SelectedIndex = selectedOverlayIndex;
            UpdateOverlayControls();
            overlayCanvas.Invalidate();
        }

        private void EditOverlayText(OverlayItem overlay)
        {
            using (var dialog = new TextEditDialog(overlay.Text, overlay.FontName, overlay.FontSize, overlay.TextColor))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    overlay.Text = dialog.Text;
                    overlay.FontName = dialog.FontName;
                    overlay.FontSize = dialog.FontSize;
                    overlay.TextColor = dialog.TextColor;

                    RefreshOverlayList();
                    UpdateOverlayControls();
                    overlayCanvas.Invalidate();
                }
            }
        }

        private void EditOverlayTiming(OverlayItem overlay)
        {
            using (var dialog = new TimingEditDialog(overlay.StartTime, overlay.EndTime))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    overlay.StartTime = dialog.StartTime;
                    overlay.EndTime = dialog.EndTime;

                    RefreshOverlayList();
                    UpdateOverlayControls();
                    overlayCanvas.Invalidate();
                }
            }
        }

        private void DeleteOverlay(OverlayItem overlay)
        {
            if (MessageBox.Show("Overlay'i silmek istediğinizden emin misiniz?", "Silme Onayı",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (cachedImages.ContainsKey(overlay))
                {
                    cachedImages[overlay].Dispose();
                    cachedImages.Remove(overlay);
                }

                overlayItems.Remove(overlay);
                selectedOverlayIndex = -1;
                RefreshOverlayList();
                UpdateOverlayControls();
                overlayCanvas.Invalidate();
            }
        }

        private void SetupOverlayControls()
        {
            listBoxOverlays.DrawMode = DrawMode.OwnerDrawFixed;
            listBoxOverlays.ItemHeight = 25;
            listBoxOverlays.DrawItem += ListBoxOverlays_DrawItem;
            listBoxOverlays.SelectedIndexChanged += ListBoxOverlays_SelectedIndexChanged;

            listBoxVideos.DrawMode = DrawMode.OwnerDrawFixed;
            listBoxVideos.ItemHeight = 20;
            listBoxVideos.DrawItem += ListBoxVideos_DrawItem;

            numericStartTime.Maximum = decimal.MaxValue;
            numericEndTime.Maximum = decimal.MaxValue;
            numericX.Maximum = 1920;
            numericY.Maximum = 1080;
            numericWidth.Maximum = 1920;
            numericHeight.Maximum = 1080;
            numericFontSize.Minimum = 8;
            numericFontSize.Maximum = 200;
            numericFontSize.Value = 24;

            comboBoxFont.Items.AddRange(FontFamily.Families.Select(f => f.Name).ToArray());
            if (comboBoxFont.Items.Contains("Arial"))
                comboBoxFont.SelectedItem = "Arial";
            else if (comboBoxFont.Items.Count > 0)
                comboBoxFont.SelectedIndex = 0;

            UpdateOverlayControls();
        }

        private void ListBoxOverlays_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= overlayItems.Count) return;

            e.DrawBackground();

            var item = overlayItems[e.Index];
            string displayText = "";
            Color iconColor = Color.White;

            switch (item.Type)
            {
                case OverlayType.Image:
                    displayText = $"🖼️ {Path.GetFileName(item.FilePath)} ({item.StartTime:F1}s-{item.EndTime:F1}s)";
                    iconColor = Color.LightBlue;
                    break;
                case OverlayType.Text:
                    string preview = item.Text.Length > 20 ? item.Text.Substring(0, 20) + "..." : item.Text;
                    displayText = $"📝 \"{preview}\" ({item.StartTime:F1}s-{item.EndTime:F1}s)";
                    iconColor = Color.LightGreen;
                    break;
            }

            using (SolidBrush brush = new SolidBrush(e.Index == listBoxOverlays.SelectedIndex ? Color.White : iconColor))
            {
                e.Graphics.DrawString(displayText, e.Font, brush, e.Bounds.Left + 5, e.Bounds.Top + 5);
            }

            e.DrawFocusRectangle();
        }

        private void ListBoxVideos_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= videoFiles.Count) return;

            e.DrawBackground();

            string displayText = $"{e.Index + 1}. {Path.GetFileName(videoFiles[e.Index])}";

            using (SolidBrush brush = new SolidBrush(e.Index == listBoxVideos.SelectedIndex ? Color.White : Color.LightGray))
            {
                e.Graphics.DrawString(displayText, e.Font, brush, e.Bounds.Left + 5, e.Bounds.Top + 2);
            }

            e.DrawFocusRectangle();
        }

        private void ListBoxOverlays_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedOverlayIndex = listBoxOverlays.SelectedIndex;
            UpdateOverlayControls();
            overlayCanvas.Invalidate();
        }

        private void UpdateOverlayControls()
        {
            bool hasSelection = selectedOverlayIndex >= 0 && selectedOverlayIndex < overlayItems.Count;

            groupBoxOverlayEdit.Enabled = hasSelection;

            if (hasSelection && !updatingControls)
            {
                updatingControls = true;

                var item = overlayItems[selectedOverlayIndex];
                numericStartTime.Value = (decimal)item.StartTime;
                numericEndTime.Value = (decimal)item.EndTime;
                numericX.Value = item.X;
                numericY.Value = item.Y;
                numericWidth.Value = item.Width;
                numericHeight.Value = item.Height;

                if (item.Type == OverlayType.Text)
                {
                    textBoxContent.Text = item.Text;
                    numericFontSize.Value = item.FontSize;
                    if (comboBoxFont.Items.Contains(item.FontName))
                        comboBoxFont.SelectedItem = item.FontName;
                    buttonTextColor.BackColor = item.TextColor;
                    groupBoxTextProperties.Enabled = true;
                }
                else
                {
                    textBoxContent.Text = item.FilePath;
                    groupBoxTextProperties.Enabled = false;
                }

                updatingControls = false;
            }
            else if (!hasSelection)
            {
                textBoxContent.Clear();
                groupBoxTextProperties.Enabled = false;
            }
        }

        private void SetupVolumeControl()
        {
            trackBarVolume.Minimum = 0;
            trackBarVolume.Maximum = 500;
            trackBarVolume.Value = 100;
            trackBarVolume.TickFrequency = 50;
            currentVolume = 100;
            lblVolumeValue.Text = "100%";
        }

        private void SetupKeyboardShortcuts()
        {
            this.KeyPreview = true;
            this.KeyDown += EditForm_KeyDown;
        }

        private void EditForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (!isVideoLoaded) return;

            switch (e.KeyCode)
            {
                case Keys.Space:
                    e.Handled = true;
                    TogglePlayPause();
                    break;
                case Keys.Escape:
                    e.Handled = true;
                    btnStop_Click(null, null);
                    break;
                case Keys.Left:
                    e.Handled = true;
                    SeekRelative(-5000);
                    break;
                case Keys.Right:
                    e.Handled = true;
                    SeekRelative(5000);
                    break;
                case Keys.Home:
                    e.Handled = true;
                    SeekToPosition(startTrimMs);
                    break;
                case Keys.End:
                    e.Handled = true;
                    SeekToPosition(endTrimMs - 1000);
                    break;
                case Keys.I:
                    if (e.Control)
                    {
                        e.Handled = true;
                        btnSetStart_Click(null, null);
                    }
                    break;
                case Keys.O:
                    if (e.Control)
                    {
                        e.Handled = true;
                        btnSetEnd_Click(null, null);
                    }
                    break;
                case Keys.P:
                    if (e.Control)
                    {
                        e.Handled = true;
                        btnTrimVideo_Click(null, null);
                    }
                    break;
                case Keys.E:
                    if (e.Control)
                    {
                        e.Handled = true;
                        btnExport_Click(null, null);
                    }
                    break;
                case Keys.Up:
                    e.Handled = true;
                    ChangeVolume(10);
                    break;
                case Keys.Down:
                    e.Handled = true;
                    ChangeVolume(-10);
                    break;
                case Keys.Delete:
                    if (selectedOverlayIndex >= 0)
                    {
                        e.Handled = true;
                        btnRemoveOverlay_Click(null, null);
                    }
                    break;
                case Keys.F2:
                    e.Handled = true;
                    //btnToggleOverlayEdit_Click(null, null);
                    break;
                case Keys.F1:
                    e.Handled = true;
                    ShowKeyboardShortcuts();
                    break;
            }
        }

        private void ChangeVolume(int delta)
        {
            int newVolume = Math.Max(0, Math.Min(500, currentVolume + delta));
            trackBarVolume.Value = newVolume;
            SetVolume(newVolume);
        }

        private void SetVolume(int volume)
        {
            currentVolume = volume;
            lblVolumeValue.Text = $"{volume}%";

            if (mediaPlayer != null)
            {
                mediaPlayer.Volume = volume;
            }
        }

        private void TogglePlayPause()
        {
            if (mediaPlayer != null && isVideoLoaded)
            {
                if (mediaPlayer.IsPlaying)
                {
                    mediaPlayer.Pause();
                }
                else
                {
                    if (currentPositionMs < startTrimMs || currentPositionMs >= endTrimMs)
                    {
                        mediaPlayer.Time = startTrimMs;
                    }
                    mediaPlayer.Play();
                }
            }
        }

        private void SeekRelative(long offsetMs)
        {
            if (mediaPlayer != null && isVideoLoaded)
            {
                long newPosition = currentPositionMs + offsetMs;
                newPosition = Math.Max(0, Math.Min(newPosition, videoDurationMs));
                mediaPlayer.Time = newPosition;
            }
        }

        private void SeekToPosition(long positionMs)
        {
            if (mediaPlayer != null && isVideoLoaded)
            {
                positionMs = Math.Max(0, Math.Min(positionMs, videoDurationMs));
                mediaPlayer.Time = positionMs;
            }
        }

        private void ShowKeyboardShortcuts()
        {
            string shortcuts = @"⌨️ KLAVYE KISAYOLLARI:

🎮 OYNATMA KONTROLLERI:
• Boşluk (Space) - Oynat/Duraklat
• Escape - Durdur
• ← Sol Ok - 5 saniye geri sar
• → Sağ Ok - 5 saniye ileri sar
• Home - Başlangıç noktasına git
• End - Bitiş noktasına git

🔊 SES KONTROLLERI:
• ↑ Yukarı Ok - Ses +10%
• ↓ Aşağı Ok - Ses -10%

✂️ KESME İŞLEMLERİ:
• Ctrl + I - Başlangıç noktası ayarla
• Ctrl + O - Bitiş noktası ayarla
• Ctrl + P - Kırpılmış bölgeyi oynat
• Ctrl + E - Videoyu dışa aktar

🎨 OVERLAY İŞLEMLERİ:
• F2 - Visual edit modu aç/kapat
• Delete - Seçili overlay'i sil
• Sağ Tık - Overlay context menü
• Drag & Drop - Overlay taşıma (video içinde)
• Resize handles - Boyutlandırma (sınır korumalı)

ℹ️ YARDIM:
• F1 - Bu yardım penceresini göster

👤 Kullanıcı: CaYatur
📅 Tarih: 2025-07-15 00:48:34 UTC";

            MessageBox.Show(shortcuts, "Klavye Kısayolları", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void InitializeVLC()
        {
            try
            {
                Core.Initialize();
                libVLC = new LibVLC();
                mediaPlayer = new MediaPlayer(libVLC);
                vlcControl.MediaPlayer = mediaPlayer;

                mediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
                mediaPlayer.LengthChanged += MediaPlayer_LengthChanged;
                mediaPlayer.EndReached += MediaPlayer_EndReached;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"VLC başlatılırken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupTimer()
        {
            positionTimer = new System.Windows.Forms.Timer();
            positionTimer.Interval = 100;
            positionTimer.Tick += PositionTimer_Tick;
        }

        private void PositionTimer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer != null && isVideoLoaded)
            {
                currentPositionMs = mediaPlayer.Time;
                UpdatePositionDisplay();
                UpdateDetailedInfo();

                if (!isDraggingPosition && !isDraggingStart && !isDraggingEnd)
                {
                    UpdateTimelineDisplay();
                }

                if (currentPositionMs >= endTrimMs && mediaPlayer.IsPlaying)
                {
                    mediaPlayer.Pause();
                }
            }
        }

        private void UpdateDetailedInfo()
        {
            if (videoDurationMs == 0) return;

            long elapsedFromStart = Math.Max(0, currentPositionMs - startTrimMs);
            TimeSpan elapsed = TimeSpan.FromMilliseconds(elapsedFromStart);

            long remainingToEnd = Math.Max(0, endTrimMs - currentPositionMs);
            TimeSpan remaining = TimeSpan.FromMilliseconds(remainingToEnd);

            long trimDuration = endTrimMs - startTrimMs;
            TimeSpan trimTime = TimeSpan.FromMilliseconds(trimDuration);

            double progressPercent = trimDuration > 0 ? (double)elapsedFromStart / trimDuration * 100 : 0;

            lblDetailedInfo.Text = $"📊 Kesim: {elapsed:mm\\:ss\\:fff} / {trimTime:mm\\:ss\\:fff} " +
                                 $"({progressPercent:F1}%) | Kalan: {remaining:mm\\:ss\\:fff}";
        }

        private void MediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            currentPositionMs = e.Time;
        }

        private void MediaPlayer_LengthChanged(object sender, MediaPlayerLengthChangedEventArgs e)
        {
            videoDurationMs = e.Length;
            endTrimMs = videoDurationMs;

            this.Invoke((System.Windows.Forms.MethodInvoker)delegate
            {
                UpdateVideoInfo();
                EnableControls(true);
                isVideoLoaded = true;
                positionTimer.Start();

                if (mediaPlayer != null)
                {
                    mediaPlayer.Volume = currentVolume;
                }

                // Video yüklendiğinde aspect ratio'yu güncelle
                videoAspectRatio = 16f / 9f; // Varsayılan, gerçek aspect ratio VLC'den alınabilir
                CalculateActualVideoRect();

                // Overlay canvas'ı güncelle
                UpdateOverlayCanvasPosition();
            });
        }

        private void MediaPlayer_EndReached(object sender, EventArgs e)
        {
            this.Invoke((System.Windows.Forms.MethodInvoker)delegate
            {
                mediaPlayer.Stop();
            });
        }

        private void btnAddImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff|All Files|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    double currentTimeSeconds = currentPositionMs / 1000.0;

                    var overlay = new OverlayItem
                    {
                        Type = OverlayType.Image,
                        FilePath = ofd.FileName,
                        StartTime = currentTimeSeconds,
                        EndTime = currentTimeSeconds + 5.0,
                        X = 100,
                        Y = 100,
                        Width = 300,
                        Height = 200
                    };

                    overlayItems.Add(overlay);
                    RefreshOverlayList();

                    if (!isOverlayEditMode)
                    {
                        btnToggleOverlayEdit_Click(null, null);
                    }

                    selectedOverlayIndex = overlayItems.Count - 1;
                    listBoxOverlays.SelectedIndex = selectedOverlayIndex;
                    UpdateOverlayControls();
                    overlayCanvas.Invalidate();
                }
            }
        }

        private void btnAddText_Click(object sender, EventArgs e)
        {
            double currentTimeSeconds = currentPositionMs / 1000.0;

            var overlay = new OverlayItem
            {
                Type = OverlayType.Text,
                Text = "Yeni Metin",
                StartTime = currentTimeSeconds,
                EndTime = currentTimeSeconds + 3.0,
                X = 200,
                Y = 200,
                Width = 400,
                Height = 60,
                FontSize = 36,
                FontName = "Arial",
                TextColor = Color.White
            };

            overlayItems.Add(overlay);
            RefreshOverlayList();

            if (!isOverlayEditMode)
            {
                btnToggleOverlayEdit_Click(null, null);
            }

            selectedOverlayIndex = overlayItems.Count - 1;
            listBoxOverlays.SelectedIndex = selectedOverlayIndex;
            UpdateOverlayControls();
            overlayCanvas.Invalidate();

            EditOverlayText(overlay);
        }

        private void btnRemoveOverlay_Click(object sender, EventArgs e)
        {
            if (selectedOverlayIndex >= 0 && selectedOverlayIndex < overlayItems.Count)
            {
                var overlay = overlayItems[selectedOverlayIndex];

                if (cachedImages.ContainsKey(overlay))
                {
                    cachedImages[overlay].Dispose();
                    cachedImages.Remove(overlay);
                }

                overlayItems.RemoveAt(selectedOverlayIndex);
                RefreshOverlayList();
                selectedOverlayIndex = -1;
                listBoxOverlays.SelectedIndex = -1;
                UpdateOverlayControls();
                overlayCanvas.Invalidate();
            }
        }

        private void RefreshOverlayList()
        {
            listBoxOverlays.Items.Clear();
            for (int i = 0; i < overlayItems.Count; i++)
            {
                listBoxOverlays.Items.Add($"Item {i}");
            }
            listBoxOverlays.Invalidate();
        }

        private void btnAddVideo_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Video Files|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.flv;*.m4v;*.3gp|All Files|*.*";
                ofd.Multiselect = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    foreach (string file in ofd.FileNames)
                    {
                        if (!videoFiles.Contains(file))
                        {
                            videoFiles.Add(file);
                        }
                    }
                    RefreshVideoList();
                }
            }
        }

        private void btnRemoveVideo_Click(object sender, EventArgs e)
        {
            if (listBoxVideos.SelectedIndex >= 0)
            {
                videoFiles.RemoveAt(listBoxVideos.SelectedIndex);
                RefreshVideoList();
            }
        }

        private void btnMoveVideoUp_Click(object sender, EventArgs e)
        {
            int index = listBoxVideos.SelectedIndex;
            if (index > 0)
            {
                var temp = videoFiles[index];
                videoFiles[index] = videoFiles[index - 1];
                videoFiles[index - 1] = temp;
                RefreshVideoList();
                listBoxVideos.SelectedIndex = index - 1;
            }
        }

        private void btnMoveVideoDown_Click(object sender, EventArgs e)
        {
            int index = listBoxVideos.SelectedIndex;
            if (index >= 0 && index < videoFiles.Count - 1)
            {
                var temp = videoFiles[index];
                videoFiles[index] = videoFiles[index + 1];
                videoFiles[index + 1] = temp;
                RefreshVideoList();
                listBoxVideos.SelectedIndex = index + 1;
            }
        }

        private void RefreshVideoList()
        {
            listBoxVideos.Items.Clear();
            for (int i = 0; i < videoFiles.Count; i++)
            {
                listBoxVideos.Items.Add($"Video {i}");
            }
            listBoxVideos.Invalidate();

            btnMergeVideos.Enabled = videoFiles.Count > 1;
        }

        private void numericStartTime_ValueChanged(object sender, EventArgs e)
        {
            if (!updatingControls) UpdateSelectedOverlay();
        }

        private void numericEndTime_ValueChanged(object sender, EventArgs e)
        {
            if (!updatingControls) UpdateSelectedOverlay();
        }

        private void numericX_ValueChanged(object sender, EventArgs e)
        {
            if (!updatingControls) UpdateSelectedOverlay();
        }

        private void numericY_ValueChanged(object sender, EventArgs e)
        {
            if (!updatingControls) UpdateSelectedOverlay();
        }

        private void numericWidth_ValueChanged(object sender, EventArgs e)
        {
            if (!updatingControls) UpdateSelectedOverlay();
        }

        private void numericHeight_ValueChanged(object sender, EventArgs e)
        {
            if (!updatingControls) UpdateSelectedOverlay();
        }

        private void textBoxContent_TextChanged(object sender, EventArgs e)
        {
            if (!updatingControls) UpdateSelectedOverlay();
        }

        private void numericFontSize_ValueChanged(object sender, EventArgs e)
        {
            if (!updatingControls) UpdateSelectedOverlay();
        }

        private void comboBoxFont_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!updatingControls) UpdateSelectedOverlay();
        }

        private void buttonTextColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                cd.Color = buttonTextColor.BackColor;
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    buttonTextColor.BackColor = cd.Color;
                    if (!updatingControls) UpdateSelectedOverlay();
                }
            }
        }

        private void UpdateSelectedOverlay()
        {
            if (selectedOverlayIndex >= 0 && selectedOverlayIndex < overlayItems.Count)
            {
                var item = overlayItems[selectedOverlayIndex];
                item.StartTime = (double)numericStartTime.Value;
                item.EndTime = (double)numericEndTime.Value;

                // Video sınırları içinde tut
                item.X = Math.Max(0, Math.Min((int)numericX.Value, 1920 - item.Width));
                item.Y = Math.Max(0, Math.Min((int)numericY.Value, 1080 - item.Height));
                item.Width = Math.Max(20, Math.Min((int)numericWidth.Value, 1920 - item.X));
                item.Height = Math.Max(20, Math.Min((int)numericHeight.Value, 1080 - item.Y));

                if (item.Type == OverlayType.Text)
                {
                    item.Text = textBoxContent.Text;
                    item.FontSize = (int)numericFontSize.Value;
                    item.FontName = comboBoxFont.SelectedItem?.ToString() ?? "Arial";
                    item.TextColor = buttonTextColor.BackColor;
                }

                RefreshOverlayList();
                overlayCanvas.Invalidate();
            }
        }

        private void btnOpenVideo_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Video Files|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.flv;*.m4v;*.3gp|All Files|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    LoadVideo(ofd.FileName);
                }
            }
        }

        

        private string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} MB";
            return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
        }

        private void UpdateVideoInfo()
        {
            if (videoDurationMs > 0)
            {
                TimeSpan duration = TimeSpan.FromMilliseconds(videoDurationMs);
                lblDuration.Text = $"🎬 Toplam: {duration:hh\\:mm\\:ss}";

                panelTimeline.Invalidate();
                UpdateTrimInfo();

                double maxSeconds = videoDurationMs / 1000.0;
                numericStartTime.Maximum = (decimal)maxSeconds;
                numericEndTime.Maximum = (decimal)maxSeconds;
            }
        }

        private void UpdatePositionDisplay()
        {
            TimeSpan position = TimeSpan.FromMilliseconds(currentPositionMs);
            lblCurrentTime.Text = position.ToString(@"hh\:mm\:ss");
        }

        private void UpdateTrimInfo()
        {
            TimeSpan startTime = TimeSpan.FromMilliseconds(startTrimMs);
            TimeSpan endTime = TimeSpan.FromMilliseconds(endTrimMs);
            TimeSpan trimmedDuration = TimeSpan.FromMilliseconds(endTrimMs - startTrimMs);

            double trimPercent = videoDurationMs > 0 ? (double)(endTrimMs - startTrimMs) / videoDurationMs * 100 : 0;

            lblTrimInfo.Text = $"✂️ Başlangıç: {startTime:hh\\:mm\\:ss} | " +
                             $"🏁 Bitiş: {endTime:hh\\:mm\\:ss}\n" +
                             $"⏱️ Kesim Süresi: {trimmedDuration:hh\\:mm\\:ss} " +
                             $"({trimPercent:F1}% orijinalin)";
        }

        private void EnableControls(bool enabled)
        {
            btnPlay.Enabled = enabled;
            btnPause.Enabled = enabled;
            btnStop.Enabled = enabled;
            btnSetStart.Enabled = enabled;
            btnSetEnd.Enabled = enabled;
            btnTrimVideo.Enabled = enabled;
            btnExport.Enabled = enabled;
            trackBarVolume.Enabled = enabled;

            btnAddImage.Enabled = enabled;
            btnAddText.Enabled = enabled;
            btnToggleOverlayEdit.Enabled = enabled;
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (mediaPlayer != null && isVideoLoaded)
            {
                if (currentPositionMs < startTrimMs || currentPositionMs >= endTrimMs)
                {
                    mediaPlayer.Time = startTrimMs;
                }
                mediaPlayer.Play();
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.Pause();
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.Stop();
                mediaPlayer.Time = startTrimMs;
            }
        }

        private void btnSetStart_Click(object sender, EventArgs e)
        {
            startTrimMs = currentPositionMs;
            if (startTrimMs >= endTrimMs)
            {
                endTrimMs = Math.Min(startTrimMs + 1000, videoDurationMs);
            }
            panelTimeline.Invalidate();
            UpdateTrimInfo();
        }

        private void btnSetEnd_Click(object sender, EventArgs e)
        {
            endTrimMs = currentPositionMs;
            if (endTrimMs <= startTrimMs)
            {
                startTrimMs = Math.Max(endTrimMs - 1000, 0);
            }
            panelTimeline.Invalidate();
            UpdateTrimInfo();
        }

        private void btnTrimVideo_Click(object sender, EventArgs e)
        {
            if (mediaPlayer != null && isVideoLoaded)
            {
                mediaPlayer.Time = startTrimMs;
                mediaPlayer.Play();
            }
        }

        private void trackBarVolume_Scroll(object sender, EventArgs e)
        {
            SetVolume(trackBarVolume.Value);
        }

        // Export ve timeline metodları aynı kalacak - önceki versiyonda yazdığım gibi
        private void btnExport_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentVideoPath))
            {
                MessageBox.Show("Önce bir video yükleyin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "MP4 Video|*.mp4";
                sfd.FileName = Path.GetFileNameWithoutExtension(currentVideoPath) + "_edited.mp4";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    ExportVideoWithOverlays(sfd.FileName);
                }
            }
        }

        private async void ExportVideoWithOverlays(string outputPath)
        {
            try
            {
                progressBar.Visible = true;
                progressBar.Style = ProgressBarStyle.Marquee;
                btnExport.Enabled = false;

                string ffmpegPath = FindFFmpegPath();
                if (string.IsNullOrEmpty(ffmpegPath))
                {
                    MessageBox.Show("FFmpeg bulunamadı! Lütfen FFmpeg'i indirip sistem PATH'ine ekleyin veya program klasörüne koyun.\n\nFFmpeg indirme: https://ffmpeg.org/download.html",
                        "FFmpeg Bulunamadı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    progressBar.Visible = false;
                    btnExport.Enabled = true;
                    return;
                }

                string tempDir = Path.GetTempPath();
                string tempInputFile = Path.Combine(tempDir, $"video_input_{Guid.NewGuid().ToString("N")[..8]}.tmp");
                string tempOutputFile = Path.Combine(tempDir, $"video_output_{Guid.NewGuid().ToString("N")[..8]}.mp4");

                try
                {
                    File.Copy(currentVideoPath, tempInputFile, true);

                    double startSeconds = startTrimMs / 1000.0;
                    double endSeconds = endTrimMs / 1000.0;

                    List<string> arguments = new List<string>
                    {
                        "-hide_banner",
                        "-loglevel", "warning",
                        "-i", tempInputFile
                    };

                    List<string> tempOverlayFiles = new List<string>();
                    var imageOverlays = overlayItems.Where(o => o.Type == OverlayType.Image && File.Exists(o.FilePath)).ToList();

                    foreach (var overlay in imageOverlays)
                    {
                        string tempOverlay = Path.Combine(tempDir, $"overlay_{Guid.NewGuid().ToString("N")[..8]}{Path.GetExtension(overlay.FilePath)}");
                        File.Copy(overlay.FilePath, tempOverlay, true);
                        tempOverlayFiles.Add(tempOverlay);
                        arguments.AddRange(new[] { "-i", tempOverlay });
                    }

                    arguments.AddRange(new[]
                    {
                        "-ss", startSeconds.ToString("F3", System.Globalization.CultureInfo.InvariantCulture),
                        "-to", endSeconds.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)
                    });

                    if (overlayItems.Count > 0)
                    {
                        List<string> filterCommands = new List<string>();
                        string currentVideoStream = "0:v";
                        int inputIndex = 1;
                        int filterIndex = 0;

                        foreach (var overlay in imageOverlays)
                        {
                            double overlayStart = Math.Max(0, overlay.StartTime - startSeconds);
                            double overlayEnd = Math.Min(endSeconds - startSeconds, overlay.EndTime - startSeconds);

                            if (overlayStart < overlayEnd && overlayEnd > 0)
                            {
                                string scaledLabel = $"scaled{filterIndex}";
                                string overlayLabel = $"out{filterIndex}";

                                filterCommands.Add($"[{inputIndex}:v]scale={overlay.Width}:{overlay.Height}[{scaledLabel}]");

                                string enableExpr = $"between(t,{overlayStart.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)},{overlayEnd.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)})";
                                filterCommands.Add($"[{currentVideoStream}][{scaledLabel}]overlay={overlay.X}:{overlay.Y}:enable='{enableExpr}'[{overlayLabel}]");

                                currentVideoStream = overlayLabel;
                                inputIndex++;
                                filterIndex++;
                            }
                        }

                        foreach (var overlay in overlayItems.Where(o => o.Type == OverlayType.Text))
                        {
                            double overlayStart = Math.Max(0, overlay.StartTime - startSeconds);
                            double overlayEnd = Math.Min(endSeconds - startSeconds, overlay.EndTime - startSeconds);

                            if (overlayStart < overlayEnd && overlayEnd > 0)
                            {
                                string textLabel = $"out{filterIndex}";
                                string hexColor = $"0x{overlay.TextColor.R:X2}{overlay.TextColor.G:X2}{overlay.TextColor.B:X2}";

                                string escapedText = overlay.Text
                                    .Replace("\\", "\\\\\\\\")
                                    .Replace("'", "\\\\\\'")
                                    .Replace(":", "\\\\:")
                                    .Replace(",", "\\\\,")
                                    .Replace("[", "\\\\[")
                                    .Replace("]", "\\\\]")
                                    .Replace("%", "\\\\%");

                                string fontPath = @"C\:\\Windows\\Fonts\\arial.ttf";
                                string enableExpr = $"between(t,{overlayStart.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)},{overlayEnd.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)})";

                                string drawTextCmd = $"[{currentVideoStream}]drawtext=" +
                                    $"fontfile='{fontPath}':" +
                                    $"text='{escapedText}':" +
                                    $"fontsize={overlay.FontSize}:" +
                                    $"fontcolor={hexColor}:" +
                                    $"x={overlay.X}:" +
                                    $"y={overlay.Y}:" +
                                    $"enable='{enableExpr}'[{textLabel}]";

                                filterCommands.Add(drawTextCmd);
                                currentVideoStream = textLabel;
                                filterIndex++;
                            }
                        }

                        if (filterCommands.Count > 0)
                        {
                            string filterComplex = string.Join(";", filterCommands);
                            arguments.AddRange(new[] { "-filter_complex", filterComplex });
                            arguments.AddRange(new[] { "-map", $"[{currentVideoStream}]" });
                        }
                        else
                        {
                            arguments.AddRange(new[] { "-map", "0:v" });
                        }
                    }
                    else
                    {
                        arguments.AddRange(new[] { "-map", "0:v" });
                    }

                    arguments.AddRange(new[] { "-c:v", "libx264", "-preset", "medium", "-crf", "23" });

                    arguments.AddRange(new[] { "-map", "0:a" });
                    if (currentVolume != 100)
                    {
                        double volumeMultiplier = currentVolume / 100.0;
                        arguments.AddRange(new[]
                        {
                            "-filter:a",
                            $"volume={volumeMultiplier.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}",
                            "-c:a", "aac",
                            "-b:a", "128k"
                        });
                    }
                    else
                    {
                        arguments.AddRange(new[] { "-c:a", "aac", "-b:a", "128k" });
                    }

                    arguments.AddRange(new[]
                    {
                        "-movflags", "+faststart",
                        "-y",
                        tempOutputFile
                    });

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = ffmpegPath,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        WorkingDirectory = tempDir
                    };

                    psi.Arguments = string.Join(" ", arguments.Select(arg =>
                    {
                        if (arg.Contains(" ") || arg.Contains("\\") || arg.Contains(":") ||
                            arg.Contains("'") || arg.Contains("[") || arg.Contains("]") ||
                            arg.Contains("=") || arg.Contains(";"))
                        {
                            return $"\"{arg}\"";
                        }
                        return arg;
                    }));

                    Process process = new Process { StartInfo = psi };

                    StringBuilder output = new StringBuilder();
                    StringBuilder error = new StringBuilder();

                    process.OutputDataReceived += (s, e) => {
                        if (e.Data != null)
                        {
                            output.AppendLine(e.Data);
                        }
                    };

                    process.ErrorDataReceived += (s, e) => {
                        if (e.Data != null)
                        {
                            error.AppendLine(e.Data);
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    bool finished = await Task.Run(() => process.WaitForExit(600000));

                    if (!finished)
                    {
                        try { process.Kill(); } catch { }
                        throw new Exception("FFmpeg işlemi zaman aşımına uğradı");
                    }

                    progressBar.Visible = false;
                    btnExport.Enabled = true;

                    if (process.ExitCode == 0 && File.Exists(tempOutputFile) && new FileInfo(tempOutputFile).Length > 0)
                    {
                        if (File.Exists(outputPath))
                            File.Delete(outputPath);

                        File.Move(tempOutputFile, outputPath);

                        FileInfo exportedFile = new FileInfo(outputPath);
                        string overlayInfo = overlayItems.Count > 0 ? $" | 🎨 {overlayItems.Count} overlay" : "";
                        string volumeInfo = currentVolume != 100 ? $" | 🔊 Ses: {currentVolume}%" : "";

                        MessageBox.Show($"Video başarıyla dışa aktarıldı!\n\n" +
                                      $"📁 Konum: {outputPath}\n" +
                                      $"📊 Boyut: {FormatFileSize(exportedFile.Length)}{overlayInfo}{volumeInfo}\n\n" +
                                      $"🎯 Katman sistemi ile mükemmel render!\n" +
                                      $"👤 Editör: CaYatur | 📅 2025-07-15 00:48:34 UTC",
                                      "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        if (MessageBox.Show("Dosyayı klasörde görmek ister misiniz?", "Dosyayı Göster", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            try
                            {
                                Process.Start("explorer.exe", $"/select,\"{outputPath}\"");
                            }
                            catch
                            {
                                try
                                {
                                    Process.Start("explorer.exe", Path.GetDirectoryName(outputPath));
                                }
                                catch { }
                            }
                        }
                    }
                    else
                    {
                        string errorMessage = error.ToString();
                        MessageBox.Show($"Video dışa aktarılırken hata oluştu!\n\n{errorMessage}", "FFmpeg Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    foreach (string tempFile in tempOverlayFiles)
                    {
                        try { File.Delete(tempFile); } catch { }
                    }
                }
                finally
                {
                    try
                    {
                        if (File.Exists(tempInputFile))
                            File.Delete(tempInputFile);
                        if (File.Exists(tempOutputFile))
                            File.Delete(tempOutputFile);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                progressBar.Visible = false;
                btnExport.Enabled = true;
                MessageBox.Show($"Dışa aktarma hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnMergeVideos_Click(object sender, EventArgs e)
        {
            if (videoFiles.Count < 2)
            {
                MessageBox.Show("En az 2 video dosyası gereklidir.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "MP4 Video|*.mp4";
                sfd.FileName = "merged_video.mp4";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    MergeVideos(sfd.FileName);
                }
            }
        }

        private async void MergeVideos(string outputPath)
        {
            try
            {
                progressBar.Visible = true;
                progressBar.Style = ProgressBarStyle.Marquee;
                btnMergeVideos.Enabled = false;

                string ffmpegPath = FindFFmpegPath();
                if (string.IsNullOrEmpty(ffmpegPath))
                {
                    MessageBox.Show("FFmpeg bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    progressBar.Visible = false;
                    btnMergeVideos.Enabled = true;
                    return;
                }

                // Geçici dosya listesi oluştur
                string tempDir = Path.GetTempPath();
                string listFile = Path.Combine(tempDir, $"concat_list_{Guid.NewGuid().ToString("N")[..8]}.txt");
                List<string> tempVideoFiles = new List<string>();
                List<string> reencodedVideoFiles = new List<string>();

                try
                {
                    // 1. Videoları ortak profile re-encode et
                    for (int i = 0; i < videoFiles.Count; i++)
                    {
                        string tempVideo = Path.Combine(tempDir, $"video_{i}_{Guid.NewGuid().ToString("N")[..8]}.mp4");
                        // Hedef profil: 1920x1080, 30fps, H264/AAC
                        string args = $"-i \"{videoFiles[i]}\" -vf \"scale=1920:1080,fps=30\" -c:v libx264 -preset fast -c:a aac -b:a 192k -y \"{tempVideo}\"";

                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = ffmpegPath,
                            Arguments = args,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        using (var proc = Process.Start(psi))
                        {
                            await Task.Run(() => proc.WaitForExit());
                        }

                        reencodedVideoFiles.Add(tempVideo);
                    }

                    // 2. Concat listesi oluştur
                    StringBuilder listContent = new StringBuilder();
                    foreach (var file in reencodedVideoFiles)
                        listContent.AppendLine($"file '{file.Replace("\\", "/")}'");
                    File.WriteAllText(listFile, listContent.ToString());

                    // 3. Birleştir
                    List<string> arguments = new List<string>
            {
                "-hide_banner",
                "-loglevel", "warning",
                "-f", "concat",
                "-safe", "0",
                "-i", listFile,
                "-c", "copy",
                "-y",
                outputPath
            };

                    ProcessStartInfo mergePsi = new ProcessStartInfo
                    {
                        FileName = ffmpegPath,
                        Arguments = string.Join(" ", arguments.Select(arg =>
                            arg.Contains(" ") || arg.Contains("\\") ? $"\"{arg}\"" : arg)),
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        WorkingDirectory = tempDir
                    };

                    Debug.WriteLine($"FFmpeg merge komutu: {mergePsi.FileName} {mergePsi.Arguments}");

                    Process process = new Process { StartInfo = mergePsi };

                    StringBuilder output = new StringBuilder();
                    StringBuilder error = new StringBuilder();

                    process.OutputDataReceived += (s, e) => {
                        if (e.Data != null)
                        {
                            output.AppendLine(e.Data);
                            Debug.WriteLine($"FFmpeg Merge Output: {e.Data}");
                        }
                    };
                    process.ErrorDataReceived += (s, e) => {
                        if (e.Data != null)
                        {
                            error.AppendLine(e.Data);
                            Debug.WriteLine($"FFmpeg Merge Error: {e.Data}");
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    bool finished = await Task.Run(() => process.WaitForExit(600000)); // 10 dakika timeout

                    progressBar.Visible = false;
                    btnMergeVideos.Enabled = true;

                    if (!finished)
                    {
                        process.Kill();
                        throw new Exception("Video birleştirme işlemi zaman aşımına uğradı (10 dakika)");
                    }

                    if (process.ExitCode == 0 && File.Exists(outputPath) && new FileInfo(outputPath).Length > 0)
                    {
                        FileInfo mergedFile = new FileInfo(outputPath);

                        MessageBox.Show($"Videolar başarıyla birleştirildi!\n\n" +
                                      $"📁 Konum: {outputPath}\n" +
                                      $"📊 Boyut: {FormatFileSize(mergedFile.Length)}\n" +
                                      $"🎬 Video sayısı: {videoFiles.Count}\n\n" +
                                      $"✅ Birleştirme tamamlandı!",
                                      "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        if (MessageBox.Show("Dosyayı klasörde görmek ister misiniz?", "Dosyayı Göster", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            try
                            {
                                Process.Start("explorer.exe", $"/select,\"{outputPath}\"");
                            }
                            catch
                            {
                                Process.Start("explorer.exe", Path.GetDirectoryName(outputPath));
                            }
                        }
                    }
                    else
                    {
                        string errorMessage = error.ToString();
                        MessageBox.Show($"Video birleştirme hatası!\n\nFFmpeg çıkış kodu: {process.ExitCode}\n\n{errorMessage}",
                                      "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                finally
                {
                    // Geçici dosyaları temizle
                    try
                    {
                        if (File.Exists(listFile))
                            File.Delete(listFile);

                        foreach (string tempFile in tempVideoFiles)
                        {
                            if (File.Exists(tempFile))
                                File.Delete(tempFile);
                        }
                        foreach (string tempFile in reencodedVideoFiles)
                        {
                            if (File.Exists(tempFile))
                                File.Delete(tempFile);
                        }
                    }
                    catch (Exception cleanupEx)
                    {
                        Debug.WriteLine($"Geçici dosya temizleme hatası: {cleanupEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                progressBar.Visible = false;
                btnMergeVideos.Enabled = true;
                MessageBox.Show($"Video birleştirme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string FindFFmpegPath()
        {
            string localFFmpeg = Path.Combine(Application.StartupPath, "ffmpeg.exe");
            if (File.Exists(localFFmpeg))
                return localFFmpeg;

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "where",
                    Arguments = "ffmpeg",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                    {
                        string firstPath = output.Split('\n')[0].Trim();
                        if (File.Exists(firstPath))
                            return firstPath;
                    }
                }
            }
            catch { }

            return null;
        }

        private void UpdateTimelineDisplay()
        {
            panelTimeline.Invalidate();
        }

        private void panelTimeline_Paint(object sender, PaintEventArgs e)
        {
            if (videoDurationMs == 0) return;

            Graphics g = e.Graphics;
            Rectangle rect = panelTimeline.ClientRectangle;

            g.FillRectangle(Brushes.DarkGray, rect);

            float startX = (float)startTrimMs / videoDurationMs * rect.Width;
            float endX = (float)endTrimMs / videoDurationMs * rect.Width;
            float width = endX - startX;

            Rectangle selectedRect = new Rectangle((int)startX, 0, (int)width, rect.Height);
            g.FillRectangle(Brushes.LightBlue, selectedRect);

            foreach (var overlay in overlayItems)
            {
                double startSec = overlay.StartTime;
                double endSec = overlay.EndTime;
                double videoDurationSec = videoDurationMs / 1000.0;

                float overlayStartX = (float)(startSec / videoDurationSec) * rect.Width;
                float overlayEndX = (float)(endSec / videoDurationSec) * rect.Width;
                float overlayWidth = overlayEndX - overlayStartX;

                if (overlayWidth > 2)
                {
                    Color overlayColor = overlay.Type == OverlayType.Image ? Color.Orange : Color.LimeGreen;
                    Rectangle overlayRect = new Rectangle((int)overlayStartX, rect.Height - 8, (int)overlayWidth, 6);

                    using (SolidBrush brush = new SolidBrush(overlayColor))
                    {
                        g.FillRectangle(brush, overlayRect);
                    }
                    g.DrawRectangle(Pens.White, overlayRect);
                }
            }

            using (Pen pen = new Pen(Color.Red, 3))
            {
                g.DrawLine(pen, startX, 0, startX, rect.Height);
                g.DrawLine(pen, endX, 0, endX, rect.Height);
            }

            float posX = (float)currentPositionMs / videoDurationMs * rect.Width;
            using (Pen pen = new Pen(Color.Yellow, 2))
            {
                g.DrawLine(pen, posX, 0, posX, rect.Height);
            }
        }

        private void panelTimeline_MouseDown(object sender, MouseEventArgs e)
        {
            if (videoDurationMs == 0) return;

            float clickPosition = (float)e.X / panelTimeline.Width * videoDurationMs;
            float startX = (float)startTrimMs / videoDurationMs * panelTimeline.Width;
            float endX = (float)endTrimMs / videoDurationMs * panelTimeline.Width;

            if (Math.Abs(e.X - startX) < 15)
            {
                isDraggingStart = true;
                panelTimeline.Cursor = Cursors.SizeWE;
            }
            else if (Math.Abs(e.X - endX) < 15)
            {
                isDraggingEnd = true;
                panelTimeline.Cursor = Cursors.SizeWE;
            }
            else
            {
                isDraggingPosition = true;
                currentPositionMs = (long)clickPosition;
                if (mediaPlayer != null && isVideoLoaded)
                {
                    mediaPlayer.Time = currentPositionMs;
                }
                panelTimeline.Cursor = Cursors.Hand;
            }

            panelTimeline.Capture = true;
        }

        private void panelTimeline_MouseMove(object sender, MouseEventArgs e)
        {
            if (videoDurationMs == 0) return;

            if (isDraggingStart)
            {
                startTrimMs = (long)((float)e.X / panelTimeline.Width * videoDurationMs);
                startTrimMs = Math.Max(0, Math.Min(startTrimMs, endTrimMs - 1000));
                panelTimeline.Invalidate();
                UpdateTrimInfo();
            }
            else if (isDraggingEnd)
            {
                endTrimMs = (long)((float)e.X / panelTimeline.Width * videoDurationMs);
                endTrimMs = Math.Max(startTrimMs + 1000, Math.Min(endTrimMs, videoDurationMs));
                panelTimeline.Invalidate();
                UpdateTrimInfo();
            }
            else if (isDraggingPosition)
            {
                currentPositionMs = (long)((float)e.X / panelTimeline.Width * videoDurationMs);
                currentPositionMs = Math.Max(0, Math.Min(currentPositionMs, videoDurationMs));
                if (mediaPlayer != null && isVideoLoaded)
                {
                    mediaPlayer.Time = currentPositionMs;
                }
                panelTimeline.Invalidate();
            }
        }

        private void panelTimeline_MouseUp(object sender, MouseEventArgs e)
        {
            isDraggingStart = false;
            isDraggingEnd = false;
            isDraggingPosition = false;
            panelTimeline.Capture = false;
            panelTimeline.Cursor = Cursors.Default;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            positionTimer?.Stop();
            positionTimer?.Dispose();
            overlayUpdateTimer?.Stop();
            overlayUpdateTimer?.Dispose();

            foreach (var image in cachedImages.Values)
            {
                try { image?.Dispose(); } catch { }
            }
            cachedImages.Clear();

            mediaPlayer?.Stop();
            currentMedia?.Dispose();
            mediaPlayer?.Dispose();
            libVLC?.Dispose();

            base.OnFormClosing(e);
        }
    }

    public class OverlayItem
    {
        public OverlayType Type { get; set; }
        public string FilePath { get; set; } = "";
        public string Text { get; set; } = "";
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int FontSize { get; set; } = 24;
        public string FontName { get; set; } = "Arial";
        public Color TextColor { get; set; } = Color.White;
    }

    public enum OverlayType
    {
        Image,
        Text
    }
}