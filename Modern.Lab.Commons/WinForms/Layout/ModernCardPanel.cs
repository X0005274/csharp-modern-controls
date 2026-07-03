using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Modern.Lab.WinForms.Controls.Layout
{
    /// <summary>
    /// Card-look container panel for grouping screen areas (search bar, footer
    /// statistics, ...) with a consistent surface.
    ///
    /// This is a plain WinForms Panel drawn with GDI+ — NOT an ElementHost — so
    /// it may host any WinForms children including the modern leaf controls
    /// (contract rule 5: area layout stays WinForms; ElementHost cannot host
    /// WinForms children, a native panel can).
    ///
    /// Visuals follow the card tokens: white surface, subtle border, radius 8.
    /// </summary>
    [ToolboxItem(true)]
    public class ModernCardPanel : Panel
    {
        // Card tokens mirrored from Themes/Tokens.xaml (GDI+ cannot read XAML
        // resources): Brush.Surface / Brush.BorderSubtle / Radius.Lg.
        private static readonly Color SurfaceColor = Color.FromArgb(255, 255, 255);
        private static readonly Color BorderColor = Color.FromArgb(229, 231, 235);
        private const int CardCornerRadius = 8;

        /// <summary>Creates the panel with the card surface and default padding.</summary>
        public ModernCardPanel()
        {
            // Children inherit BackColor, so the panel itself is the surface
            // color; only the corner areas are repainted with the parent color.
            this.BackColor = SurfaceColor;
            this.Padding = new Padding(12, 8, 12, 8);
            this.SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.ResizeRedraw,
                true);
        }

        /// <summary>Paints the rounded card surface and border.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (this.Width <= 1 || this.Height <= 1)
            {
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath cardPath = CreateRoundedPath(
                new Rectangle(0, 0, this.Width - 1, this.Height - 1), CardCornerRadius))
            {
                // Repaint the area outside the rounded rect with the parent
                // background so the corners do not show as white squares.
                Color outsideColor = this.Parent != null ? this.Parent.BackColor : SystemColors.Control;

                using (Region outside = new Region(new Rectangle(0, 0, this.Width, this.Height)))
                {
                    outside.Exclude(cardPath);

                    using (SolidBrush outsideBrush = new SolidBrush(outsideColor))
                    {
                        e.Graphics.FillRegion(outsideBrush, outside);
                    }
                }

                using (Pen borderPen = new Pen(BorderColor))
                {
                    e.Graphics.DrawPath(borderPen, cardPath);
                }
            }
        }

        private static GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();

            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180f, 90f);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270f, 90f);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0f, 90f);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90f, 90f);
            path.CloseFigure();

            return path;
        }
    }
}
