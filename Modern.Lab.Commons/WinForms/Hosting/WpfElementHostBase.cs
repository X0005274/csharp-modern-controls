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
    /// 모든 래퍼 컨트롤의 공통 부모 클래스입니다.
    ///
    /// WinForms 폼은 WPF 컨트롤을 직접 올릴 수 없고, 반드시 ElementHost라는
    /// "다리" 컨트롤 안에 넣어야 합니다. 이 클래스는 그 ElementHost를 상속해서,
    /// 생성될 때 안에 들어갈 WPF 컨트롤을 자동으로 만들어 끼워 넣습니다.
    ///
    /// 디자인 타임 정책 (docs/design-notes.md 2·4·5장):
    /// - WPF 컨트롤은 디자이너에 절대 호스팅하지 않는다. 생성자와 OnHandleCreated
    ///   양쪽 모두 같은 가드를 적용해 동작을 결정적으로 만든다.
    /// - 디자인 타임 WPF 생성 실패(스테일 어셈블리, pack URI 해석 실패 등)는
    ///   try/catch로 흡수한다. 컨트롤 하나가 깨져도 호스트 폼 디자이너는 살아야 한다.
    /// - 대신 OnPaint에서 RenderTargetBitmap 스냅샷을 그려 미리보기를 제공하고,
    ///   렌더링이 불가능하면 자리표시자(타입명 + 테두리)로 폴백한다.
    /// </summary>
    /// <typeparam name="TWpf">감쌀 WPF 컨트롤의 형식(예: ModernButtonControl)</typeparam>
    [ToolboxItem(false)]                 // 베이스 자체는 도구 상자에 표시하지 않음
    [DesignerCategory("Code")]           // 빈 컴포넌트 디자이너가 열리지 않도록
    // ElementHost 기본 디자이너(ElementHostDesigner)는 폼 디자이너에서 Child(호스팅된
    // WPF 컨트롤)를 새로 만들어 직렬화해 버려, 생성자가 끼운 Wpf 와 어긋나 디자인이
    // 깨집니다. 평범한 ControlDesigner 로 바꿔 래퍼를 "불투명 컨트롤"로 취급하게 하면
    // Child 를 건드리지 않으므로 드래그/재직렬화 후에도 정상 동작합니다.
    [Designer("System.Windows.Forms.Design.ControlDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public abstract class WpfElementHostBase<TWpf> : ElementHost
        where TWpf : System.Windows.FrameworkElement, new()
    {
        // LicenseManager.UsageMode 는 디자이너가 생성자를 부를 때만 신뢰할 수 있으므로
        // 생성자에서 한 번 읽어 보관합니다. (this.DesignMode 는 생성자 시점엔 항상 false)
        private readonly bool isDesignTime;

        // 디자인 타임 스냅샷 미리보기 캐시. 크기/속성이 바뀔 때만 다시 렌더링합니다.
        private Bitmap previewCache;

        /// <summary>
        /// 안쪽에 들어 있는 실제 WPF 컨트롤입니다. 자식 래퍼들이 이 객체의 속성을
        /// 읽고 씁니다. 디자인 타임에 생성이 실패한 경우에만 null일 수 있으므로,
        /// 래퍼의 속성 접근은 null-safe 해야 합니다.
        /// </summary>
        protected TWpf Wpf { get; private set; }

        /// <summary>
        /// 생성자: WPF 컨트롤을 만들고, 런타임이면 ElementHost의 자식으로 끼웁니다.
        /// </summary>
        protected WpfElementHostBase()
        {
            this.isDesignTime = LicenseManager.UsageMode == LicenseUsageMode.Designtime;

            if (this.isDesignTime)
            {
                // 디자이너 프로세스 안에서도 WPF 생성자 + XAML 파싱이 실행됩니다.
                // 여기서 예외가 나도 호스트 폼의 디자이너를 죽이지 않도록 흡수하고,
                // OnPaint 의 자리표시자 폴백에 맡깁니다.
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
                // 런타임 생성 실패는 정상적인 예외 흐름으로 그대로 전파되어야 합니다.
                this.Wpf = new TWpf();
                this.Child = this.Wpf;
            }
        }

        /// <summary>
        /// 런타임에 핸들이 만들어질 때 WPF 호스팅을 보완합니다. 디자인 표면의
        /// 컨트롤도 실제 Win32 핸들을 가지므로 여기에도 반드시 가드를 둡니다 —
        /// 생성자에서만 가드하면 디자이너에서 붙었다 안 붙었다 하는
        /// 비결정 동작이 됩니다.
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
        /// 디자인 타임에는 WPF 스냅샷(또는 자리표시자)을 그려 미리보기를 제공합니다.
        /// 런타임에는 Child(실제 WPF)가 그려지므로 아무것도 하지 않습니다.
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
        /// 크기가 바뀌면 스냅샷을 다시 렌더링해야 하므로 캐시를 버립니다.
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.InvalidateDesignTimePreview();
        }

        /// <summary>
        /// 디자인 타임 미리보기 캐시를 무효화합니다. 래퍼는 속성 setter에서
        /// 이 메서드를 호출해 속성 변경이 미리보기에 반영되게 해야 합니다.
        /// 런타임에는 아무 일도 하지 않습니다.
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
        /// ElementHost가 원래 가지고 있는 Child(자식) 속성은 디자이너에서
        /// 직접 만질 필요가 없으므로 속성창과 코드 저장에서 숨깁니다.
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

        // WPF 컨트롤을 오프스크린에서 Measure/Arrange 한 뒤 RenderTargetBitmap 으로
        // 렌더링해 GDI+ 비트맵으로 변환합니다. 디자이너 입장에서는 그냥 그림이므로
        // 직렬화·마우스 캡처 문제가 없습니다. 실패하면 null(→ 자리표시자 폴백).
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

                    // Bitmap(stream)은 스트림 수명에 묶이므로 복사본을 만들어 돌려줍니다.
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

        // 스냅샷 렌더링이 불가능할 때의 자리표시자: 옅은 테두리 + 타입명.
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
