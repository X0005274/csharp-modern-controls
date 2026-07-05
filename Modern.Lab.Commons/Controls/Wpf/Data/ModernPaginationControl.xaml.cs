using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Modern.Lab.Controls.Wpf.Data
{
    /// <summary>
    /// 모던 페이지 바 (서버 페이징용).
    /// - TotalCount / PageSize: 전체 건수와 페이지당 건수 → 페이지 수 자동 계산
    /// - CurrentPage: 현재 페이지 (1부터; 범위를 벗어나면 자동 보정)
    /// - PageChanged: 현재 페이지가 바뀔 때 발생 — 폼이 해당 페이지를 조회한다
    ///
    /// 페이지 번호는 현재 페이지를 중심으로 최대 7개가 표시된다.
    /// </summary>
    public partial class ModernPaginationControl : UserControl
    {
        private const int MaxVisiblePages = 7;

        /// <summary>전체 건수. 페이지 수 계산과 "총 N건" 표시에 쓰인다.</summary>
        public static readonly DependencyProperty TotalCountProperty =
            DependencyProperty.Register(
                "TotalCount",
                typeof(int),
                typeof(ModernPaginationControl),
                new PropertyMetadata(0, OnPagingShapeChanged));

        /// <summary>페이지당 건수. 1 미만은 1로 보정된다.</summary>
        public static readonly DependencyProperty PageSizeProperty =
            DependencyProperty.Register(
                "PageSize",
                typeof(int),
                typeof(ModernPaginationControl),
                new PropertyMetadata(20, OnPagingShapeChanged, CoercePageSize));

        /// <summary>총 건수 표기 형식. {0}에 전체 건수가 들어간다.</summary>
        public static readonly DependencyProperty TotalCountFormatProperty =
            DependencyProperty.Register(
                "TotalCountFormat",
                typeof(string),
                typeof(ModernPaginationControl),
                new PropertyMetadata("총 {0:N0}건", OnPagingShapeChanged));

        /// <summary>현재 페이지 (1부터). 범위를 벗어나면 자동 보정된다.</summary>
        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register(
                "CurrentPage",
                typeof(int),
                typeof(ModernPaginationControl),
                new FrameworkPropertyMetadata(
                    1,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnCurrentPageChanged,
                    CoerceCurrentPage));

        private readonly ObservableCollection<PaginationPageItem> pageItems;

        /// <summary>현재 페이지가 바뀔 때 발생한다 (버튼 클릭·코드 할당 공통).</summary>
        public event EventHandler PageChanged;

        public ModernPaginationControl()
        {
            this.pageItems = new ObservableCollection<PaginationPageItem>();
            this.InitializeComponent();
            this.PageItemsControl.ItemsSource = this.pageItems;
            this.RebuildPages();
        }

        /// <summary>전체 건수.</summary>
        public int TotalCount
        {
            get { return (int)this.GetValue(TotalCountProperty); }
            set { this.SetValue(TotalCountProperty, value); }
        }

        /// <summary>페이지당 건수.</summary>
        public int PageSize
        {
            get { return (int)this.GetValue(PageSizeProperty); }
            set { this.SetValue(PageSizeProperty, value); }
        }

        /// <summary>총 건수 표기 형식 ({0} = 전체 건수).</summary>
        public string TotalCountFormat
        {
            get { return (string)this.GetValue(TotalCountFormatProperty); }
            set { this.SetValue(TotalCountFormatProperty, value); }
        }

        /// <summary>현재 페이지 (1부터).</summary>
        public int CurrentPage
        {
            get { return (int)this.GetValue(CurrentPageProperty); }
            set { this.SetValue(CurrentPageProperty, value); }
        }

        /// <summary>전체 페이지 수 (최소 1).</summary>
        public int PageCount
        {
            get
            {
                int pageSize = Math.Max(1, this.PageSize);
                int pages = (this.TotalCount + pageSize - 1) / pageSize;
                return Math.Max(1, pages);
            }
        }

        private static object CoercePageSize(DependencyObject d, object baseValue)
        {
            return Math.Max(1, (int)baseValue);
        }

        private static object CoerceCurrentPage(DependencyObject d, object baseValue)
        {
            ModernPaginationControl control = (ModernPaginationControl)d;
            int value = (int)baseValue;

            if (value < 1)
            {
                return 1;
            }

            if (value > control.PageCount)
            {
                return control.PageCount;
            }

            return value;
        }

        private static void OnPagingShapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernPaginationControl control = (ModernPaginationControl)d;

            // 전체 건수/페이지 크기가 바뀌면 현재 페이지를 새 범위로 보정한다.
            control.CoerceValue(CurrentPageProperty);
            control.RebuildPages();
        }

        private static void OnCurrentPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernPaginationControl control = (ModernPaginationControl)d;

            control.RebuildPages();

            if (control.PageChanged != null)
            {
                control.PageChanged(control, EventArgs.Empty);
            }
        }

        // 총 건수 표시, 이전/다음 활성 상태, 페이지 번호 버튼(슬라이딩 윈도)을 갱신한다.
        private void RebuildPages()
        {
            int pageCount = this.PageCount;
            int current = this.CurrentPage;

            // 총 건수 텍스트 — 형식 문자열 오류는 형식 그대로 출력하는 것으로 완화한다.
            string totalFormat = this.TotalCountFormat;

            try
            {
                this.TotalText.Text = string.Format(CultureInfo.InvariantCulture, totalFormat ?? string.Empty, this.TotalCount);
            }
            catch (FormatException)
            {
                this.TotalText.Text = totalFormat;
            }

            this.PrevButton.IsEnabled = current > 1;
            this.NextButton.IsEnabled = current < pageCount;

            // 현재 페이지를 가운데 두는 최대 7개 윈도
            int start = current - (MaxVisiblePages / 2);

            if (start > pageCount - MaxVisiblePages + 1)
            {
                start = pageCount - MaxVisiblePages + 1;
            }

            if (start < 1)
            {
                start = 1;
            }

            int end = Math.Min(pageCount, start + MaxVisiblePages - 1);

            this.pageItems.Clear();

            for (int number = start; number <= end; number++)
            {
                this.pageItems.Add(new PaginationPageItem(number, number == current));
            }
        }

        private void PageButton_Click(object sender, RoutedEventArgs e)
        {
            PaginationPageItem item = ((Button)sender).DataContext as PaginationPageItem;

            if (item != null)
            {
                this.CurrentPage = item.Number;
            }
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            this.CurrentPage = this.CurrentPage - 1;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            this.CurrentPage = this.CurrentPage + 1;
        }
    }
}
