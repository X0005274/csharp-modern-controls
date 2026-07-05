using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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
                new PropertyMetadata(null));

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

        /// <summary>행 선택이 바뀔 때 발생한다.</summary>
        public event EventHandler SelectionChanged;

        /// <summary>높이 변화로 표시 가능 행 수(VisibleRowCapacity)가 바뀔 때 발생한다.</summary>
        public event EventHandler VisibleRowCapacityChanged;

        // 마지막으로 통지한 표시 가능 행 수 — 같은 값이면 이벤트를 삼킨다.
        private int lastCapacity;

        public ModernDataGridControl()
        {
            this.InitializeComponent();
            this.SizeChanged += this.OnControlSizeChanged;

            // 행 수 자동 갱신: 소스 교체/필터 변경 등으로 Items가 바뀔 때마다
            // 상태바의 카운트 텍스트를 다시 쓴다.
            ((INotifyCollectionChanged)this.InnerDataGrid.Items).CollectionChanged += this.OnItemsCollectionChanged;
        }

        /// <summary>표시할 행 목록.</summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)this.GetValue(ItemsSourceProperty); }
            set { this.SetValue(ItemsSourceProperty, value); }
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
                double headerHeight = (double)this.FindResource("Size.GridHeaderHeight");

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

            if (columns == null)
            {
                return;
            }

            foreach (ModernDataGridColumn definition in columns)
            {
                this.InnerDataGrid.Columns.Add(CreateColumn(definition));
            }
        }

        private static DataGridTextColumn CreateColumn(ModernDataGridColumn definition)
        {
            DataGridTextColumn column = new DataGridTextColumn();
            column.Header = definition.HeaderText;

            Binding binding = new Binding(definition.DataPropertyName);

            // 표시 형식: "N0" 같은 단순 형식은 "{0:N0}"으로 해석된다(WPF 규칙).
            // 원본이 타입 컬럼(int/decimal/DateTime)일 때만 효과가 있다.
            if (!string.IsNullOrEmpty(definition.Format))
            {
                binding.StringFormat = definition.Format;
            }

            column.Binding = binding;

            if (definition.Width > 0d)
            {
                column.Width = new DataGridLength(definition.Width);
            }
            else
            {
                column.Width = new DataGridLength(1d, DataGridLengthUnitType.Star);
            }

            if (definition.TextAlignment != GridTextAlignment.Left)
            {
                TextAlignment alignment = definition.TextAlignment == GridTextAlignment.Center
                    ? TextAlignment.Center
                    : TextAlignment.Right;

                Style elementStyle = new Style(typeof(TextBlock));
                elementStyle.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, alignment));
                column.ElementStyle = elementStyle;
            }

            return column;
        }

        private static void OnStatusBarAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernDataGridControl)d).RefreshStatusBar();
        }

        private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.RefreshStatusBar();
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
