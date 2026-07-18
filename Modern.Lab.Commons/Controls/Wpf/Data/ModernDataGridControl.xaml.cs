using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Modern.Lab.Controls.Wpf.Common;

namespace Modern.Lab.Controls.Wpf.Data
{
    /// <summary>
    /// 모던 읽기 전용 데이터 그리드.
    /// - ItemsSource: 임의의 IEnumerable (DataView, IList, ...)
    /// - SelectedItem: 현재 행 (양방향)
    /// - AutoGenerateColumns: 기본값 true; ApplyColumns 호출 시 명시적 컬럼으로 전환
    /// - SelectionChanged: 행 선택이 바뀔 때 발생
    /// - ShowStatusBar: 그리드 하단 상태바 표시 여부 (행 수 자동 표기 + StatusText)
    /// </summary>
    public partial class ModernDataGridControl : UserControl
    {
        /// <summary>표시할 행 목록. 임의의 IEnumerable (DataView, IList, ...).</summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
                typeof(IEnumerable),
                typeof(ModernDataGridControl),
                new PropertyMetadata(null, OnItemsSourcePropertyChanged));

        /// <summary>
        /// 데이터가 0건일 때 데이터 영역 가운데에 표시할 안내 문구.
        /// 기본 "No data" — 화면 문맥에 맞게 바꾸거나 빈 문자열로 끈다.
        /// </summary>
        public static readonly DependencyProperty EmptyTextProperty =
            DependencyProperty.Register(
                "EmptyText",
                typeof(string),
                typeof(ModernDataGridControl),
                new PropertyMetadata("No data", OnEmptyTextChanged));

        /// <summary>
        /// 컬럼 너비를 헤더 캡션과 데이터 내용 중 더 넓은 쪽에 맞춰 자동 계산할지
        /// 여부. ApplyColumns로 정의된 컬럼에만 적용되며, 켜져 있으면 데이터가
        /// 바뀔 때마다 각 컬럼이 잘림 없이 표시되는 최소 너비로 재계산된다
        /// (정의의 Width는 무시). 스크롤 중 너비가 흔들리는 WPF Auto 크기 대신
        /// 전체 데이터를 미리 측정해 고정 픽셀 너비를 넣는다.
        /// </summary>
        public static readonly DependencyProperty AutoFitColumnsProperty =
            DependencyProperty.Register(
                "AutoFitColumns",
                typeof(bool),
                typeof(ModernDataGridControl),
                new PropertyMetadata(false, OnAutoFitColumnsChanged));

