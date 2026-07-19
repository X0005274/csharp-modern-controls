using System;
using System.ComponentModel;
using System.Drawing;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Input
{
    /// <summary>
    /// 드롭다운 버튼 (Button + ContextMenuStrip 조합을 대체;
    /// WPF ModernDropDownButtonControl을 ElementHost로 호스팅).
    ///
    /// 버튼을 클릭하면 메뉴가 열리고, 항목 클릭 시 ItemClicked가 발생한다.
    /// 메뉴 항목은 코드/명칭 계약(DataSource/DisplayMember/ValueMember)으로
    /// 할당한다 — 엑셀 내보내기 옵션처럼 한 버튼에 여러 동작을 묶을 때 사용.
    /// </summary>
    [ToolboxItem(true)]
    [DefaultEvent("ItemClicked")]
    public class ModernDropDownButton : WpfElementHostBase<Modern.Lab.Controls.Wpf.Input.ModernDropDownButtonControl>
    {
        private object dataSource;

        // 디자인 타임 WPF 생성이 실패한 경우(Wpf == null)에도 속성 그리드가
        // 동작하도록 하는 폴백 저장소.
        private string fallbackText;
        private string fallbackDisplayMember;
        private string fallbackValueMember;
        private string fallbackEnabledMember;
        private Modern.Lab.Controls.Wpf.Input.ButtonKind fallbackKind;

        /// <summary>메뉴 항목이 클릭될 때 발생한다.</summary>
        public event EventHandler<Modern.Lab.Controls.Wpf.Input.DropDownItemClickedEventArgs> ItemClicked;

        /// <summary>버튼 시각 종류 — ModernButton과 같은 문법
        /// (Secondary 기본, Execute = Success 초록 채움).</summary>
        [Category("모던 컨트롤")]
        [Description("버튼 시각 종류 (Secondary 기본, Execute = 초록 채움)")]
        [DefaultValue(Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary)]
        public Modern.Lab.Controls.Wpf.Input.ButtonKind Kind
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.Kind;
                }

                return this.fallbackKind;
            }
            set
            {
                this.fallbackKind = value;

                if (this.Wpf != null)
                {
                    this.Wpf.Kind = value;
                }
            }
        }

        /// <summary>메뉴 팝업이 현재 열려 있는지 여부.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsDropDownOpen
        {
            get { return this.Wpf != null && this.Wpf.IsDropDownOpen; }
        }

        /// <summary>
        /// 항목 실행 가능 여부로 쓸 컬럼/속성 이름 (bool 또는 "Y"/"true"/"1").
        /// 비우면 모든 항목이 활성이다. 비활성 항목은 회색으로 표시되고
        /// 클릭되지 않는다 — 컨텍스트 메뉴의 비활성 표시와 같은 의미.
        /// </summary>
        [Category("모던 컨트롤")]
        [Description("항목 실행 가능 여부 컬럼/속성 이름 (비우면 전부 활성)")]
        [DefaultValue("")]
        public string EnabledMember
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.EnabledMemberPath;
                }

                return this.fallbackEnabledMember;
            }
            set
            {
                this.fallbackEnabledMember = value;

                if (this.Wpf != null)
                {
                    this.Wpf.EnabledMemberPath = value;
                }
            }
        }

        /// <summary>적절한 기본 크기로 컨트롤을 생성한다.</summary>
        public ModernDropDownButton()
        {
            this.Size = new Size(110, 32);
            this.fallbackText = "메뉴";
            this.fallbackDisplayMember = string.Empty;
            this.fallbackValueMember = string.Empty;
            this.fallbackEnabledMember = string.Empty;
            this.fallbackKind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;

            if (this.Wpf != null)
            {
                this.Wpf.Text = this.fallbackText;
                this.Wpf.ItemClicked += this.OnWpfItemClicked;
            }
        }

        /// <summary>버튼 캡션 (셰브런은 자동으로 붙는다).</summary>
        [Category("모던 컨트롤")]
        [Description("버튼에 표시할 텍스트")]
        [Browsable(true)]
        [Localizable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue("메뉴")]
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
        /// 메뉴 항목 목록: DataTable, DataView, IList 또는 임의의 IEnumerable.
        /// null이면 메뉴를 비운다.
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

        /// <summary>메뉴 표시 텍스트로 사용할 컬럼/속성 이름(WinForms 호환 이름).</summary>
        [Category("모던 컨트롤")]
        [Description("메뉴 표시 텍스트로 사용할 컬럼/속성 이름")]
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

        /// <summary>항목 값으로 사용할 컬럼/속성 이름(WinForms 호환 이름).</summary>
        [Category("모던 컨트롤")]
        [Description("항목 값으로 사용할 컬럼/속성 이름")]
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

        private void OnWpfItemClicked(object sender, Modern.Lab.Controls.Wpf.Input.DropDownItemClickedEventArgs e)
        {
            if (this.ItemClicked != null)
            {
                this.ItemClicked(this, e);
            }
        }
    }
}
