using System.ComponentModel;
using System.Drawing;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Display
{
    /// <summary>
    /// 상태 표시 pill 배지 (색 있는 Label 사용을 대체;
    /// WPF ModernStatusBadgeControl을 ElementHost로 호스팅).
    ///
    /// 승인/반려/대기, 운영/개발 같은 상태를 색 있는 알약으로 표시한다.
    /// Color에 배경색만 주면 글자색은 배경과 같은 색상 계열로 자동 유도된다
    /// (요약 칩과 동일한 규칙).
    /// </summary>
    [ToolboxItem(true)]
    public class ModernStatusBadge : WpfElementHostBase<Modern.Lab.Controls.Wpf.Display.ModernStatusBadgeControl>
    {
        // 디자인 타임 WPF 생성이 실패한 경우(Wpf == null)에도 속성 그리드가
        // 동작하도록 하는 폴백 저장소.
        private string fallbackText;
        private string fallbackColor;

        /// <summary>적절한 기본 크기로 컨트롤을 생성한다.</summary>
        public ModernStatusBadge()
        {
            this.Size = new Size(70, 24);
            this.fallbackText = "상태";
            this.fallbackColor = string.Empty;

            if (this.Wpf != null)
            {
                this.Wpf.Text = this.fallbackText;
            }
        }

        /// <summary>배지 텍스트.</summary>
        [Category("모던 컨트롤")]
        [Description("배지에 표시할 텍스트")]
        [Browsable(true)]
        [Localizable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue("상태")]
        public override string Text
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.Text;
                }

                return this.fallbackText;
            }
            set
            {
                this.fallbackText = value;

                if (this.Wpf != null)
                {
                    this.Wpf.Text = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>
        /// 배경색 문자열 ("#DCFCE7" hex 또는 "SkyBlue" 색 이름).
        /// 글자색은 자동 유도. 비우면 중립 회색 배지.
        /// </summary>
        [Category("모던 컨트롤")]
        [Description("배지 배경색 (hex 또는 색 이름 문자열; 비우면 중립 회색, 글자색은 자동)")]
        [DefaultValue("")]
        public string Color
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.Color;
                }

                return this.fallbackColor;
            }
            set
            {
                this.fallbackColor = value;

                if (this.Wpf != null)
                {
                    this.Wpf.Color = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }
    }
}
