using System.Drawing;
using System.Windows.Forms;

namespace Modern.Lab.Samples
{
    /// <summary>
    /// Shell form of the sample gallery. Sample screens are registered here as
    /// controls get added to the library.
    /// </summary>
    public class SampleShellForm : Form
    {
        public SampleShellForm()
        {
            this.InitializeLayout();
        }

        private void InitializeLayout()
        {
            this.Text = "Modern.Lab Samples";
            this.ClientSize = new Size(960, 640);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9f);
        }
    }
}
