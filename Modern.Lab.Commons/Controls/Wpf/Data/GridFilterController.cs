using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using Modern.Lab.Controls.Wpf.Input;

namespace Modern.Lab.Controls.Wpf.Data
{
    /// <summary>
    /// ModernDataGridControl의 컬럼 값 필터 컨트롤러 — AllowColumnFilters가
    /// 켜지면 텍스트/배지 컬럼에 GridFilterAttach.Filterable을 설정해 헤더
    /// 템플릿의 깔때기 버튼(헤더 셀 맨 오른쪽 고정, 정렬 글리프 바로 다음)이
    /// 나타나고, 클릭 시 그 컬럼의 고유 값 체크리스트 팝업이 열린다. 팝업의
    /// 검색 입력은 자동완성·초성 매칭으로 체크할 값을 빠르게 찾게 한다. 체크를
    /// 바꾸는 즉시 반영(엑셀식 값 필터)되며, 필터가 걸린 컬럼은 깔때기가
    /// 액센트색(IsActive)으로 칠해진다.
    ///
    /// 필터는 데이터 소스를 바꾸지 않고 **뷰에만** 적용한다:
    /// - DataTable/DataView 소스(BindingListCollectionView)는 CustomFilter
    ///   식으로, 그 외 IList 소스는 Filter 조건자로 거른다.
    /// - 상태바 행 수/빈 안내(EmptyText)는 뷰 기준이라 자동으로 따라온다.
    /// - 선택 상태는 컬럼 이름 기준으로 유지되어 DataSource 재할당(재조회)
    ///   후에도 같은 필터가 다시 적용된다.
    /// </summary>
    internal sealed class GridFilterController
    {
        // 팝업 체크리스트에 올리는 고유 값 상한 — 넘치면 앞쪽만 보여준다.
        private const int maxDistinctValues = 500;

        private readonly DataGrid grid;
        private readonly FrameworkElement resourceSource;
        private readonly Func<IEnumerable> itemsSourceGetter;

        // 컬럼 이름 → 선택된 값 집합. 항목이 없으면 그 컬럼은 필터 없음(전체).
        private readonly Dictionary<string, HashSet<string>> selections =
                new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        // 필터 대상 컬럼 → 데이터 컬럼 이름 (팝업 대상 해석 + 활성 표시 갱신용).
        private readonly Dictionary<DataGridColumn, string> filterColumns =
                new Dictionary<DataGridColumn, string>();

        // 재사용 팝업과 현재 팝업이 다루는 컬럼/체크박스 목록.
        private Popup popup;
        private string popupColumn;
        private StackPanel popupList;
        private CheckBox popupAll;
        private ModernTextBoxControl popupSearch;
        private List<string> popupValues;
        private bool suppressPopupEvents;

        /// <summary>값 필터 상태가 바뀔 때(체크/해제/Clear) 발생한다 — 페이징
        /// 화면이 전체 결과에 같은 필터를 적용해 페이지를 재계산할 때 쓴다.</summary>
        internal event EventHandler FiltersChanged;

        /// <summary>팝업 고유 값의 원천 — 페이징 화면은 페이지 조각 대신 전체
        /// 결과(DataTable 등)를 지정한다. null이면 현재 ItemsSource에서 모은다.</summary>
        internal object ValueSource;

        internal GridFilterController(
                DataGrid grid, FrameworkElement resourceSource, Func<IEnumerable> itemsSourceGetter)
        {
            this.grid = grid;
            this.resourceSource = resourceSource;
            this.itemsSourceGetter = itemsSourceGetter;
        }

        /// <summary>행이 현재 컬럼 필터를 전부 통과하는지 판정한다 — 페이징
        /// 화면이 전체 결과를 직접 거를 때 쓴다 (필터 없으면 항상 true).</summary>
        internal bool Matches(object item)
        {
            return this.selections.Count == 0 || this.MatchesAllFilters(item);
        }

