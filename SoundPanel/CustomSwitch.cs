using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace SoundPanel
{
    [DefaultEvent("CheckedChanged")]
    public class CustomSwitch : Control
    {
        private int cornerRadius = 14;
        private bool showBorder = true;
        private Color borderColor = Color.FromArgb(78, 87, 114);
        private int borderThickness = 2;
        private Color backgroundColor = Color.FromArgb(37, 43, 63);
        private Color textColor = Color.White;

        // Switch özellik alanları
        private bool isChecked = false;
        private bool defaultChecked = false;
        private Color onColor = Color.FromArgb(76, 175, 80);
        private Color offColor = Color.FromArgb(244, 67, 54);
        private Color onTextColor = Color.White;
        private Color offTextColor = Color.White;
        private Image onImage = null;
        private Image offImage = null;

        // Thumb renkleri için yeni özellikler
        private Color onThumbColor = Color.FromArgb(70, 130, 200);
        private Color offThumbColor = Color.FromArgb(220, 80, 80);
        private Color onThumbBorderColor = Color.FromArgb(50, 100, 160);
        private Color offThumbBorderColor = Color.FromArgb(180, 60, 60);

        // Animasyon için - System.Timers.Timer kullanarak UI mesaj döngüsüne bağlı kalmayı önle
        private System.Timers.Timer animationTimer;
        private float animationProgress = 0f;
        private bool isAnimating = false;
        private const int animationDuration = 200; // ms
        private const int animationSteps = 20;

        // Mouse durumu
        private bool isHovering = false;
        private bool isPressed = false;
        private bool leftMouseDown = false;

        [Category("Switch")]
        [DefaultValue(false)]
        public bool Checked
        {
            get => isChecked;
            set
            {
                if (isChecked != value)
                {
                    isChecked = value;
                    StartAnimation();
                    OnCheckedChanged(EventArgs.Empty);
                }
            }
        }

        [Category("Switch")]
        [DefaultValue(false)]
        public bool DefaultChecked
        {
            get => defaultChecked;
            set { defaultChecked = value; }
        }

        [Category("Switch")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color OnColor
        {
            get => onColor;
            set { onColor = value; Invalidate(); }
        }

        [Category("Switch")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color OffColor
        {
            get => offColor;
            set { offColor = value; Invalidate(); }
        }

        [Category("Switch")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color OnTextColor
        {
            get => onTextColor;
            set { onTextColor = value; Invalidate(); }
        }

        [Category("Switch")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color OffTextColor
        {
            get => offTextColor;
            set { offTextColor = value; Invalidate(); }
        }

        [Category("Switch")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public Image OnImage
        {
            get => onImage;
            set { onImage = value; Invalidate(); }
        }

        [Category("Switch")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public Image OffImage
        {
            get => offImage;
            set { offImage = value; Invalidate(); }
        }

        // Yeni thumb renk özellikleri
        [Category("Switch")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color OnThumbColor
        {
            get => onThumbColor;
            set { onThumbColor = value; Invalidate(); }
        }

        [Category("Switch")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color OffThumbColor
        {
            get => offThumbColor;
            set { offThumbColor = value; Invalidate(); }
        }

        [Category("Switch")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color OnThumbBorderColor
        {
            get => onThumbBorderColor;
            set { onThumbBorderColor = value; Invalidate(); }
        }

        [Category("Switch")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color OffThumbBorderColor
        {
            get => offThumbBorderColor;
            set { offThumbBorderColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] // EKLENDİ
        public int CornerRadius
        {
            get => cornerRadius;
            set { cornerRadius = Math.Max(0, value); Invalidate(); }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool ShowBorder
        {
            get => showBorder;
            set { showBorder = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color BorderColor
        {
            get => borderColor;
            set { borderColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int BorderThickness
        {
            get => borderThickness;
            set { borderThickness = Math.Max(1, value); Invalidate(); }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color SwitchBackColor
        {
            get => backgroundColor;
            set { backgroundColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color TextColor
        {
            get => textColor;
            set { textColor = value; Invalidate(); }
        }

        // Hover ve pressed renkleri
        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color HoverColor { get; set; } = Color.FromArgb(27, 31, 46);
        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color PressedColor { get; set; } = Color.FromArgb(22, 25, 37);
        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color DisabledBackColor { get; set; } = Color.FromArgb(24, 26, 34);
        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color DisabledTextColor { get; set; } = Color.FromArgb(90, 95, 110);
        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color DisabledBorderColor { get; set; } = Color.FromArgb(54, 60, 80);

        public event EventHandler CheckedChanged;

        private string onText = "ON";
        private string offText = "OFF";
        private Font onOffTextFont = null;
        private float onOffTextFontSize = 0f;

        [Category("Switch")]
        [Description("Açık durumunda gösterilecek metin.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] // EKLENDİ
        public string OnText
        {
            get => onText;
            set { onText = value ?? ""; Invalidate(); }
        }

        [Category("Switch")]
        [Description("Kapalı durumunda gösterilecek metin.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] // EKLENDİ
        public string OffText
        {
            get => offText;
            set { offText = value ?? ""; Invalidate(); }
        }

        [Category("Switch")]
        [Description("ON/OFF metni için özel font.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] // EKLENDİ
        public Font OnOffTextFont
        {
            get => onOffTextFont;
            set { onOffTextFont = value; Invalidate(); }
        }

        [Category("Switch")]
        [Description("ON/OFF metni için özel font boyutu. 0 ise mevcut font kullanılır.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)] // EKLENDİ
        public float OnOffTextFontSize
        {
            get => onOffTextFontSize;
            set { onOffTextFontSize = value; Invalidate(); }
        }

        public CustomSwitch()
        {
            DoubleBuffered = true;
            Size = new Size(100, 30); // Daha uzun ve ince
            Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Selectable, true);
            SetStyle(ControlStyles.UserMouse, true);
            SetStyle(ControlStyles.StandardClick, false);
            SetStyle(ControlStyles.StandardDoubleClick, false);
            BackColor = Color.Transparent;
            TabStop = true;

            // Varsayılan corner radius
            cornerRadius = 14;

            // Animasyon timer'ını ayarla (System.Timers.Timer) - SynchronizingObject set edilerek UI thread'e marshal edilecek
            animationTimer = new System.Timers.Timer(animationDuration / animationSteps);
            animationTimer.AutoReset = true;
            animationTimer.Elapsed += AnimationTimer_Elapsed;

            // Varsayılan durumu ayarla
            if (defaultChecked)
            {
                isChecked = true;
                animationProgress = 1f;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            try
            {
                // Marshal elapsed events to UI thread of this control
                if (animationTimer != null)
                    animationTimer.SynchronizingObject = this;
            }
            catch { }
        }

        private void StartAnimation()
        {
            if (!isAnimating)
            {
                isAnimating = true;
                animationTimer?.Start();
            }
        }

        private void AnimationTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Ensure updates happen on UI thread - SynchronizingObject is set in OnHandleCreated when possible
            if (IsDisposed) return;

            if (InvokeRequired)
            {
                try { BeginInvoke(new Action(AnimationStep)); }
                catch { }
            }
            else
            {
                AnimationStep();
            }
        }

        private void AnimationStep()
        {
            float step = 1f / animationSteps;

            if (isChecked)
            {
                animationProgress += step;
                if (animationProgress >= 1f)
                {
                    animationProgress = 1f;
                    animationTimer?.Stop();
                    isAnimating = false;
                }
            }
            else
            {
                animationProgress -= step;
                if (animationProgress <= 0f)
                {
                    animationProgress = 0f;
                    animationTimer?.Stop();
                    isAnimating = false;
                }
            }

            Invalidate();
        }

        protected virtual void OnCheckedChanged(EventArgs e)
        {
            CheckedChanged?.Invoke(this, e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            float halfBorder = showBorder && borderThickness > 0 ? borderThickness / 2f : 0f;
            float radius = Math.Max(0, Math.Min(cornerRadius, (Math.Min(Width, Height) - borderThickness) / 2f));

            RectangleF rect = new RectangleF(
                halfBorder + 0.5f,
                halfBorder + 0.5f,
                Width - borderThickness - 1f,
                Height - borderThickness - 1f
            );

            using (GraphicsPath path = GetRoundedRectF(rect, radius))
            {
                // Arka plan rengini animasyonla hesapla
                Color backColor;
                if (!Enabled)
                {
                    backColor = DisabledBackColor;
                }
                else
                {
                    Color currentColor = InterpolateColor(offColor, onColor, animationProgress);

                    if (isPressed)
                        backColor = InterpolateColor(currentColor, PressedColor, 0.3f);
                    else if (isHovering)
                        backColor = InterpolateColor(currentColor, HoverColor, 0.2f);
                    else
                        backColor = currentColor;
                }

                using (SolidBrush bgBrush = new SolidBrush(backColor))
                    e.Graphics.FillPath(bgBrush, path);

                // Border
                if (showBorder && borderThickness > 0)
                {
                    Color bColor = Enabled ? borderColor : DisabledBorderColor;
                    using (Pen pen = new Pen(bColor, borderThickness))
                    {
                        pen.Alignment = PenAlignment.Center;
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            }

            // Switch thumb (kaydırıcı kutu) çiz
            DrawSwitchThumb(e.Graphics, rect, radius);

            // Metin ve ikon çiz
            DrawTextAndIcon(e.Graphics, rect);
        }

        private void DrawSwitchThumb(Graphics g, RectangleF rect, float mainRadius)
        {
            // Tam kutu şeklinde ve alanı maksimum doldurucak switch tasarımı
            float thumbWidth = rect.Width * 0.5f; // Yarım genişlik
            float thumbHeight = rect.Height - 4f; // Üst alt 2px margin
            float thumbMargin = 2f; // Minimum margin

            // Thumb pozisyonunu animasyonla hesapla - sağa sola hareket
            float thumbTravel = rect.Width - thumbWidth - (thumbMargin * 2);
            float thumbX = rect.Left + thumbMargin + (thumbTravel * animationProgress);
            float thumbY = rect.Top + thumbMargin;

            RectangleF thumbRect = new RectangleF(thumbX, thumbY, thumbWidth, thumbHeight);

            // Ana switch'in corner radius'ını baz alarak thumb radius hesapla
            // Ana radius'un %80'i kadar olsun ama thumb boyutunu aşmasın
            float maxThumbRadius = Math.Min(thumbWidth, thumbHeight) / 2f;
            float thumbRadius = Math.Min(mainRadius * 0.8f, maxThumbRadius);

            using (GraphicsPath thumbPath = GetRoundedRectF(thumbRect, thumbRadius))
            {
                // Thumb gölgesi
                RectangleF shadowRect = new RectangleF(thumbX + 1, thumbY + 1, thumbWidth, thumbHeight);
                using (GraphicsPath shadowPath = GetRoundedRectF(shadowRect, thumbRadius))
                using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
                    g.FillPath(shadowBrush, shadowPath);

                // Thumb ana renkli kısmı - özelleştirilebilir renklerle
                Color thumbColor = InterpolateColor(offThumbColor, onThumbColor, animationProgress);

                using (SolidBrush thumbBrush = new SolidBrush(thumbColor))
                    g.FillPath(thumbBrush, thumbPath);

                // Thumb kenarı - özelleştirilebilir renklerle
                Color borderThumbColor = InterpolateColor(offThumbBorderColor, onThumbBorderColor, animationProgress);
                using (Pen thumbPen = new Pen(borderThumbColor, 1.5f))
                    g.DrawPath(thumbPen, thumbPath);
            }
        }

        private void DrawTextAndIcon(Graphics g, RectangleF rect)
        {
            // Kaydırıcı düğmenin üzerinde metin/ikon göstermek için
            float thumbWidth = rect.Width * 0.5f;
            float thumbHeight = rect.Height - 4f;
            float thumbMargin = 2f;

            // Thumb pozisyonunu hesapla
            float thumbTravel = rect.Width - thumbWidth - (thumbMargin * 2);
            float thumbX = rect.Left + thumbMargin + (thumbTravel * animationProgress);
            float thumbY = rect.Top + thumbMargin;

            // Kaydırıcı düğmenin alanını hesapla
            RectangleF thumbArea = new RectangleF(thumbX, thumbY, thumbWidth, thumbHeight);

            // Şu anki duruma göre hangi görsel/metin gösterileceğini belirle
            if (isChecked)
            {
                // Açık durumunda ON görseli/metni kaydırıcı üzerinde
                if (onImage != null)
                {
                    DrawImageInArea(g, onImage, thumbArea);
                }
                else if (!string.IsNullOrEmpty(OnText))
                {
                    Color txtColor = Enabled ? onTextColor : DisabledTextColor;
                    DrawTextInArea(g, OnText, thumbArea, txtColor);
                }
            }
            else
            {
                // Kapalı durumunda OFF görseli/metni kaydırıcı üzerinde
                if (offImage != null)
                {
                    DrawImageInArea(g, offImage, thumbArea);
                }
                else if (!string.IsNullOrEmpty(OffText))
                {
                    Color txtColor = Enabled ? offTextColor : DisabledTextColor;
                    DrawTextInArea(g, OffText, thumbArea, txtColor);
                }
            }
        }

        private void DrawImageInArea(Graphics g, Image image, RectangleF area)
        {
            if (image == null || area.Width <= 0 || area.Height <= 0) return;

            float imageSize = Math.Min(area.Width, area.Height) * 0.6f;
            float imageX = area.Left + (area.Width - imageSize) / 2f;
            float imageY = area.Top + (area.Height - imageSize) / 2f;

            RectangleF imgRect = new RectangleF(imageX, imageY, imageSize, imageSize);
            g.DrawImage(image, imgRect);
        }

        private void DrawTextInArea(Graphics g, string text, RectangleF area, Color color)
        {
            if (area.Width <= 0 || area.Height <= 0) return;

            Font fontToUse = Font;
            if (onOffTextFont != null)
            {
                if (onOffTextFontSize > 0)
                    fontToUse = new Font(onOffTextFont.FontFamily, onOffTextFontSize, onOffTextFont.Style);
                else
                    fontToUse = onOffTextFont;
            }
            else if (onOffTextFontSize > 0)
            {
                fontToUse = new Font(Font.FontFamily, onOffTextFontSize, Font.Style);
            }

            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                sf.Trimming = StringTrimming.EllipsisCharacter;

                using (SolidBrush brush = new SolidBrush(color))
                {
                    g.DrawString(text, fontToUse, brush, area, sf);
                }
            }
            if (fontToUse != null && fontToUse != Font && (onOffTextFont == null || fontToUse != onOffTextFont))
                fontToUse.Dispose();
        }

        private Color InterpolateColor(Color color1, Color color2, float progress)
        {
            progress = Math.Max(0, Math.Min(1, progress));

            int r = (int)(color1.R + (color2.R - color1.R) * progress);
            int g = (int)(color1.G + (color2.G - color1.G) * progress);
            int b = (int)(color1.B + (color2.B - color1.B) * progress);
            int a = (int)(color1.A + (color2.A - color1.A) * progress);

            return Color.FromArgb(a, r, g, b);
        }

        private GraphicsPath GetRoundedRectF(RectangleF bounds, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(bounds);
            }
            else
            {
                float diameter = radius * 2f;
                SizeF sizeF = new SizeF(diameter, diameter);
                RectangleF arc = new RectangleF(bounds.Location, sizeF);

                // Sol üst
                path.AddArc(arc, 180, 90);
                // Sağ üst
                arc.X = bounds.Right - diameter;
                path.AddArc(arc, 270, 90);
                // Sağ alt
                arc.Y = bounds.Bottom - diameter;
                path.AddArc(arc, 0, 90);
                // Sol alt
                arc.X = bounds.Left;
                path.AddArc(arc, 90, 90);

                path.CloseFigure();
            }
            return path;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (Enabled)
            {
                isHovering = true;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (Enabled)
            {
                isHovering = false;
                if (!leftMouseDown)
                {
                    isPressed = false;
                }
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!Enabled) return;

            if (e.Button == MouseButtons.Left)
            {
                leftMouseDown = true;
                isPressed = true;
                Focus();
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (!Enabled) return;

            if (e.Button == MouseButtons.Left && leftMouseDown)
            {
                leftMouseDown = false;
                isPressed = false;

                Rectangle clientRect = new Rectangle(0, 0, Width, Height);
                if (clientRect.Contains(e.Location))
                {
                    Checked = !Checked; // Toggle durumu
                }

                Invalidate();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (Enabled && leftMouseDown)
            {
                Rectangle clientRect = new Rectangle(0, 0, Width, Height);
                bool mouseInside = clientRect.Contains(e.Location);

                if (isPressed != mouseInside)
                {
                    isPressed = mouseInside;
                    Invalidate();
                }
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            if (!Enabled)
            {
                isHovering = false;
                isPressed = false;
                leftMouseDown = false;
            }
            Invalidate();
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Enter || keyData == Keys.Space)
                return true;
            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (Enabled && (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space))
            {
                isPressed = true;
                Invalidate();

                Checked = !Checked; // Toggle durumu

                // Kısa bir süre sonra pressed durumunu kaldır
                System.Threading.Timer timer = null;
                timer = new System.Threading.Timer((state) =>
                {
                    try
                    {
                        if (!IsDisposed)
                        {
                            BeginInvoke(new Action(() =>
                            {
                                isPressed = false;
                                Invalidate();
                            }));
                        }
                    }
                    catch { }
                    finally
                    {
                        timer?.Dispose();
                    }
                }, null, 100, System.Threading.Timeout.Infinite);
            }
            base.OnKeyDown(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            isPressed = false;
            leftMouseDown = false;
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try { animationTimer?.Stop(); } catch { }
                animationTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}