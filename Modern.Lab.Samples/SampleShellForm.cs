using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Modern.Lab.Samples
{
    /// <summary>
    /// 샘플 갤러리의 셸 폼: 왼쪽 내비게이션 스트립과 콘텐츠 패널로 구성된다.
    /// 각 샘플 화면은 최상위가 아닌 자식으로 임베드되는 평범한 Form이다.
    /// 새 샘플은 RegisterSamples에서 AddSample 호출 하나로 등록한다.
    /// </summary>
    public class SampleShellForm : Form
    {
        private FlowLayoutPanel navPanel;
        private Panel contentPanel;
        private Form currentSample;
        private readonly Dictionary<string, Func<Form>> sampleFactories;

        public SampleShellForm()
        {
            this.sampleFactories = new Dictionary<string, Func<Form>>();
            this.InitializeLayout();

            // 다크 테마 한 줄 적용 (타이틀바 + 폼 배경 + 하드코딩 라이트 색 치환).
            // 라이트 모드에서는 no-op — 실사용 폼도 이 위치에 이 한 줄이면 된다.
            Modern.Lab.Theming.ModernThemeWinForms.Apply(this);

            this.RegisterSamples();
            this.AddThemeSwitcher();
            this.ShowFirstSample();
        }

        /// <summary>
        /// 내비 하단의 테마 전환 버튼들. 라이브 전환은 미지원(WPF StaticResource가
        /// 로드 시 확정)이므로, 선택한 테마 인자로 같은 exe를 재실행하고 현재 창을
        /// 닫는 "재시작 방식"으로 전환한다 — 실사용 앱의 설정 저장 후 재시작과 동일한 UX.
        /// </summary>
        private void AddThemeSwitcher()
        {
            Panel spacer = new Panel();
            spacer.Size = new Size(180, 14);
            this.navPanel.Controls.Add(spacer);

            Label caption = new Label();
            caption.Text = "Theme (restart)";
            caption.AutoSize = false;
            caption.Size = new Size(180, 20);
            caption.Font = new Font("Segoe UI Semibold", 8.5f);
            caption.ForeColor = Modern.Lab.Theming.ModernTheme.TextSecondary;
            this.navPanel.Controls.Add(caption);

            foreach (Modern.Lab.Theming.ModernTheme.ThemeMode mode
                    in Enum.GetValues(typeof(Modern.Lab.Theming.ModernTheme.ThemeMode)))
            {
                bool current = mode == Modern.Lab.Theming.ModernTheme.Mode;

                Button themeButton = new Button();
                themeButton.Text = (current ? "●  " : "    ") + mode.ToString();
                themeButton.Width = this.navPanel.Width - 20;
                themeButton.Height = 28;
                themeButton.FlatStyle = FlatStyle.Flat;
                themeButton.FlatAppearance.BorderSize = 0;
                themeButton.TextAlign = ContentAlignment.MiddleLeft;
                themeButton.BackColor = Color.Transparent;
                themeButton.ForeColor = current
                        ? Modern.Lab.Theming.ModernTheme.Accent
                        : Modern.Lab.Theming.ModernTheme.TextPrimary;
                themeButton.Enabled = !current;

                Modern.Lab.Theming.ModernTheme.ThemeMode captured = mode;
                themeButton.Click += (sender, args) => { this.RestartWithTheme(captured); };
                this.navPanel.Controls.Add(themeButton);
            }
        }

        /// <summary>선택 테마 인자로 새 프로세스를 띄우고 현재 갤러리를 닫는다.</summary>
        private void RestartWithTheme(Modern.Lab.Theming.ModernTheme.ThemeMode mode)
        {
            System.Diagnostics.Process.Start(
                    Application.ExecutablePath,
                    "--theme=" + mode.ToString().ToLowerInvariant());
            this.Close();
        }

        private void InitializeLayout()
        {
            this.Text = "Modern.Lab Samples";

            // 회사 실사용 영역(약 1700×800)에 맞춘 크기 — 내비 200을 제외한
            // 콘텐츠 영역이 1540×약 800이 되도록.
            this.ClientSize = new Size(1740, 840);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9f);

            // 폼/내비 배경은 하드코딩 대신 ModernTheme 팔레트에서 —
            // 일반 WinForms 폼은 자동으로 어두워지지 않으므로 이렇게 직접 칠한다.
            this.BackColor = Modern.Lab.Theming.ModernTheme.SurfaceAlt;

            this.navPanel = new FlowLayoutPanel();
            this.navPanel.Dock = DockStyle.Left;
            this.navPanel.Width = 200;
            this.navPanel.FlowDirection = FlowDirection.TopDown;
            this.navPanel.WrapContents = false;
            this.navPanel.Padding = new Padding(8);
            this.navPanel.BackColor = Modern.Lab.Theming.ModernTheme.Background;

            this.contentPanel = new Panel();
            this.contentPanel.Dock = DockStyle.Fill;

            this.Controls.Add(this.contentPanel);
            this.Controls.Add(this.navPanel);
        }

        private void RegisterSamples()
        {
            // 각 샘플 화면을 여기서 AddSample 호출 하나로 등록한다.
            this.AddSample("Item History", () => new ItemHistoryForm());
            this.AddSample("Logistics & Request", () => new LogisticsRequestForm());
            this.AddSample("Equipment / Lots", () => new EquipmentLotForm());
            this.AddSample("Carrier Editor", () => new CarrierEditForm());
            this.AddSample("직원관리", () => new EmployeeManagementForm());
            this.AddSample("Step Indicator", () => new StepFlowDemoForm());
        }

        private void AddSample(string title, Func<Form> factory)
        {
            this.sampleFactories[title] = factory;

            Button navButton = new Button();
            navButton.Text = title;
            navButton.Width = this.navPanel.Width - 20;
            navButton.Height = 36;
            navButton.FlatStyle = FlatStyle.Flat;
            navButton.FlatAppearance.BorderSize = 0;
            navButton.TextAlign = ContentAlignment.MiddleLeft;
            navButton.BackColor = Color.Transparent;
            navButton.ForeColor = Modern.Lab.Theming.ModernTheme.TextPrimary;
            navButton.Click += (sender, args) => this.ShowSample(title);

            this.navPanel.Controls.Add(navButton);
        }

        private void ShowFirstSample()
        {
            // "--sample=<제목>" 인자로 특정 샘플을 바로 열 수 있다 (테스트/시연용).
            if (!string.IsNullOrEmpty(Program.StartupSample)
                    && this.sampleFactories.ContainsKey(Program.StartupSample))
            {
                this.ShowSample(Program.StartupSample);
                return;
            }

            foreach (KeyValuePair<string, Func<Form>> entry in this.sampleFactories)
            {
                this.ShowSample(entry.Key);
                return;
            }
        }

        private void ShowSample(string title)
        {
            Func<Form> factory;

            if (!this.sampleFactories.TryGetValue(title, out factory))
            {
                return;
            }

            if (this.currentSample != null)
            {
                this.contentPanel.Controls.Remove(this.currentSample);
                this.currentSample.Dispose();
                this.currentSample = null;
            }

            Form sample = factory();
            sample.TopLevel = false;
            sample.FormBorderStyle = FormBorderStyle.None;
            sample.Dock = DockStyle.Fill;

            // 다크 테마 한 줄 적용: 샘플 폼의 .Designer.cs에 직렬화된 라이트 색
            // (폼 배경, 캡션 라벨 배경 등)을 다크 팔레트로 치환한다 — 실사용 폼은
            // 각 폼 생성자에서 InitializeComponent() 직후 이 한 줄이면 된다.
            Modern.Lab.Theming.ModernThemeWinForms.Apply(sample);

            // 오픈 시 깜빡임(ElementHost 생성·그리드 AutoFit 중간 레이아웃)은
            // 각 폼이 생성자에서 ModernLoadCover.Attach(this) 한 줄로 스스로
            // 가린다 — 여는 쪽(이 셸이든 회사 메인 프레임이든)은 손대지 않는다.
            this.contentPanel.Controls.Add(sample);
            this.currentSample = sample;
            sample.Show();
        }
    }
}
