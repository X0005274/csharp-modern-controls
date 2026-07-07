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
    /// 모든 WinForms 래퍼 컨트롤의 공통 베이스 클래스.
    ///
    /// WinForms 폼은 WPF 컨트롤을 직접 호스팅할 수 없고 반드시 ElementHost
    /// 브리지를 거쳐야 한다. 이 클래스는 ElementHost에서 파생되어 내부 WPF
    /// 컨트롤을 자동으로 생성하므로, 구체 래퍼는 내부 컨트롤의 속성을 디자이너
    /// 속성 그리드에 다시 노출하기만 하면 된다.
    ///
    /// 디자인 타임 정책 (docs/design-notes.md 2, 4, 5절):
    /// - 디자인 표면에서는 WPF 컨트롤을 절대 호스팅하지 않는다. 동작이
    ///   결정적이도록 생성자와 OnHandleCreated 양쪽에 동일한 가드를 적용한다.
    /// - 디자인 타임에 WPF 컨트롤 생성 중 실패(오래된 어셈블리, pack URI 해석
    ///   실패 등)는 삼킨다: 깨진 컨트롤 하나가 호스트 폼의 디자이너를 죽여서는
    ///   안 된다.
    /// - 대신 OnPaint가 RenderTargetBitmap 스냅숏을 미리보기로 그리고, 렌더링
    ///   실패 시 placeholder(타입 이름 + 테두리)로 폴백한다.
    /// </summary>
    /// <typeparam name="TWpf">감싸는 WPF 컨트롤의 타입 (예: ModernButtonControl)</typeparam>
    [ToolboxItem(false)]                 // 베이스 자체는 도구 상자에 표시하지 않는다
    [DesignerCategory("Code")]           // 빈 컴포넌트 디자이너가 열리는 것을 방지한다
    // 기본 ElementHost 디자이너(ElementHostDesigner)는 디자인 표면에서 호스팅된
    // Child를 다시 생성하고 직렬화하는데, 이는 생성자에서 만든 Wpf 인스턴스와
    // 충돌하여 폼을 망가뜨린다. 일반 ControlDesigner를 쓰면 래퍼가 불투명한
    // 컨트롤로 취급된다: 디자이너가 Child를 건드리지 않으므로 드래그/재직렬화가
    // 안전하게 유지된다.
    [Designer("System.Windows.Forms.Design.ControlDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public abstract class WpfElementHostBase<TWpf> : ElementHost
        where TWpf : System.Windows.FrameworkElement, new()
    {
        // LicenseManager.UsageMode는 디자이너가 생성자를 호출하는 동안에만
        // 신뢰할 수 있으므로 한 번 읽어 저장해 둔다. (this.DesignMode는 생성자
        // 안에서 항상 false다.)
        private readonly bool isDesignTime;

        // 디자인 타임 스냅숏 미리보기 캐시. 크기나 래퍼 속성이 바뀔 때만
        // 다시 렌더링된다.
        private Bitmap previewCache;

        // "크기 1px 흔들기"(리사이즈 흉내) 재진입 가드. 흔들기가 유발하는
        // OnResize가 InvalidateDesignTimePreview로 되돌아와 무한 재귀하는 것을 막는다.
        private bool isNudgingBounds;

        /// <summary>
        /// 내부에 호스팅되는 실제 WPF 컨트롤. 래퍼 서브클래스가 이 컨트롤의
        /// 속성을 읽고 쓴다. 디자인 타임 생성이 실패한 경우에만 null일 수
        /// 있으므로, 래퍼의 속성 접근자는 null 안전해야 한다.
        /// </summary>
        protected TWpf Wpf { get; private set; }

        /// <summary>
        /// WPF 컨트롤을 생성하고, 런타임에만 자식으로 호스팅한다.
        /// </summary>
        protected WpfElementHostBase()
        {
            this.isDesignTime = LicenseManager.UsageMode == LicenseUsageMode.Designtime;

            if (this.isDesignTime)
            {
                // WPF 생성자와 XAML 파싱은 디자이너 프로세스 안에서도 실행된다.
                // 호스트 폼의 디자이너가 살아남도록 여기서 실패를 모두 삼킨다;
                // OnPaint가 placeholder로 폴백한다.
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
                // 런타임 생성 실패는 정상적으로 전파되어야 한다.
                this.Wpf = new TWpf();
                this.Child = this.Wpf;
            }
        }

        /// <summary>
        /// 런타임 핸들이 생성될 때 호스팅을 완료한다. 디자인 표면의 컨트롤도
        /// 실제 Win32 핸들을 가지므로 여기에도 가드가 필요하다 — 생성자만
        /// 가드하면 디자이너 표시 여부가 비결정적이 된다(핸들 타이밍에 따라
        /// 붙거나 안 붙거나 함).
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
        /// 디자인 타임에는 WPF 스냅숏(또는 placeholder)을 미리보기로 그린다.
        /// 런타임에는 호스팅된 Child가 스스로 그리므로 아무것도 하지 않는다.
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
        /// 크기가 바뀔 때 스냅숏 캐시를 버려 새 크기로 다시 렌더링되게 한다.
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.InvalidateDesignTimePreview();
        }

        /// <summary>
        /// 디자인 타임 미리보기 캐시를 무효화하고 새 스냅숏을 즉시 렌더한다.
        /// 속성 변경이 미리보기에 반영되도록 래퍼는 속성 setter에서 이 메서드를
        /// 호출해야 한다. 런타임에는 아무것도 하지 않는다.
        ///
        /// 값 변경이 미리보기에 즉시 반영되는 실제 조건은 두 가지다:
        /// (1) 스냅숏 자체가 바뀐 값으로 렌더돼야 한다 — 이는 RenderPreviewBitmap이
        ///     렌더 전에 바인딩/DataTrigger 큐를 소화하고 재배치함으로써 보장한다
        ///     (자세한 원인은 그 메서드 주석과 docs/design-notes.md 5-1절 참고).
        /// (2) 디자이너 표면이 실제로 다시 그려져야 한다 — Invalidate만으로는 다음
        ///     메시지 루프 틱에야 반영돼 "포커스를 옮겨야 보이는" 것처럼 느껴지므로,
        ///     여기서 캐시를 미리 만들고 Update로 동기 리페인트해 즉시 반영한다.
        /// </summary>
        protected void InvalidateDesignTimePreview()
        {
            if (!(this.isDesignTime || this.DesignMode))
            {
                return;
            }

            // 낡은 캐시를 버리고 "그 자리에서 동기로" 다시 렌더한다.
            //
            // WPF 디스패처의 BeginInvoke로 재렌더를 미루던 방식은 디자인 타임에서
            // 신뢰할 수 없다: 디자인 표면에는 WPF 콘텐츠가 실제로 호스팅되지 않아
            // (HwndSource 없음) 디스패처의 Background 큐를 펌프하는 주체가 없고,
            // 그 결과 콜백이 큐에 박혀 한참 뒤(또는 컨트롤 dispose 시)에야 발화한다.
            // 반면 RenderPreviewBitmap은 어느 문맥에서 불러도 올바른 값으로 렌더됨을
            // 확인했다(내부에서 Dispatcher.Invoke로 바인딩 큐를 동기 소화). 따라서
            // 여기서 동기로 렌더해 캐시에 담고, Invalidate+Update로 즉시 다시 그린다.
            // (분석: docs/design-notes.md 5-2절)
            this.DisposePreviewCache();
            if (this.Width > 0 && this.Height > 0)
            {
                this.previewCache = this.RenderPreviewBitmap();
            }

            if (this.IsHandleCreated)
            {
                // Invalidate만으로는 디자인 표면이 다음 페인트 틱에야 갱신되므로,
                // Update로 즉시(동기) WM_PAINT를 돌려 방금 만든 캐시를 바로 반영한다.
                this.Invalidate();
                this.Update();

                // 자기 자신(또는 직계 부모)만 Update해서는 화면이 안 바뀐다.
                // 계측 결과: 값 변경 직후 우리 OnPaint가 새 스냅숏을 정상으로 그리고
                // 부모 영역 무효화+Update까지 실행돼도 화면은 옛 모습이며, 이후 8초간
                // 이 컨트롤에 WM_PAINT가 오지 않는다. 반면 디자인 표면 클릭(선택 변경)
                // 이나 리사이즈 때는 폼의 "모든" 컨트롤이 일제히 다시 페인트되며 그제서야
                // 반영된다. 즉 VS 디자인 표면은 루트(디자이너 프레임) 수준에서 합성되고,
                // 중간 컨트롤의 개별 페인트는 그 합성 버퍼에 반영되지 않는다.
                // 따라서 부모가 아니라 "최상위 루트"에서 이 컨트롤 영역을 무효화해
                // 디자이너가 그 영역을 다시 합성(자식 포함 재페인트)하도록 강제한다.
                // (분석: docs/design-notes.md 5-3절)
                Control root = this;
                while (root.Parent != null)
                {
                    root = root.Parent;
                }

                if (!object.ReferenceEquals(root, this) && root.IsHandleCreated)
                {
                    Rectangle rootRect = root.RectangleToClient(this.RectangleToScreen(this.ClientRectangle));
                    rootRect.Inflate(2, 2); // 테두리·어도너 겹침 여유
                    root.Invalidate(rootRect, true);
                    root.Update();
                }

                // 크기 1px 흔들기(리사이즈 흉내) — 이 환경에서 검증된 "유일한" 화면
                // 갱신 경로. 계측 결과, 새 스냅숏을 자기 창에 실제 WM_PAINT로 두 번
                // 그려도(자기 Update + 루트 무효화 경유) VS 디자이너 화면은 갱신되지
                // 않았고, 오직 실제 크기 변경만 반영됐다. 크기 변경은
                // WM_WINDOWPOSCHANGED를 통해 디자이너(ControlDesigner/BehaviorService)
                // 의 표면 재동기화를 강제하기 때문이다. 폭(도킹 등으로 폭이 고정이면
                // 높이)을 1px 늘렸다 즉시 되돌려 그 경로를 그대로 태운다. 최종 크기는
                // 원래 값 그대로이므로 .Designer.cs 직렬화에는 아무 변화가 없다.
                // (분석: docs/design-notes.md 5-3절)
                if (!this.isNudgingBounds)
                {
                    this.isNudgingBounds = true;
                    try
                    {
                        int originalWidth = this.Width;
                        this.Width = originalWidth + 1;
                        if (this.Width == originalWidth + 1)
                        {
                            this.Width = originalWidth;
                        }
                        else
                        {
                            // Dock 등으로 폭이 고정된 컨트롤은 높이로 시도한다.
                            int originalHeight = this.Height;
                            this.Height = originalHeight + 1;
                            if (this.Height == originalHeight + 1)
                            {
                                this.Height = originalHeight;
                            }
                        }
                    }
                    finally
                    {
                        this.isNudgingBounds = false;
                    }
                }
            }
            else
            {
                // 핸들이 아직 없으면(로드 초기) 첫 페인트가 알아서 그린다.
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
        /// ElementHost 자체의 Child 속성은 디자이너에서 건드릴 일이 없으므로
        /// 속성 그리드와 직렬화에서 숨긴다.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new System.Windows.UIElement Child
        {
            get { return base.Child; }
            set { base.Child = value; }
        }

        // ===== 무효 상속 속성 숨김 =====
        // 아래 WinForms 상속 속성들은 WPF 콘텐츠에 아무 효과가 없다(모든 색·폰트는
        // Themes/Tokens.xaml 토큰이 소스). 속성 그리드에 보이면 "바꿨는데 안 변하는"
        // 혼동을 주므로 그리드와 직렬화에서 숨긴다. 기능 자체는 base로 그대로
        // 위임하므로 기존 코드가 설정해도 깨지지 않는다.

        /// <summary>WPF 콘텐츠에 효과 없음 — 디자인 토큰이 색상의 소스.</summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override System.Drawing.Color BackColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

        /// <summary>WPF 콘텐츠에 효과 없음 — 디자인 토큰이 색상의 소스.</summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override System.Drawing.Color ForeColor
        {
            get { return base.ForeColor; }
            set { base.ForeColor = value; }
        }

        /// <summary>WPF 콘텐츠에 효과 없음 — 디자인 토큰이 타이포그래피의 소스.</summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Font Font
        {
            get { return base.Font; }
            set { base.Font = value; }
        }

        /// <summary>WPF 콘텐츠에 효과 없음.</summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Image BackgroundImage
        {
            get { return base.BackgroundImage; }
            set { base.BackgroundImage = value; }
        }

        /// <summary>WPF 콘텐츠에 효과 없음.</summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override ImageLayout BackgroundImageLayout
        {
            get { return base.BackgroundImageLayout; }
            set { base.BackgroundImageLayout = value; }
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

        // WPF 컨트롤을 화면 밖에서 측정/배치한 뒤 RenderTargetBitmap을 거쳐
        // GDI+ 비트맵으로 렌더링한다. 디자이너에게는 그냥 그림일 뿐이므로
        // 직렬화나 마우스 캡처 문제가 없다.
        // 실패 시 null을 반환한다(placeholder 폴백).
        private Bitmap RenderPreviewBitmap()
        {
            if (this.Wpf == null)
            {
                return null;
            }

            try
            {
                System.Windows.Size size = new System.Windows.Size(this.Width, this.Height);

                // 1단계: 최초 측정/배치로 시각 트리를 활성화한다(바인딩/트리거가
                // 살아있는 상태가 된다).
                this.Wpf.Measure(size);
                this.Wpf.Arrange(new System.Windows.Rect(size));

                // 2단계: 대기 중인 바인딩/DataTrigger를 먼저 소화시킨다.
                // Kind→FontSize/FontWeight, Required/TitleBar→Visibility 같은 변경은
                // XAML에서 ElementName 바인딩(예: {Binding Kind, ElementName=RootControl})에
                // 연결된 DataTrigger로 처리되며, 이는 DispatcherPriority.DataBind로
                // "비동기" 반영된다. 즉 래퍼 setter에서 this.Wpf.Kind를 대입한 직후에는
                // 트리거가 아직 적용되지 않았다. 여기서 큐를 비우지 않고 바로 렌더하면
                // "변경 직전"(예: Title이 아니라 Body) 모습이 스냅숏에 찍혀, 값 변경이
                // 미리보기에 반영되지 않는다. (분석: docs/design-notes.md 5-1절)
                this.Wpf.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.DataBind,
                    new Action(delegate { }));

                // 3단계: 트리거가 바꾼 FontSize/FontWeight/Visibility로 다시 측정/배치
                // 한다. 2단계 없이 UpdateLayout만 하면 바뀌기 전 폰트로 레이아웃이
                // 잡히고, 2단계 후 재배치를 안 하면 레이아웃이 더러운 채 렌더된다.
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

                    // Bitmap(stream)은 스트림 수명에 묶여 있으므로,
                    // 분리된 복사본을 대신 돌려준다.
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

        // 스냅숏을 렌더링할 수 없을 때의 placeholder: 옅은 테두리 + 타입 이름.
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
