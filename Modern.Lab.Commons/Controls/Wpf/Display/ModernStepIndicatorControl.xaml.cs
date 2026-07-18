using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Modern.Lab.Controls.Wpf.Common;

namespace Modern.Lab.Controls.Wpf.Display
{
    /// <summary>
    /// 가로 진행 단계 표시(스텝 인디케이터). 공정 이벤트 흐름
    /// (예: Created → Released → JobPrep → JobStart → JobEnd, 또는 → Scrapped)을
    /// 한 줄로 보여 "지금 어디까지 왔는지"를 한눈에 파악하게 한다.
    ///
    /// - ItemsSource: 임의의 IEnumerable (DataView, IList, ...)
    /// - LabelMemberPath: 단계 이름 컬럼/속성
    /// - StateMemberPath: 단계 상태 컬럼/속성 — 값은 ModernStepState 이름 문자열
    ///   ("Completed"/"Current"/"Pending"/"Failed", 대소문자 무시)
    ///
    /// 소스나 멤버 경로가 바뀔 때마다 다시 그리므로 할당 순서는 상관없다(계약 규칙 3).
    /// 색·글리프는 전부 디자인 토큰(Themes/Tokens.xaml)에서 가져온다.
    ///
    /// 가용 폭이 부족하면 자동으로 표시를 강등한다 (스텝 수·폭 → 모드 순수 함수,
    /// 상태 없음). 어느 모드에서도 첫/현재/마지막 단계는 잘리지 않는다:
    /// 1) 균등 축소 — 스텝 폭을 118→64px까지 줄이고 레이블은 말줄임+툴팁
    /// 2) 적응 접기 — 폭 배분 우선순위: 첫/앵커±1/마지막 노드 → 앵커(현재/실패)
    ///    레이블 → 앵커 직전 "최근" 레이블(합쳐 최대 5개, 셀 92px) → 남는 폭은
    ///    앵커 주변 노드(40px)로 채우고, 안 들어가는 구간만 "⋯" 노드로 접는다
    /// 3) 압축 — 최소 셋마저 안 들어가면 레이블 전부 생략, 노드 폭을 26~40px로
    ///    줄여 하한을 실제 폭까지 내린다
    ///
    /// 가용 폭의 기준은 Measure constraint다 — ActualWidth는 ElementHost가
    /// 콘텐츠 DesiredSize보다 좁아질 때 호스트 폭을 반영하지 않으므로 쓰면 안 된다.
    /// </summary>
    public partial class ModernStepIndicatorControl : UserControl
    {
        // Segoe MDL2 Assets 글리프: 실패 X(E711).
        private const string GlyphFailed = "";
        // 완료/현재/대기 노드는 글리프 대신 단계 숫자를 그린다 — 완료는 옅은
        // 액센트 틴트 채움, 현재는 진한 액센트 채움 + 흰 숫자(농도 램프)로 구분한다.

        // ===== 폭 강등 사다리 상수 =====
        // 여유 있을 때의 스텝 폭 (기존 고정값).
        private const double fullStepWidth = 118d;
        // 균등 축소의 하한 — 이보다 좁으면 레이블이 의미를 잃어 접기로 강등.
        private const double minLabeledStepWidth = 64d;
        // 접기 모드에서 레이블 없는 노드 셀 폭.
        private const double compactStepWidth = 40d;
        // 접기 모드의 "⋯" 노드 셀 폭.
        private const double gapStepWidth = 28d;
        // 레이블 없는 압축 노드의 최소 셀 폭 (노드 원 26px).
        private const double minNodeStepWidth = 26d;
        // 접기 모드에서 레이블이 붙는 셀 폭 — 레이블은 말줄임+툴팁이므로
        // 여러 개 붙어도 노드 공간을 심하게 잠식하지 않게 좁게 잡는다.
        private const double labeledStepWidth = 92d;
        // 접기 모드에서 레이블을 유지하는 "최근 단계" 최대 개수(앵커 포함) —
        // 앵커에서 과거 방향으로, 폭이 허용하는 만큼만 붙는다.
        private const int maxRecentLabels = 5;
        // 셀 폭 대비 레이블 좌우 여백.
        private const double labelSidePadding = 6d;

