using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Display
{
    /// <summary>
    /// 자동 소멸 알림 토스트 (MessageBox 남용을 대체; WPF ModernToastControl을
    /// ElementHost로 호스팅).
    ///
    /// 폼에 하나 배치해 두고 Show(message, kind)를 호출하면 부모의 우하단에
    /// 나타났다가 DurationMs 후 자동으로 사라진다. "저장되었습니다" 같은
    /// 확인이 필요 없는 완료/안내 알림에 사용한다 — 사용자의 확인(예/아니오)이
    /// 필요한 경우에는 계속 MessageBox를 쓴다.
    /// </summary>
    [ToolboxItem(true)]
    public class ModernToast : WpfElementHostBase<Modern.Lab.Controls.Wpf.Display.ModernToastControl>
    {
        private const int EdgeMargin = 16;

        private readonly Timer hideTimer;
        private int durationMs;

        /// <summary>기본 크기와 숨김 상태로 컨트롤을 생성한다.</summary>
        public ModernToast()
        {
            this.Size = new Size(280, 44);
            this.durationMs = 2500;

            this.hideTimer = new Timer();
            this.hideTimer.Tick += this.OnHideTimerTick;

            // 토스트는 Show가 호출될 때만 나타난다. 디자인 타임에는 배치를 위해
            // 보이는 상태를 유지한다.
            if (!this.DesignMode)
            {
                this.Visible = false;
            }
        }

        /// <summary>표시 유지 시간(밀리초). 기본 2500.</summary>
        [Category("모던 컨트롤")]
        [Description("토스트 표시 유지 시간(밀리초)")]
        [DefaultValue(2500)]
        public int DurationMs
        {
            get
            {
                return this.durationMs;
            }
            set
            {
                this.durationMs = Math.Max(500, value);
            }
        }

        /// <summary>정보(Info) 종류로 알림을 표시한다.</summary>
        public void Show(string message)
        {
            this.Show(message, Modern.Lab.Controls.Wpf.Display.ToastKind.Info);
        }

        /// <summary>
        /// 알림을 표시한다: 내용에 맞게 크기를 조정하고 부모 우하단에 배치한 뒤
        /// DurationMs 후 자동으로 숨긴다. 연속 호출 시 타이머가 다시 시작된다.
        /// </summary>
        public void Show(string message, Modern.Lab.Controls.Wpf.Display.ToastKind kind)
        {
            if (this.Wpf == null)
            {
                return;
            }

            this.Wpf.Message = message;
            this.Wpf.Kind = kind;

            // 내용에 맞는 크기 계산 (메시지 길이에 따라 폭이 달라진다)
            this.Wpf.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
            int width = (int)Math.Ceiling(this.Wpf.DesiredSize.Width);
            int height = (int)Math.Ceiling(this.Wpf.DesiredSize.Height);
            this.Size = new Size(Math.Max(160, width), Math.Max(40, height));

            this.RepositionBottomRight();

            this.Visible = true;
            this.BringToFront();

            this.hideTimer.Stop();
            this.hideTimer.Interval = this.durationMs;
            this.hideTimer.Start();
        }

        /// <summary>표시 중인 알림을 즉시 숨긴다.</summary>
        public void HideToast()
        {
            this.hideTimer.Stop();
            this.Visible = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.hideTimer.Dispose();
            }

            base.Dispose(disposing);
        }

        private void RepositionBottomRight()
        {
            if (this.Parent == null)
            {
                return;
            }

            this.Location = new Point(
                Math.Max(0, this.Parent.ClientSize.Width - this.Width - EdgeMargin),
                Math.Max(0, this.Parent.ClientSize.Height - this.Height - EdgeMargin));
        }

        private void OnHideTimerTick(object sender, EventArgs e)
        {
            this.HideToast();
        }
    }
}
