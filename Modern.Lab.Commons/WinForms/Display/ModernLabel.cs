using System.ComponentModel;
using System.Drawing;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Display
{
    /// <summary>
    /// Drop-in replacement for System.Windows.Forms.Label
    /// (WPF ModernLabelControl hosted through ElementHost).
    ///
    /// Compatible members: Text (override, localizable), Enabled.
    /// Unlike Label there is no AutoSize; give the control an explicit size
    /// (long text is trimmed with an ellipsis).
    /// </summary>
    [ToolboxItem(true)]
    public class ModernLabel : WpfElementHostBase<Modern.Lab.Controls.Wpf.Display.ModernLabelControl>
    {
        // Fallback storage so the property grid still works when design-time
        // WPF construction failed (Wpf == null).
        private string fallbackText;
        private Modern.Lab.Controls.Wpf.Display.LabelKind fallbackKind;
        private bool fallbackRequired;

        /// <summary>Creates the control with a sensible default size.</summary>
        public ModernLabel()
        {
            this.Size = new Size(120, 24);
            this.fallbackText = "레이블";
            this.fallbackKind = Modern.Lab.Controls.Wpf.Display.LabelKind.Body;
            this.fallbackRequired = false;
        }

        /// <summary>Shows a red asterisk after the text (required-field mark).</summary>
        [Category("모던 컨트롤")]
        [Description("필수 입력 표시(빨간 별표)를 텍스트 뒤에 붙일지 여부")]
        [DefaultValue(false)]
        public bool Required
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.Required;
                }

                return this.fallbackRequired;
            }
            set
            {
                this.fallbackRequired = value;

                if (this.Wpf != null)
                {
                    this.Wpf.Required = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>Text to display.</summary>
        [Category("모던 컨트롤")]
        [Description("표시할 텍스트")]
        [Browsable(true)]
        [Localizable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue("레이블")]
        public override string Text
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.Text;
                }

                return this.fallbackText;
            }
            set
            {
                this.fallbackText = value;

                if (this.Wpf != null)
                {
                    this.Wpf.Text = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>Typography role (Body/Title/Label/Helper).</summary>
        [Category("모던 컨트롤")]
        [Description("타이포그래피 역할(Body/Title/Label/Helper)")]
        [DefaultValue(Modern.Lab.Controls.Wpf.Display.LabelKind.Body)]
        public Modern.Lab.Controls.Wpf.Display.LabelKind Kind
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.Kind;
                }

                return this.fallbackKind;
            }
            set
            {
                this.fallbackKind = value;

                if (this.Wpf != null)
                {
                    this.Wpf.Kind = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }
    }
}
