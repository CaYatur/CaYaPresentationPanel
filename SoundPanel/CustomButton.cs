using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace SoundPanel
{
    [DefaultEvent("Click")]
    public class CustomButton : Control
    {
        private int cornerRadius = 14;
        private bool showBorder = true;
        private Color borderColor = ColorTranslator.FromHtml("#4e5772");
        private int borderThickness = 3;
        private Color backgroundColor = ColorTranslator.FromHtml("#c2c6d3");
        private Color textColor = Color.White;
        private Image buttonImage = null;
        private ContentAlignment imageAlign = ContentAlignment.MiddleCenter;
        private Padding imagePadding = new Padding(0);
        private ContentAlignment textAlign = ContentAlignment.MiddleCenter;
        private int imageScaleReduction = 0;
        private bool textBelowImage = false;
        private bool allowTextWrapping = false;
        private int imageTextGap = 4;

        // Yeni özellikler
        private int imageTextScaleReduction = 8; // Resim+yazı birlikte varken resim için ek küçültme
        private int imageTextPadding = 8; // Resim+yazı birlikte varken kenarlardan minimum uzaklık

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int CornerRadius
        {
            get => cornerRadius;
            set { cornerRadius = Math.Max(0, value); Invalidate(); }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(true)]
        public bool ShowBorder
        {
            get => showBorder;
            set { showBorder = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(typeof(Color), "#4e5772")]
        public Color BorderColor
        {
            get => borderColor;
            set { borderColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(3)]
        public int BorderThickness
        {
            get => borderThickness;
            set { borderThickness = Math.Max(1, value); Invalidate(); }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(typeof(Color), "#c2c6d3")]
        public Color ButtonBackColor
        {
            get => backgroundColor;
            set { backgroundColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(typeof(Color), "White")]
        public Color TextColor
        {
            get => textColor;
            set { textColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(null)]
        [Localizable(true)]
        [Editor("System.Drawing.Design.ImageEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
        public Image ButtonImage
        {
            get => buttonImage;
            set { buttonImage = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(ContentAlignment.MiddleCenter)]
        public ContentAlignment ImageAlign
        {
            get => imageAlign;
            set { imageAlign = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Localizable(true)]
        public Padding ImagePadding
        {
            get => imagePadding;
            set { imagePadding = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(ContentAlignment.MiddleCenter)]
        [Localizable(true)]
        public ContentAlignment TextAlign
        {
            get => textAlign;
            set { textAlign = value; Invalidate(); }
        }

        [Category("Appearance")]
        [Description("Resim küçültme miktarı. Pozitif değer resimi küçültür; negatif değer resimi büyütür")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(0)]
        public int ImageScaleReduction
        {
            get => imageScaleReduction;
            set { imageScaleReduction = value; Invalidate(); }
        }

        [Category("Appearance")]
        [Description("Resim ve yazı birlikte varken resim için ek küçültme miktarı")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(8)]
        public int ImageTextScaleReduction
        {
            get => imageTextScaleReduction;
            set { imageTextScaleReduction = Math.Max(0, value); Invalidate(); }
        }

        [Category("Appearance")]
        [Description("Resim ve yazı birlikte varken kenarlardan minimum uzaklık")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(8)]
        public int ImageTextPadding
        {
            get => imageTextPadding;
            set { imageTextPadding = Math.Max(0, value); Invalidate(); }
        }

        [Category("Appearance")]
        [Description("Resim ve yazı birlikte varken yazıyı resmin altında ortalayarak gösterir")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(false)]
        public bool TextBelowImage
        {
            get => textBelowImage;
            set { textBelowImage = value; Invalidate(); }
        }

        [Category("Appearance")]
        [Description("Yazı sığmayınca alt satıra geçmesini sağlar")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(false)]
        public bool AllowTextWrapping
        {
            get => allowTextWrapping;
            set { allowTextWrapping = value; Invalidate(); }
        }

        [Category("Appearance")]
        [Description("Resim ve yazı arasındaki boşluk miktarı")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(4)]
        public int ImageTextGap
        {
            get => imageTextGap;
            set { imageTextGap = Math.Max(0, value); Invalidate(); }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(typeof(Color), "#b7bfcf")]
        public Color HoverColor { get; set; } = ColorTranslator.FromHtml("#b7bfcf");

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(typeof(Color), "#a2a9b9")]
        public Color PressedColor { get; set; } = ColorTranslator.FromHtml("#a2a9b9");

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(typeof(Color), "#e5e7ec")]
        public Color DisabledBackColor { get; set; } = ColorTranslator.FromHtml("#e5e7ec");

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(typeof(Color), "#b0b3bb")]
        public Color DisabledTextColor { get; set; } = ColorTranslator.FromHtml("#b0b3bb");

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(typeof(Color), "#d1d5dd")]
        public Color DisabledBorderColor { get; set; } = ColorTranslator.FromHtml("#d1d5dd");

        private bool isHovering = false;
        private bool isPressed = false;
        private bool leftMouseDown = false;

        public CustomButton()
        {
            DoubleBuffered = true;
            Size = new Size(42, 42);
            Font = new Font("Segoe UI", 14f, FontStyle.Bold);
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Selectable, true);
            SetStyle(ControlStyles.UserMouse, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.StandardClick, false);
            SetStyle(ControlStyles.StandardDoubleClick, false);
            BackColor = Color.Transparent;
            TabStop = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // EN YÜKSEK KALİTE RENDER AYARLARI
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // Daha hassas float hesaplamalar
            float borderOffset = showBorder && borderThickness > 0 ? borderThickness * 0.5f : 0f;
            float adjustedRadius = Math.Min(cornerRadius, Math.Min(Width, Height) * 0.5f - borderOffset);

            RectangleF drawRect = new RectangleF(
                borderOffset,
                borderOffset,
                Width - borderThickness,
                Height - borderThickness
            );

            using (GraphicsPath buttonPath = CreateSmoothRoundedRect(drawRect, adjustedRadius))
            {
                // Arka plan rengi
                Color currentBackColor = GetCurrentBackgroundColor();
                using (SolidBrush backgroundBrush = new SolidBrush(currentBackColor))
                {
                    e.Graphics.FillPath(backgroundBrush, buttonPath);
                }

                // Çerçeve çizimi - daha pürüzsüz
                if (showBorder && borderThickness > 0)
                {
                    Color currentBorderColor = Enabled ? borderColor : DisabledBorderColor;
                    using (Pen borderPen = new Pen(currentBorderColor, borderThickness))
                    {
                        borderPen.Alignment = PenAlignment.Center;
                        borderPen.LineJoin = LineJoin.Round;
                        borderPen.StartCap = LineCap.Round;
                        borderPen.EndCap = LineCap.Round;

                        e.Graphics.DrawPath(borderPen, buttonPath);
                    }
                }
            }

            // İçerik çizimi
            RectangleF contentArea = new RectangleF(
                borderThickness + 1,
                borderThickness + 1,
                Width - (borderThickness * 2) - 2,
                Height - (borderThickness * 2) - 2
            );

            DrawContent(e.Graphics, contentArea);
        }

        private Color GetCurrentBackgroundColor()
        {
            if (!Enabled) return DisabledBackColor;
            if (isPressed) return PressedColor;
            if (isHovering) return HoverColor;
            return backgroundColor;
        }

        private GraphicsPath CreateSmoothRoundedRect(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();

            if (radius <= 0 || rect.Width <= 0 || rect.Height <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            // Yarıçapı sınırla
            float maxRadius = Math.Min(rect.Width, rect.Height) * 0.5f;
            radius = Math.Min(radius, maxRadius);

            float diameter = radius * 2f;
            RectangleF arcRect = new RectangleF(0, 0, diameter, diameter);

            // Sol üst köşe
            arcRect.X = rect.X;
            arcRect.Y = rect.Y;
            path.AddArc(arcRect, 180, 90);

            // Sağ üst köşe
            arcRect.X = rect.Right - diameter;
            path.AddArc(arcRect, 270, 90);

            // Sağ alt köşe
            arcRect.Y = rect.Bottom - diameter;
            path.AddArc(arcRect, 0, 90);

            // Sol alt köşe
            arcRect.X = rect.Left;
            path.AddArc(arcRect, 90, 90);

            path.CloseFigure();
            return path;
        }

        private void DrawContent(Graphics g, RectangleF contentRect)
        {
            bool hasImage = buttonImage != null;
            bool hasText = !string.IsNullOrEmpty(Text);

            if (hasImage && hasText)
            {
                DrawImageAndText(g, contentRect);
            }
            else if (hasImage)
            {
                DrawImageOnly(g, contentRect);
            }
            else if (hasText)
            {
                DrawTextOnly(g, contentRect);
            }
        }

        private void DrawImageAndText(Graphics g, RectangleF contentRect)
        {
            // Resim+yazı birlikte varken kenarlardan padding ekle
            RectangleF paddedRect = new RectangleF(
                contentRect.X + imageTextPadding,
                contentRect.Y + imageTextPadding,
                contentRect.Width - (imageTextPadding * 2),
                contentRect.Height - (imageTextPadding * 2)
            );

            if (textBelowImage)
            {
                DrawImageAboveText(g, paddedRect);
            }
            else
            {
                DrawImageBesideText(g, paddedRect);
            }
        }

        private void DrawImageAboveText(Graphics g, RectangleF contentRect)
        {
            // Yazı boyutunu hesapla
            SizeF textSize = MeasureTextSize(g, contentRect.Width);

            // Resim için kalan alan - ek küçültme uygula
            float availableImageHeight = contentRect.Height - textSize.Height - imageTextGap;
            float maxImageSize = Math.Min(contentRect.Width, availableImageHeight);
            maxImageSize = Math.Max(1f, maxImageSize - imageScaleReduction - imageTextScaleReduction);

            // Resim boyutlarını hesapla (oran koruyarak)
            SizeF scaledImageSize = CalculateScaledImageSize(maxImageSize);

            // Toplam içerik yüksekliği
            float totalHeight = scaledImageSize.Height + imageTextGap + textSize.Height;
            float startY = contentRect.Y + (contentRect.Height - totalHeight) * 0.5f;

            // Resmi çiz
            RectangleF imageRect = new RectangleF(
                contentRect.X + (contentRect.Width - scaledImageSize.Width) * 0.5f,
                startY,
                scaledImageSize.Width,
                scaledImageSize.Height
            );

            DrawImageSmooth(g, imageRect);

            // Yazıyı çiz
            RectangleF textRect = new RectangleF(
                contentRect.X,
                imageRect.Bottom + imageTextGap,
                contentRect.Width,
                textSize.Height
            );

            DrawTextSmooth(g, textRect, StringAlignment.Center, StringAlignment.Center);
        }

        private void DrawImageBesideText(Graphics g, RectangleF contentRect)
        {
            float gap = imageTextGap;
            SizeF textSize = MeasureTextSize(g, contentRect.Width * 0.5f); // Yazı için daha az alan bırak

            float availableWidth = contentRect.Width - gap - textSize.Width;
            float imageSize = Math.Min(availableWidth, contentRect.Height);
            // Resim+yazı birlikte varken ek küçültme uygula
            imageSize = Math.Max(1f, imageSize - imageScaleReduction - imageTextScaleReduction);

            SizeF scaledImageSize = CalculateScaledImageSize(imageSize);

            // Toplam genişlik
            float totalWidth = scaledImageSize.Width + gap + textSize.Width;
            float startX = contentRect.X + (contentRect.Width - totalWidth) * 0.5f;

            // Resim - dikey olarak ortalanmış
            RectangleF imageRect = new RectangleF(
                startX,
                contentRect.Y + (contentRect.Height - scaledImageSize.Height) * 0.5f,
                scaledImageSize.Width,
                scaledImageSize.Height
            );

            DrawImageSmooth(g, imageRect);

            // Yazı - resim ile aynı dikey seviyede ortalanmış
            RectangleF textRect = new RectangleF(
                imageRect.Right + gap,
                contentRect.Y + (contentRect.Height - textSize.Height) * 0.5f,
                textSize.Width,
                textSize.Height
            );

            DrawTextSmooth(g, textRect, StringAlignment.Near, StringAlignment.Center);
        }

        private void DrawImageOnly(Graphics g, RectangleF contentRect)
        {
            float maxSize = Math.Min(contentRect.Width - imagePadding.Horizontal,
                                   contentRect.Height - imagePadding.Vertical);
            maxSize = Math.Max(1f, maxSize - imageScaleReduction);

            SizeF scaledSize = CalculateScaledImageSize(maxSize);

            RectangleF imageRect = new RectangleF(
                contentRect.X + (contentRect.Width - scaledSize.Width) * 0.5f,
                contentRect.Y + (contentRect.Height - scaledSize.Height) * 0.5f,
                scaledSize.Width,
                scaledSize.Height
            );

            DrawImageSmooth(g, imageRect);
        }

        private void DrawTextOnly(Graphics g, RectangleF contentRect)
        {
            DrawTextSmooth(g, contentRect, GetHorizontalAlignment(), GetVerticalAlignment());
        }

        private SizeF CalculateScaledImageSize(float maxSize)
        {
            if (buttonImage == null) return SizeF.Empty;

            float aspectRatio = (float)buttonImage.Width / buttonImage.Height;
            float width = maxSize;
            float height = maxSize;

            if (aspectRatio > 1f)
            {
                height = maxSize / aspectRatio;
            }
            else if (aspectRatio < 1f)
            {
                width = maxSize * aspectRatio;
            }

            return new SizeF(width, height);
        }

        private void DrawImageSmooth(Graphics g, RectangleF rect)
        {
            if (buttonImage == null) return;

            // En yüksek kalite ayarları resim için
            var oldMode = g.InterpolationMode;
            var oldSmoothing = g.SmoothingMode;
            var oldPixelOffset = g.PixelOffsetMode;

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            g.DrawImage(buttonImage, rect);

            // Ayarları geri yükle
            g.InterpolationMode = oldMode;
            g.SmoothingMode = oldSmoothing;
            g.PixelOffsetMode = oldPixelOffset;
        }

        private void DrawTextSmooth(Graphics g, RectangleF rect, StringAlignment horizontal, StringAlignment vertical)
        {
            if (string.IsNullOrEmpty(Text)) return;

            Color textColorToUse = Enabled ? textColor : DisabledTextColor;

            using (SolidBrush textBrush = new SolidBrush(textColorToUse))
            using (StringFormat format = new StringFormat())
            {
                format.Alignment = horizontal;
                format.LineAlignment = vertical;
                format.Trimming = StringTrimming.EllipsisCharacter;

                if (allowTextWrapping && textBelowImage)
                {
                    format.FormatFlags = 0; // Wrapping'e izin ver
                }
                else
                {
                    format.FormatFlags = StringFormatFlags.NoWrap;
                }

                // Anti-aliased text rendering
                var oldHint = g.TextRenderingHint;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                g.DrawString(Text, Font, textBrush, rect, format);

                g.TextRenderingHint = oldHint;
            }
        }

        private SizeF MeasureTextSize(Graphics g, float maxWidth)
        {
            if (string.IsNullOrEmpty(Text)) return SizeF.Empty;

            if (allowTextWrapping && textBelowImage)
            {
                return g.MeasureString(Text, Font, (int)maxWidth);
            }
            else
            {
                return g.MeasureString(Text, Font);
            }
        }

        private StringAlignment GetHorizontalAlignment()
        {
            switch (textAlign)
            {
                case ContentAlignment.TopLeft:
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.BottomLeft:
                    return StringAlignment.Near;
                case ContentAlignment.TopCenter:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.BottomCenter:
                    return StringAlignment.Center;
                default:
                    return StringAlignment.Far;
            }
        }

        private StringAlignment GetVerticalAlignment()
        {
            switch (textAlign)
            {
                case ContentAlignment.TopLeft:
                case ContentAlignment.TopCenter:
                case ContentAlignment.TopRight:
                    return StringAlignment.Near;
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.MiddleRight:
                    return StringAlignment.Center;
                default:
                    return StringAlignment.Far;
            }
        }

        // Mouse ve diğer event handler'lar aynı kalır
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
                    try
                    {
                        OnClick(EventArgs.Empty);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"CustomButton Click Error: {ex.Message}");
                    }
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

                try
                {
                    OnClick(EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"CustomButton Keyboard Click Error: {ex.Message}");
                }

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
    }
}