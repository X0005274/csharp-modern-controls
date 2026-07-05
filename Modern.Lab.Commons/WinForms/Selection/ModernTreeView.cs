using System;
using System.ComponentModel;
using System.Drawing;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Selection
{
    /// <summary>
    /// System.Windows.Forms.TreeView의 대체 컨트롤 (조직도·분류 계층 선택;
    /// WPF ModernTreeViewControl을 ElementHost로 호스팅).
    ///
    /// TreeNode를 코드로 쌓는 대신, 서버 조직 테이블(평면 자기참조:
    /// 키/부모키/명칭)을 DataSource로 그대로 할당한다. 부모 키가 비어 있거나
    /// 목록에 없는 행이 루트가 된다.
    /// </summary>
    [ToolboxItem(true)]
    public class ModernTreeView : WpfElementHostBase<Modern.Lab.Controls.Wpf.Selection.ModernTreeViewControl>
    {
        private object dataSource;

        // 디자인 타임 WPF 생성이 실패한 경우(Wpf == null)에도 속성 그리드가
        // 동작하도록 하는 폴백 저장소.
        private string fallbackIdMember;
        private string fallbackParentIdMember;
        private string fallbackDisplayMember;
        private string fallbackForeColorMember;

        /// <summary>선택 노드가 바뀔 때 발생한다.</summary>
        public event EventHandler SelectedValueChanged;

        /// <summary>적절한 기본 크기로 컨트롤을 생성한다.</summary>
        public ModernTreeView()
        {
            this.Size = new Size(200, 300);
            this.fallbackIdMember = string.Empty;
            this.fallbackParentIdMember = string.Empty;
            this.fallbackDisplayMember = string.Empty;
            this.fallbackForeColorMember = string.Empty;

            if (this.Wpf != null)
            {
                this.Wpf.SelectedValueChanged += this.OnWpfSelectedValueChanged;
            }
        }

        /// <summary>
        /// 트리를 구성할 행 목록: 키/부모키/명칭 컬럼을 가진 DataTable, DataView,
        /// IList 또는 임의의 IEnumerable. null이면 트리를 비운다.
        /// 재할당 시 기존 SelectedValue가 새 트리에 없으면 미선택으로 초기화된다.
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

        /// <summary>노드 키로 사용할 컬럼/속성 이름.</summary>
        [Category("모던 컨트롤")]
        [Description("노드 키로 사용할 컬럼/속성 이름")]
        [DefaultValue("")]
        public string IdMember
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.IdMemberPath;
                }

                return this.fallbackIdMember;
            }
            set
            {
                this.fallbackIdMember = value;

                if (this.Wpf != null)
                {
                    this.Wpf.IdMemberPath = value;
                }
            }
        }

        /// <summary>부모 키로 사용할 컬럼/속성 이름.</summary>
        [Category("모던 컨트롤")]
        [Description("부모 키로 사용할 컬럼/속성 이름")]
        [DefaultValue("")]
        public string ParentIdMember
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.ParentIdMemberPath;
                }

                return this.fallbackParentIdMember;
            }
            set
            {
                this.fallbackParentIdMember = value;

                if (this.Wpf != null)
                {
                    this.Wpf.ParentIdMemberPath = value;
                }
            }
        }

        /// <summary>노드 표시 텍스트로 사용할 컬럼/속성 이름(WinForms 호환 이름).</summary>
        [Category("모던 컨트롤")]
        [Description("노드 표시 텍스트로 사용할 컬럼/속성 이름")]
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

        /// <summary>
        /// 노드 텍스트 색으로 사용할 컬럼/속성 이름 (선택 사항).
        /// 값은 "#DC2626" 같은 색 문자열 — 비어 있거나 해석 불가면 기본색.
        /// 상태(Scrap 등)에 따라 노드를 색으로 구분할 때 쓴다.
        /// </summary>
        [Category("모던 컨트롤")]
        [Description("노드 텍스트 색 컬럼/속성 이름 — 값은 #RRGGBB 색 문자열")]
        [DefaultValue("")]
        public string ForeColorMember
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.ForeColorMemberPath;
                }

                return this.fallbackForeColorMember;
            }
            set
            {
                this.fallbackForeColorMember = value;

                if (this.Wpf != null)
                {
                    this.Wpf.ForeColorMemberPath = value;
                }
            }
        }

        /// <summary>
        /// 선택 노드의 키 (IdMember 기준). null = 미선택.
        /// DataSource보다 먼저 설정해도 트리 구성 시 적용되고,
        /// 설정 시 선택 노드가 보이도록 조상이 자동 펼쳐진다.
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
                if (this.Wpf != null)
                {
                    this.Wpf.SelectedValue = value;
                }
            }
        }

        /// <summary>선택 노드의 원본 행 (DataRowView 등). 미선택이면 null.</summary>
        [Browsable(false)]
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
        }

        /// <summary>모든 노드를 펼친다.</summary>
        public void ExpandAll()
        {
            if (this.Wpf != null)
            {
                this.Wpf.ExpandAll();
            }
        }

        /// <summary>모든 노드를 접는다.</summary>
        public void CollapseAll()
        {
            if (this.Wpf != null)
            {
                this.Wpf.CollapseAll();
            }
        }

        private void OnWpfSelectedValueChanged(object sender, EventArgs e)
        {
            if (this.SelectedValueChanged != null)
            {
                this.SelectedValueChanged(this, EventArgs.Empty);
            }
        }
    }
}
