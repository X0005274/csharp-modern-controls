using System;
using System.ComponentModel;
using System.Drawing;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Input
{
    /// <summary>
    /// 온/오프 토글 스위치 (설정성 켬/끔 용도의 CheckBox 대체;
    /// WPF ModernToggleSwitchControl을 ElementHost로 호스팅).
    ///
    /// 호환 멤버: Text(override, localizable), Checked, CheckedChanged, Enabled —
    /// ModernCheckBox와 동일한 API에 비주얼만 스위치.
    /// 용도 구분: 설정성 "켬/끔"에는 스위치, 다중 "선택/포함"에는 체크박스.
    /// </summary>
    [ToolboxItem(true)]
    public class ModernToggleSwitch : WpfElementHostBase<Modern.Lab.Controls.Wpf.Input.ModernToggleSwitchControl>
    {
        // 디자인 타임 WPF 생성이 실패한 경우(Wpf == null)에도 속성 그리드가
        // 동작하도록 하는 폴백 저장소.
        private string fallbackText;
        private bool fallbackChecked;

        /// <summary>상태가 바뀔 때 발생한다(WinForms 호환 이름).</summary>
        public event EventHandler CheckedChanged;

        /// <summary>적절한 기본 크기로 컨트롤을 생성한다.</summary>
        public ModernToggleSwitch()
        {
            this.Size = new Size(140, 24);
            this.fallbackText = "토글";
            this.fallbackChecked = false;

            if (this.Wpf != null)
            {
                this.Wpf.Text = this.fallbackText;
                this.Wpf.CheckedChanged += this.OnWpfCheckedChanged;
            }
        }

        /// <summary>스위치 옆에 표시되는 레이블 텍스트.</summary>
        [Category("모던 컨트롤")]
        [Description("스위치 옆에 표시할 텍스트")]
        [Browsable(true)]
        [Localizable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue("토글")]
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

        /// <summary>켬/끔 상태(CheckBox.Checked와 동일한 의미).</summary>
        [Category("모던 컨트롤")]
        [Description("켬/끔 상태")]
        [DefaultValue(false)]
        public bool Checked
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.IsChecked;
                }

                return this.fallbackChecked;
            }
            set
            {
                this.fallbackChecked = value;

                if (this.Wpf != null)
                {
                    this.Wpf.IsChecked = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        private void OnWpfCheckedChanged(object sender, EventArgs e)
        {
            if (this.CheckedChanged != null)
            {
                this.CheckedChanged(this, EventArgs.Empty);
            }
        }
    }
}
