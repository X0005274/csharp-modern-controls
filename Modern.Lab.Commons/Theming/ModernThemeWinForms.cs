using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace Modern.Lab.Theming
{
    /// <summary>
    /// 기존 WinForms 폼에 다크 테마를 "한 줄"로 적용하는 헬퍼.
    ///
    /// 사용법 — 각 폼 생성자에서 <c>InitializeComponent()</c> 직후:
    /// <code>
    /// Modern.Lab.Theming.ModernThemeWinForms.Apply(this);
    /// </code>
    ///
    /// 하는 일 (다크 모드일 때만 — 라이트 모드에서는 완전한 no-op이라 항상 호출해도 안전):
    ///   1. OS 타이틀바를 다크로 전환 (DWM immersive dark mode)
    ///   2. 폼 배경을 <see cref="ModernTheme.Background"/>로
    ///   3. 자식 컨트롤 전체를 재귀 순회하며, 라이트 모드에서 관행적으로 하드코딩되던
    ///      색(흰색, 247/248/250, 243/244/246, SystemColors 기본값 등)을 대응하는
    ///      다크 팔레트 색으로 치환 — <c>.Designer.cs</c>는 한 줄도 고칠 필요 없다.
    ///
    /// 치환은 "알려진 라이트 토큰 색과 정확히 일치"할 때만 일어나므로, 상태색(빨강/
    /// 초록 배지 등) 같은 의도적인 색은 건드리지 않는다. 런타임에 동적으로 추가한
    /// 컨트롤은 추가 후 <see cref="Apply(Form)"/>를 다시 호출하면 된다(폼 생성 시
    /// 1회 호출 기준으로 설계).
    /// </summary>
    public static class ModernThemeWinForms
    {
        // ---- DWM 타이틀바 다크 (Windows 10 1809+) ----

        // Windows 10 20H1(2004) 이상에서 쓰는 문서화된 속성 값.
        private const int DwmUseImmersiveDarkMode = 20;

        // Windows 10 1809~1909가 쓰던 이전 속성 값 (20이 실패하면 폴백).
        private const int DwmUseImmersiveDarkModeBefore20H1 = 19;

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        /// <summary>
        /// 화면 루트에 다크 테마를 적용한다: (Form이면) 타이틀바 + 배경 + 자식 컨트롤 색 치환.
        /// 라이트 모드에서는 아무 것도 하지 않는다.
        ///
        /// root는 Form일 필요가 없다 — 사내/서드파티 프레임워크처럼 화면이
        /// UserControl이나 자체 베이스 컨트롤 기반이어도 그대로 넘기면 된다.
        /// root가 Form이 아니면 타이틀바는 건드리지 않으므로, 최상위 폼에는
        /// <see cref="ApplyDarkTitleBar(Form)"/>를 한 번 따로 호출한다.
        /// </summary>
        public static void Apply(Control root)
        {
            if (root == null)
            {
                return;
            }

            if (!ModernTheme.IsDark)
            {
                return;
            }

            Form form = root as Form;

            if (form != null)
            {
                ApplyDarkTitleBar(form);
            }

            root.BackColor = ModernTheme.Background;
            RemapChildColors(root);
        }

        /// <summary>
        /// OS 타이틀바만 다크로 전환한다 (Apply가 내부에서 호출; 개별 사용도 가능).
        /// 핸들이 아직 없으면 핸들 생성 시점에 자동 적용된다.
        /// 라이트 모드이거나 OS가 지원하지 않으면 조용히 무시된다.
        /// </summary>
        public static void ApplyDarkTitleBar(Form form)
        {
            if (form == null)
            {
                return;
            }

            if (!ModernTheme.IsDark)
            {
                return;
            }

            if (form.IsHandleCreated)
            {
                SetDarkTitleBar(form.Handle);
            }

            // RecreateHandle 후에도 다시 적용되도록 구독은 유지한다.
            form.HandleCreated -= OnFormHandleCreated;
            form.HandleCreated += OnFormHandleCreated;
        }

        private static void OnFormHandleCreated(object sender, EventArgs e)
        {
            Form form = sender as Form;

            if (form != null)
            {
                SetDarkTitleBar(form.Handle);
            }
        }

        private static void SetDarkTitleBar(IntPtr handle)
        {
            int enabled = 1;

            try
            {
                int result = DwmSetWindowAttribute(handle, DwmUseImmersiveDarkMode, ref enabled, sizeof(int));

                if (result != 0)
                {
                    DwmSetWindowAttribute(handle, DwmUseImmersiveDarkModeBefore20H1, ref enabled, sizeof(int));
                }
            }
            catch (DllNotFoundException)
            {
                // dwmapi가 없는 환경(서버 코어 등) — 타이틀바만 라이트로 남고 앱은 정상.
            }
            catch (EntryPointNotFoundException)
            {
                // 구형 OS — 동일하게 무시.
            }
        }

        // ---- 하드코딩 라이트 색 → 다크 팔레트 치환 ----

        // 라이트 모드에서 관행적으로 쓰이던 색들의 ARGB. 정확히 일치할 때만 치환한다.
        private static readonly int LightSurfaceArgb = Color.FromArgb(255, 255, 255, 255).ToArgb();      // Color.White
        private static readonly int LightSurfaceAltArgb = Color.FromArgb(255, 249, 250, 251).ToArgb();   // SurfaceAlt(라이트)
        private static readonly int LegacyFormBackArgb = Color.FromArgb(255, 247, 248, 250).ToArgb();    // 예전 폼/캡션 배경 관행
        private static readonly int LightBackgroundArgb = Color.FromArgb(255, 243, 244, 246).ToArgb();   // Background(라이트)
        private static readonly int SystemControlArgb = SystemColors.Control.ToArgb();                   // WinForms 기본 회색

        private static readonly int LightTextPrimaryArgb = Color.FromArgb(255, 17, 24, 39).ToArgb();     // TextPrimary(라이트)
        private static readonly int LightTextSecondaryArgb = Color.FromArgb(255, 107, 114, 128).ToArgb();// TextSecondary(라이트)
        private static readonly int LightNeutralTextArgb = Color.FromArgb(255, 55, 65, 81).ToArgb();     // NeutralText(라이트)
        private static readonly int SystemControlTextArgb = SystemColors.ControlText.ToArgb();           // 기본 검정 텍스트

        private static void RemapChildColors(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                // WPF(ElementHost) 컨트롤은 Tokens.Dark.xaml이 스스로 처리한다.
                if (child is ElementHost)
                {
                    continue;
                }

                Color mappedBack;

                if (TryMapBackColor(child.BackColor, out mappedBack))
                {
                    child.BackColor = mappedBack;
                }

                Color mappedFore;

                if (TryMapForeColor(child.ForeColor, out mappedFore))
                {
                    child.ForeColor = mappedFore;
                }

                RemapChildColors(child);
            }
        }

        private static bool TryMapBackColor(Color source, out Color mapped)
        {
            // Transparent 등 알파가 있는 색은 의도된 것 — 건드리지 않는다.
            if (source.A != 255)
            {
                mapped = Color.Empty;
                return false;
            }

            int argb = source.ToArgb();

            if (argb == LightSurfaceArgb)
            {
                mapped = ModernTheme.Surface;
                return true;
            }

            if (argb == LightSurfaceAltArgb || argb == LegacyFormBackArgb)
            {
                mapped = ModernTheme.SurfaceAlt;
                return true;
            }

            if (argb == LightBackgroundArgb || argb == SystemControlArgb)
            {
                mapped = ModernTheme.Background;
                return true;
            }

            mapped = Color.Empty;
            return false;
        }

        private static bool TryMapForeColor(Color source, out Color mapped)
        {
            if (source.A != 255)
            {
                mapped = Color.Empty;
                return false;
            }

            int argb = source.ToArgb();

            if (argb == LightTextPrimaryArgb || argb == SystemControlTextArgb)
            {
                mapped = ModernTheme.TextPrimary;
                return true;
            }

            if (argb == LightTextSecondaryArgb)
            {
                mapped = ModernTheme.TextSecondary;
                return true;
            }

            if (argb == LightNeutralTextArgb)
            {
                mapped = ModernTheme.NeutralText;
                return true;
            }

            mapped = Color.Empty;
            return false;
        }
    }
}
