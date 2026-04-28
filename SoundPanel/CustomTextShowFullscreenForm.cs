using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SoundPanel
{
    public class CustomTextShowFullscreenForm : Form
    {
        public class ImageItem
        {
            public Image Image { get; set; }
            public Rectangle Rect { get; set; }
        }
        public class TextItem
        {
            public string Text { get; set; }
            public Font Font { get; set; }
            public Color Color { get; set; }
            public RectangleF Rect { get; set; }
        }

        // New unified layer item to preserve ordering
        public class LayerItem
        {
            public bool IsImage { get; set; }
            public Image Image { get; set; }
            public Rectangle ImageRect { get; set; }

            public bool IsText { get; set; }
            public string Text { get; set; }
            public Font Font { get; set; }
            public Color Color { get; set; }
            public RectangleF TextRect { get; set; }
        }

        private List<ImageItem> images;
        private List<TextItem> texts;
        private List<LayerItem> orderedLayers; // if set, draw by this order
        private Color bgColor;

        // Existing constructor (kept) -> fallback
        public CustomTextShowFullscreenForm(string _unused, Font _unused2, Color _unused3, Color bgColor, List<ImageItem> images, List<TextItem> texts, Screen screen)
        {
            InitializeBasic(bgColor);

            this.images = new List<ImageItem>();
            foreach (var img in images)
            {
                this.images.Add(new ImageItem { Image = img.Image, Rect = img.Rect });
            }
            this.texts = new List<TextItem>();
            foreach (var t in texts)
            {
                this.texts.Add(new TextItem { Text = t.Text, Font = t.Font, Color = t.Color, Rect = t.Rect });
            }
        }

        // New constructor accepting ordered layers
        public CustomTextShowFullscreenForm(string _unused, Font _unused2, Color _unused3, Color bgColor, List<LayerItem> orderedLayers, Screen screen)
        {
            InitializeBasic(bgColor);
            this.orderedLayers = new List<LayerItem>();
            if (orderedLayers != null)
            {
                // clone references
                foreach (var l in orderedLayers)
                {
                    this.orderedLayers.Add(new LayerItem
                    {
                        IsImage = l.IsImage,
                        Image = l.Image,
                        ImageRect = l.ImageRect,
                        IsText = l.IsText,
                        Text = l.Text,
                        Font = l.Font,
                        Color = l.Color,
                        TextRect = l.TextRect
                    });
                }
            }
        }

        private void InitializeBasic(Color bgColor)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            // Bounds will be set by caller after construction if needed
            this.TopMost = true;
            this.BackColor = bgColor;
            this.DoubleBuffered = true;
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) this.Close(); };
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Normal;
            this.bgColor = bgColor;

            this.Paint += (s, e) =>
            {
                // Use the instance field bgColor so UpdateContent can change it later
                e.Graphics.Clear(this.bgColor);

                if (this.orderedLayers != null && this.orderedLayers.Count > 0)
                {
                    // draw in order
                    foreach (var layer in this.orderedLayers)
                    {
                        if (layer.IsImage && layer.Image != null)
                        {
                            e.Graphics.DrawImage(layer.Image, layer.ImageRect);
                        }
                        else if (layer.IsText && !string.IsNullOrEmpty(layer.Text) && layer.Font != null)
                        {
                            using (Brush brush = new SolidBrush(layer.Color))
                            {
                                StringFormat sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };
                                e.Graphics.DrawString(layer.Text, layer.Font, brush, layer.TextRect, sf);
                            }
                        }
                    }
                }
                else
                {
                    // fallback: draw images then texts (legacy)
                    if (this.images != null)
                    {
                        foreach (var img in this.images)
                        {
                            e.Graphics.DrawImage(img.Image, img.Rect);
                        }
                    }
                    if (this.texts != null)
                    {
                        foreach (var t in this.texts)
                        {
                            using (Brush brush = new SolidBrush(t.Color))
                            {
                                StringFormat sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };
                                e.Graphics.DrawString(t.Text, t.Font, brush, t.Rect, sf);
                            }
                        }
                    }
                }
            };
        }

        // Existing UpdateContent (legacy)
        public void UpdateContent(Color bgColor, List<ImageItem> images, List<TextItem> texts)
        {
            this.bgColor = bgColor;
            this.images = new List<ImageItem>();
            foreach (var img in images)
                this.images.Add(new ImageItem { Image = img.Image, Rect = img.Rect });
            this.texts = new List<TextItem>();
            foreach (var t in texts)
                this.texts.Add(new TextItem { Text = t.Text, Font = t.Font, Color = t.Color, Rect = t.Rect });
            this.orderedLayers = null; // clear ordered layers if any
            this.BackColor = bgColor;
            this.Invalidate();
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CustomTextShowFullscreenForm));
            SuspendLayout();
            // 
            // CustomTextShowFullscreenForm
            // 
            ClientSize = new Size(284, 261);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "CustomTextShowFullscreenForm";
            ResumeLayout(false);

        }

        // New UpdateContent that accepts ordered layers
        public void UpdateContent(Color bgColor, List<LayerItem> orderedLayers)
        {
            this.bgColor = bgColor;
            this.orderedLayers = new List<LayerItem>();
            if (orderedLayers != null)
            {
                foreach (var l in orderedLayers)
                {
                    this.orderedLayers.Add(new LayerItem
                    {
                        IsImage = l.IsImage,
                        Image = l.Image,
                        ImageRect = l.ImageRect,
                        IsText = l.IsText,
                        Text = l.Text,
                        Font = l.Font,
                        Color = l.Color,
                        TextRect = l.TextRect
                    });
                }
            }
            this.images = null;
            this.texts = null;
            this.BackColor = bgColor;
            this.Invalidate();
        }
    }
}