        // 표시 모드 — 가용 폭과 스텝 수로 결정된다.
        private enum StepLayoutMode
        {
            Uniform,
            Collapsed
        }

        /// <summary>단계 행 목록. 임의의 IEnumerable (DataView, IList, ...).</summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
                typeof(IEnumerable),
                typeof(ModernStepIndicatorControl),
                new PropertyMetadata(null, OnDataShapeChanged));

        /// <summary>단계 이름으로 쓸 컬럼/속성 이름.</summary>
        public static readonly DependencyProperty LabelMemberPathProperty =
            DependencyProperty.Register(
                "LabelMemberPath",
                typeof(string),
                typeof(ModernStepIndicatorControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>단계 상태로 쓸 컬럼/속성 이름(값은 ModernStepState 이름 문자열).</summary>
        public static readonly DependencyProperty StateMemberPathProperty =
            DependencyProperty.Register(
                "StateMemberPath",
                typeof(string),
                typeof(ModernStepIndicatorControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        private readonly ObservableCollection<StepIndicatorItem> stepModels;

        // 소스에서 뽑아 둔 (레이블, 상태) 목록 — 리사이즈 때 소스를 다시 읽지 않는다.
        private readonly List<StepRow> parsedRows;

        // 마지막으로 적용한 레이아웃 서명 — 같은 결과면 리사이즈 중 재구성을 건너뛴다.
        private string lastLayoutSignature;

        // 마지막 Measure에서 받은 가용 폭 — 강등 판정의 기준.
        // ActualWidth를 쓰면 안 된다: ElementHost는 콘텐츠 DesiredSize가 호스트보다
        // 커지면 WPF 루트를 호스트 폭이 아닌 원하는 폭으로 배치하므로, ActualWidth가
        // 호스트 폭 아래로 줄지 않아 "폭이 충분하다"고 오판하고 오른쪽이 잘린다.
        // Measure constraint는 호스트가 주는 진짜 가용 폭이다.
        private double lastAvailableWidth;

        public ModernStepIndicatorControl()
        {
            this.stepModels = new ObservableCollection<StepIndicatorItem>();
            this.parsedRows = new List<StepRow>();
            this.InitializeComponent();
            this.StepItemsControl.ItemsSource = this.stepModels;
        }

        /// <summary>단계 행 목록.</summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)this.GetValue(ItemsSourceProperty); }
            set { this.SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>단계 이름으로 쓸 컬럼/속성 이름.</summary>
        public string LabelMemberPath
        {
            get { return (string)this.GetValue(LabelMemberPathProperty); }
            set { this.SetValue(LabelMemberPathProperty, value); }
        }

        /// <summary>단계 상태로 쓸 컬럼/속성 이름.</summary>
        public string StateMemberPath
        {
            get { return (string)this.GetValue(StateMemberPathProperty); }
            set { this.SetValue(StateMemberPathProperty, value); }
        }

        private static void OnDataShapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernStepIndicatorControl)d).RebuildSteps();
        }

        /// <summary>
        /// 호스트(ElementHost 등)가 주는 가용 폭을 Measure 시점에 받아 강등을
        /// 판정한다. 판정 결과가 바뀌면 표시 모델이 교체되고 재측정이 한 번 더
        /// 돌지만, 같은 폭 → 같은 서명이므로 즉시 수렴한다(무한 루프 없음).
        /// </summary>
        protected override Size MeasureOverride(Size constraint)
        {
            if (!double.IsInfinity(constraint.Width) && constraint.Width > 0d)
            {
                this.lastAvailableWidth = constraint.Width;
                this.ApplyLayout(constraint.Width);
            }

            return base.MeasureOverride(constraint);
        }

        // 소스 행을 (레이블, 상태) 목록으로 다시 뽑고 레이아웃을 적용한다.
        // null/빈 소스는 빈 표시로 렌더링하며 예외를 던지지 않는다.
        private void RebuildSteps()
        {
            this.parsedRows.Clear();
            this.lastLayoutSignature = null;

            IEnumerable source = this.ItemsSource;

            if (source != null)
            {
                foreach (object row in source)
                {
                    StepRow parsed = new StepRow();
                    parsed.Label = ToText(MemberPathReader.Read(row, this.LabelMemberPath));
                    parsed.State = ParseState(ToText(MemberPathReader.Read(row, this.StateMemberPath)));
                    this.parsedRows.Add(parsed);
                }
            }

            this.ApplyLayout(this.lastAvailableWidth);
        }

        // ===== 폭 강등 사다리 =====

        // 가용 폭과 스텝 수로 모드를 판정하고 표시 모델을 다시 만든다.
        // (폭, 스텝 수, 현재 위치) → 결과가 결정적이며, 서명이 같으면 건너뛴다.
        private void ApplyLayout(double availableWidth)
        {
            int count = this.parsedRows.Count;

            if (count == 0)
            {
                this.stepModels.Clear();
                this.lastLayoutSignature = "empty";
                return;
            }

            // 첫 Measure 전(폭 미상)에는 기본 폭으로 그린다 — 첫 Measure가 재판정한다.
            double width = availableWidth;

            if (width <= 0d)
            {
                width = count * fullStepWidth;
            }

            StepLayoutMode mode;
            double uniformWidth = Math.Min(fullStepWidth, Math.Floor(width / count));
            int anchorIndex = this.FindAnchorIndex();

            if (uniformWidth >= minLabeledStepWidth)
            {
                mode = StepLayoutMode.Uniform;
            }
            else
            {
                mode = StepLayoutMode.Collapsed;
                uniformWidth = compactStepWidth;
            }

            // 접기 모드 세부 — 폭 배분 우선순위:
            // ① 최소 셋(첫/앵커±1/마지막) 노드 ② 앵커 레이블 ③ 앵커 직전
            // "최근" 레이블 최대 4개 ④ 남는 폭은 앵커 주변 노드 확장.
            // 최소 셋+앵커 레이블마저 안 들어가면 레이블을 전부 포기하고
            // 노드 폭을 26~40px로 압축한다.
            List<int> visible = null;
            HashSet<int> labeled = null;
            double nodeWidth = compactStepWidth;

            if (mode == StepLayoutMode.Collapsed)
            {
                visible = BuildVisibleIndices(count, anchorIndex);
                labeled = new HashSet<int>();
                labeled.Add(anchorIndex);

                if (CollapsedTotal(visible, labeled, compactStepWidth) > width)
                {
                    labeled.Clear();
                    int gapCells = CountGaps(visible);
                    nodeWidth = Math.Max(
                            minNodeStepWidth,
                            Math.Min(compactStepWidth,
                                    Math.Floor((width - (gapCells * gapStepWidth)) / visible.Count)));
                }
                else
                {
                    ExpandRecentLabels(visible, labeled, anchorIndex, width);
                    ExpandVisibleToWidth(visible, labeled, count, anchorIndex, width);
                }
            }

            string signature = mode + ":" + uniformWidth.ToString("0") + ":" + nodeWidth.ToString("0")
                    + ":" + (labeled == null ? 0 : labeled.Count) + ":" + count + ":" + anchorIndex
                    + ":" + (visible == null ? 0 : visible.Count);

            if (signature == this.lastLayoutSignature)
            {
                return;
            }

            this.lastLayoutSignature = signature;
            this.stepModels.Clear();

            if (mode == StepLayoutMode.Uniform)
            {
                for (int index = 0; index < count; index++)
                {
                    this.stepModels.Add(this.BuildStepItem(
                            this.parsedRows[index], index, count, uniformWidth, true));
                }
            }
            else
            {
                this.BuildCollapsedItems(count, visible, labeled, nodeWidth);
            }
        }

        // "최근 레이블 창": 앵커 직전 단계들에 폭이 허용하는 만큼(최대
        // maxRecentLabels-1개) 레이블을 더 준다 — 현재 단계만이 아니라
        // "직전에 무슨 일이 있었는지"까지 글자로 읽히게 한다.
        private static void ExpandRecentLabels(
                List<int> visible, HashSet<int> labeled, int anchorIndex, double width)
        {
            for (int offset = 1; offset < maxRecentLabels; offset++)
            {
                int candidate = anchorIndex - offset;

                if (candidate < 0)
                {
                    return;
                }

                List<int> trialVisible = new List<int>(visible);

                if (!trialVisible.Contains(candidate))
                {
                    trialVisible.Add(candidate);
                    trialVisible.Sort();
                }

                HashSet<int> trialLabeled = new HashSet<int>(labeled);
                trialLabeled.Add(candidate);

                if (CollapsedTotal(trialVisible, trialLabeled, compactStepWidth) > width)
                {
                    return;
                }

                visible.Clear();
                visible.AddRange(trialVisible);
                labeled.Add(candidate);
            }
        }

        // 접힘 모드에서 실제 노드로 남길 최소 인덱스 셋: 첫/앵커-1/앵커/앵커+1/마지막.
        private static List<int> BuildVisibleIndices(int count, int anchorIndex)
        {
            List<int> visible = new List<int>();

            foreach (int candidate in new int[] { 0, anchorIndex - 1, anchorIndex, anchorIndex + 1, count - 1 })
            {
                if (candidate >= 0 && candidate < count && !visible.Contains(candidate))
                {
                    visible.Add(candidate);
                }
            }

            visible.Sort();
            return visible;
        }

        // 정렬된 표시 인덱스 목록의 필요 폭: 셀 폭 합 + 불연속 구간마다 "⋯" 노드 폭.
        private static double CollapsedTotal(
                List<int> visible, HashSet<int> labeled, double nodeWidth)
        {
            double total = CountGaps(visible) * gapStepWidth;

            foreach (int index in visible)
            {
                total += labeled.Contains(index) ? labeledStepWidth : nodeWidth;
            }

            return total;
        }

        // 정렬된 표시 인덱스 목록에서 불연속 구간(= "⋯" 노드) 수를 센다.
        private static int CountGaps(List<int> visible)
        {
            int gaps = 0;

            for (int i = 1; i < visible.Count; i++)
            {
                if (visible[i] - visible[i - 1] > 1)
                {
                    gaps = gaps + 1;
                }
            }

            return gaps;
        }

        // 남는 폭만큼 앵커 주변으로 표시 창을 넓힌다 — 좌우를 번갈아 한 칸씩
        // 추가하며, 추가해도 폭에 들어갈 때만 채택한다. 인접 노드 추가가 "⋯"
        // 구간을 닫아 폭이 오히려 줄 수 있으므로 매번 전체 폭을 재계산한다.
        private static void ExpandVisibleToWidth(
                List<int> visible, HashSet<int> labeled, int count, int anchorIndex, double width)
        {
            bool leftOpen = true;
            bool rightOpen = true;

            for (int offset = 1; offset < count && (leftOpen || rightOpen); offset++)
            {
                if (leftOpen)
                {
                    leftOpen = TryAddVisible(visible, labeled, anchorIndex - offset, count, width);
                }

                if (rightOpen)
                {
                    rightOpen = TryAddVisible(visible, labeled, anchorIndex + offset, count, width);
                }
            }
        }

        // 후보 인덱스를 표시 목록에 넣어 보고 폭에 들어가면 채택한다.
        // 반환값: 이 방향으로 계속 넓혀도 되는가 (범위 밖/폭 초과면 중단).
        private static bool TryAddVisible(
                List<int> visible, HashSet<int> labeled, int candidate, int count, double width)
        {
            if (candidate < 0 || candidate >= count)
            {
                return false;
            }

            if (visible.Contains(candidate))
            {
                return true;
            }

            List<int> trial = new List<int>(visible);
            trial.Add(candidate);
            trial.Sort();

            if (CollapsedTotal(trial, labeled, compactStepWidth) > width)
            {
                return false;
            }

            visible.Clear();
            visible.AddRange(trial);
            return true;
        }

        // 접기 모드: 계산된 표시 인덱스만 실제 노드로 남기고, 연속하지 않는
        // 구간은 "⋯" 노드 하나로 접는다. labeled에 든 인덱스(앵커 포함 최근
        // 창)만 레이블 셀이 되고, 최소 셋마저 안 들어가는 극단 폭에서는
        // labeled가 비어 전부 압축 노드가 된다.
        private void BuildCollapsedItems(
                int count, List<int> visible, HashSet<int> labeled, double nodeWidth)
        {
            int previous = -1;

            foreach (int index in visible)
            {
                if (previous >= 0 && index - previous > 1)
                {
                    this.stepModels.Add(this.BuildGapItem(previous + 1, index - 1));
                }

                bool showLabel = labeled.Contains(index);
                this.stepModels.Add(this.BuildStepItem(
                        this.parsedRows[index], index, count,
                        showLabel ? labeledStepWidth : nodeWidth, showLabel));

                previous = index;
            }
        }

        // 레이블/컴팩트/접힘 모드의 기준 단계: 현재 → 실패 → 마지막 완료 → 첫 단계 순.
        private int FindAnchorIndex()
        {
            int lastCompleted = 0;

            for (int index = 0; index < this.parsedRows.Count; index++)
            {
                if (this.parsedRows[index].State == ModernStepState.Current)
                {
                    return index;
                }

                if (this.parsedRows[index].State == ModernStepState.Completed)
                {
                    lastCompleted = index;
                }
            }

            for (int index = 0; index < this.parsedRows.Count; index++)
            {
                if (this.parsedRows[index].State == ModernStepState.Failed)
                {
                    return index;
                }
            }

            return lastCompleted;
        }

        // 단계 하나의 표시 모델을 상태와 위치(첫/마지막)로부터 만든다.
        private StepIndicatorItem BuildStepItem(StepRow row, int index, int count, double cellWidth, bool showLabel)
        {
            Brush accent = this.Brush("Brush.Accent");
            Brush onAccent = this.Brush("Brush.OnAccent");
            Brush surface = this.Brush("Brush.Surface");
            Brush selectedBackground = this.Brush("Brush.SelectedBackground");
            Brush selectedText = this.Brush("Brush.SelectedText");
            Brush border = this.Brush("Brush.Border");
            Brush textPrimary = this.Brush("Brush.TextPrimary");
            Brush textSecondary = this.Brush("Brush.TextSecondary");
            Brush connectorOff = this.Brush("Brush.BorderSubtle");
            Brush error = this.Brush("Brush.ErrorBorder");
            Brush errorText = this.Brush("Brush.ErrorText");

            FontFamily bodyFont = this.FontFamily;
            // 아이콘 폰트는 하드코딩하지 않고 토큰(Font.Family.Icon)에서 가져온다.
            FontFamily mdl2 = (FontFamily)this.FindResource("Font.Family.Icon");

            StepIndicatorItem item = new StepIndicatorItem();
            item.Label = row.Label;
            item.LabelWeight = FontWeights.Normal;
            item.CellWidth = cellWidth;
            item.LabelMaxWidth = Math.Max(24d, cellWidth - labelSidePadding);
            item.LabelVisibility = showLabel ? Visibility.Visible : Visibility.Collapsed;

            // 툴팁은 항상 전체 텍스트 — 말줄임(균등 축소)이나 레이블 숨김
            // (컴팩트/접힘)에서도 단계 이름을 확인할 수 있다.
            item.ToolTipText = string.IsNullOrEmpty(row.Label)
                    ? null
                    : (index + 1).ToString() + ". " + row.Label;

            // 진행이 이 노드에 도달했는가(왼쪽) / 통과했는가(오른쪽).
            bool reached = row.State == ModernStepState.Completed
                || row.State == ModernStepState.Current
                || row.State == ModernStepState.Failed;
            bool passed = row.State == ModernStepState.Completed;

            item.LeftConnectorBrush = reached ? accent : connectorOff;
            item.RightConnectorBrush = passed ? accent : connectorOff;
            item.LeftConnectorVisibility = index > 0 ? Visibility.Visible : Visibility.Hidden;
            item.RightConnectorVisibility = index < count - 1 ? Visibility.Visible : Visibility.Hidden;

            switch (row.State)
            {
                case ModernStepState.Completed:
                    // 완료: 옅은 액센트 틴트로 채운 원 + 진한 숫자 (테두리 없음).
                    // 지나온 단계일수록 옅고 현재가 가장 진한 "농도 램프"가
                    // 현재 단계와의 차별점이다. 색 쌍은 선택 강조 토큰을 재사용해
                    // 7종 테마 모두에서 대비가 보장된다.
                    item.NodeBackground = selectedBackground;
                    item.NodeBorderBrush = selectedBackground;
                    item.NodeForeground = selectedText;
                    item.Glyph = (index + 1).ToString();
                    item.GlyphFontFamily = bodyFont;
                    item.LabelForeground = textPrimary;
                    break;

                case ModernStepState.Current:
                    // 현재: 진한 액센트로 꽉 채운 원 + 흰 숫자 — 옅은 틴트의 완료
                    // 단계들 끝에서 가장 진한 노드라 "지금 위치"가 즉시 드러난다.
                    item.NodeBackground = accent;
                    item.NodeBorderBrush = accent;
                    item.NodeForeground = onAccent;
                    item.Glyph = (index + 1).ToString();
                    item.GlyphFontFamily = bodyFont;
                    item.LabelForeground = textPrimary;
                    item.LabelWeight = FontWeights.SemiBold;
                    break;

                case ModernStepState.Failed:
                    item.NodeBackground = error;
                    item.NodeBorderBrush = error;
                    item.NodeForeground = onAccent;
                    item.Glyph = GlyphFailed;
                    item.GlyphFontFamily = mdl2;
                    item.LabelForeground = errorText;
                    item.LabelWeight = FontWeights.SemiBold;
                    break;

                default:
                    // Pending
                    item.NodeBackground = surface;
                    item.NodeBorderBrush = border;
                    item.NodeForeground = textSecondary;
                    item.Glyph = (index + 1).ToString();
                    item.GlyphFontFamily = bodyFont;
                    item.LabelForeground = textSecondary;
                    break;
            }

            return item;
        }

        // 접힘 모드의 "⋯" 노드 — 원 없이 흐린 글리프만 그려 실제 단계와 구분한다.
        // 접힌 구간을 지나왔으면(직전 단계 완료) 연결선을 액센트로 이어 진행선이
        // 끊겨 보이지 않게 한다. 툴팁에 접힌 단계 목록을 담는다.
        private StepIndicatorItem BuildGapItem(int hiddenStart, int hiddenEnd)
        {
            Brush accent = this.Brush("Brush.Accent");
            Brush connectorOff = this.Brush("Brush.BorderSubtle");
            Brush textSecondary = this.Brush("Brush.TextSecondary");

            bool passed = this.parsedRows[hiddenStart].State == ModernStepState.Completed;
            Brush connector = passed ? accent : connectorOff;

            StepIndicatorItem item = new StepIndicatorItem();
            item.Label = string.Empty;
            item.LabelWeight = FontWeights.Normal;
            item.Glyph = "⋯";
            item.GlyphFontFamily = this.FontFamily;
            item.NodeBackground = Brushes.Transparent;
            item.NodeBorderBrush = Brushes.Transparent;
            item.NodeForeground = textSecondary;
            item.LabelForeground = textSecondary;
            item.LeftConnectorBrush = connector;
            item.RightConnectorBrush = connector;
            item.LeftConnectorVisibility = Visibility.Visible;
            item.RightConnectorVisibility = Visibility.Visible;
            item.CellWidth = gapStepWidth;
            item.LabelMaxWidth = gapStepWidth;
            item.LabelVisibility = Visibility.Collapsed;

            StringBuilder tip = new StringBuilder();
            tip.Append(hiddenEnd - hiddenStart + 1).Append(" steps");

            for (int index = hiddenStart; index <= hiddenEnd; index++)
            {
                tip.Append(index == hiddenStart ? ": " : ", ");
                tip.Append(index + 1).Append(". ").Append(this.parsedRows[index].Label);
            }

            item.ToolTipText = tip.ToString();
            return item;
        }

        private Brush Brush(string tokenKey)
        {
            return (Brush)this.FindResource(tokenKey);
        }

        private static ModernStepState ParseState(string text)
        {
            ModernStepState state;

            if (Enum.TryParse(text, true, out state))
            {
                return state;
            }

            return ModernStepState.Pending;
        }

        private static string ToText(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return value.ToString();
        }

        // 연결선 판단을 위해 개수를 먼저 알아야 하므로 임시로 모으는 경량 구조체.
        private struct StepRow
        {
            public string Label;
            public ModernStepState State;
        }
    }
}
