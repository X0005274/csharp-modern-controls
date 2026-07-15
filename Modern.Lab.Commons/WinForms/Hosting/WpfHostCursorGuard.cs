using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace Modern.Lab.WinForms.Controls.Hosting
{
    /// <summary>
    /// 폼 단위 커서 방어 헬퍼. 컨트롤 트리를 내려가며 모든 ElementHost의
    /// Cursor 속성 매핑을 제거한다 — <see cref="WpfHostOptions.DisableCursorPropertyMap"/>
    /// 전역 플래그의 폼 단위 버전으로, 특정 화면에만 선별 적용하거나 이미
    /// Wait가 박힌 화면의 응급 처치로 쓴다 (제거 이후 첫 마우스 이동부터
    /// WPF가 커서를 다시 관리해 정상으로 돌아온다).
    ///
    /// <c>ModernThemeWinForms.Apply(this)</c>처럼 폼 생성자에서
    /// <c>InitializeComponent()</c> 직후 호출한다. Apply 호출 이후에 동적으로
    /// 추가된 컨트롤은 커버하지 못하므로, 전체 적용이 목적이면 전역 플래그를
    /// 권장한다.
    /// </summary>
    public static class WpfHostCursorGuard
    {
        /// <summary>root 이하 모든 ElementHost의 Cursor 매핑을 제거한다.</summary>
        public static void Apply(Control root)
        {
            if (root == null)
            {
                return;
            }

            ElementHost host = root as ElementHost;
            if (host != null)
            {
                RemoveCursorMapping(host);
            }

            foreach (Control child in root.Controls)
            {
                Apply(child);
            }
        }

        /// <summary>
        /// ElementHost 하나의 Cursor 매핑을 제거하고, 매핑이 이미 WPF 쪽에
        /// 복사해 둔 커서 잔류(Child와 내부 호스트 컨테이너의 Cursor/ForceCursor)도
        /// 함께 걷어낸다 — 이미 Wait가 박힌 화면의 응급 처치가 가능한 이유.
        /// </summary>
        public static void RemoveCursorMapping(ElementHost host)
        {
            if (host == null || host.PropertyMap == null)
            {
                return;
            }

            host.PropertyMap.Remove("Cursor");

            if (host.Child != null)
            {
                host.Child.ClearValue(System.Windows.FrameworkElement.CursorProperty);
                host.Child.ClearValue(System.Windows.FrameworkElement.ForceCursorProperty);

                // 기본 매핑은 Child가 아니라 내부 호스트 컨테이너(어댑터)에 커서를
                // 쓰므로, Child의 비주얼 부모까지 함께 초기화한다.
                System.Windows.FrameworkElement adapter =
                    System.Windows.Media.VisualTreeHelper.GetParent(host.Child)
                        as System.Windows.FrameworkElement;
                if (adapter != null)
                {
                    adapter.ClearValue(System.Windows.FrameworkElement.CursorProperty);
                    adapter.ClearValue(System.Windows.FrameworkElement.ForceCursorProperty);
                }
            }
        }
    }
}
