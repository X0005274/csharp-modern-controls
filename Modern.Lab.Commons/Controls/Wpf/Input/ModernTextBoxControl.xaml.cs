using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Modern.Lab.Controls.Wpf.Input
{
    /// <summary>
    /// 모던 한 줄 텍스트 입력.
    /// - Text: 입력 텍스트 (양방향)
    /// - Placeholder: 텍스트가 비어 있는 동안 표시되는 힌트
    /// - IsReadOnly: 읽기 전용 상태
    /// - CharacterCasing: 입력 즉시 대문자/소문자 강제 (코드/ID 입력용)
    /// - AllowedCharacters: 허용 문자 집합 — 그 외 문자는 입력/붙여넣기 모두 제거
    /// - AutoCompleteItemsSource: 추천 드롭다운의 후보 항목
    /// - TextChanged: Text가 바뀔 때마다 발생
    /// - EnterPressed: Enter 키를 눌렀을 때 발생 (엔터로 검색)
    ///
    /// IME 참고: placeholder 표시와 추천 필터링은 내부 TextBox.TextChanged에서
    /// 동작하는데, 이 이벤트는 한글 조합 중에도 발생하므로 자음 하나만 입력해도
    /// placeholder가 숨겨지고 추천이 필터링된다.
    /// (Text DP 자체는 조합이 확정될 때 갱신된다 — WPF는 조합이 진행 중인 동안
    /// 바인딩 소스 업데이트를 미룬다.)
    /// </summary>
    public partial class ModernTextBoxControl : UserControl
    {
        // 후보 개수의 안전 상한 — 화면 표시 제한이 아니다. 매칭된 후보는 모두
        // 목록에 담기고 드롭다운의 MaxHeight + 스크롤로 보여준다. 이 값은
        // 로컬 소스가 비정상적으로 클 때(수만 건) UI가 멈추는 것만 막는다.
        private const int MaxSuggestionCount = 100;

        /// <summary>입력 텍스트. 기본적으로 양방향 바인딩.</summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(ModernTextBoxControl),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnTextPropertyChanged));

        /// <summary>입력이 비어 있는 동안 표시되는 힌트 텍스트.</summary>
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(
                "Placeholder",
                typeof(string),
                typeof(ModernTextBoxControl),
                new PropertyMetadata(string.Empty));

        /// <summary>에디터의 읽기 전용 상태.</summary>
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(
                "IsReadOnly",
                typeof(bool),
                typeof(ModernTextBoxControl),
                new PropertyMetadata(false));

        /// <summary>입력 즉시 대문자/소문자로 강제 변환 (기본 Normal = 변환 없음).</summary>
        public static readonly DependencyProperty CharacterCasingProperty =
            DependencyProperty.Register(
                "CharacterCasing",
                typeof(CharacterCasing),
                typeof(ModernTextBoxControl),
                new PropertyMetadata(CharacterCasing.Normal, OnCharacterCasingChanged));

        /// <summary>
        /// 허용 문자 집합. 비어 있으면 제한 없음. 지정하면 타이핑·붙여넣기·IME를
        /// 가리지 않고 집합에 없는 문자가 제거된다 (예: 대문자 ID 입력용
        /// "ABC...XYZ0123456789.-").
        /// </summary>
        public static readonly DependencyProperty AllowedCharactersProperty =
            DependencyProperty.Register(
                "AllowedCharacters",
                typeof(string),
                typeof(ModernTextBoxControl),
                new PropertyMetadata(string.Empty));

        /// <summary>필수 입력 필드 표시 — 필드 왼쪽에 빨간 세로 바를 그린다.</summary>
        public static readonly DependencyProperty RequiredProperty =
            DependencyProperty.Register(
                "Required",
                typeof(bool),
                typeof(ModernTextBoxControl),
                new PropertyMetadata(false));

        /// <summary>
        /// 추천 드롭다운의 후보 항목. 임의의 IEnumerable; 각 항목의 ToString()이
        /// 입력한 텍스트와 매칭된다(contains, 대소문자 무시).
        /// null이면 자동완성이 비활성화된다.
        /// </summary>
        public static readonly DependencyProperty AutoCompleteItemsSourceProperty =
            DependencyProperty.Register(
                "AutoCompleteItemsSource",
                typeof(IEnumerable),
                typeof(ModernTextBoxControl),
                new PropertyMetadata(null, OnAutoCompleteItemsSourceChanged));

        private readonly ObservableCollection<string> suggestionItems;
        private bool suppressSuggestions;

        /// <summary><see cref="Text"/>가 바뀔 때마다 발생한다.</summary>
        public event EventHandler TextChanged;

        /// <summary>에디터 안에서 Enter 키를 눌렀을 때 발생한다.</summary>
        public event EventHandler EnterPressed;

        public ModernTextBoxControl()
        {
            this.suggestionItems = new ObservableCollection<string>();
            this.InitializeComponent();
            this.SuggestionList.ItemsSource = this.suggestionItems;
        }

        /// <summary>입력 텍스트.</summary>
        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        /// <summary>입력이 비어 있는 동안 표시되는 힌트 텍스트.</summary>
        public string Placeholder
        {
            get { return (string)this.GetValue(PlaceholderProperty); }
            set { this.SetValue(PlaceholderProperty, value); }
        }

        /// <summary>에디터의 읽기 전용 상태.</summary>
        public bool IsReadOnly
        {
            get { return (bool)this.GetValue(IsReadOnlyProperty); }
            set { this.SetValue(IsReadOnlyProperty, value); }
        }

        /// <summary>입력 즉시 대문자/소문자로 강제 변환 (기본 Normal).</summary>
        public CharacterCasing CharacterCasing
        {
            get { return (CharacterCasing)this.GetValue(CharacterCasingProperty); }
            set { this.SetValue(CharacterCasingProperty, value); }
        }

        /// <summary>허용 문자 집합 (빈 문자열 = 제한 없음).</summary>
        public string AllowedCharacters
        {
            get { return (string)this.GetValue(AllowedCharactersProperty); }
            set { this.SetValue(AllowedCharactersProperty, value); }
        }

        /// <summary>필수 입력 필드 표시(필드 왼쪽 빨간 세로 바).</summary>
        public bool Required
        {
            get { return (bool)this.GetValue(RequiredProperty); }
            set { this.SetValue(RequiredProperty, value); }
        }

        /// <summary>추천 드롭다운의 후보 항목. null이면 자동완성이 비활성화된다.</summary>
        public IEnumerable AutoCompleteItemsSource
        {
            get { return (IEnumerable)this.GetValue(AutoCompleteItemsSourceProperty); }
            set { this.SetValue(AutoCompleteItemsSourceProperty, value); }
        }

        /// <summary>키보드 포커스를 에디터로 이동한다.</summary>
        public void FocusEditor()
        {
            this.InnerTextBox.Focus();
        }

        // 대소문자 강제는 WPF TextBox 자체 기능(CharacterCasing)에 위임한다 —
        // 입력 시점에 변환되므로 TextChanged에는 이미 변환된 텍스트가 들어온다.
        private static void OnCharacterCasingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernTextBoxControl control = (ModernTextBoxControl)d;
            control.InnerTextBox.CharacterCasing = (CharacterCasing)e.NewValue;
        }

        // 후보 목록이 입력 중에 비동기로 도착하는 시나리오(서버 typeahead) 지원:
        // 에디터에 포커스가 있는 동안 소스를 재할당하면 추천을 즉시 다시 필터링한다.
        // 포커스가 없으면(코드로 텍스트/소스만 바꾼 경우) 드롭다운을 열지 않는다.
        private static void OnAutoCompleteItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernTextBoxControl control = (ModernTextBoxControl)d;

            if (control.InnerTextBox.IsKeyboardFocused)
            {
                control.RefreshSuggestions();
            }
        }

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernTextBoxControl control = (ModernTextBoxControl)d;

            if (control.TextChanged != null)
            {
                control.TextChanged(control, EventArgs.Empty);
            }
        }

        // IME 조합 중에도 발생한다 — placeholder와 추천 드롭다운을 구동하여
        // 둘 다 첫 자음 입력부터 반응하게 한다.
        private void InnerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 허용 문자 제한: 텍스트가 정리되면 중첩 TextChanged가 나머지를
            // 처리하므로 이번 호출은 여기서 끝낸다.
            if (this.EnforceAllowedCharacters())
            {
                return;
            }

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

        // AllowedCharacters에 없는 문자를 제거하고 캐럿 위치를 보정한다.
        // 타이핑·붙여넣기·IME 확정 텍스트가 모두 TextChanged를 지나므로
        // 이 한 곳에서 전부 걸러진다. 변경이 있었으면 true.
        private bool EnforceAllowedCharacters()
        {
            string allowed = this.AllowedCharacters;
            string text = this.InnerTextBox.Text;

            if (string.IsNullOrEmpty(allowed) || text.Length == 0)
            {
                return false;
            }

            StringBuilder filtered = new StringBuilder(text.Length);
            int caret = this.InnerTextBox.CaretIndex;
            int newCaret = caret;

            for (int index = 0; index < text.Length; index++)
            {
                char current = text[index];

                if (allowed.IndexOf(current) >= 0)
                {
                    filtered.Append(current);
                }
                else if (index < caret)
                {
                    newCaret = newCaret - 1;
                }
            }

            if (filtered.Length == text.Length)
            {
                return false;
            }

            this.InnerTextBox.Text = filtered.ToString();
            this.InnerTextBox.CaretIndex = Math.Max(0, newCaret);
            return true;
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

                // 한국어 인식 매칭: 자음 자모는 음절의 초성과 매칭되고(초성 검색)
                // IME 조합 중간 음절도 계속 매칭된다.
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

        // 드롭다운을 다시 열지 않고 추천 항목을 에디터에 적용한다.
        private void CommitSuggestion(string value)
        {
            this.suppressSuggestions = true;
            this.InnerTextBox.Text = value;
            this.InnerTextBox.CaretIndex = value.Length;
            this.CloseSuggestions();
        }

        // 방향키로 열린 드롭다운을 탐색하는 동안 포커스는 에디터에 남는다.
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
                    // 먼저 확정한 뒤, KeyDown이 완성된 텍스트로 EnterPressed를
                    // 발생시키게 한다(검색창 동작).
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
