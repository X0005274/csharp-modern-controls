using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Modern.Lab.Controls.Wpf.Common;

namespace Modern.Lab.Controls.Wpf.Selection
{
    /// <summary>
    /// 모던 트리 뷰 (조직도·분류 계층 선택).
    ///
    /// 서버 조직 테이블 관례인 **평면 자기참조 테이블**을 그대로 받는다:
    /// - ItemsSource: 임의의 IEnumerable (DataView, IList, ...)
    /// - IdMemberPath / ParentIdMemberPath / DisplayMemberPath: 키/부모키/명칭 컬럼
    /// - 부모 키가 비어 있거나 목록에 없으면 루트 노드가 된다
    /// - SelectedValue: 선택 노드의 키 (null = 미선택). 설정 시 조상이 자동 펼쳐진다
    ///
    /// 할당 순서 내성: SelectedValue를 ItemsSource보다 먼저 설정해도 되고,
    /// 재할당 시 값이 새 트리에 없으면 미선택으로 초기화된다.
    /// </summary>
    public partial class ModernTreeViewControl : UserControl
    {
        /// <summary>트리를 구성할 행 목록 (평면 자기참조 테이블).</summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
                typeof(IEnumerable),
                typeof(ModernTreeViewControl),
                new PropertyMetadata(null, OnDataShapeChanged));

