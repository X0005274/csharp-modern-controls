using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Modern.Lab.WinForms.Controls.Hosting
{
    /// <summary>
    /// Common base class for all WinForms wrapper controls.
    ///
    /// A WinForms form cannot host a WPF control directly; it must go through an
    /// ElementHost bridge. This class derives from ElementHost and creates the
    /// inner WPF control automatically, so concrete wrappers only re-expose the
    /// inner control's properties to the designer property grid.
    ///
    /// Design-time policy (docs/design-notes.md sections 2, 4, 5):
    /// - Never host the WPF control on the design surface. The same guard is
    ///   applied in both the constructor and OnHandleCreated so the behavior is
    ///   deterministic.
    /// - Failures while constructing the WPF control at design time (stale
    ///   assembly, pack URI resolution, etc.) are swallowed: one broken control
    ///   must never kill the host form's designer.
    /// - Instead, OnPaint draws a RenderTargetBitmap snapshot as a preview and
    ///   falls back to a placeholder (type name + border) when rendering fails.
    /// </summary>
    /// <typeparam name="TWpf">Type of the wrapped WPF control (e.g. ModernButtonControl)</typeparam>
    [ToolboxItem(false)]                 // the base itself is not shown in the toolbox
    [DesignerCategory("Code")]           // avoid opening an empty component designer
    // The default ElementHost designer (ElementHostDesigner) re-creates and
    // serializes the hosted Child on the design surface, which conflicts with the
    // Wpf instance created in the constructor and corrupts the form. Using the
    // plain ControlDesigner treats the wrapper as an opaque control: the designer
    // never touches Child, so dragging/re-serialization stays safe.
    [Designer("System.Windows.Forms.Design.ControlDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public abstract class WpfElementHostBase<TWpf> : ElementHost
        where TWpf : System.Windows.FrameworkElement, new()
    {
        // LicenseManager.UsageMode is only reliable while the designer invokes the
        // constructor, so it is read once and stored. (this.DesignMode is always
        // false inside the constructor.)
        private readonly bool isDesignTime;

        // Cache for the design-time snapshot preview. Re-rendered only when the
        // size or a wrapper property changes.
        private Bitmap previewCache;

        /// <summary>
        /// The actual WPF control hosted inside. Wrapper subclasses read and write
        /// its properties. It can be null only when design-time construction
        /// failed, so wrapper property accessors must be null-safe.
        /// </summary>
        protected TWpf Wpf { get; private set; }

        /// <summary>
        /// Creates the WPF control and, at runtime only, hosts it as the child.
        /// </summary>
        protected WpfElementHostBase()
        {
            this.isDesignTime = LicenseManager.UsageMode == LicenseUsageMode.Designtime;

            if (this.isDesignTime)
            {
                // The WPF constructor plus XAML parsing runs inside the designer
                // process too. Swallow any failure here so the host form's
                // designer survives; OnPaint falls back to the placeholder.
                try
                {
                    this.Wpf = new TWpf();
                }
                catch (Exception)
                {
                    this.Wpf = null;
                }
            }
            else
            {
                // A runtime construction failure must propagate normally.
                this.Wpf = new TWpf();
                this.Child = this.Wpf;
            }
        }

        /// <summary>
        /// Completes hosting when the runtime handle is created. Controls on the
        /// design surface own real Win32 handles as well, so the guard is required
        /// here too — guarding only the constructor makes designer visibility
        /// non-deterministic (attached or not depending on handle timing).
        /// </summary>
        protected override void OnHandleCreated(EventArgs e)
        {
            if (!this.isDesignTime && !this.DesignMode && this.Child == null && this.Wpf != null)
            {
                this.Child = this.Wpf;
            }

            base.OnHandleCreated(e);
        }

        /// <summary>
        /// At design time, draws the WPF snapshot (or the placeholder) as a
        /// preview. At runtime the hosted Child paints itself, so this does nothing.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (this.isDesignTime || this.DesignMode)
            {
                this.PaintDesignTimePreview(e.Graphics);
            }
        }

        /// <summary>
        /// Drops the snapshot cache on resize so it is re-rendered at the new size.
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.InvalidateDesignTimePreview();
        }

        /// <summary>
        /// Invalidates the design-time preview cache. Wrappers must call this from
        /// property setters so property changes show up in the preview. Does
        /// nothing at runtime.
        /// </summary>
        protected void InvalidateDesignTimePreview()
        {
            if (this.isDesignTime || this.DesignMode)
            {
                this.DisposePreviewCache();
                this.Invalidate();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.DisposePreviewCache();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// ElementHost's own Child property never needs to be touched from the
        /// designer, so it is hidden from the property grid and serialization.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new System.Windows.UIElement Child
        {
            get { return base.Child; }
            set { base.Child = value; }
        }

        private void PaintDesignTimePreview(Graphics graphics)
        {
            if (this.Width <= 0 || this.Height <= 0)
            {
                return;
            }

            if (this.previewCache == null ||
                this.previewCache.Width != this.Width ||
                this.previewCache.Height != this.Height)
            {
                this.DisposePreviewCache();
                this.previewCache = this.RenderPreviewBitmap();
            }

            if (this.previewCache != null)
            {
                graphics.DrawImage(this.previewCache, 0, 0);
            }
            else
            {
                this.PaintPlaceholder(graphics);
            }
        }

        // Measures/arranges the WPF control off-screen and renders it through
        // RenderTargetBitmap into a GDI+ bitmap. To the designer this is just a
        // picture, so there are no serialization or mouse-capture issues.
        // Returns null on failure (placeholder fallback).
        private Bitmap RenderPreviewBitmap()
        {
            if (this.Wpf == null)
            {
                return null;
            }

            try
            {
                System.Windows.Size size = new System.Windows.Size(this.Width, this.Height);
                this.Wpf.Measure(size);
                this.Wpf.Arrange(new System.Windows.Rect(size));
                this.Wpf.UpdateLayout();

                RenderTargetBitmap target = new RenderTargetBitmap(
                    this.Width, this.Height, 96d, 96d, PixelFormats.Pbgra32);
                target.Render(this.Wpf);

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(target));

                using (MemoryStream stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    stream.Position = 0;

                    // Bitmap(stream) stays bound to the stream's lifetime, so hand
                    // back a detached copy instead.
                    using (Bitmap streamBound = new Bitmap(stream))
                    {
                        return new Bitmap(streamBound);
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Placeholder when the snapshot cannot be rendered: light border + type name.
        private void PaintPlaceholder(Graphics graphics)
        {
            Rectangle bounds = new Rectangle(0, 0, this.Width - 1, this.Height - 1);

            using (System.Drawing.Pen borderPen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(209, 213, 219)))
            {
                graphics.DrawRectangle(borderPen, bounds);
            }

            using (SolidBrush textBrush = new SolidBrush(System.Drawing.Color.FromArgb(107, 114, 128)))
            {
                graphics.DrawString(this.GetType().Name, this.Font, textBrush, 4f, 4f);
            }
        }

        private void DisposePreviewCache()
        {
            if (this.previewCache != null)
            {
                this.previewCache.Dispose();
                this.previewCache = null;
            }
        }
    }
}
