using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Modern.Lab.Controls.Wpf.Input
{
    /// <summary>
    /// Modern single-line text input.
    /// - Text: input text (two-way)
    /// - Placeholder: hint shown while the text is empty
    /// - IsReadOnly: read-only state
    /// - AutoCompleteItemsSource: candidate items for the suggestion dropdown
    /// - TextChanged: raised whenever Text changes
    /// - EnterPressed: raised when the Enter key is pressed (search-on-enter)
    ///
    /// IME note: placeholder visibility and suggestion filtering run off the
    /// inner TextBox.TextChanged, which fires during Hangul composition, so a
    /// single consonant already hides the placeholder and filters suggestions.
    /// (The Text DP itself updates when the composition commits — WPF defers
    /// binding source updates while a composition is active.)
    /// </summary>
    public partial class ModernTextBoxControl : UserControl
    {
        private const int MaxSuggestionCount = 8;

        /// <summary>Input text. Two-way by default.</summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(ModernTextBoxControl),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnTextPropertyChanged));

        /// <summary>Hint text shown while the input is empty.</summary>
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(
                "Placeholder",
                typeof(string),
                typeof(ModernTextBoxControl),
                new PropertyMetadata(string.Empty));

        /// <summary>Read-only state of the editor.</summary>
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(
                "IsReadOnly",
                typeof(bool),
                typeof(ModernTextBoxControl),
                new PropertyMetadata(false));

        /// <summary>
        /// Candidate items for the suggestion dropdown. Any IEnumerable; each
        /// item's ToString() is matched (contains, case-insensitive) against the
        /// typed text. Null disables autocomplete.
        /// </summary>
        public static readonly DependencyProperty AutoCompleteItemsSourceProperty =
            DependencyProperty.Register(
                "AutoCompleteItemsSource",
                typeof(IEnumerable),
                typeof(ModernTextBoxControl),
                new PropertyMetadata(null));

        private readonly ObservableCollection<string> suggestionItems;
        private bool suppressSuggestions;

        /// <summary>Raised whenever <see cref="Text"/> changes.</summary>
        public event EventHandler TextChanged;

        /// <summary>Raised when the Enter key is pressed inside the editor.</summary>
        public event EventHandler EnterPressed;

        public ModernTextBoxControl()
        {
            this.suggestionItems = new ObservableCollection<string>();
            this.InitializeComponent();
            this.SuggestionList.ItemsSource = this.suggestionItems;
        }

        /// <summary>Input text.</summary>
        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        /// <summary>Hint text shown while the input is empty.</summary>
        public string Placeholder
        {
            get { return (string)this.GetValue(PlaceholderProperty); }
            set { this.SetValue(PlaceholderProperty, value); }
        }

        /// <summary>Read-only state of the editor.</summary>
        public bool IsReadOnly
        {
            get { return (bool)this.GetValue(IsReadOnlyProperty); }
            set { this.SetValue(IsReadOnlyProperty, value); }
        }

        /// <summary>Candidate items for the suggestion dropdown. Null disables autocomplete.</summary>
        public IEnumerable AutoCompleteItemsSource
        {
            get { return (IEnumerable)this.GetValue(AutoCompleteItemsSourceProperty); }
            set { this.SetValue(AutoCompleteItemsSourceProperty, value); }
        }

        /// <summary>Moves keyboard focus into the editor.</summary>
        public void FocusEditor()
        {
            this.InnerTextBox.Focus();
        }

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernTextBoxControl control = (ModernTextBoxControl)d;

            if (control.TextChanged != null)
            {
                control.TextChanged(control, EventArgs.Empty);
            }
        }

        // Fires during IME composition too — drives the placeholder and the
        // suggestion dropdown so both react to the very first consonant.
        private void InnerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.PlaceholderText.Visibility = string.IsNullOrEmpty(this.InnerTextBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;

            if (this.suppressSuggestions)
            {
                this.suppressSuggestions = false;
                return;
            }

            this.RefreshSuggestions();
        }

        private void RefreshSuggestions()
        {
            string typed = this.InnerTextBox.Text;
            IEnumerable candidates = this.AutoCompleteItemsSource;

            this.suggestionItems.Clear();

            if (candidates == null || string.IsNullOrEmpty(typed) || this.IsReadOnly)
            {
                this.CloseSuggestions();
                return;
            }

            foreach (object candidate in candidates)
            {
                if (candidate == null)
                {
                    continue;
                }

                string candidateText = candidate.ToString();

                // Korean-aware matching: consonant jamo match syllable initials
                // (초성 검색) and IME intermediate syllables keep matching.
                if (HangulTextMatcher.Contains(candidateText, typed) &&
                    !string.Equals(candidateText, typed, StringComparison.Ordinal))
                {
                    this.suggestionItems.Add(candidateText);

                    if (this.suggestionItems.Count >= MaxSuggestionCount)
                    {
                        break;
                    }
                }
            }

            if (this.suggestionItems.Count > 0)
            {
                this.SuggestionList.SelectedIndex = -1;
                this.SuggestionPopup.IsOpen = true;
            }
            else
            {
                this.CloseSuggestions();
            }
        }

        private void CloseSuggestions()
        {
            this.SuggestionPopup.IsOpen = false;
            this.SuggestionList.SelectedIndex = -1;
        }

        // Applies a suggestion to the editor without re-opening the dropdown.
        private void CommitSuggestion(string value)
        {
            this.suppressSuggestions = true;
            this.InnerTextBox.Text = value;
            this.InnerTextBox.CaretIndex = value.Length;
            this.CloseSuggestions();
        }

        // Arrow keys navigate the open dropdown while focus stays in the editor.
        private void InnerTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!this.SuggestionPopup.IsOpen)
            {
                return;
            }

            if (e.Key == Key.Down)
            {
                if (this.SuggestionList.SelectedIndex < this.suggestionItems.Count - 1)
                {
                    this.SuggestionList.SelectedIndex = this.SuggestionList.SelectedIndex + 1;
                    this.SuggestionList.ScrollIntoView(this.SuggestionList.SelectedItem);
                }

                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                if (this.SuggestionList.SelectedIndex >= 0)
                {
                    this.SuggestionList.SelectedIndex = this.SuggestionList.SelectedIndex - 1;

                    if (this.SuggestionList.SelectedItem != null)
                    {
                        this.SuggestionList.ScrollIntoView(this.SuggestionList.SelectedItem);
                    }
                }

                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                string selected = this.SuggestionList.SelectedItem as string;

                if (selected != null)
                {
                    // Commit, then let KeyDown raise EnterPressed with the
                    // completed text (search-box behavior).
                    this.CommitSuggestion(selected);
                }
                else
                {
                    this.CloseSuggestions();
                }
            }
            else if (e.Key == Key.Escape)
            {
                this.CloseSuggestions();
                e.Handled = true;
            }
        }

        private void SuggestionItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item = (ListBoxItem)sender;
            string value = item.DataContext as string;

            if (value != null)
            {
                this.CommitSuggestion(value);
            }

            e.Handled = true;
        }

        private void InnerTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            this.CloseSuggestions();
        }

        private void InnerTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && this.EnterPressed != null)
            {
                this.EnterPressed(this, EventArgs.Empty);
            }
        }
    }
}
