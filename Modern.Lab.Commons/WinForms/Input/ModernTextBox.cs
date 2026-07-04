using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Input
{
    /// <summary>
    /// Drop-in replacement for System.Windows.Forms.TextBox
    /// (WPF ModernTextBoxControl hosted through ElementHost).
    ///
    /// Compatible members: Text (override, localizable), TextChanged (the
    /// standard WinForms event, raised when the inner WPF text changes),
    /// ReadOnly, Enabled, AutoCompleteMode/AutoCompleteSource/
    /// AutoCompleteCustomSource (search-box style suggestion dropdown; every
    /// non-None mode behaves as Suggest, and only CustomSource is supported).
    /// Additional members: PlaceholderText, EnterPressed (search-on-enter; the
    /// WinForms KeyDown event does not fire for keys handled inside the hosted
    /// WPF editor).
    /// </summary>
    [ToolboxItem(true)]
    public class ModernTextBox : WpfElementHostBase<Modern.Lab.Controls.Wpf.Input.ModernTextBoxControl>
    {
        // Fallback storage so the property grid still works when design-time
        // WPF construction failed (Wpf == null).
        private string fallbackText;
        private string fallbackPlaceholder;
        private bool fallbackReadOnly;

        private AutoCompleteMode autoCompleteMode;
        private AutoCompleteSource autoCompleteSource;
        private AutoCompleteStringCollection autoCompleteCustomSource;

        /// <summary>Raised when the Enter key is pressed inside the editor.</summary>
        public event EventHandler EnterPressed;

        /// <summary>Creates the control with a sensible default size.</summary>
        public ModernTextBox()
        {
            this.Size = new Size(200, 32);
            this.fallbackText = string.Empty;
            this.fallbackPlaceholder = string.Empty;
            this.fallbackReadOnly = false;
            this.autoCompleteMode = AutoCompleteMode.None;
            this.autoCompleteSource = AutoCompleteSource.None;
            this.autoCompleteCustomSource = null;

            if (this.Wpf != null)
            {
                this.Wpf.TextChanged += this.OnWpfTextChanged;
                this.Wpf.EnterPressed += this.OnWpfEnterPressed;
            }
        }

        /// <summary>Input text.</summary>
        [Category("모던 컨트롤")]
        [Description("입력하거나 표시할 텍스트")]
        [Browsable(true)]
        [Localizable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue("")]
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

        /// <summary>Hint text shown while the input is empty.</summary>
        [Category("모던 컨트롤")]
        [Description("입력이 비어 있을 때 표시할 힌트 텍스트")]
        [Localizable(true)]
        [DefaultValue("")]
        public string PlaceholderText
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.Placeholder;
                }

                return this.fallbackPlaceholder;
            }
            set
            {
                this.fallbackPlaceholder = value;

                if (this.Wpf != null)
                {
                    this.Wpf.Placeholder = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>Read-only state (same semantics as TextBox.ReadOnly).</summary>
        [Category("모던 컨트롤")]
        [Description("읽기 전용 여부")]
        [DefaultValue(false)]
        public bool ReadOnly
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.IsReadOnly;
                }

                return this.fallbackReadOnly;
            }
            set
            {
                this.fallbackReadOnly = value;

                if (this.Wpf != null)
                {
                    this.Wpf.IsReadOnly = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>
        /// Autocomplete behavior (WinForms-compatible name). Any value other
        /// than None enables the suggestion dropdown (Suggest behavior).
        /// </summary>
        [Category("모던 컨트롤")]
        [Description("자동완성 동작 — None 외의 값은 모두 제안 목록(Suggest)으로 동작")]
        [DefaultValue(AutoCompleteMode.None)]
        public AutoCompleteMode AutoCompleteMode
        {
            get
            {
                return this.autoCompleteMode;
            }
            set
            {
                this.autoCompleteMode = value;
                this.ApplyAutoComplete();
            }
        }

        /// <summary>
        /// Autocomplete source (WinForms-compatible name). Only CustomSource is
        /// supported; other values disable the dropdown.
        /// </summary>
        [Category("모던 컨트롤")]
        [Description("자동완성 원본 — CustomSource만 지원")]
        [DefaultValue(AutoCompleteSource.None)]
        public AutoCompleteSource AutoCompleteSource
        {
            get
            {
                return this.autoCompleteSource;
            }
            set
            {
                this.autoCompleteSource = value;
                this.ApplyAutoComplete();
            }
        }

        /// <summary>
        /// Custom autocomplete candidates (WinForms-compatible name and type).
        /// Assign after filling; re-assign to refresh the candidates.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AutoCompleteStringCollection AutoCompleteCustomSource
        {
            get
            {
                return this.autoCompleteCustomSource;
            }
            set
            {
                this.autoCompleteCustomSource = value;
                this.ApplyAutoComplete();
            }
        }

        // Pushes the effective candidate list to the WPF control. Order-tolerant:
        // Mode/Source/CustomSource may be assigned in any order (contract rule 3).
        private void ApplyAutoComplete()
        {
            if (this.Wpf == null)
            {
                return;
            }

            bool enabled =
                this.autoCompleteMode != AutoCompleteMode.None &&
                this.autoCompleteSource == AutoCompleteSource.CustomSource &&
                this.autoCompleteCustomSource != null;

            this.Wpf.AutoCompleteItemsSource = enabled ? this.autoCompleteCustomSource : null;
        }

        // Raises the standard WinForms TextChanged so existing handler wiring
        // (this.textBox.TextChanged += ...) keeps working after the swap.
        private void OnWpfTextChanged(object sender, EventArgs e)
        {
            this.OnTextChanged(EventArgs.Empty);
        }

        private void OnWpfEnterPressed(object sender, EventArgs e)
        {
            if (this.EnterPressed != null)
            {
                this.EnterPressed(this, EventArgs.Empty);
            }
        }
    }
}
