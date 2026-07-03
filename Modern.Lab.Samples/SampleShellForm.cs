using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Modern.Lab.Samples
{
    /// <summary>
    /// Shell form of the sample gallery: a left navigation strip plus a content
    /// panel. Each sample screen is an ordinary Form embedded as a non-top-level
    /// child. Register new samples with one AddSample call in RegisterSamples.
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
            this.RegisterSamples();
            this.ShowFirstSample();
        }

        private void InitializeLayout()
        {
            this.Text = "Modern.Lab Samples";
            this.ClientSize = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9f);
            this.BackColor = Color.FromArgb(247, 248, 250);

            this.navPanel = new FlowLayoutPanel();
            this.navPanel.Dock = DockStyle.Left;
            this.navPanel.Width = 200;
            this.navPanel.FlowDirection = FlowDirection.TopDown;
            this.navPanel.WrapContents = false;
            this.navPanel.Padding = new Padding(8);
            this.navPanel.BackColor = Color.FromArgb(243, 244, 246);

            this.contentPanel = new Panel();
            this.contentPanel.Dock = DockStyle.Fill;

            this.Controls.Add(this.contentPanel);
            this.Controls.Add(this.navPanel);
        }

        private void RegisterSamples()
        {
            this.AddSample("Controls Gallery", () => new ControlsGalleryForm());
            this.AddSample("직원관리", () => new EmployeeManagementForm());
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
            navButton.Click += (sender, args) => this.ShowSample(title);

            this.navPanel.Controls.Add(navButton);
        }

        private void ShowFirstSample()
        {
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

            this.contentPanel.Controls.Add(sample);
            this.currentSample = sample;
            sample.Show();
        }
    }
}
