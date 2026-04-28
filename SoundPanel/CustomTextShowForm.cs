using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace SoundPanel
{
    public partial class CustomTextShowForm : Form
    {
        public Screen SelectedScreen { get; private set; }

        public CustomTextShowForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(32, 34, 44);
            ShowScreenSelection();
        }

        private void ShowScreenSelection()
        {
            var screens = Screen.AllScreens;
            var primaryScreen = Screen.PrimaryScreen;
            Form selectForm = new Form
            {
                Text = "Ekran Seçin",
                Width = 500,
                Height = 80 + 80 * screens.Length,
                StartPosition = FormStartPosition.CenterScreen,
                BackColor = Color.FromArgb(32, 34, 44),
                ForeColor = Color.White,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };
            for (int i = 0; i < screens.Length; i++)
            {
                var scr = screens[i];
                string label = $"{scr.DeviceName} ({scr.Bounds.Width}x{scr.Bounds.Height})";
                if (scr == primaryScreen)
                    label += "  (ANA EKRAN)";
                var btn = new Button
                {
                    Text = label,
                    Width = 440,
                    Height = 60,
                    Top = 20 + 70 * i,
                    Left = 20,
                    Tag = scr,
                    BackColor = scr == primaryScreen ? Color.FromArgb(80, 160, 220) : Color.FromArgb(60, 130, 200),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold)
                };
                btn.Click += (s, e) =>
                {
                    SelectedScreen = (Screen)((Button)s).Tag;
                    selectForm.DialogResult = DialogResult.OK;
                    selectForm.Close();
                };
                selectForm.Controls.Add(btn);
            }
            if (selectForm.ShowDialog() != DialogResult.OK)
            {
                this.Close();
                return;
            }
            // Ekran seçildikten sonra özelleþtirme arayüzünü baþlat
            InitializeCustomizationUI();
        }

        private Panel previewPanel;
        private PictureBox previewPictureBox;
        private Button btnAddText;
        private Button btnAddImage;
        private Button btnBgColor;
        private Button btnTextColor;
        private Button btnShowFullscreen;
        private Button btnCloseFullscreen;
        private Button btnDeleteItem; // Sil butonu
        private Color backgroundColor = Color.Black;
        private Color textColor = Color.White; // Varsayýlan beyaz
        private string displayText = "";
        private Font displayFont = new Font("Segoe UI", 24, FontStyle.Bold);
        private Image displayImage = null;
        private Point imageLocation = new Point(50, 50);
        private float previewScale = 1.0f;
        private Size screenSize;
        private RectangleF textRect = new RectangleF(40, 40, 300, 80);
        private Rectangle imageRect = new Rectangle(50, 50, 200, 150);
        private bool resizingText = false;
        private bool resizingImage = false;
        private const int handleSize = 12;

        // Unified layer system
        private enum LayerType { Image, Text }
        private abstract class Layer
        {
            public LayerType Type { get; }
            public RectangleF Rect { get; set; }
            public bool Visible { get; set; } = true;
            protected Layer(LayerType t, RectangleF rect) { Type = t; Rect = rect; }
            public abstract void Draw(Graphics g);
            public virtual bool Contains(Point p) => Rectangle.Round(Rect).Contains(p);
        }
        private class ImageLayer : Layer
        {
            public Image Image { get; set; }
            public float OriginalAspect { get; set; } = 1f; // width/height
            public ImageLayer(Image img, RectangleF rect) : base(LayerType.Image, rect) { Image = img; if (img != null && img.Height != 0) OriginalAspect = img.Width / (float)img.Height; }
            public override void Draw(Graphics g)
            {
                if (Image == null) return;
                g.DrawImage(Image, Rect);
            }
        }
        private class TextLayer : Layer
        {
            public string Text { get; set; }
            public Font Font { get; set; }
            public Color Color { get; set; }
            public TextLayer(string text, Font font, Color color, RectangleF rect) : base(LayerType.Text, rect) { Text = text; Font = font; Color = color; }
            public override void Draw(Graphics g)
            {
                using (Brush brush = new SolidBrush(Color))
                {
                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };
                    g.DrawString(Text, Font, brush, Rect, sf);
                }
            }
        }

        private List<Layer> layers = new List<Layer>();
        private int selectedLayerIndex = -1; // index in layers

        // mouse / handle state
        private enum HandleType { None, Move, N, S, E, W, NE, NW, SE, SW }
        private HandleType currentHandle = HandleType.None;
        private int handleHitIndex = -1; // which layer
        private PointF dragOffset;

        // resizing helpers
        private bool isResizing = false;
        private RectangleF originalRectF;
        private PointF resizeAnchor; // fixed corner or side anchor in real coords

        private void InitializeCustomizationUI()
        {
            this.Controls.Clear();
            this.BackColor = Color.FromArgb(32, 34, 44);
            // Responsive: pencereyi ekran çözünürlüðüne göre ayarla
            int screenW = SelectedScreen.Bounds.Width;
            int screenH = SelectedScreen.Bounds.Height;
            int baseW = 1200, baseH = 800;
            int formW = Math.Max(baseW, Math.Min(screenW, 1600));
            int formH = Math.Max(baseH, Math.Min(screenH, 900));
            this.Size = new Size(formW, formH);

            // Responsive preview panel boyutu
            int maxPreviewW = (int)(formW * 0.6), maxPreviewH = (int)(formH * 0.7);
            screenSize = SelectedScreen.Bounds.Size;
            float scaleW = maxPreviewW / (float)screenSize.Width;
            float scaleH = maxPreviewH / (float)screenSize.Height;
            previewScale = Math.Min(scaleW, scaleH);
            int previewW = (int)(screenSize.Width * previewScale);
            int previewH = (int)(screenSize.Height * previewScale);

            previewPanel = new Panel
            {
                Left = 30,
                Top = 30,
                Width = previewW,
                Height = previewH,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = backgroundColor,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            this.Controls.Add(previewPanel);

            previewPictureBox = new PictureBox
            {
                Left = 0,
                Top = 0,
                Width = previewPanel.Width,
                Height = previewPanel.Height,
                BackColor = backgroundColor,
                SizeMode = PictureBoxSizeMode.Normal,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            previewPanel.Controls.Add(previewPictureBox);
            previewPictureBox.Paint += PreviewPictureBox_Paint;
            previewPictureBox.MouseDown += PreviewPictureBox_MouseDown;
            previewPictureBox.MouseMove += PreviewPictureBox_MouseMove;
            previewPictureBox.MouseUp += PreviewPictureBox_MouseUp;
            previewPictureBox.DoubleClick += PreviewPictureBox_DoubleClick;
            previewPictureBox.Cursor = Cursors.Default;

            // Responsive buton konumlarý
            int btnLeft = previewPanel.Right + 30;
            int btnTop = previewPanel.Top;
            int btnW = 140, btnH = 44, btnGap = 12;

            btnAddText = new Button
            {
                Text = "Yazý Ekle",
                Left = btnLeft,
                Top = btnTop,
                Width = btnW,
                Height = btnH,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 130, 200),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnAddText.Click += BtnAddText_Click;
            this.Controls.Add(btnAddText);

            btnAddImage = new Button
            {
                Text = "Resim Ekle",
                Left = btnLeft,
                Top = btnAddText.Bottom + btnGap,
                Width = btnW,
                Height = btnH,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 130, 200),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnAddImage.Click += BtnAddImage_Click;
            this.Controls.Add(btnAddImage);

            btnBgColor = new Button
            {
                Text = "Arka Plan Rengi",
                Left = btnLeft,
                Top = btnAddImage.Bottom + btnGap,
                Width = btnW,
                Height = btnH,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 130, 200),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnBgColor.Click += BtnBgColor_Click;
            this.Controls.Add(btnBgColor);

            btnShowFullscreen = new Button
            {
                Text = "Tam Ekran Göster",
                Left = btnLeft,
                Top = btnBgColor.Bottom + 2 * btnGap, // btnTextColor yerine btnBgColor
                Width = btnW + 30,
                Height = btnH + 10,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 130, 200),
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnShowFullscreen.Click += BtnShowFullscreen_Click;
            this.Controls.Add(btnShowFullscreen);

            // Ana formda tam ekraný kapat butonu
            btnCloseFullscreen = new Button
            {
                Text = "Kapat",
                Left = btnShowFullscreen.Left,
                Top = btnShowFullscreen.Bottom + btnGap,
                Width = btnShowFullscreen.Width,
                Height = btnShowFullscreen.Height,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(200, 60, 60),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Visible = false
            };
            btnCloseFullscreen.Click += BtnCloseFullscreen_Click;
            this.Controls.Add(btnCloseFullscreen);

            btnDeleteItem = new Button
            {
                Text = "Sil",
                Left = btnAddText.Left,
                Top = btnCloseFullscreen.Bottom + 20,
                Width = btnAddText.Width,
                Height = btnAddText.Height,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(200, 60, 60),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnDeleteItem.Click += BtnDeleteItem_Click;
            this.Controls.Add(btnDeleteItem);

            // Katman sýralama butonlarý
            int layerBtnW = 100, layerBtnH = 36, layerBtnGap = 8;
            int layerLeft = btnLeft + btnW + 40;
            int layerTop = btnTop;
            Button btnBringToFront = new Button { Text = "En Üste", Left = layerLeft, Top = layerTop, Width = layerBtnW, Height = layerBtnH, ForeColor = Color.White, BackColor = Color.FromArgb(60, 130, 200), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            Button btnSendToBack = new Button { Text = "En Alta", Left = layerLeft, Top = layerTop + layerBtnH + layerBtnGap, Width = layerBtnW, Height = layerBtnH, ForeColor = Color.White, BackColor = Color.FromArgb(60, 130, 200), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            Button btnForward = new Button { Text = "Öne Al", Left = layerLeft + layerBtnW + 10, Top = layerTop, Width = layerBtnW, Height = layerBtnH, ForeColor = Color.White, BackColor = Color.FromArgb(60, 130, 200), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            Button btnBackward = new Button { Text = "Arkaya Al", Left = layerLeft + layerBtnW + 10, Top = layerTop + layerBtnH + layerBtnGap, Width = layerBtnW, Height = layerBtnH, ForeColor = Color.White, BackColor = Color.FromArgb(60, 130, 200), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btnBringToFront.Click += (s, e) => ChangeLayerOrder(true, true);
            btnSendToBack.Click += (s, e) => ChangeLayerOrder(false, true);
            btnForward.Click += (s, e) => ChangeLayerOrder(true, false);
            btnBackward.Click += (s, e) => ChangeLayerOrder(false, false);
            this.Controls.Add(btnBringToFront);
            this.Controls.Add(btnSendToBack);
            this.Controls.Add(btnForward);
            this.Controls.Add(btnBackward);

            previewPictureBox.Invalidate();
        }

        private void PreviewPictureBox_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(backgroundColor);
            e.Graphics.ScaleTransform(previewScale, previewScale);

            // Draw layers in order
            for (int i = 0; i < layers.Count; i++)
            {
                var layer = layers[i];
                if (!layer.Visible) continue;
                layer.Draw(e.Graphics);
                if (i == selectedLayerIndex)
                {
                    DrawSelection(e.Graphics, Rectangle.Round(layer.Rect));
                }
            }
        }

        private void DrawSelection(Graphics g, Rectangle rect)
        {
            // translucent overlay outside selection
            using (Brush overlay = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
            {
                // paint overlay by drawing four rectangles around selection
                g.FillRectangle(overlay, 0, 0, rect.Left, (int)(previewPictureBox.Height / previewScale));
                g.FillRectangle(overlay, rect.Right, 0, (int)(previewPictureBox.Width / previewScale) - rect.Right, (int)(previewPictureBox.Height / previewScale));
                g.FillRectangle(overlay, rect.Left, 0, rect.Width, rect.Top);
                g.FillRectangle(overlay, rect.Left, rect.Bottom, rect.Width, (int)(previewPictureBox.Height / previewScale) - rect.Bottom);
            }
            // selection rectangle
            using (Pen p = new Pen(Color.White, 1))
            {
                p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                g.DrawRectangle(p, rect);
            }
            // draw handles
            DrawResizeHandles(g, rect);
        }

        // Geliþmiþ handle çizimi (köþe ve kenar)
        private void DrawResizeHandles(Graphics g, Rectangle rect)
        {
            // 8 handle: 4 köþe, 4 kenar ortasý
            var points = new[] {
                new Point(rect.Left, rect.Top),
                new Point(rect.Right, rect.Top),
                new Point(rect.Right, rect.Bottom),
                new Point(rect.Left, rect.Bottom),
                new Point(rect.Left + rect.Width/2, rect.Top),
                new Point(rect.Right, rect.Top + rect.Height/2),
                new Point(rect.Left + rect.Width/2, rect.Bottom),
                new Point(rect.Left, rect.Top + rect.Height/2)
            };
            foreach (var pt in points)
            {
                g.FillRectangle(Brushes.White, pt.X - handleSize / 2, pt.Y - handleSize / 2, handleSize, handleSize);
                g.DrawRectangle(Pens.Black, pt.X - handleSize / 2, pt.Y - handleSize / 2, handleSize, handleSize);
            }
        }

        private HandleType GetHandleAtPoint(Rectangle rect, Point pt)
        {
            var handles = new[] {
                new { Type = HandleType.NW, Pt = new Point(rect.Left, rect.Top) },
                new { Type = HandleType.NE, Pt = new Point(rect.Right, rect.Top) },
                new { Type = HandleType.SE, Pt = new Point(rect.Right, rect.Bottom) },
                new { Type = HandleType.SW, Pt = new Point(rect.Left, rect.Bottom) },
                new { Type = HandleType.N, Pt = new Point(rect.Left + rect.Width/2, rect.Top) },
                new { Type = HandleType.E, Pt = new Point(rect.Right, rect.Top + rect.Height/2) },
                new { Type = HandleType.S, Pt = new Point(rect.Left + rect.Width/2, rect.Bottom) },
                new { Type = HandleType.W, Pt = new Point(rect.Left, rect.Top + rect.Height/2) }
            };
            foreach (var h in handles)
            {
                var handleRect = new Rectangle(h.Pt.X - handleSize / 2, h.Pt.Y - handleSize / 2, handleSize, handleSize);
                if (handleRect.Contains(pt))
                    return h.Type;
            }
            return HandleType.None;
        }

        private void PreviewPictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            Point real = new Point((int)(e.X / previewScale), (int)(e.Y / previewScale));
            // check handles first topmost
            for (int i = layers.Count - 1; i >= 0; i--)
            {
                var rect = Rectangle.Round(layers[i].Rect);
                var ht = GetHandleAtPoint(rect, real);
                if (ht != HandleType.None)
                {
                    selectedLayerIndex = i;
                    currentHandle = ht;
                    handleHitIndex = i;
                    // record original rect and anchor for stable resizing
                    originalRectF = layers[i].Rect;
                    isResizing = true;
                    resizeAnchor = GetOppositeAnchor(originalRectF, ht);
                    previewPictureBox.Invalidate();
                    return;
                }
                if (rect.Contains(real))
                {
                    selectedLayerIndex = i;
                    currentHandle = HandleType.Move;
                    handleHitIndex = i;
                    dragOffset = new PointF(real.X - rect.X, real.Y - rect.Y);
                    isResizing = false;
                    previewPictureBox.Invalidate();
                    return;
                }
            }
            // none selected
            selectedLayerIndex = -1;
            currentHandle = HandleType.None;
            handleHitIndex = -1;
            isResizing = false;
            previewPictureBox.Invalidate();
        }

        private void PreviewPictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            Point real = new Point((int)(e.X / previewScale), (int)(e.Y / previewScale));
            if (isResizing && handleHitIndex >= 0 && handleHitIndex < layers.Count)
            {
                var layer = layers[handleHitIndex];
                var startRect = Rectangle.Round(originalRectF);
                bool keepAspect = Control.ModifierKeys.HasFlag(Keys.Shift) && layer is ImageLayer; // Shift to lock aspect for images

                // Use helper to compute new rect based on original rect + anchor
                Rectangle newRect = ResizeLayerRect(layer, startRect, currentHandle, real, keepAspect, resizeAnchor);

                // apply
                layer.Rect = RectangleF.FromLTRB(newRect.Left, newRect.Top, newRect.Right, newRect.Bottom);
                layers[handleHitIndex] = layer;
                previewPictureBox.Invalidate();
            }
            else if (currentHandle == HandleType.Move && handleHitIndex >= 0 && handleHitIndex < layers.Count)
            {
                Point realP = real;
                var layer = layers[handleHitIndex];
                var rect = Rectangle.Round(layer.Rect);
                rect.X = (int)(realP.X - dragOffset.X);
                rect.Y = (int)(realP.Y - dragOffset.Y);
                layer.Rect = RectangleF.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
                layers[handleHitIndex] = layer;
                previewPictureBox.Invalidate();
            }
            else
            {
                // update cursor depending on hover over handles or items
                Cursor cur = Cursors.Default;
                for (int i = layers.Count - 1; i >= 0; i--)
                {
                    var rect = Rectangle.Round(layers[i].Rect);
                    var ht = GetHandleAtPoint(rect, real);
                    if (ht != HandleType.None)
                    {
                        cur = Cursors.SizeAll; // slightly generic; could map to corner cursors
                        break;
                    }
                    if (rect.Contains(real))
                    {
                        cur = Cursors.Hand;
                        break;
                    }
                }
                previewPictureBox.Cursor = cur;
            }
        }

        // Helper to compute opposite anchor point for a handle from the original rectangle
        private PointF GetOppositeAnchor(RectangleF rect, HandleType handle)
        {
            switch (handle)
            {
                case HandleType.NW: return new PointF(rect.Right, rect.Bottom);
                case HandleType.NE: return new PointF(rect.Left, rect.Bottom);
                case HandleType.SE: return new PointF(rect.Left, rect.Top);
                case HandleType.SW: return new PointF(rect.Right, rect.Top);
                case HandleType.N: return new PointF((rect.Left + rect.Right) / 2f, rect.Bottom);
                case HandleType.S: return new PointF((rect.Left + rect.Right) / 2f, rect.Top);
                case HandleType.W: return new PointF(rect.Right, (rect.Top + rect.Bottom) / 2f);
                case HandleType.E: return new PointF(rect.Left, (rect.Top + rect.Bottom) / 2f);
                default: return new PointF(rect.Left, rect.Top);
            }
        }

        // Helper to compute new rectangle based on handle and mouse position. Uses anchor to keep opposite side fixed.
        private Rectangle ResizeLayerRect(Layer layer, Rectangle originalRect, HandleType handle, Point real, bool keepAspect, PointF anchor)
        {
            int left = originalRect.Left, top = originalRect.Top, right = originalRect.Right, bottom = originalRect.Bottom;
            int minSize = (layer is TextLayer) ? 20 : 20;

            // Use anchor (fixed) and current mouse position to compute new rectangle sides
            switch (handle)
            {
                case HandleType.Move:
                    left = (int)(real.X - dragOffset.X);
                    top = (int)(real.Y - dragOffset.Y);
                    right = left + originalRect.Width;
                    bottom = top + originalRect.Height;
                    break;
                case HandleType.NW:
                    left = real.X; top = real.Y; right = (int)anchor.X; bottom = (int)anchor.Y; break;
                case HandleType.NE:
                    right = real.X; top = real.Y; left = (int)anchor.X; bottom = (int)anchor.Y; break;
                case HandleType.SE:
                    right = real.X; bottom = real.Y; left = (int)anchor.X; top = (int)anchor.Y; break;
                case HandleType.SW:
                    left = real.X; bottom = real.Y; right = (int)anchor.X; top = (int)anchor.Y; break;
                case HandleType.N:
                    top = real.Y; bottom = (int)anchor.Y; left = (int)(anchor.X - originalRect.Width/2); right = (int)(anchor.X + originalRect.Width/2); break;
                case HandleType.S:
                    bottom = real.Y; top = (int)anchor.Y; left = (int)(anchor.X - originalRect.Width/2); right = (int)(anchor.X + originalRect.Width/2); break;
                case HandleType.W:
                    left = real.X; right = (int)anchor.X; top = (int)(anchor.Y - originalRect.Height/2); bottom = (int)(anchor.Y + originalRect.Height/2); break;
                case HandleType.E:
                    right = real.X; left = (int)anchor.X; top = (int)(anchor.Y - originalRect.Height/2); bottom = (int)(anchor.Y + originalRect.Height/2); break;
            }

            // Enforce minimums
            if (right <= left) right = left + minSize;
            if (bottom <= top) bottom = top + minSize;

            int newW = right - left;
            int newH = bottom - top;

            if (keepAspect && layer is ImageLayer il)
            {
                float asp = il.OriginalAspect;
                if (asp <= 0) asp = newW / (float)Math.Max(1, newH);

                // For corner handles, compute width based on mouse X then set height to match aspect (keeping anchor fixed)
                if (handle == HandleType.NW || handle == HandleType.NE || handle == HandleType.SE || handle == HandleType.SW)
                {
                    // width determined by distance between anchor.X and mouse.X
                    int desiredW = Math.Abs((int)anchor.X - real.X);
                    int desiredH = Math.Max(minSize, (int)(desiredW / asp));

                    if (handle == HandleType.NW)
                    {
                        left = (int)anchor.X - desiredW;
                        top = (int)anchor.Y - desiredH;
                        right = (int)anchor.X;
                        bottom = (int)anchor.Y;
                    }
                    else if (handle == HandleType.NE)
                    {
                        left = (int)anchor.X;
                        right = (int)anchor.X + desiredW;
                        top = (int)anchor.Y - desiredH;
                        bottom = (int)anchor.Y;
                    }
                    else if (handle == HandleType.SE)
                    {
                        left = (int)anchor.X;
                        top = (int)anchor.Y;
                        right = (int)anchor.X + desiredW;
                        bottom = (int)anchor.Y + desiredH;
                    }
                    else // SW
                    {
                        right = (int)anchor.X;
                        left = (int)anchor.X - desiredW;
                        top = (int)anchor.Y;
                        bottom = (int)anchor.Y + desiredH;
                    }
                }
                else
                {
                    // edge handles: adjust primary dimension then compute other
                    if (handle == HandleType.N || handle == HandleType.S)
                    {
                        int desiredH = Math.Abs((int)anchor.Y - real.Y);
                        int desiredW = Math.Max(minSize, (int)(desiredH * asp));
                        left = (int)anchor.X - desiredW / 2;
                        right = left + desiredW;
                        if (handle == HandleType.N) { top = (int)anchor.Y - desiredH; bottom = (int)anchor.Y; }
                        else { top = (int)anchor.Y; bottom = (int)anchor.Y + desiredH; }
                    }
                    else // W or E
                    {
                        int desiredW = Math.Abs((int)anchor.X - real.X);
                        int desiredH = Math.Max(minSize, (int)(desiredW / asp));
                        top = (int)anchor.Y - desiredH / 2;
                        bottom = top + desiredH;
                        if (handle == HandleType.W) { left = (int)anchor.X - desiredW; right = (int)anchor.X; }
                        else { left = (int)anchor.X; right = (int)anchor.X + desiredW; }
                    }
                }
            }

            // Final clamps
            if (right - left < minSize) right = left + minSize;
            if (bottom - top < minSize) bottom = top + minSize;

            return Rectangle.FromLTRB(left, top, right, bottom);
        }

        private void PreviewPictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            currentHandle = HandleType.None;
            handleHitIndex = -1;
            isResizing = false;
        }

        private void BtnAddText_Click(object sender, EventArgs e)
        {
            using (var dlg = new TextEditDialog("", displayFont.Name, (int)displayFont.Size, Color.White)) // Varsayýlan beyaz
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    var font = new Font(dlg.FontName, dlg.FontSize, FontStyle.Bold);
                    SizeF sz = TextRenderer.MeasureText(dlg.Text, font);
                    var rect = new RectangleF(40 + layers.Count * 30, 40 + layers.Count * 30, sz.Width + 40, sz.Height + 20);
                    layers.Add(new TextLayer(dlg.Text, font, dlg.TextColor, rect));
                    selectedLayerIndex = layers.Count - 1;
                    previewPictureBox.Invalidate();
                }
            }
        }

        private void BtnAddImage_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Resim Dosyalarý|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Load image safely from file and downscale very large images to avoid OutOfMemory
                        const int MaxDimension = 4096; // maximum width or height to keep
                        Image img = LoadResizedImage(ofd.FileName, MaxDimension);

                        // Panel boyutuna göre orantýlý þekilde sýðdýr
                        int maxW = previewPictureBox.Width, maxH = previewPictureBox.Height;
                        float scale = Math.Min(maxW / (float)img.Width, maxH / (float)img.Height);
                        int w = (int)(img.Width * scale * 0.5f); // %50 ile baþlasýn
                        int h = (int)(img.Height * scale * 0.5f);
                        int x = (maxW / 2 - w / 2);
                        int y = (maxH / 2 - h / 2);
                        // Ölçek panelden büyükse kýrp
                        w = Math.Min(w, maxW);
                        h = Math.Min(h, maxH);
                        var rect = new RectangleF(x, y, w, h);
                        var il = new ImageLayer(img, rect);
                        // ensure original aspect recorded (already in constructor)
                        layers.Add(il);
                        selectedLayerIndex = layers.Count - 1;
                        previewPictureBox.Invalidate();
                    }
                    catch (OutOfMemoryException)
                    {
                        MessageBox.Show("Seçilen resim çok büyük veya bozuk. Lütfen daha küçük bir resim deneyin.", "Resim Yükleme Hatasý", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Resim yüklenirken hata oluþtu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Helper: load image from file stream and downscale if larger than maxDimension. Returns a Bitmap independent of the file stream.
        private Image LoadResizedImage(string path, int maxDimension)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var src = Image.FromStream(fs, useEmbeddedColorManagement: false, validateImageData: false))
                {
                    int w = src.Width;
                    int h = src.Height;
                    int maxSide = Math.Max(w, h);
                    // If not too large, return a clone bitmap to detach from stream
                    if (maxSide <= maxDimension)
                    {
                        return new Bitmap(src);
                    }

                    float scale = maxDimension / (float)maxSide;
                    int newW = Math.Max(1, (int)(w * scale));
                    int newH = Math.Max(1, (int)(h * scale));

                    var dest = new Bitmap(newW, newH);
                    using (var g = Graphics.FromImage(dest))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.DrawImage(src, 0, 0, newW, newH);
                    }
                    return dest;
                }
            }
        }

        private void BtnBgColor_Click(object sender, EventArgs e)
        {
            using (var cd = new ColorDialog())
            {
                cd.Color = backgroundColor;
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    backgroundColor = cd.Color;
                    previewPanel.BackColor = backgroundColor;
                    previewPictureBox.BackColor = backgroundColor;
                    previewPictureBox.Invalidate();
                }
            }
        }

        private void BtnTextColor_Click(object sender, EventArgs e)
        {
            using (var cd = new ColorDialog())
            {
                cd.Color = textColor;
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    textColor = cd.Color;
                    previewPictureBox.Invalidate();
                }
            }
        }

        private CustomTextShowFullscreenForm fullscreenForm = null;

        private void BtnShowFullscreen_Click(object sender, EventArgs e)
        {
            // Build ordered layer list to preserve editor order (bottom->top)
            var ordered = new List<CustomTextShowFullscreenForm.LayerItem>();
            foreach (var layer in layers)
            {
                if (layer is ImageLayer il)
                {
                    ordered.Add(new CustomTextShowFullscreenForm.LayerItem
                    {
                        IsImage = true,
                        Image = il.Image,
                        ImageRect = Rectangle.Round(il.Rect)
                    });
                }
                else if (layer is TextLayer tl)
                {
                    ordered.Add(new CustomTextShowFullscreenForm.LayerItem
                    {
                        IsText = true,
                        Text = tl.Text,
                        Font = tl.Font,
                        Color = tl.Color,
                        TextRect = tl.Rect
                    });
                }
            }

            if (fullscreenForm != null && !fullscreenForm.IsDisposed)
            {
                // update using ordered layers
                fullscreenForm.UpdateContent(backgroundColor, ordered);
                fullscreenForm.BringToFront();
                btnShowFullscreen.Text = "Ýçeriði Güncelle";
                btnCloseFullscreen.Visible = true;
            }
            else
            {
                // create fullscreen form with ordered layers
                fullscreenForm = new CustomTextShowFullscreenForm("", null, Color.White, backgroundColor, ordered, SelectedScreen);
                // ensure bounds set to selected screen
                try { fullscreenForm.Bounds = SelectedScreen.Bounds; } catch { }
                fullscreenForm.FormClosed += (s, args) => { fullscreenForm = null; btnShowFullscreen.Text = "Tam Ekran Göster"; btnCloseFullscreen.Visible = false; };
                fullscreenForm.Show();
                btnShowFullscreen.Text = "Ýçeriði Güncelle";
                btnCloseFullscreen.Visible = true;
            }
        }

        private void BtnCloseFullscreen_Click(object sender, EventArgs e)
        {
            if (fullscreenForm != null && !fullscreenForm.IsDisposed)
            {
                fullscreenForm.Close();
                fullscreenForm = null;
                btnShowFullscreen.Text = "Tam Ekran Göster";
                btnCloseFullscreen.Visible = false;
            }
        }

        private void BtnDeleteItem_Click(object sender, EventArgs e)
        {
            if (selectedLayerIndex != -1 && selectedLayerIndex < layers.Count)
            {
                layers.RemoveAt(selectedLayerIndex);
                selectedLayerIndex = -1;
                previewPictureBox.Invalidate();
            }
        }

        private void PreviewPictureBox_DoubleClick(object sender, EventArgs e)
        {
            MouseEventArgs me = e as MouseEventArgs;
            if (me == null) return;
            Point real = new Point((int)(me.X / previewScale), (int)(me.Y / previewScale));

            for (int i = layers.Count - 1; i >= 0; i--)
            {
                var rect = Rectangle.Round(layers[i].Rect);
                if (rect.Contains(real) && layers[i] is TextLayer)
                {
                    var t = layers[i] as TextLayer;
                    using (var dlg = new TextEditDialog(t.Text, t.Font.Name, (int)t.Font.Size, t.Color))
                    {
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            t.Text = dlg.Text;
                            t.Font = new Font(dlg.FontName, dlg.FontSize, FontStyle.Bold);
                            t.Color = dlg.TextColor;
                            layers[i] = t;
                            previewPictureBox.Invalidate();
                        }
                    }
                    break;
                }
            }
        }

        private bool dragging = false;

        private void ChangeLayerOrder(bool forward, bool toEdge)
        {
            if (selectedLayerIndex < 0 || selectedLayerIndex >= layers.Count) return;
            var item = layers[selectedLayerIndex];
            int idx = selectedLayerIndex;
            layers.RemoveAt(idx);
            int newIdx;
            if (forward)
                newIdx = toEdge ? layers.Count : Math.Min(layers.Count, idx + 1);
            else
                newIdx = toEdge ? 0 : Math.Max(0, idx - 1);
            layers.Insert(newIdx, item);
            selectedLayerIndex = newIdx;
            previewPictureBox.Invalidate();
        }

        private void CustomTextShowForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (fullscreenForm != null && !fullscreenForm.IsDisposed)
            {
                fullscreenForm.Close();
                fullscreenForm = null;
                btnShowFullscreen.Text = "Tam Ekran Göster";
                btnCloseFullscreen.Visible = false;
            }
        }
    }
}
