using System;
using System.ComponentModel;
using System.Drawing;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Selection
{
    /// <summary>
    /// 배타 선택 라디오 그룹 (GroupBox + RadioButton 묶음을 대체;
    /// WPF ModernRadioGroupControl을 ElementHost로 호스팅).
    ///
    /// ComboBox 데이터 계약 명명을 따른다: DataSource가 항목을 담고,
    /// DisplayMember가 표시 텍스트를, ValueMember가 선택 값을 지정한다.
    /// SelectedValue를 DataSource보다 먼저 설정해도 된다(계약 규칙 3).
    /// </summary>
    [ToolboxItem(true)]
    public class ModernRadioGroup : WpfElementHostBase<Modern.Lab.Controls.Wpf.Selection.ModernRadioGroupControl>
    {
        private object dataSource;

        // 디자인 타임 WPF 생성이 실패한 경우(Wpf == null)에도 속성 그리드가
        // 동작하도록 하는 폴백 저장소.
        private string fallbackDisplayMember;
        private string fallbackValueMember;
        private bool fallbackVertical;

        /// <summary>선택 값이 바뀔 때 발생한다.</summary>
        public event EventHandler SelectedValueChanged;

        /// <summary>적절한 기본 크기로 컨트롤을 생성한다.</summary>
        public ModernRadioGroup()
        {
            this.Size = new Size(300, 24);
            this.fallbackDisplayMember = string.Empty;
            this.fallbackValueMember = string.Empty;
            this.fallbackVertical = false;

            if (this.Wpf != null)
            {
                this.Wpf.SelectedValueChanged += this.OnWpfSelectedValueChanged;
            }
        }

        /// <summary>
        /// 항목 목록: DataTable, DataView, IList 또는 임의의 IEnumerable.
        /// 재할당 시 기존 SelectedValue가 새 목록에 없으면 미선택으로 초기화된다.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object DataSource
        {
            get
            {
                return this.dataSource;
            }
            set
            {
                this.dataSource = value;

                if (this.Wpf != null)
                {
                    this.Wpf.ItemsSource = DataSourceConverter.ToItemsSource(value);
                }
            }
        }

        /// <summary>표시 텍스트로 사용할 컬럼/속성 이름(WinForms 호환 이름).</summary>
        [Category("모던 컨트롤")]
        [Description("표시 텍스트로 사용할 컬럼/속성 이름")]
        [DefaultValue("")]
        public string DisplayMember
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.DisplayMemberPath;
                }

                return this.fallbackDisplayMember;
            }
            set
            {
                this.fallbackDisplayMember = value;

                if (this.Wpf != null)
                {
                    this.Wpf.DisplayMemberPath = value;
                }
            }
        }

        /// <summary>선택 값으로 사용할 컬럼/속성 이름(WinForms 호환 이름).</summary>
        [Category("모던 컨트롤")]
        [Description("선택 값으로 사용할 컬럼/속성 이름")]
        [DefaultValue("")]
        public string ValueMember
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.ValueMemberPath;
                }

                return this.fallbackValueMember;
            }
            set
            {
                this.fallbackValueMember = value;

                if (this.Wpf != null)
                {
                    this.Wpf.ValueMemberPath = value;
                }
            }
        }

        /// <summary>
        /// 선택된 값 (ValueMember 기준). null = 미선택.
        /// DataSource보다 먼저 설정해도 목록 구성 시 적용된다.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedValue
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.SelectedValue;
                }

                return null;
            }
            set
            {
                if (this.Wpf != null)
                {
                    this.Wpf.SelectedValue = value;
                }
            }
        }

        /// <summary>true면 항목을 세로로 나열한다 (기본은 가로).</summary>
        [Category("모던 컨트롤")]
        [Description("항목을 세로로 나열할지 여부")]
        [DefaultValue(false)]
        public bool Vertical
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.Vertical;
                }

                return this.fallbackVertical;
            }
            set
            {
                this.fallbackVertical = value;

                if (this.Wpf != null)
                {
                    this.Wpf.Vertical = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        private void OnWpfSelectedValueChanged(object sender, EventArgs e)
        {
            if (this.SelectedValueChanged != null)
            {
                this.SelectedValueChanged(this, EventArgs.Empty);
            }
        }
    }
}
