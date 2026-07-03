using System;
using System.Windows.Forms;

namespace Modern.Lab.Samples
{
    /// <summary>샘플 갤러리 진입점입니다. WPF 호스팅을 위해 STA 스레드로 실행합니다.</summary>
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SampleShellForm());
        }
    }
}
