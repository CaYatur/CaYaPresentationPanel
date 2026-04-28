using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SoundPanel
{
    public enum Orientation
    {
        Horizontal,
        Vertical
    }

    [ToolboxItem(true)]
    public class ModernTrackBar : UserControl
    {
        // Değerler
        private int minimum = 0, maximum = 100, value = 50;
        private Orientation orientation = Orientation.Horizontal;

        // Renkler
        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color TrackBackColor { get; set; } = Color.FromArgb(120, 80, 200);
        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        public Color TrackProgressColor { get; set; } = Color.FromArgb(180, 80, 220);
        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        public Color ThumbColor { get; set; } = Color.White;
        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        public Color ThumbBorderColor { get; set; } = Color.FromArgb(180, 80, 220);
        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        public Color BackgroundColor { get; set; } = Color.Black;

        // Kontur (çerçeve)
        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        public Color BorderColor { get; set; } = Color.FromArgb(180, 80, 220);
        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        public int BorderThickness { get; set; } = 2; // 0: kapalı

        // Köşe yarıçapı
        [Category("Appearance"), DefaultValue(0)]
        public int CornerRadius { get; set; } = 0;

        // Oranlar
        [Category("Layout"), DefaultValue(0.25f)]
        public float TrackThicknessRatio { get; set; } = 0.25f;
        [Category("Layout"), DefaultValue(2f)]
        public float ThumbSizeRatio { get; set; } = 2f;

        // Ek: Thumb uçtaki boşluk (piksel cinsinden) - thumb uçlarda biraz mesafe bırakır
        [Category("Layout"), DefaultValue(4f)]
        public float ThumbEndSpacing { get; set; } = 4f;

        [Category("Appearance"), DefaultValue(Orientation.Horizontal)]
        public Orientation Orientation
        {
            get => orientation;
            set { orientation = value; Invalidate(); }
        }

        [Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int Minimum { get => minimum; set { minimum = value; Invalidate(); } }
        [Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int Maximum { get => maximum; set { maximum = value; Invalidate(); } }

        [Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int Value
        {
            get => value;
            set
            {
                if (this.value != value)
                {
                    this.value = Math.Max(minimum, Math.Min(maximum, value));
                    Invalidate();
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        // Eventler
        public event EventHandler ValueChanged;
        public event EventHandler ScrollStart; // Kullanıcı sürüklemeye başlarsa
        public event EventHandler ScrollEnd;   // Kullanıcı bırakınca

        private bool isDragging = false;

        public ModernTrackBar()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw, true);
            Size = new Size(320, 60);
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            int minDim = 10; // Küçük boyutlar için daha düşük min
            if (Width < minDim || Height < minDim) return;

            bool isHorizontal = (Orientation == Orientation.Horizontal);
            float mainLength = isHorizontal ? Width : Height;
            float mainThickness = isHorizontal ? Height : Width;

            // Dinamik min/max ile kalınlık ayarı
            int minTrackThickness = 2;
            int maxTrackThickness = Math.Max(6, (int)mainThickness - 2);
            int minThumbSize = 6;
            int maxThumbSize = Math.Max(10, (int)mainThickness - 2);

            // Use float arithmetic (no truncating casts) to avoid subtle shifts
            float trackThicknessF = Math.Max(minTrackThickness, Math.Min(maxTrackThickness, mainThickness * TrackThicknessRatio));
            float thumbSizeF = Math.Max(minThumbSize, Math.Min(maxThumbSize, trackThicknessF * ThumbSizeRatio));
            int borderThickness = Math.Min(BorderThickness, Math.Max(2, (int)(trackThicknessF / 4)));

            float barMarginF = Math.Max(thumbSizeF / 2f, 2f);
            float barLengthF = Math.Max(10f, mainLength - 2f * barMarginF);

            // Ensure ThumbEndSpacing is not larger than half of barLength
            float endSpacing = Math.Max(0f, Math.Min(ThumbEndSpacing, barLengthF / 2f - 1f));

            int radius = Math.Min(CornerRadius, Math.Min(Width, Height) / 2);

            // Dikey trackbar çok dar ise track/ thumb minimumda olsun
            if (!isHorizontal && Width < 15)
            {
                trackThicknessF = minTrackThickness;
                thumbSizeF = minThumbSize;
                barMarginF = Math.Max(thumbSizeF / 2f, 2f);
                barLengthF = Math.Max(10f, mainLength - 2f * barMarginF);
                endSpacing = Math.Max(0f, Math.Min(ThumbEndSpacing, barLengthF / 2f - 1f));
            }

            // --- ARKA PLAN ---
            RectangleF bgRectF = new RectangleF(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = GetRoundedRect(bgRectF, radius))
            using (SolidBrush bgBrush = new SolidBrush(BackgroundColor))
            {
                e.Graphics.FillPath(bgBrush, path);

                // --- KONTUR (ÇERÇEVE) ---
                if (borderThickness > 0)
                {
                    using (Pen borderPen = new Pen(BorderColor, borderThickness))
                    {
                        borderPen.Alignment = PenAlignment.Inset;
                        borderPen.LineJoin = LineJoin.Round;
                        borderPen.StartCap = LineCap.Round;
                        borderPen.EndCap = LineCap.Round;
                        e.Graphics.DrawPath(borderPen, path);
                    }
                }
            }

            // --- TRACK & PROGRESS ---
            RectangleF trackRectF, progressRectF;
            float percent = (float)(Value - Minimum) / Math.Max(1, (Maximum - Minimum));
            percent = Math.Max(0f, Math.Min(1f, percent));

            if (isHorizontal)
            {
                // Use precise centering without integer rounding
                float barX = barMarginF;
                float barY = (Height - trackThicknessF) / 2f;
                trackRectF = new RectangleF(barX, barY, barLengthF, trackThicknessF);

                using (SolidBrush trackBrush = new SolidBrush(TrackBackColor))
                    e.Graphics.FillRectangle(trackBrush, trackRectF);

                float progressWidthF = barLengthF * percent;
                if (progressWidthF > 0.5f)
                {
                    progressRectF = new RectangleF(barX, barY, progressWidthF, trackThicknessF);
                    using (SolidBrush progressBrush = new SolidBrush(TrackProgressColor))
                        e.Graphics.FillRectangle(progressBrush, progressRectF);
                }

                // --- THUMB (use end spacing so thumb center doesn't touch exact ends)
                float barStartCenter = barX + endSpacing;
                float barEndCenter = barX + barLengthF - endSpacing;
                float effectiveLength = Math.Max(1f, barEndCenter - barStartCenter);

                float thumbCenterX = barStartCenter + percent * effectiveLength;
                float thumbCenterY = barY + trackThicknessF / 2f;
                float thumbX = thumbCenterX - thumbSizeF / 2f;
                float thumbY = thumbCenterY - thumbSizeF / 2f;

                // Clamp center
                thumbCenterX = Math.Max(barStartCenter, Math.Min(barEndCenter, thumbCenterX));
                thumbX = thumbCenterX - thumbSizeF / 2f;

                using (SolidBrush thumbBrush = new SolidBrush(ThumbColor))
                using (Pen thumbPen = new Pen(ThumbBorderColor, Math.Max(1f, thumbSizeF / 10f)))
                {
                    e.Graphics.FillEllipse(thumbBrush, thumbX, thumbY, thumbSizeF, thumbSizeF);
                    e.Graphics.DrawEllipse(thumbPen, thumbX, thumbY, thumbSizeF, thumbSizeF);
                }
            }
            else
            {
                // Vertical - Hassas ortalama için düzeltme
                float barY = barMarginF;
                float barHeightF = barLengthF;

                // Dikey trackbar için hassas ortalama - 0.5f çıkararak pixel perfect hizalama
                float barX = (Width - trackThicknessF) / 2f - 0.5f;

                trackRectF = new RectangleF(barX, barY, trackThicknessF, barHeightF);

                using (SolidBrush trackBrush = new SolidBrush(TrackBackColor))
                    e.Graphics.FillRectangle(trackBrush, trackRectF);

                float progressHeightF = barHeightF * percent;
                if (progressHeightF > 0.5f)
                {
                    progressRectF = new RectangleF(barX, barY + barHeightF - progressHeightF, trackThicknessF, progressHeightF);
                    using (SolidBrush progressBrush = new SolidBrush(TrackProgressColor))
                        e.Graphics.FillRectangle(progressBrush, progressRectF);
                }

                float barStartCenter = barY + endSpacing;
                float barEndCenter = barY + barHeightF - endSpacing;
                float effectiveLength = Math.Max(1f, barEndCenter - barStartCenter);

                // Thumb için de aynı ortalama düzeltmesi
                float thumbCenterX = barX + trackThicknessF / 2f;
                float thumbCenterY = barStartCenter + (1f - percent) * effectiveLength;
                float thumbX = thumbCenterX - thumbSizeF / 2f;
                float thumbY = thumbCenterY - thumbSizeF / 2f;

                thumbCenterY = Math.Max(barStartCenter, Math.Min(barEndCenter, thumbCenterY));
                thumbY = thumbCenterY - thumbSizeF / 2f;

                using (SolidBrush thumbBrush = new SolidBrush(ThumbColor))
                using (Pen thumbPen = new Pen(ThumbBorderColor, Math.Max(1f, thumbSizeF / 10f)))
                {
                    e.Graphics.FillEllipse(thumbBrush, thumbX, thumbY, thumbSizeF, thumbSizeF);
                    e.Graphics.DrawEllipse(thumbPen, thumbX, thumbY, thumbSizeF, thumbSizeF);
                }
            }
        }

        // Mouse events
        protected override void OnMouseDown(MouseEventArgs e)
        {
            isDragging = true;
            this.Capture = true;
            ScrollStart?.Invoke(this, EventArgs.Empty); // Sürükleme başladı
            UpdateValueFromMouse(e.X, e.Y);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (isDragging) UpdateValueFromMouse(e.X, e.Y);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            isDragging = false;
            this.Capture = false;
            ScrollEnd?.Invoke(this, EventArgs.Empty); // Sürükleme bitti
        }

        private void UpdateValueFromMouse(int mouseX, int mouseY)
        {
            bool isHorizontal = (Orientation == Orientation.Horizontal);
            float mainLength = isHorizontal ? Width : Height;
            float mainThickness = isHorizontal ? Height : Width;

            int minTrackThickness = 2;
            int maxTrackThickness = Math.Max(6, (int)mainThickness - 2);
            int minThumbSize = 6;
            int maxThumbSize = Math.Max(10, (int)mainThickness - 2);

            float trackThicknessF = Math.Max(minTrackThickness, Math.Min(maxTrackThickness, mainThickness * TrackThicknessRatio));
            float thumbSizeF = Math.Max(minThumbSize, Math.Min(maxThumbSize, trackThicknessF * ThumbSizeRatio));
            float barMarginF = Math.Max(thumbSizeF / 2f, 2f);
            float barLengthF = Math.Max(10f, mainLength - 2f * barMarginF);

            float endSpacing = Math.Max(0f, Math.Min(ThumbEndSpacing, barLengthF / 2f - 1f));

            if (!isHorizontal && Width < 15)
            {
                trackThicknessF = minTrackThickness;
                thumbSizeF = minThumbSize;
                barMarginF = Math.Max(thumbSizeF / 2f, 2f);
                barLengthF = Math.Max(10f, mainLength - 2f * barMarginF);
                endSpacing = Math.Max(0f, Math.Min(ThumbEndSpacing, barLengthF / 2f - 1f));
            }

            if (isHorizontal)
            {
                float barX = barMarginF;
                float barStartCenter = barX + endSpacing;
                float barEndCenter = barX + barLengthF - endSpacing;
                float effectiveLength = Math.Max(1f, barEndCenter - barStartCenter);

                // center from mouse
                float center = mouseX;
                center = Math.Max(barStartCenter, Math.Min(barEndCenter, center));
                float percent = (center - barStartCenter) / effectiveLength;
                percent = Math.Max(0f, Math.Min(1f, percent));

                int val = (int)Math.Round(percent * (Maximum - Minimum)) + Minimum;
                Value = Math.Max(Minimum, Math.Min(Maximum, val));
            }
            else
            {
                float barY = Math.Max(0f, barMarginF);
                float barStartCenter = barY + endSpacing;
                float barEndCenter = barY + barLengthF - endSpacing;
                float effectiveLength = Math.Max(1f, barEndCenter - barStartCenter);

                float center = mouseY;
                center = Math.Max(barStartCenter, Math.Min(barEndCenter, center));
                float percent = 1.0f - (center - barStartCenter) / effectiveLength;
                percent = Math.Max(0f, Math.Min(1f, percent));

                int val = (int)Math.Round(percent * (Maximum - Minimum)) + Minimum;
                Value = Math.Max(Minimum, Math.Min(Maximum, val));
            }
        }

        private GraphicsPath GetRoundedRect(RectangleF bounds, int radius)
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

        // Overload for integer rectangle used elsewhere
        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            return GetRoundedRect(new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height), radius);
        }
    }
}