        private void RaiseFiltersChanged()
        {
            if (this.FiltersChanged != null)
            {
                this.FiltersChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>컬럼 재구성 시 컬럼 매핑을 비운다 (선택 상태는 유지).</summary>
        internal void OnColumnsRebuilt()
        {
            this.filterColumns.Clear();

            if (this.popup != null)
            {
                this.popup.IsOpen = false;
            }
        }

        /// <summary>데이터 소스가 바뀌면 유지 중인 선택을 새 뷰에 다시 적용한다.</summary>
        internal void OnItemsSourceChanged()
        {
            this.ApplyFilters();
        }

        /// <summary>
        /// 컬럼을 값 필터 대상으로 등록한다 — 체크박스/버튼 컬럼은 대상이
        /// 아니므로 건너뛴다. 깔때기 버튼 자체는 헤더 템플릿에 있고, 여기서는
        /// Filterable 첨부 속성으로 표시를 켜고 활성 상태를 되비춘다.
        /// </summary>
        internal void AttachHeader(DataGridColumn column, ModernDataGridColumn definition)
        {
            if (definition.Kind == GridColumnKind.CheckBox || definition.Kind == GridColumnKind.Button)
            {
                return;
            }

            this.filterColumns[column] = definition.DataPropertyName;
            GridFilterAttach.SetFilterable(column, true);
            this.UpdateActiveFlags(definition.DataPropertyName);
        }

        // 필터 활성 여부를 컬럼 첨부 속성(IsActive)에 되비춘다 — 헤더 템플릿이
        // 이 값으로 깔때기를 액센트색으로 칠해 걸린 컬럼이 한눈에 보인다.
        private void UpdateActiveFlags(string columnName)
        {
            bool active = this.selections.ContainsKey(columnName);

            foreach (KeyValuePair<DataGridColumn, string> entry in this.filterColumns)
            {
                if (entry.Value == columnName)
                {
                    GridFilterAttach.SetIsActive(entry.Key, active);
                }
            }
        }

        // ===== 팝업 =====

        /// <summary>헤더 깔때기 버튼 클릭 — 그 컬럼의 값 체크리스트 팝업을 연다.</summary>
        internal void OpenPopupFor(DataGridColumn column, UIElement anchor)
        {
            string columnName;

            if (column == null || !this.filterColumns.TryGetValue(column, out columnName))
            {
                return;
            }

            if (this.popup == null)
            {
                this.popup = new Popup();
                this.popup.StaysOpen = false;
                this.popup.AllowsTransparency = true;
                this.popup.Placement = PlacementMode.Bottom;
            }

            if (this.popup.IsOpen && this.popupColumn == columnName)
            {
                this.popup.IsOpen = false;
                return;
            }

            this.popupColumn = columnName;
            this.popup.Child = this.BuildPopupContent(columnName);
            this.popup.PlacementTarget = anchor;
            this.popup.IsOpen = true;
        }

        // 팝업 내용: 초성 자동완성 검색 + "(All)" + 고유 값 체크리스트(스크롤)
        // + Clear Filter. 검색은 체크 상태를 건드리지 않고 목록만 좁힌다.
        private UIElement BuildPopupContent(string columnName)
        {
            List<string> values = this.CollectDistinctValues(columnName);
            HashSet<string> selected;
            this.selections.TryGetValue(columnName, out selected);
            this.popupValues = values;

            this.suppressPopupEvents = true;

            this.popupSearch = new ModernTextBoxControl();
            this.popupSearch.Placeholder = "Search values";
            this.popupSearch.AutoCompleteItemsSource = values;
            this.popupSearch.Margin = new Thickness(0d, 0d, 0d, 8d);
            this.popupSearch.TextChanged += this.OnPopupSearchTextChanged;

            this.popupAll = new CheckBox();
            this.popupAll.Content = "(All)";
            this.popupAll.FontWeight = FontWeights.SemiBold;
            this.popupAll.Margin = new Thickness(2d, 2d, 2d, 6d);
            this.popupAll.IsChecked = selected == null;
            this.popupAll.Checked += this.OnAllToggled;
            this.popupAll.Unchecked += this.OnAllToggled;

            this.popupList = new StackPanel();
            this.RebuildPopupValueItems(selected);

            this.suppressPopupEvents = false;

            ScrollViewer scroll = new ScrollViewer();
            scroll.Content = this.popupList;
            scroll.MaxHeight = 240d;
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            Button clear = new Button();
            clear.Content = "Clear Filter";
            clear.Margin = new Thickness(0d, 8d, 0d, 0d);
            clear.Padding = new Thickness(10d, 3d, 10d, 3d);
            clear.HorizontalAlignment = HorizontalAlignment.Stretch;
            clear.Click += this.OnClearClick;

            StackPanel layout = new StackPanel();
            layout.Children.Add(this.popupSearch);
            layout.Children.Add(this.popupAll);
            layout.Children.Add(new Separator());
            layout.Children.Add(scroll);
            layout.Children.Add(clear);

            Border card = new Border();
            card.Background = (Brush)this.resourceSource.FindResource("Brush.Surface");
            card.BorderBrush = (Brush)this.resourceSource.FindResource("Brush.Border");
            card.BorderThickness = new Thickness(1d);
            card.CornerRadius = (CornerRadius)this.resourceSource.FindResource("Radius.Sm");
            card.Padding = new Thickness(10d, 8d, 10d, 8d);
            card.MinWidth = 170d;
            card.MaxWidth = 260d;
            card.Child = layout;

            System.Windows.Documents.TextElement.SetForeground(
                    card, (Brush)this.resourceSource.FindResource("Brush.TextPrimary"));
            return card;
        }

        // 검색 입력이 바뀌면 체크 상태는 유지한 채 보이는 고유 값만 다시 만든다.
        // ModernTextBoxControl이 같은 후보 목록을 제안하고, 여기서도 같은 초성
        // 매처를 써 추천 선택/직접 입력의 결과가 일관되게 보이게 한다.
        private void OnPopupSearchTextChanged(object sender, EventArgs e)
        {
            HashSet<string> selected;
            this.selections.TryGetValue(this.popupColumn, out selected);
            this.RebuildPopupValueItems(selected);
        }

        private void RebuildPopupValueItems(HashSet<string> selected)
        {
            string keyword = this.popupSearch == null ? string.Empty : this.popupSearch.Text;
            int matchCount = 0;

            this.popupList.Children.Clear();

            foreach (string value in this.popupValues)
            {
                if (!HangulTextMatcher.Contains(value, keyword))
                {
                    continue;
                }

                CheckBox item = new CheckBox();
                item.Content = value.Length == 0 ? "(Blanks)" : value;
                item.Tag = value;
                item.Margin = new Thickness(2d);
                item.IsChecked = selected == null || selected.Contains(value);
                item.Checked += this.OnValueToggled;
                item.Unchecked += this.OnValueToggled;
                this.popupList.Children.Add(item);
                matchCount = matchCount + 1;
            }

            if (matchCount == 0)
            {
                TextBlock empty = new TextBlock();
                empty.Text = "No matching values";
                empty.Margin = new Thickness(2d);
                empty.Foreground = (Brush)this.resourceSource.FindResource("Brush.TextSecondary");
                this.popupList.Children.Add(empty);
            }
        }

        // "(All)": 체크 = 필터 해제(전체), 해제 = 아무 값도 선택 안 함(전체 숨김).
        private void OnAllToggled(object sender, RoutedEventArgs e)
        {
            if (this.suppressPopupEvents)
            {
                return;
            }

            bool all = this.popupAll.IsChecked == true;
            this.suppressPopupEvents = true;

            foreach (object child in this.popupList.Children)
            {
                CheckBox item = child as CheckBox;

                if (item != null)
                {
                    item.IsChecked = all;
                }
            }

            this.suppressPopupEvents = false;

            if (all)
            {
                this.selections.Remove(this.popupColumn);
            }
            else
            {
                this.selections[this.popupColumn] = new HashSet<string>(StringComparer.Ordinal);
            }

            this.ApplyFilters();
            this.UpdateActiveFlags(this.popupColumn);
            this.RaiseFiltersChanged();
        }

        // 값 체크 변경: 검색으로 일부 값만 보일 때도 보이지 않는 값의 선택 상태는
        // 유지한다. 전체 고유 값이 체크되면 필터를 해제한다.
        private void OnValueToggled(object sender, RoutedEventArgs e)
        {
            if (this.suppressPopupEvents)
            {
                return;
            }

            HashSet<string> existing;
            HashSet<string> selected;
            this.selections.TryGetValue(this.popupColumn, out existing);

            if (existing == null)
            {
                selected = new HashSet<string>(this.popupValues, StringComparer.Ordinal);
            }
            else
            {
                selected = new HashSet<string>(existing, StringComparer.Ordinal);
            }

            foreach (object child in this.popupList.Children)
            {
                CheckBox item = child as CheckBox;

                if (item == null)
                {
                    continue;
                }

                if (item.IsChecked == true)
                {
                    selected.Add((string)item.Tag);
                }
                else
                {
                    selected.Remove((string)item.Tag);
                }
            }

            bool allChecked = selected.Count == this.popupValues.Count;

            if (allChecked)
            {
                this.selections.Remove(this.popupColumn);
            }
            else
            {
                this.selections[this.popupColumn] = selected;
            }

            this.suppressPopupEvents = true;
            this.popupAll.IsChecked = allChecked;
            this.suppressPopupEvents = false;

            this.ApplyFilters();
            this.UpdateActiveFlags(this.popupColumn);
            this.RaiseFiltersChanged();
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            this.selections.Remove(this.popupColumn);
            this.ApplyFilters();
            this.UpdateActiveFlags(this.popupColumn);
            this.RaiseFiltersChanged();
            this.popup.IsOpen = false;
        }

        // ===== 값 수집 + 필터 적용 =====

        // 컬럼의 고유 값 목록 — 다른 컬럼 필터와 무관하게 원본 전체에서 모은다.
        private List<string> CollectDistinctValues(string columnName)
        {
            HashSet<string> unique = new HashSet<string>(StringComparer.Ordinal);
            List<string> values = new List<string>();
            IEnumerable items = this.ResolveValueItems();

            if (items != null)
            {
                foreach (object item in items)
                {
                    string value = ReadValue(item, columnName);

                    if (unique.Add(value))
                    {
                        values.Add(value);

                        if (values.Count >= maxDistinctValues)
                        {
                            break;
                        }
                    }
                }
            }

            values.Sort(StringComparer.OrdinalIgnoreCase);
            return values;
        }

        // 고유 값을 모을 행 목록을 정한다 — ValueSource(전체 결과)가 지정돼
        // 있으면 그것을, 아니면 현재 ItemsSource를 쓴다. DataView는 CustomFilter가
        // 이미 걸려 있을 수 있으므로 원본 테이블에서 모은다.
        private IEnumerable ResolveValueItems()
        {
            object source = this.ValueSource;

            if (source == null)
            {
                source = this.itemsSourceGetter();
            }

            DataTable table = source as DataTable;

            if (table != null)
            {
                return table.Rows;
            }

            DataView view = source as DataView;

            if (view != null)
            {
                return view.Table.Rows;
            }

            return source as IEnumerable;
        }

        // 행(DataRow/DataRowView/POCO)에서 컬럼 값을 문자열로 읽는다.
        private static string ReadValue(object item, string columnName)
        {
            DataRow row = item as DataRow;
            DataRowView rowView = item as DataRowView;

            if (rowView != null)
            {
                row = rowView.Row;
            }

            if (row != null)
            {
                if (!row.Table.Columns.Contains(columnName))
                {
                    return string.Empty;
                }

                object cell = row[columnName];
                return cell == null || cell == DBNull.Value ? string.Empty : cell.ToString();
            }

            if (item == null)
            {
                return string.Empty;
            }

            PropertyDescriptor property = TypeDescriptor.GetProperties(item).Find(columnName, false);

            if (property == null)
            {
                return string.Empty;
            }

            object value = property.GetValue(item);
            return value == null ? string.Empty : value.ToString();
        }

        // 현재 선택을 뷰에 적용한다 — DataView는 CustomFilter 식, 그 외는
        // Filter 조건자. 소스 자체는 건드리지 않는다.
        internal void ApplyFilters()
        {
            IEnumerable source = this.itemsSourceGetter();

            if (source == null)
            {
                return;
            }

            ICollectionView view = CollectionViewSource.GetDefaultView(source);
            BindingListCollectionView bindingView = view as BindingListCollectionView;

            if (bindingView != null)
            {
                if (bindingView.CanCustomFilter)
                {
                    bindingView.CustomFilter = this.BuildDataViewFilter();
                }

                return;
            }

            if (view == null || !view.CanFilter)
            {
                return;
            }

            view.Filter = this.selections.Count == 0
                    ? (Predicate<object>)null
                    : new Predicate<object>(this.MatchesAllFilters);
            view.Refresh();
        }

        private bool MatchesAllFilters(object item)
        {
            foreach (KeyValuePair<string, HashSet<string>> entry in this.selections)
            {
                if (!entry.Value.Contains(ReadValue(item, entry.Key)))
                {
                    return false;
                }
            }

            return true;
        }

        // DataView CustomFilter 식 — 컬럼별 "값 중 하나" 절을 AND로 잇는다.
        private string BuildDataViewFilter()
        {
            if (this.selections.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder filter = new StringBuilder();

            foreach (KeyValuePair<string, HashSet<string>> entry in this.selections)
            {
                if (filter.Length > 0)
                {
                    filter.Append(" AND ");
                }

                filter.Append(BuildColumnClause(entry.Key, entry.Value));
            }

            return filter.ToString();
        }

        // 한 컬럼의 값 절 — 타입 컬럼(int 등)도 문자열로 변환해 비교하고,
        // 빈 값 선택은 NULL/빈 문자열을 함께 포함한다. 아무 값도 선택하지
        // 않았으면 모두 숨긴다.
        private static string BuildColumnClause(string columnName, HashSet<string> values)
        {
            string safeColumn = "[" + columnName.Replace("]", "]]") + "]";
            string asText = "CONVERT(" + safeColumn + ", 'System.String')";
            StringBuilder clause = new StringBuilder("(");
            bool first = true;
            bool includeBlank = false;

            foreach (string value in values)
            {
                if (value.Length == 0)
                {
                    includeBlank = true;
                    continue;
                }

                if (!first)
                {
                    clause.Append(" OR ");
                }

                clause.Append(asText).Append(" = '").Append(value.Replace("'", "''")).Append("'");
                first = false;
            }

            if (includeBlank)
            {
                if (!first)
                {
                    clause.Append(" OR ");
                }

                clause.Append(safeColumn).Append(" IS NULL OR ").Append(asText).Append(" = ''");
                first = false;
            }

            if (first)
            {
                clause.Append("1 = 0");
            }

            clause.Append(")");
            return clause.ToString();
        }
    }
}
