using System;
using System.Windows.Forms;

namespace Modern.Lab.Samples
{
    /// <summary>Sample gallery entry point. Runs on an STA thread for WPF hosting.</summary>
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
