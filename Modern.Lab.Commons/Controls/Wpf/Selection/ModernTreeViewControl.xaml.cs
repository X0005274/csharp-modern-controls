using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
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
