using System;
using System.Windows.Forms;

namespace Modern.Lab.Samples
{
    /// <summary>Sample entry point. Runs on an STA thread for WPF hosting.</summary>
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Shell hosts every sample screen; add new screens in
            // SampleShellForm.RegisterSamples with one AddSample call.
            Application.Run(new SampleShellForm());
        }
    }
}
