using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Selection
{
    /// <summary>
    /// System.Windows.Forms.ComboBox의 드롭인 대체 컨트롤
    /// (WPF ModernComboBoxControl을 ElementHost로 호스팅).
    ///
    /// 호환 멤버: DataSource(DataTable/DataView/IList/IEnumerable),
    /// DisplayMember, ValueMember, SelectedValue, SelectedItem, SelectedIndex,
    /// Items, SelectedIndexChanged, Enabled, DropDownStyle(DropDownList =
    /// 선택 전용; DropDown/Simple = 입력하면 한국어 초성 매칭으로 목록이
    /// 필터링되는 검색형 콤보).
    ///
    /// 계약 동작 (docs/design-notes.md 6-1절):
    /// - SelectedValue는 DataSource보다 먼저 할당해도 된다; 값이 보류되었다가
    ///   데이터가 도착하면 적용된다(규칙 3).
    /// - DataSource를 할당하면 보류 값이 없을 때 첫 행이 선택되어 WinForms
    ///   ComboBox 동작과 일치하며, 할당 한 번당 SelectedIndexChanged가
    ///   정확히 한 번 발생한다.
    /// - null/빈 데이터는 빈 목록으로 렌더링되며 절대 예외를 던지지 않는다.
    /// </summary>
    [ToolboxItem(true)]
    public class ModernComboBox : WpfElementHostBase<Modern.Lab.Controls.Wpf.Selection.ModernComboBoxControl>
    {
        // DataSource가 할당되지 않았을 때 사용하는 Items 컬렉션 (combo.Items.Add(...)).
        private readonly ObservableCollection<object> manualItems;

        private object dataSource;
        private object pendingSelectedValue;
        private bool hasPendingSelectedValue;
        private bool suppressSelectionChanged;

        // 디자인 타임 WPF 생성이 실패한 경우(Wpf == null)에도 속성 그리드가
        // 동작하도록 하는 폴백 저장소.
        private string fallbackDisplayMember;
        private string fallbackValueMember;
        private string fallbackPlaceholder;
        private ComboBoxStyle fallbackDropDownStyle;

        /// <summary>선택이 바뀔 때 발생한다(WinForms 호환 이름).</summary>
        public event EventHandler SelectedIndexChanged;

        /// <summary>적절한 기본 크기로 컨트롤을 생성한다.</summary>
        public ModernComboBox()
        {
            this.Size = new Size(200, 32);
            this.manualItems = new ObservableCollection<object>();
            this.fallbackDisplayMember = string.Empty;
            this.fallbackValueMember = string.Empty;
            this.fallbackPlaceholder = string.Empty;

            // 기본값은 System.Windows.Forms.ComboBox와 동일: DropDown(편집 가능).
            // 입력하면 목록이 필터링되고, 텍스트를 지우면 선택도 지워진다.
            this.fallbackDropDownStyle = ComboBoxStyle.DropDown;

            if (this.Wpf != null)
            {
                this.Wpf.ItemsSource = this.manualItems;
                this.Wpf.IsEditable = true;
                this.Wpf.SelectionChanged += this.OnWpfSelectionChanged;
            }
        }

        /// <summary>
        /// 데이터 소스: DataTable, DataView, IList 또는 임의의 IEnumerable.
        /// 할당하면 선택이 초기화되고, 보류 중인 SelectedValue가 있으면 적용되며
        /// (없으면 첫 행 선택), SelectedIndexChanged가 한 번 발생한다.
        /// null을 할당하면 수동 Items 컬렉션으로 폴백한다.
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

                if (this.Wpf == null)
                {
                    return;
                }

                this.suppressSelectionChanged = true;

                try
                {
                    this.Wpf.SelectedItem = null;
                    this.Wpf.ItemsSource = DataSourceConverter.ToItemsSource(value) ?? this.manualItems;

                    if (this.hasPendingSelectedValue)
                    {
                        this.Wpf.SelectedValue = this.pendingSelectedValue;
                        this.pendingSelectedValue = null;
                        this.hasPendingSelectedValue = false;
                    }
                    else if (this.Wpf.SelectedItem == null)
                    {
                        // WinForms ComboBox와 동일: 기본적으로 첫 행을 선택한다.
                        this.SelectFirstItemIfAny();
                    }
                }
                finally
                {
                    this.suppressSelectionChanged = false;
                }

                this.RaiseSelectedIndexChanged();
            }
        }

        /// <summary>
        /// 아무것도 선택/입력되지 않은 동안 표시되는 힌트 텍스트.
        /// 일관된 API를 위해 ModernTextBox와 같은 속성 이름을 쓴다.
        /// </summary>
        [Category("모던 컨트롤")]
        [Description("미선택/미입력 상태에서 표시할 힌트 텍스트")]
        [Localizable(true)]
        [DefaultValue("")]
        public string PlaceholderText
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.Placeholder;
                }

                return this.fallbackPlaceholder;
            }
            set
            {
                this.fallbackPlaceholder = value;

                if (this.Wpf != null)
                {
                    this.Wpf.Placeholder = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>
        /// 선택 스타일(WinForms 호환 이름과 기본값). DropDown(기본, 편집 가능)은
        /// 입력하는 동안 바인딩된 목록을 필터링하고(초성 검색 포함) 텍스트를
        /// 지우면 선택도 지워진다; DropDownList는 선택 전용; Simple은
        /// DropDown처럼 동작한다.
        /// </summary>
        [Category("모던 컨트롤")]
        [Description("DropDown(기본)/Simple=입력으로 목록 필터링(검색형), DropDownList=선택 전용")]
        [DefaultValue(ComboBoxStyle.DropDown)]
        public ComboBoxStyle DropDownStyle
        {
            get
            {
                return this.fallbackDropDownStyle;
            }
            set
            {
                this.fallbackDropDownStyle = value;

                if (this.Wpf != null)
                {
                    this.Wpf.IsEditable = value != ComboBoxStyle.DropDownList;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>표시 텍스트로 사용되는 컬럼/속성 이름(WinForms 호환).</summary>
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

        /// <summary>값으로 사용되는 컬럼/속성 이름(WinForms 호환).</summary>
        [Category("모던 컨트롤")]
        [Description("SelectedValue로 사용할 컬럼/속성 이름")]
        [DefaultValue("")]
        public string ValueMember
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.SelectedValuePath;
                }

                return this.fallbackValueMember;
            }
            set
            {
                this.fallbackValueMember = value;

                if (this.Wpf != null)
                {
                    this.Wpf.SelectedValuePath = value;
                }
            }
        }

        /// <summary>
        /// 선택된 항목의 값. DataSource보다 먼저 할당해도 된다(값이 보류되었다가
        /// 데이터가 도착하면 적용된다 — 계약 규칙 3).
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
                if (this.Wpf == null)
                {
                    return;
                }

                if (this.HasBoundItems())
                {
                    this.Wpf.SelectedValue = value;
                }
                else
                {
                    this.pendingSelectedValue = value;
                    this.hasPendingSelectedValue = true;
                }
            }
        }

        /// <summary>현재 선택된 항목(DataTable 소스의 경우 DataRowView).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedItem
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.SelectedItem;
                }

                return null;
            }
            set
            {
                if (this.Wpf != null)
                {
                    this.Wpf.SelectedItem = value;
                }
            }
        }

        /// <summary>선택된 항목의 인덱스(아무것도 선택되지 않았으면 -1).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.SelectedIndex;
                }

                return -1;
            }
            set
            {
                if (this.Wpf != null)
                {
                    this.Wpf.SelectedIndex = value;
                }
            }
        }

        /// <summary>
        /// 수동 항목 컬렉션 (combo.Items.Add(...)). DataSource가 할당되지 않은
        /// 동안에만 사용되며, DataSource가 우선한다.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IList Items
        {
            get { return this.manualItems; }
        }

        /// <summary>
        /// 현재 선택의 표시 텍스트(WinForms ComboBox.Text). setter는
        /// DropDown/Simple 스타일에서 편집 가능 텍스트를 쓰며, DropDownList에서는
        /// 아무 동작도 하지 않는다 — SelectedValue/SelectedIndex로 선택할 것.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.SelectionText;
                }

                return string.Empty;
            }
            set
            {
                if (this.Wpf != null)
                {
                    this.Wpf.SetEditableText(value);
                }
            }
        }

        /// <summary>
        /// 드롭다운을 멀티컬럼(코드+명칭 등)으로 구성한다. 그리드와 동일한
        /// ModernDataGridColumn 정의를 재사용하며, 헤더 행이 표시되고 검색형
        /// 콤보의 타이핑 필터는 모든 컬럼(코드 포함)을 대상으로 동작한다.
        /// 필드의 선택 텍스트는 계속 DisplayMember(명칭)를 따른다.
        /// DataSource 할당 전에 호출한다.
        /// </summary>
        public void ConfigureDropDownColumns(params Modern.Lab.Controls.Wpf.Data.ModernDataGridColumn[] columns)
        {
            if (this.Wpf != null)
            {
                this.Wpf.ApplyDropDownColumns(columns);
            }
        }

        private bool HasBoundItems()
        {
            return this.Wpf != null &&
                   this.Wpf.ItemsSource != null &&
                   !object.ReferenceEquals(this.Wpf.ItemsSource, this.manualItems);
        }

        private void SelectFirstItemIfAny()
        {
            IEnumerator enumerator = this.Wpf.ItemsSource.GetEnumerator();

            if (enumerator.MoveNext())
            {
                this.Wpf.SelectedItem = enumerator.Current;
            }
        }

        private void OnWpfSelectionChanged(object sender, EventArgs e)
        {
            if (!this.suppressSelectionChanged)
            {
                this.RaiseSelectedIndexChanged();
            }
        }

        private void RaiseSelectedIndexChanged()
        {
            if (this.SelectedIndexChanged != null)
            {
                this.SelectedIndexChanged(this, EventArgs.Empty);
            }
        }
    }
}
