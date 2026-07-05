using System;
using System.ComponentModel;
using System.Drawing;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Data
{
    /// <summary>
    /// System.Windows.Forms.DataGridView의 드롭인 대체 컨트롤(읽기 전용 목록
    /// 시나리오; WPF ModernDataGridControl을 ElementHost로 호스팅).
    ///
    /// 호환 멤버: DataSource(DataTable/DataView/IList/IEnumerable),
    /// AutoGenerateColumns, RowCount, SelectionChanged, Enabled.
    ///
    /// 계약 동작 (docs/design-notes.md 6-1절):
    /// - DataSource를 할당하면 선택이 초기화되고, 데이터가 있으면 첫 행이
    ///   선택되며(DataGridView의 초기 현재 행과 동일), 할당 한 번당
    ///   SelectionChanged가 정확히 한 번 발생한다.
    /// - null/빈 데이터는 빈 그리드로 렌더링되며 절대 예외를 던지지 않는다.
    /// </summary>
    [ToolboxItem(true)]
    public class ModernDataGrid : WpfElementHostBase<ModernDataGridControl>
    {
        private object dataSource;
        private bool suppressSelectionChanged;

        // 디자인 타임 WPF 생성이 실패한 경우(Wpf == null)에도 속성 그리드가
        // 동작하도록 하는 폴백 저장소.
        private bool fallbackAutoGenerateColumns;
        private bool fallbackShowStatusBar;
        private string fallbackStatusText;
        private string fallbackStatusCountFormat;

        /// <summary>행 선택이 바뀔 때 발생한다(WinForms 호환 이름).</summary>
        public event EventHandler SelectionChanged;

        /// <summary>높이 변화로 표시 가능 행 수(VisibleRowCapacity)가 바뀔 때 발생한다.</summary>
        public event EventHandler VisibleRowCapacityChanged;

        /// <summary>적절한 기본 크기로 컨트롤을 생성한다.</summary>
        public ModernDataGrid()
        {
            this.Size = new Size(480, 240);
            this.fallbackAutoGenerateColumns = true;
            this.fallbackShowStatusBar = false;
            this.fallbackStatusText = string.Empty;
            this.fallbackStatusCountFormat = "{0:N0} rows";

            if (this.Wpf != null)
            {
                this.Wpf.SelectionChanged += this.OnWpfSelectionChanged;
                this.Wpf.VisibleRowCapacityChanged += this.OnWpfVisibleRowCapacityChanged;
            }
        }

        /// <summary>
        /// 현재 높이에서 세로 스크롤 없이 표시 가능한 행 수 (최소 1).
        /// ModernPagination.PageSize에 넣으면 페이지 크기가 화면 높이를 따라간다 —
        /// VisibleRowCapacityChanged에서 갱신하면 리사이즈에도 자동 대응.
        /// </summary>
        [Browsable(false)]
        public int VisibleRowCapacity
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.VisibleRowCapacity;
                }

                return 1;
            }
        }

        private void OnWpfVisibleRowCapacityChanged(object sender, EventArgs e)
        {
            if (this.VisibleRowCapacityChanged != null)
            {
                this.VisibleRowCapacityChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 데이터 소스: DataTable, DataView, IList 또는 임의의 IEnumerable.
        /// 할당하면 선택이 초기화되고, 데이터가 있으면 첫 행이 선택되며,
        /// SelectionChanged가 한 번 발생한다. null을 할당하면 그리드를 비운다.
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
                    this.Wpf.ItemsSource = DataSourceConverter.ToItemsSource(value);

                    if (this.Wpf.RowCount > 0)
                    {
                        // DataGridView와 동일: 바인딩 시 첫 행이 현재 행이 된다.
                        this.Wpf.SelectedIndex = 0;
                    }
                }
                finally
                {
                    this.suppressSelectionChanged = false;
                }

                this.RaiseSelectionChanged();
            }
        }

        /// <summary>데이터 소스로부터 컬럼을 자동 생성할지 여부(WinForms 호환).</summary>
        [Category("모던 컨트롤")]
        [Description("데이터 원본에서 컬럼을 자동 생성할지 여부")]
        [DefaultValue(true)]
        public bool AutoGenerateColumns
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.AutoGenerateColumns;
                }

                return this.fallbackAutoGenerateColumns;
            }
            set
            {
                this.fallbackAutoGenerateColumns = value;

                if (this.Wpf != null)
                {
                    this.Wpf.AutoGenerateColumns = value;
                }
            }
        }

        /// <summary>그리드 하단 상태바 표시 여부 (행 수 자동 표기 + StatusText).</summary>
        [Category("모던 컨트롤")]
        [Description("그리드 하단 상태바 표시 여부 — 왼쪽 행 수 자동 표기, 오른쪽 StatusText")]
        [DefaultValue(false)]
        public bool ShowStatusBar
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.ShowStatusBar;
                }

                return this.fallbackShowStatusBar;
            }
            set
            {
                this.fallbackShowStatusBar = value;

                if (this.Wpf != null)
                {
                    this.Wpf.ShowStatusBar = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>상태바 오른쪽에 표시할 자유 텍스트 (선택 대상·조회 조건 등).</summary>
        [Category("모던 컨트롤")]
        [Description("상태바 오른쪽에 표시할 자유 텍스트")]
        [Localizable(true)]
        [DefaultValue("")]
        public string StatusText
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.StatusText;
                }

                return this.fallbackStatusText;
            }
            set
            {
                this.fallbackStatusText = value;

                if (this.Wpf != null)
                {
                    this.Wpf.StatusText = value;
                }
            }
        }

        /// <summary>상태바 왼쪽 행 수 표기 형식. {0}에 현재 행 수가 들어간다.</summary>
        [Category("모던 컨트롤")]
        [Description("상태바 행 수 표기 형식 — {0}에 현재 행 수가 들어간다")]
        [Localizable(true)]
        [DefaultValue("{0:N0} rows")]
        public string StatusCountFormat
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.StatusCountFormat;
                }

                return this.fallbackStatusCountFormat;
            }
            set
            {
                this.fallbackStatusCountFormat = value;

                if (this.Wpf != null)
                {
                    this.Wpf.StatusCountFormat = value;
                }
            }
        }

        /// <summary>현재 표시 중인 행 수(WinForms 호환 이름).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int RowCount
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.RowCount;
                }

                return 0;
            }
        }

        /// <summary>현재 선택된 행 항목(DataTable 소스의 경우 DataRowView).</summary>
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

        /// <summary>선택된 행의 인덱스(아무것도 선택되지 않았으면 -1).</summary>
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
        /// 컬럼을 명시적 정의로 교체하고 자동 생성을 끈다.
        /// DataSource 할당 전에 호출한다.
        /// </summary>
        public void ConfigureColumns(params ModernDataGridColumn[] columns)
        {
            this.fallbackAutoGenerateColumns = false;

            if (this.Wpf != null)
            {
                this.Wpf.ApplyColumns(columns);
            }
        }

        private void OnWpfSelectionChanged(object sender, EventArgs e)
        {
            if (!this.suppressSelectionChanged)
            {
                this.RaiseSelectionChanged();
            }
        }

        private void RaiseSelectionChanged()
        {
            if (this.SelectionChanged != null)
            {
                this.SelectionChanged(this, EventArgs.Empty);
            }
        }
    }
}
