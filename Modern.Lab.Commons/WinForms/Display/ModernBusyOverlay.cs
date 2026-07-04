using System.ComponentModel;
using System.Drawing;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Display
{
    /// <summary>
    /// 조회/처리 중 대상 영역을 덮는 로딩 오버레이 (신규 개념; WPF
    /// ModernBusyOverlayControl을 ElementHost로 호스팅).
    ///
    /// 사용법: 덮을 영역(보통 그리드)과 같은 Dock/Bounds로 배치하고 z-순서를
    /// 위(컨테이너 index 0)로 둔다. Busy = true면 표시 + 앞으로,
    /// false면 숨김. 기본은 숨김 상태.
    ///
    /// 실제 폼에서는 백그라운드 조회 시작 시 true, UI 스레드 Invoke로 결과를
    /// 반영한 뒤 false로 되돌린다.
    /// </summary>
    [ToolboxItem(true)]
    public class ModernBusyOverlay : WpfElementHostBase<Modern.Lab.Controls.Wpf.Display.ModernBusyOverlayControl>
    {
        // 디자인 타임 WPF 생성이 실패한 경우(Wpf == null)에도 속성 그리드가
        // 동작하도록 하는 폴백 저장소.
        private string fallbackMessage;
        private bool busy;

        /// <summary>기본 크기와 숨김 상태로 컨트롤을 생성한다.</summary>
        public ModernBusyOverlay()
        {
            this.Size = new Size(400, 300);
            this.fallbackMessage = "처리 중...";
            this.busy = false;

            // 오버레이는 필요할 때만 나타난다. 디자인 타임에는 배치를 위해
            // 보이는 상태를 유지한다.
            if (!this.DesignMode)
            {
                this.Visible = false;
            }
        }

        /// <summary>
        /// 로딩 표시 여부. true면 오버레이를 표시하고 맨 앞으로 가져오며,
        /// false면 숨긴다.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Busy
        {
            get
            {
                return this.busy;
            }
            set
            {
                this.busy = value;

                if (value)
                {
                    this.Visible = true;
                    this.BringToFront();
                }
                else
                {
                    this.Visible = false;
                }
            }
        }

        /// <summary>스피너 아래 표시되는 안내 메시지.</summary>
        [Category("모던 컨트롤")]
        [Description("스피너 아래 표시할 안내 메시지")]
        [Localizable(true)]
        [DefaultValue("처리 중...")]
        public string Message
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.Message;
                }

                return this.fallbackMessage;
            }
            set
            {
                this.fallbackMessage = value;

                if (this.Wpf != null)
                {
                    this.Wpf.Message = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }
    }
}
