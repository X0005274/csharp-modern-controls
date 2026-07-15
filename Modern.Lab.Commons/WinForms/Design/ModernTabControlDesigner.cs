using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Modern.Lab.WinForms.Controls.Layout;

namespace Modern.Lab.WinForms.Design
{
    /// <summary>
    /// <see cref="ModernTabControl"/> 전용 폼 디자이너.
    ///
    /// 표준 TabControl 디자이너와 같은 사용성을 제공한다:
    /// - 스마트 태그/컨텍스트 메뉴의 "탭 추가" / "선택 탭 제거" 동사.
    /// - 디자인 타임에도 헤더 클릭으로 탭 전환 (GetHitTest로 헤더 영역의 실제
    ///   마우스 입력을 컨트롤에 통과시킨다).
    /// - 자식으로는 <see cref="ModernTabPage"/>만 허용 — 일반 컨트롤은 활성
    ///   페이지 위에 떨어뜨리면 페이지의 자식으로 들어간다.
    ///
    /// 이 컨트롤은 순수 WinForms(GDI+)라 ElementHost 계열의 디자인 타임 제약
    /// (설계 노트 1~5장)과는 무관하다 — 일반적인 컨테이너 디자이너 규칙만 따른다.
    /// </summary>
    public class ModernTabControlDesigner : ParentControlDesigner
    {
        private DesignerVerbCollection verbs;

        /// <summary>"탭 추가" / "선택 탭 제거" 디자이너 동사.</summary>
        public override DesignerVerbCollection Verbs
        {
            get
            {
                if (this.verbs == null)
                {
                    this.verbs = new DesignerVerbCollection();
                    this.verbs.Add(new DesignerVerb("탭 추가", this.OnAddTab));
                    this.verbs.Add(new DesignerVerb("선택 탭 제거", this.OnRemoveTab));
                }

                return this.verbs;
            }
        }

        /// <summary>직접 자식은 ModernTabPage만 허용한다.</summary>
        public override bool CanParent(Control control)
        {
            return control is ModernTabPage;
        }

        /// <summary>
        /// 헤더 영역의 클릭은 디자이너가 가로채지 않고 컨트롤에 넘긴다 —
        /// 디자인 타임에도 탭 전환이 동작하게 하는 표준 기법.
        /// </summary>
        protected override bool GetHitTest(Point point)
        {
            ModernTabControl tab = this.Control as ModernTabControl;

            if (tab == null)
            {
                return false;
            }

            Point client = tab.PointToClient(point);
            return tab.HeaderBounds.Contains(client);
        }

        /// <summary>새 ModernTabPage를 만들어 붙이고 선택한다 (Undo 가능한 트랜잭션).</summary>
        private void OnAddTab(object sender, EventArgs e)
        {
            IDesignerHost host = (IDesignerHost)this.GetService(typeof(IDesignerHost));

            if (host == null)
            {
                return;
            }

            ModernTabControl tab = (ModernTabControl)this.Control;

            using (DesignerTransaction transaction = host.CreateTransaction("ModernTabControl 탭 추가"))
            {
                PropertyDescriptor controlsProperty = TypeDescriptor.GetProperties(tab)["Controls"];
                this.RaiseComponentChanging(controlsProperty);

                ModernTabPage page = (ModernTabPage)host.CreateComponent(typeof(ModernTabPage));
                page.Text = page.Name;
                tab.Controls.Add(page);
                tab.SelectedIndex = tab.TabCount - 1;

                this.RaiseComponentChanged(controlsProperty, null, null);
                transaction.Commit();
            }
        }

        /// <summary>현재 선택된 페이지를 제거한다 (Undo 가능한 트랜잭션).</summary>
        private void OnRemoveTab(object sender, EventArgs e)
        {
            IDesignerHost host = (IDesignerHost)this.GetService(typeof(IDesignerHost));

            if (host == null)
            {
                return;
            }

            ModernTabControl tab = (ModernTabControl)this.Control;
            ModernTabPage page = tab.SelectedTab;

            if (page == null)
            {
                return;
            }

            using (DesignerTransaction transaction = host.CreateTransaction("ModernTabControl 탭 제거"))
            {
                PropertyDescriptor controlsProperty = TypeDescriptor.GetProperties(tab)["Controls"];
                this.RaiseComponentChanging(controlsProperty);

                tab.Controls.Remove(page);
                host.DestroyComponent(page);

                this.RaiseComponentChanged(controlsProperty, null, null);
                transaction.Commit();
            }
        }
    }
}
