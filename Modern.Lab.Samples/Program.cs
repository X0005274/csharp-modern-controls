using System;
using System.Windows.Forms;

namespace Modern.Lab.Samples
{
    /// <summary>샘플 진입점. WPF 호스팅을 위해 STA 스레드에서 실행된다.</summary>
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 테마 확인용: "--dark" 또는 "--theme=<이름>"
            // (dark/orangeblue/greentomato/crimsongray/blue/lightpurple) 인자로 실행하면 해당 테마로
            // 뜬다. 테마는 반드시 첫 컨트롤(폼) 생성 전에 설정해야 한다 —
            // 실사용 앱도 Program.cs의 이 위치에서 설정하면 된다.
            foreach (string arg in args)
            {
                if (string.Equals(arg, "--dark", StringComparison.OrdinalIgnoreCase))
                {
                    Modern.Lab.Theming.ModernTheme.Mode = Modern.Lab.Theming.ModernTheme.ThemeMode.Dark;
                }
                else if (string.Equals(arg, "--cursor-guard", StringComparison.OrdinalIgnoreCase))
                {
                    // 커서 방어 확인용: 호스트 폼의 Wait 커서가 WPF 콘텐츠로
                    // 복사되지 않는지 검증할 때 켠다 (기본 off).
                    Modern.Lab.WinForms.Controls.Hosting.WpfHostOptions.DisableCursorPropertyMap = true;
                }
                else if (arg.StartsWith("--theme=", StringComparison.OrdinalIgnoreCase))
                {
                    string name = arg.Substring("--theme=".Length);
                    Modern.Lab.Theming.ModernTheme.ThemeMode mode;
                    if (Enum.TryParse(name, true, out mode))
                    {
                        Modern.Lab.Theming.ModernTheme.Mode = mode;
                    }
                }
                else if (arg.StartsWith("--fontwidth=", StringComparison.OrdinalIgnoreCase))
                {
                    // 장평 확인용: "--fontwidth=0.9"처럼 실행하면 전역 장평이
                    // 적용된다 (허용 0.8~1.2로 클램프). 테마와 마찬가지로 첫 컨트롤
                    // 생성 전에 설정해야 한다.
                    string ratioText = arg.Substring("--fontwidth=".Length);
                    double ratio;
                    if (double.TryParse(ratioText, System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out ratio))
                    {
                        Modern.Lab.Theming.ModernTheme.FontWidthRatio = ratio;
                    }
                }
            }

            // 셸이 모든 샘플 화면을 호스팅한다; 새 화면은
            // SampleShellForm.RegisterSamples에서 AddSample 호출 하나로 추가한다.
            Application.Run(new SampleShellForm());
        }
    }
}
