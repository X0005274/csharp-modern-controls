using System.ComponentModel;
using System.Drawing;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Display
{
    /// <summary>
    /// Small statistic card (replaces a hand-built Label pair; WPF
    /// ModernKpiCardControl hosted through ElementHost).
    ///
    /// Set Title once and update Value after each query
    /// (e.g. Value = grid.RowCount.ToString()).
    /// </summary>
    [ToolboxItem(true)]
    public class ModernKpiCard : WpfElementHostBase<Modern.Lab.Controls.Wpf.Display.ModernKpiCardControl>
    {
        // Fallback storage so the property grid still works when design-time
        // WPF construction failed (Wpf == null).
        private string fallbackTitle;
        private string fallbackValue;

        /// <summary>Creates the control with a sensible default size.</summary>
        public ModernKpiCard()
        {
            this.Size = new Size(160, 72);
            this.fallbackTitle = "제목";
            this.fallbackValue = "0";
        }

        /// <summary>Caption above the value.</summary>
        [Category("모던 컨트롤")]
        [Description("값 위에 표시할 제목")]
        [Localizable(true)]
        [DefaultValue("제목")]
        public string Title
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.Title;
                }

                return this.fallbackTitle;
            }
            set
            {
                this.fallbackTitle = value;

                if (this.Wpf != null)
                {
                    this.Wpf.Title = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>Highlighted value text.</summary>
        [Category("모던 컨트롤")]
        [Description("강조 표시할 값 텍스트")]
        [DefaultValue("0")]
        public string Value
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.Value;
                }

                return this.fallbackValue;
            }
            set
            {
                this.fallbackValue = value;

                if (this.Wpf != null)
                {
                    this.Wpf.Value = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }
    }
}
