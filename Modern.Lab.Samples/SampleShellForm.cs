using System.Drawing;
using System.Windows.Forms;

namespace Modern.Lab.Samples
{
    /// <summary>
    /// 샘플 갤러리의 셸 폼입니다. 컨트롤이 추가되면 여기에 샘플 화면을 등록합니다.
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