        /// <summary>현재 선택된 행 항목. 기본적으로 양방향 바인딩.</summary>
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                "SelectedItem",
                typeof(object),
                typeof(ModernDataGridControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>데이터 소스로부터 컬럼을 자동 생성할지 여부.</summary>
        public static readonly DependencyProperty AutoGenerateColumnsProperty =
            DependencyProperty.Register(
                "AutoGenerateColumns",
                typeof(bool),
                typeof(ModernDataGridControl),
                new PropertyMetadata(true));

        /// <summary>그리드 하단 상태바 표시 여부.</summary>
        public static readonly DependencyProperty ShowStatusBarProperty =
            DependencyProperty.Register(
                "ShowStatusBar",
                typeof(bool),
                typeof(ModernDataGridControl),
                new PropertyMetadata(false, OnStatusBarAppearanceChanged));

        /// <summary>상태바 오른쪽에 표시할 자유 텍스트 (빈 문자열이면 표시 없음).</summary>
        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register(
                "StatusText",
                typeof(string),
                typeof(ModernDataGridControl),
                new PropertyMetadata(string.Empty, OnStatusBarAppearanceChanged));

        /// <summary>상태바 왼쪽 행 수 표기 형식. {0}에 현재 행 수가 들어간다.</summary>
        public static readonly DependencyProperty StatusCountFormatProperty =
            DependencyProperty.Register(
                "StatusCountFormat",
                typeof(string),
                typeof(ModernDataGridControl),
                new PropertyMetadata("{0:N0} rows", OnStatusBarAppearanceChanged));

        /// <summary>
        /// 교차 행 배경(줄무늬) 표시 여부. 켜면 홀수 행이 테마 교차색
        /// (Brush.GridRowAlt)으로 칠해진다. 기본 꺼짐 — 행 높이·수평 구분선만으로
        /// 행이 구분되는 밀도에서는 줄무늬가 시각적 소음이 되기 쉽다.
        /// </summary>
        public static readonly DependencyProperty AlternatingRowColorsProperty =
            DependencyProperty.Register(
                "AlternatingRowColors",
                typeof(bool),
                typeof(ModernDataGridControl),
                new PropertyMetadata(false, OnAlternatingRowColorsChanged));

        /// <summary>
        /// 행 배경색으로 쓸 컬럼/속성 이름 (선택 사항). 값은 "#FEE2E2" 같은 색
        /// 문자열 또는 색 이름. 비어 있거나 해석 불가한 행은 기본 배경을
        /// 유지한다. 상태(Scrap 등)에 따라 행을 색으로 구분할 때 쓴다.
        /// </summary>
        public static readonly DependencyProperty RowColorMemberPathProperty =
            DependencyProperty.Register(
                "RowColorMemberPath",
                typeof(string),
                typeof(ModernDataGridControl),
                new PropertyMetadata(string.Empty, OnRowColorMemberPathChanged));

        /// <summary>
        /// 장평(글자 가로 비율) 재정의 — 0(기본) = 전역(ModernTheme.FontWidthRatio)
        /// 사용, 양수는 0.8~1.2로 클램프. 셀/헤더/배지/버튼 캡션/상태바 텍스트에
        /// 가로 스케일이 적용되고 AutoFitColumns 측정에도 반영된다.
        /// </summary>
        public static readonly DependencyProperty FontWidthRatioProperty =
            DependencyProperty.Register(
                "FontWidthRatio",
                typeof(double),
                typeof(ModernDataGridControl),
                new PropertyMetadata(0d, OnFontWidthRatioChanged));

        /// <summary>행 선택이 바뀔 때 발생한다.</summary>
        public event EventHandler SelectionChanged;

        /// <summary>버튼 컬럼(GridColumnKind.Button) 셀을 클릭할 때 발생한다.</summary>
        public event EventHandler<GridButtonClickEventArgs> CellButtonClick;

        /// <summary>높이 변화로 표시 가능 행 수(VisibleRowCapacity)가 바뀔 때 발생한다.</summary>
        public event EventHandler VisibleRowCapacityChanged;

        // 마지막으로 통지한 표시 가능 행 수 — 같은 값이면 이벤트를 삼킨다.
        private int lastCapacity;

        // ApplyColumns로 받은 컬럼 정의 — AutoFitColumns 재계산의 원천.
        private IList<ModernDataGridColumn> columnDefinitions;

        // HeaderCheckBox 옵션이 켜진 체크박스 컬럼의 헤더 체크박스 목록 —
        // 행 값 변화(전체/일부/없음)에 맞춰 상태를 갱신하기 위해 들고 있는다.
        private readonly List<CheckBox> headerCheckBoxes = new List<CheckBox>();

        public ModernDataGridControl()
        {
            this.InitializeComponent();
            this.SizeChanged += this.OnControlSizeChanged;

            // 행 수 자동 갱신: 소스 교체/필터 변경 등으로 Items가 바뀔 때마다
            // 상태바의 카운트 텍스트를 다시 쓴다.
            ((INotifyCollectionChanged)this.InnerDataGrid.Items).CollectionChanged += this.OnItemsCollectionChanged;

            // 초기(데이터 할당 전) 빈 상태 안내를 바로 표시한다.
            this.RefreshEmptyLabel();

            // 행 색상: 행이 화면에 실체화될 때마다 RowColorMember 값으로 배경을 칠한다.
            this.InnerDataGrid.LoadingRow += this.OnLoadingRow;
        }

        /// <summary>표시할 행 목록.</summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)this.GetValue(ItemsSourceProperty); }
            set { this.SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>데이터 0건일 때의 안내 문구 (기본 "No data", 빈 문자열 = 끔).</summary>
        public string EmptyText
        {
            get { return (string)this.GetValue(EmptyTextProperty); }
            set { this.SetValue(EmptyTextProperty, value); }
        }

        /// <summary>현재 선택된 행 항목.</summary>
        public object SelectedItem
        {
            get { return this.GetValue(SelectedItemProperty); }
            set { this.SetValue(SelectedItemProperty, value); }
        }

        /// <summary>데이터 소스로부터 컬럼을 자동 생성할지 여부.</summary>
        public bool AutoGenerateColumns
        {
            get { return (bool)this.GetValue(AutoGenerateColumnsProperty); }
            set { this.SetValue(AutoGenerateColumnsProperty, value); }
        }

        /// <summary>컬럼 너비를 헤더와 데이터 내용에 맞춰 자동 계산할지 여부.</summary>
        public bool AutoFitColumns
        {
            get { return (bool)this.GetValue(AutoFitColumnsProperty); }
            set { this.SetValue(AutoFitColumnsProperty, value); }
        }

        /// <summary>그리드 하단 상태바 표시 여부.</summary>
        public bool ShowStatusBar
        {
            get { return (bool)this.GetValue(ShowStatusBarProperty); }
            set { this.SetValue(ShowStatusBarProperty, value); }
        }

        /// <summary>상태바 오른쪽에 표시할 자유 텍스트.</summary>
        public string StatusText
        {
            get { return (string)this.GetValue(StatusTextProperty); }
            set { this.SetValue(StatusTextProperty, value); }
        }

        /// <summary>상태바 왼쪽 행 수 표기 형식 ({0} = 행 수).</summary>
        public string StatusCountFormat
        {
            get { return (string)this.GetValue(StatusCountFormatProperty); }
            set { this.SetValue(StatusCountFormatProperty, value); }
        }

        /// <summary>교차 행 배경(줄무늬) 표시 여부. 기본 꺼짐.</summary>
        public bool AlternatingRowColors
        {
            get { return (bool)this.GetValue(AlternatingRowColorsProperty); }
            set { this.SetValue(AlternatingRowColorsProperty, value); }
        }

        /// <summary>행 배경색으로 쓸 컬럼/속성 이름 (선택 사항).</summary>
        public string RowColorMemberPath
        {
            get { return (string)this.GetValue(RowColorMemberPathProperty); }
            set { this.SetValue(RowColorMemberPathProperty, value); }
        }

        /// <summary>장평(글자 가로 비율) 재정의. 0 = 전역(ModernTheme.FontWidthRatio) 사용.</summary>
        public double FontWidthRatio
        {
            get { return (double)this.GetValue(FontWidthRatioProperty); }
            set { this.SetValue(FontWidthRatioProperty, value); }
        }

        /// <summary>현재 표시 중인 행 수.</summary>
        public int RowCount
        {
            get { return this.InnerDataGrid.Items.Count; }
        }

        /// <summary>
        /// 현재 높이에서 세로 스크롤 없이 표시 가능한 행 수 (최소 1).
        /// 행/헤더 높이가 토큰으로 고정되어 있어 결정적으로 계산된다.
        /// 페이지 크기를 화면 높이에 맞추는 용도(ModernPagination.PageSize 연동).
        /// </summary>
        public int VisibleRowCapacity
        {
            get
            {
                double rowHeight = (double)this.FindResource("Size.RowHeight");

                // 다중 줄 헤더로 높이가 조정됐을 수 있으므로 실제 적용 값을 쓴다.
                double headerHeight = this.InnerDataGrid.ColumnHeaderHeight;

                if (double.IsNaN(headerHeight) || headerHeight <= 0d)
                {
                    headerHeight = (double)this.FindResource("Size.GridHeaderHeight");
                }

                // 바깥 카드 테두리 위아래 1px씩 제외
                double available = this.ActualHeight - headerHeight - 2.0;

                // 상태바가 보이면 그 높이만큼 행 표시 영역이 줄어든다.
                if (this.StatusBarStrip.Visibility == Visibility.Visible)
                {
                    available = available - this.StatusBarStrip.ActualHeight;
                }

                if (available < rowHeight)
                {
                    return 1;
                }

                return (int)(available / rowHeight);
            }
        }

        private void OnControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            int capacity = this.VisibleRowCapacity;

            if (capacity == this.lastCapacity)
            {
                return;
            }

            this.lastCapacity = capacity;

            if (this.VisibleRowCapacityChanged != null)
            {
                this.VisibleRowCapacityChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>선택된 행의 인덱스(아무것도 선택되지 않았으면 -1).</summary>
        public int SelectedIndex
        {
            get { return this.InnerDataGrid.SelectedIndex; }
            set { this.InnerDataGrid.SelectedIndex = value; }
        }

        /// <summary>
        /// 컬럼을 명시적 정의로 교체하고 자동 생성을 끈다.
        /// null이나 빈 목록을 넘기면 컬럼만 비운다.
        /// </summary>
        public void ApplyColumns(IList<ModernDataGridColumn> columns)
        {
            this.AutoGenerateColumns = false;
            this.InnerDataGrid.Columns.Clear();
            this.columnDefinitions = null;
            this.headerCheckBoxes.Clear();

            if (columns == null)
            {
                this.ApplyHeaderHeight();
                return;
            }

            this.columnDefinitions = new List<ModernDataGridColumn>(columns);

            // 장평은 컬럼 구성 시점에 확정된다 — 전역/재정의 값이 바뀌면
            // OnFontWidthRatioChanged가 컬럼을 다시 만든다.
            double widthRatio = this.ResolvedFontWidthRatio();

            foreach (ModernDataGridColumn definition in columns)
            {
                // 컬럼 생성은 GridColumnFactory가 담당한다 — 리소스 조회 기준(this),
                // 클릭 핸들러, 헤더 체크박스 등록 콜백만 넘기고 결과를 받는다.
                DataGridColumn column = GridColumnFactory.CreateColumn(
                    definition,
                    widthRatio,
                    this,
                    this.OnHeaderCheckBoxClick,
                    this.OnCellCheckBoxClick,
                    this.OnCellButtonClick,
                    this.headerCheckBoxes.Add);

                // 헤더 캡션도 같은 장평으로 — 문자열 대신 스케일된 TextBlock을 쓴다.
                // (헤더가 문자열이 아닌 컬럼 — 헤더 체크박스 — 은 건드리지 않는다.)
                if (Math.Abs(widthRatio - 1d) >= 0.001 && column.Header is string)
                {
                    TextBlock headerText = new TextBlock();
                    headerText.Text = definition.HeaderText;
                    headerText.LayoutTransform = GridColumnFactory.CreateWidthTransform(widthRatio);
                    column.Header = headerText;
                }

                this.InnerDataGrid.Columns.Add(column);
            }

            // 다중 줄 헤더("\n")가 있으면 헤더 높이를 줄 수에 맞춰 늘린다.
            this.ApplyHeaderHeight();

            // 데이터가 이미 할당돼 있으면(할당 순서 내성) 즉시 자동 맞춤을 적용한다.
            this.AutoFitColumnWidths();
        }

        /// <summary>
        /// 헤더 캡션의 최대 줄 수("\n" 기준)에 맞춰 헤더 높이를 조정한다.
        /// 계산 자체(줄 수 세기 + 한 줄 높이 실측)는 GridAutoFitMeasurer가 담당한다.
        /// </summary>
        private void ApplyHeaderHeight()
        {
            this.InnerDataGrid.ColumnHeaderHeight =
                GridAutoFitMeasurer.ComputeHeaderHeight(this, this.columnDefinitions);
        }

        /// <summary>재정의 값(0 = 전역)을 유효 장평으로 해석한다.</summary>
        private double ResolvedFontWidthRatio()
        {
            return Modern.Lab.Theming.ModernTheme.ResolveFontWidthRatio(this.FontWidthRatio);
        }

        // 장평이 바뀌면 이미 구성된 컬럼을 새 비율로 다시 만들고,
        // 공용 첨부 속성도 갱신해 상태바 등 XAML 텍스트 요소에도 반영한다.
        private static void OnFontWidthRatioChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernDataGridControl control = (ModernDataGridControl)d;

            Common.FontWidthScaling.SetFontWidthRatio(control, (double)e.NewValue);

            if (control.columnDefinitions != null)
            {
                control.ApplyColumns(new List<ModernDataGridColumn>(control.columnDefinitions));
            }
        }

        // 헤더 체크박스 클릭: 현재 그리드에 표시 중인 모든 행의 값을 일괄 설정한다.
        // 페이지 화면이라면 현재 페이지 조각이 대상이다 — DataTable 소스는 행 값
        // 변경이 ColumnChanged로 전파되므로 폼의 원본 동기화 로직이 그대로 동작한다.
        private void OnHeaderCheckBoxClick(object sender, RoutedEventArgs e)
        {
            CheckBox header = (CheckBox)sender;
            string memberPath = header.Tag as string;
            bool check = header.IsChecked == true;

            foreach (object item in this.InnerDataGrid.Items)
            {
                MemberPathReader.Write(item, memberPath, check);
            }

            this.RefreshHeaderCheckBoxes();
        }

        // 셀 체크박스 클릭: 헤더 체크박스 상태(전체/일부/없음)를 다시 계산한다.
        private void OnCellCheckBoxClick(object sender, RoutedEventArgs e)
        {
            this.RefreshHeaderCheckBoxes();
        }

        // 헤더 체크박스 상태를 행 값으로부터 다시 계산한다 —
        // 전체 체크 = 체크, 전체 해제/빈 그리드 = 해제, 일부만 체크 = 중간 상태.
        private void RefreshHeaderCheckBoxes()
        {
            foreach (CheckBox header in this.headerCheckBoxes)
            {
                string memberPath = header.Tag as string;
                int total = 0;
                int checkedCount = 0;

                foreach (object item in this.InnerDataGrid.Items)
                {
                    total = total + 1;

                    object value = MemberPathReader.Read(item, memberPath);

                    if (value is bool && (bool)value)
                    {
                        checkedCount = checkedCount + 1;
                    }
                }

                if (total == 0 || checkedCount == 0)
                {
                    header.IsChecked = false;
                }
                else if (checkedCount == total)
                {
                    header.IsChecked = true;
                }
                else
                {
                    header.IsChecked = null;
                }
            }
        }

        private void OnCellButtonClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            FrameworkElement element = (FrameworkElement)sender;

            if (this.CellButtonClick != null)
            {
                this.CellButtonClick(this, new GridButtonClickEventArgs(element.DataContext, element.Tag as string));
            }
        }

        private static void OnStatusBarAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernDataGridControl)d).RefreshStatusBar();
        }

        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernDataGridControl)d).AutoFitColumnWidths();
        }

        private static void OnAutoFitColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernDataGridControl)d).AutoFitColumnWidths();
        }

        // ===== 컬럼 자동 맞춤 (헤더 + 데이터 최대 폭) =====

        // 각 컬럼 너비를 헤더/데이터 실측 폭으로 재계산해 고정 픽셀 너비로 넣는다.
        // 측정 엔진은 GridAutoFitMeasurer가 담당하고, 여기서는 적용 조건
        // (AutoFitColumns 켜짐 + 명시적 컬럼 정의 존재)만 판정한다.
        private void AutoFitColumnWidths()
        {
            if (!this.AutoFitColumns || this.columnDefinitions == null)
            {
                return;
            }

            // 장평이 적용된 텍스트는 실제 폭이 비율만큼 달라지므로 측정에도 곱한다.
            GridAutoFitMeasurer.ApplyAutoFitWidths(
                this.InnerDataGrid,
                this.columnDefinitions,
                this.ItemsSource,
                this,
                this.ResolvedFontWidthRatio());
        }

        private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.RefreshStatusBar();
            this.RefreshEmptyLabel();

            // 소스 교체/행 변경에 맞춰 헤더 체크박스(전체 선택) 상태를 되비춘다.
            this.RefreshHeaderCheckBoxes();
        }

        // 빈 상태 안내: 데이터 0건 + EmptyText 비어있지 않을 때만 표시한다.
        private void RefreshEmptyLabel()
        {
            string text = this.EmptyText;
            bool show = this.InnerDataGrid.Items.Count == 0 && !string.IsNullOrEmpty(text);

            this.EmptyLabel.Text = text;
            this.EmptyLabel.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private static void OnEmptyTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernDataGridControl)d).RefreshEmptyLabel();
        }

        // 교차색은 테마 전환을 따라가도록 리소스 참조로 걸고, 끌 때는 로컬 값을
        // 지워 XAML 기본(RowBackground 단색)으로 되돌린다.
        private static void OnAlternatingRowColorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernDataGridControl control = (ModernDataGridControl)d;

            if ((bool)e.NewValue)
            {
                control.InnerDataGrid.SetResourceReference(DataGrid.AlternatingRowBackgroundProperty, "Brush.GridRowAlt");
            }
            else
            {
                control.InnerDataGrid.ClearValue(DataGrid.AlternatingRowBackgroundProperty);
            }
        }

        private static void OnRowColorMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernDataGridControl control = (ModernDataGridControl)d;

            // 이미 실체화된 행에 새 경로를 반영하려면 다시 실체화시켜 LoadingRow를 재발생시킨다.
            if (control.InnerDataGrid.Items.Count > 0)
            {
                control.InnerDataGrid.Items.Refresh();
            }
        }

        // 각 행이 화면에 실체화될 때 배경을 칠한다. RowColorMember 값이 유효한 색이면
        // 그 색으로, 아니면 로컬 배경을 지워 그리드 기본 배경을 유지한다.
        // (경로가 비어 있으면 기존 동작 그대로 — 배경을 건드리지 않는다.)
        private void OnLoadingRow(object sender, DataGridRowEventArgs e)
        {
            string path = this.RowColorMemberPath;

            if (string.IsNullOrEmpty(path))
            {
                e.Row.ClearValue(Control.BackgroundProperty);
                return;
            }

            Color color;

            if (ChipColorHelper.TryParseColor(ToText(MemberPathReader.Read(e.Row.Item, path)), out color))
            {
                SolidColorBrush brush = new SolidColorBrush(color);
                brush.Freeze();
                e.Row.Background = brush;
            }
            else
            {
                e.Row.ClearValue(Control.BackgroundProperty);
            }
        }

        private static string ToText(object value)
        {
            return value == null ? string.Empty : value.ToString();
        }

        // 상태바 표시/텍스트를 현재 속성과 행 수로 다시 그린다.
        // 형식 문자열 오류는 형식 그대로 출력하는 것으로 완화한다(예외 없음).
        private void RefreshStatusBar()
        {
            this.StatusBarStrip.Visibility = this.ShowStatusBar ? Visibility.Visible : Visibility.Collapsed;

            if (!this.ShowStatusBar)
            {
                return;
            }

            string format = this.StatusCountFormat;
            string countText;

            try
            {
                countText = string.Format(format ?? string.Empty, this.InnerDataGrid.Items.Count);
            }
            catch (FormatException)
            {
                countText = format;
            }

            this.StatusCountText.Text = countText;
            this.StatusRightText.Text = this.StatusText ?? string.Empty;
        }

        private void InnerDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SelectionChanged != null)
            {
                this.SelectionChanged(this, EventArgs.Empty);
            }
        }
    }
}