        /// <summary>노드 키 컬럼/속성 이름.</summary>
        public static readonly DependencyProperty IdMemberPathProperty =
            DependencyProperty.Register(
                "IdMemberPath",
                typeof(string),
                typeof(ModernTreeViewControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>부모 키 컬럼/속성 이름.</summary>
        public static readonly DependencyProperty ParentIdMemberPathProperty =
            DependencyProperty.Register(
                "ParentIdMemberPath",
                typeof(string),
                typeof(ModernTreeViewControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>노드 표시 텍스트 컬럼/속성 이름.</summary>
        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register(
                "DisplayMemberPath",
                typeof(string),
                typeof(ModernTreeViewControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>
        /// 노드 텍스트 색 컬럼/속성 이름 (선택 사항). 값은 "#DC2626" 같은
        /// 색 문자열이며, 비어 있거나 해석 불가하면 테마 기본색을 쓴다.
        /// </summary>
        public static readonly DependencyProperty ForeColorMemberPathProperty =
            DependencyProperty.Register(
                "ForeColorMemberPath",
                typeof(string),
                typeof(ModernTreeViewControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>
        /// 노드 글리프 컬럼/속성 이름 (선택 사항). 값은 프리셋 이름
        /// (Disc/Chip/Slice/Stack/Box/Folder/Dot, 대소문자 무시) 또는
        /// Segoe MDL2 Assets 글리프 16진 코드("E950"). 비었거나 해석 불가하면
        /// 아이콘 없이 표시한다.
        /// </summary>
        public static readonly DependencyProperty IconMemberPathProperty =
            DependencyProperty.Register(
                "IconMemberPath",
                typeof(string),
                typeof(ModernTreeViewControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>
        /// 보조 텍스트 컬럼/속성 이름 (선택 사항). 주 텍스트 뒤에 흐린 색으로
        /// 붙는다 — 모델/분류처럼 ID만으로 부족한 문맥 한 조각.
        /// </summary>
        public static readonly DependencyProperty SubTextMemberPathProperty =
            DependencyProperty.Register(
                "SubTextMemberPath",
                typeof(string),
                typeof(ModernTreeViewControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>
        /// 행 오른쪽 끝 상태 배지 텍스트 컬럼/속성 이름 (선택 사항).
        /// 값이 빈 행은 배지를 그리지 않는다.
        /// </summary>
        public static readonly DependencyProperty BadgeMemberPathProperty =
            DependencyProperty.Register(
                "BadgeMemberPath",
                typeof(string),
                typeof(ModernTreeViewControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>
        /// 배지 배경색 컬럼/속성 이름 (선택 사항). 값은 "#FEE2E2" 같은 색
        /// 문자열이고 글자색은 배경에서 자동 유도된다(그리드 배지와 동일 규칙).
        /// 비었거나 해석 불가하면 중립 회색 배지.
        /// </summary>
        public static readonly DependencyProperty BadgeColorMemberPathProperty =
            DependencyProperty.Register(
                "BadgeColorMemberPath",
                typeof(string),
                typeof(ModernTreeViewControl),
                new PropertyMetadata(string.Empty, OnDataShapeChanged));

        /// <summary>
        /// 들여쓰기 레벨마다 옅은 세로 가이드라인을 그린다 (기본 false).
        /// 3단 이상 깊은 계보에서 부모-자식 소속을 또렷하게 한다.
        /// </summary>
        public static readonly DependencyProperty ShowGuideLinesProperty =
            DependencyProperty.Register(
                "ShowGuideLines",
                typeof(bool),
                typeof(ModernTreeViewControl),
                new PropertyMetadata(false));

        /// <summary>
        /// 노드가 하나도 없을 때 가운데에 표시할 안내 문구.
        /// 기본 "No data" — 화면 문맥에 맞게 바꾸거나 빈 문자열로 끈다.
        /// </summary>
        public static readonly DependencyProperty EmptyTextProperty =
            DependencyProperty.Register(
                "EmptyText",
                typeof(string),
                typeof(ModernTreeViewControl),
                new PropertyMetadata("No data", OnEmptyTextChanged));

        /// <summary>선택 노드의 키. null은 미선택을 의미한다.</summary>
        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register(
                "SelectedValue",
                typeof(object),
                typeof(ModernTreeViewControl),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedValuePropertyChanged));

        private readonly ObservableCollection<TreeNodeItem> rootNodes;
        private readonly List<TreeNodeItem> allNodes;

        // 노드 클릭 → SelectedValue 갱신 → 노드 재적용의 순환을 막는 가드.
        private bool suppressNodeSync;

        /// <summary>선택이 바뀔 때 발생한다.</summary>
        public event EventHandler SelectedValueChanged;

        public ModernTreeViewControl()
        {
            this.rootNodes = new ObservableCollection<TreeNodeItem>();
            this.allNodes = new List<TreeNodeItem>();
            this.InitializeComponent();
            this.InnerTreeView.ItemsSource = this.rootNodes;

            // 초기(데이터 할당 전) 빈 상태 안내를 바로 표시한다.
            this.RefreshEmptyLabel();
        }

        // 빈 상태 안내: 노드 0개 + EmptyText 비어있지 않을 때만 표시한다.
        private void RefreshEmptyLabel()
        {
            string text = this.EmptyText;
            bool show = this.rootNodes.Count == 0 && !string.IsNullOrEmpty(text);

            this.EmptyLabel.Text = text;
            this.EmptyLabel.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private static void OnEmptyTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernTreeViewControl)d).RefreshEmptyLabel();
        }

        /// <summary>트리를 구성할 행 목록.</summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)this.GetValue(ItemsSourceProperty); }
            set { this.SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>노드 키 컬럼/속성 이름.</summary>
        public string IdMemberPath
        {
            get { return (string)this.GetValue(IdMemberPathProperty); }
            set { this.SetValue(IdMemberPathProperty, value); }
        }

        /// <summary>부모 키 컬럼/속성 이름.</summary>
        public string ParentIdMemberPath
        {
            get { return (string)this.GetValue(ParentIdMemberPathProperty); }
            set { this.SetValue(ParentIdMemberPathProperty, value); }
        }

        /// <summary>노드 표시 텍스트 컬럼/속성 이름.</summary>
        public string DisplayMemberPath
        {
            get { return (string)this.GetValue(DisplayMemberPathProperty); }
            set { this.SetValue(DisplayMemberPathProperty, value); }
        }

        /// <summary>노드 텍스트 색 컬럼/속성 이름 (선택 사항).</summary>
        public string ForeColorMemberPath
        {
            get { return (string)this.GetValue(ForeColorMemberPathProperty); }
            set { this.SetValue(ForeColorMemberPathProperty, value); }
        }

        /// <summary>노드 글리프 컬럼/속성 이름 (선택 사항).</summary>
        public string IconMemberPath
        {
            get { return (string)this.GetValue(IconMemberPathProperty); }
            set { this.SetValue(IconMemberPathProperty, value); }
        }

        /// <summary>보조 텍스트 컬럼/속성 이름 (선택 사항).</summary>
        public string SubTextMemberPath
        {
            get { return (string)this.GetValue(SubTextMemberPathProperty); }
            set { this.SetValue(SubTextMemberPathProperty, value); }
        }

        /// <summary>상태 배지 텍스트 컬럼/속성 이름 (선택 사항).</summary>
        public string BadgeMemberPath
        {
            get { return (string)this.GetValue(BadgeMemberPathProperty); }
            set { this.SetValue(BadgeMemberPathProperty, value); }
        }

        /// <summary>배지 배경색 컬럼/속성 이름 (선택 사항).</summary>
        public string BadgeColorMemberPath
        {
            get { return (string)this.GetValue(BadgeColorMemberPathProperty); }
            set { this.SetValue(BadgeColorMemberPathProperty, value); }
        }

        /// <summary>들여쓰기 가이드라인 표시 여부 (기본 false).</summary>
        public bool ShowGuideLines
        {
            get { return (bool)this.GetValue(ShowGuideLinesProperty); }
            set { this.SetValue(ShowGuideLinesProperty, value); }
        }

        /// <summary>노드 0개일 때의 안내 문구 (기본 "No data", 빈 문자열 = 끔).</summary>
        public string EmptyText
        {
            get { return (string)this.GetValue(EmptyTextProperty); }
            set { this.SetValue(EmptyTextProperty, value); }
        }

        /// <summary>선택 노드의 키. null은 미선택.</summary>
        public object SelectedValue
        {
            get { return this.GetValue(SelectedValueProperty); }
            set { this.SetValue(SelectedValueProperty, value); }
        }

        /// <summary>선택 노드의 원본 행 (미선택이면 null).</summary>
        public object SelectedItem
        {
            get
            {
                TreeNodeItem node = this.FindNodeByValue(this.SelectedValue);
                return node != null ? node.Row : null;
            }
        }

        /// <summary>모든 노드를 펼친다.</summary>
        public void ExpandAll()
        {
            foreach (TreeNodeItem node in this.allNodes)
            {
                node.IsExpanded = true;
            }
        }

        /// <summary>모든 노드를 접는다.</summary>
        public void CollapseAll()
        {
            foreach (TreeNodeItem node in this.allNodes)
            {
                node.IsExpanded = false;
            }
        }

        private static void OnDataShapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernTreeViewControl)d).RebuildTree();
        }

        private static void OnSelectedValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernTreeViewControl control = (ModernTreeViewControl)d;

            if (!control.suppressNodeSync)
            {
                control.ApplySelectedValueToNodes();
            }

            if (control.SelectedValueChanged != null)
            {
                control.SelectedValueChanged(control, EventArgs.Empty);
            }
        }

        // 평면 행 목록을 트리로 재구성한다. 두 단계: ① 키 → 노드 사전 구성,
        // ② 부모 키로 연결 (부모가 없거나 자기 자신이면 루트).
        // null/빈 소스와 존재하지 않는 컬럼은 빈 트리로 처리하며 예외를 던지지 않는다.
        private void RebuildTree()
        {
            foreach (TreeNodeItem existing in this.allNodes)
            {
                existing.PropertyChanged -= this.OnNodePropertyChanged;
            }

            this.rootNodes.Clear();
            this.allNodes.Clear();

            IEnumerable source = this.ItemsSource;

            if (source != null && !string.IsNullOrEmpty(this.IdMemberPath))
            {
                Dictionary<string, TreeNodeItem> nodesByKey = new Dictionary<string, TreeNodeItem>();
                List<KeyValuePair<TreeNodeItem, string>> parentKeys = new List<KeyValuePair<TreeNodeItem, string>>();

                foreach (object row in source)
                {
                    object idValue = MemberPathReader.Read(row, this.IdMemberPath);
                    string key = ToKeyString(idValue);

                    if (key.Length == 0 || nodesByKey.ContainsKey(key))
                    {
                        // 키가 없거나 중복인 행은 건너뛴다 (예외 없음).
                        continue;
                    }

                    string displayText = MemberPathReader.ReadDisplayText(row, this.DisplayMemberPath);
                    TreeNodeItem node = new TreeNodeItem(idValue, displayText, row);
                    node.PropertyChanged += this.OnNodePropertyChanged;

                    if (!string.IsNullOrEmpty(this.ForeColorMemberPath))
                    {
                        // 색 파싱은 ChipColorHelper 단일 관문 — 빈 값/해석 불가는 null로
                        // 테마 기본색을 상속한다(예외 없음).
                        node.Foreground = ChipColorHelper.TryCreateBrush(
                            ToKeyString(MemberPathReader.Read(row, this.ForeColorMemberPath)));
                    }

                    if (!string.IsNullOrEmpty(this.IconMemberPath))
                    {
                        node.IconGlyph = ResolveIconGlyph(MemberPathReader.Read(row, this.IconMemberPath));
                    }

                    if (!string.IsNullOrEmpty(this.SubTextMemberPath))
                    {
                        node.SubText = ToKeyString(MemberPathReader.Read(row, this.SubTextMemberPath));
                    }

                    if (!string.IsNullOrEmpty(this.BadgeMemberPath))
                    {
                        node.BadgeText = ToKeyString(MemberPathReader.Read(row, this.BadgeMemberPath));

                        if (!string.IsNullOrEmpty(node.BadgeText))
                        {
                            this.ApplyBadgeBrushes(node, row);
                        }
                    }

                    nodesByKey.Add(key, node);
                    this.allNodes.Add(node);

                    string parentKey = ToKeyString(MemberPathReader.Read(row, this.ParentIdMemberPath));
                    parentKeys.Add(new KeyValuePair<TreeNodeItem, string>(node, parentKey));
                }

                foreach (KeyValuePair<TreeNodeItem, string> entry in parentKeys)
                {
                    TreeNodeItem node = entry.Key;
                    string parentKey = entry.Value;
                    TreeNodeItem parent;

                    if (parentKey.Length > 0 &&
                        parentKey != ToKeyString(node.Value) &&
                        nodesByKey.TryGetValue(parentKey, out parent))
                    {
                        node.Parent = parent;
                        parent.Children.Add(node);
                    }
                    else
                    {
                        this.rootNodes.Add(node);
                    }
                }
            }

            this.RefreshEmptyLabel();

            // 보류/기존 SelectedValue 적용. 새 트리에 없는 값이면 미선택으로 정리.
            this.ApplySelectedValueToNodes();

            if (this.SelectedValue != null && this.FindNodeByValue(this.SelectedValue) == null)
            {
                this.SelectedValue = null;
            }
        }

        private void OnNodePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsSelected" || this.suppressNodeSync)
            {
                return;
            }

            TreeNodeItem node = (TreeNodeItem)sender;

            // 선택 해제(다른 노드 선택의 부수 효과)는 무시한다.
            if (!node.IsSelected)
            {
                return;
            }

            this.suppressNodeSync = true;

            try
            {
                this.SelectedValue = node.Value;
            }
            finally
            {
                this.suppressNodeSync = false;
            }
        }

        private void ApplySelectedValueToNodes()
        {
            object selected = this.SelectedValue;
            TreeNodeItem target = this.FindNodeByValue(selected);

            this.suppressNodeSync = true;

            try
            {
                foreach (TreeNodeItem node in this.allNodes)
                {
                    node.IsSelected = ReferenceEquals(node, target);
                }

                // 선택 노드가 보이도록 조상을 모두 펼친다.
                TreeNodeItem ancestor = target != null ? target.Parent : null;

                while (ancestor != null)
                {
                    ancestor.IsExpanded = true;
                    ancestor = ancestor.Parent;
                }
            }
            finally
            {
                this.suppressNodeSync = false;
            }
        }

        private TreeNodeItem FindNodeByValue(object value)
        {
            if (value == null)
            {
                return null;
            }

            string key = ToKeyString(value);

            foreach (TreeNodeItem node in this.allNodes)
            {
                if (ToKeyString(node.Value) == key)
                {
                    return node;
                }
            }

            return null;
        }

        // ===== 노드 글리프 프리셋 =====
        // 이름은 도메인 중립(공개 라이브러리)으로 두고, 앱이 자기 도메인 용어를
        // 프리셋에 매핑해 쓴다. Segoe MDL2 Assets 글리프는 대조표 렌더로 선정:
        // Disc = 얇은 원 테두리(원판형 소재), Chip = 핀 달린 IC(칩/다이),
        // Slice = 모서리 잘린 얇은 사각(절단 박편/시편), Stack = 적층(묶음/로트),
        // Box = 보관 상자(캐리어), Folder = 분류, Dot = 중립 마커.
        private static readonly Dictionary<string, string> iconPresets =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Disc", "" },
                { "Chip", "" },
                { "Slice", "" },
                { "Stack", "" },
                { "Box", "" },
                { "Folder", "" },
                { "Dot", "" }
            };

        // 아이콘 값 해석: 프리셋 이름(대소문자 무시) 또는 4자리 16진 글리프 코드.
        // 빈 값/해석 불가는 빈 문자열 — 아이콘 없이 표시한다(예외 없음).
        private static string ResolveIconGlyph(object iconValue)
        {
            string text = ToKeyString(iconValue);

            if (text.Length == 0)
            {
                return string.Empty;
            }

            string preset;

            if (iconPresets.TryGetValue(text, out preset))
            {
                return preset;
            }

            int code;

            if (text.Length == 4 && int.TryParse(
                    text,
                    System.Globalization.NumberStyles.HexNumber,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out code))
            {
                return ((char)code).ToString();
            }

            return string.Empty;
        }

        // 배지 배경/글자 브러시: 그리드 배지와 동일 규칙 — 색 문자열이 있으면
        // 그 배경 + 자동 유도 글자색(ChipColorHelper), 없으면 중립 회색 배지.
        private void ApplyBadgeBrushes(TreeNodeItem node, object row)
        {
            Color background;
            string colorText = string.IsNullOrEmpty(this.BadgeColorMemberPath)
                    ? string.Empty
                    : ToKeyString(MemberPathReader.Read(row, this.BadgeColorMemberPath));

            if (colorText.Length > 0 && ChipColorHelper.TryParseColor(colorText, out background))
            {
                SolidColorBrush backgroundBrush = new SolidColorBrush(background);
                backgroundBrush.Freeze();
                SolidColorBrush foregroundBrush = new SolidColorBrush(ChipColorHelper.DeriveForeground(background));
                foregroundBrush.Freeze();

                node.BadgeBackground = backgroundBrush;
                node.BadgeForeground = foregroundBrush;
                return;
            }

            node.BadgeBackground = (Brush)this.FindResource("Brush.HoverBackground");
            node.BadgeForeground = (Brush)this.FindResource("Brush.TextSecondary");
        }

        // 키 비교는 문자열 기준으로 한다 — DataTable의 박싱 타입 차이
        // (int vs string 등)에 관대해지기 위함.
        private static string ToKeyString(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            return value.ToString().Trim();
        }
    }
}
