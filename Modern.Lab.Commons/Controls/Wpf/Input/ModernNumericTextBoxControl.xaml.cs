using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Modern.Lab.Controls.Wpf.Input
{
    /// <summary>
    /// 모던 숫자 입력 필드 (금액/수량용 마스크 입력).
    ///
    /// - 숫자만 치면 천단위 콤마가 자동으로 붙는다 (구분자는 코드가 삽입).
    /// - 중간 위치를 수정해도 즉시 재형식화되며 입력이 막히지 않는다.
    /// - DecimalPlaces > 0 이면 소수점, AllowNegative면 맨 앞 음수 기호를 허용한다.
    /// - 포커스가 떠날 때 확정 값 기준의 정규 형식(소수 자릿수 패딩)으로 정리된다.
    ///
    /// Value = null 은 "미입력(전체)"을 의미한다.
    /// </summary>
    public partial class ModernNumericTextBoxControl : UserControl
    {
        /// <summary>입력된 값. null은 미입력(전체 조회)을 의미한다.</summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(decimal?),
                typeof(ModernNumericTextBoxControl),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnValueChanged));

        /// <summary>허용할 소수 자릿수. 0이면 정수만 입력된다.</summary>
        public static readonly DependencyProperty DecimalPlacesProperty =
            DependencyProperty.Register(
                "DecimalPlaces",
                typeof(int),
                typeof(ModernNumericTextBoxControl),
                new PropertyMetadata(0));

        /// <summary>음수 입력 허용 여부 (맨 앞 '-').</summary>
        public static readonly DependencyProperty AllowNegativeProperty =
            DependencyProperty.Register(
                "AllowNegative",
                typeof(bool),
                typeof(ModernNumericTextBoxControl),
                new PropertyMetadata(true));

        /// <summary>입력 전(빈 필드) 회색으로 표시되는 안내 텍스트.</summary>
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(
                "Placeholder",
                typeof(string),
                typeof(ModernNumericTextBoxControl),
                new PropertyMetadata(string.Empty));

        /// <summary>필수 입력 필드 표시 — 값이 비어 있는 동안 필드에 빨간 점을 표시한다.</summary>
        public static readonly DependencyProperty RequiredProperty =
            DependencyProperty.Register(
                "Required",
                typeof(bool),
                typeof(ModernNumericTextBoxControl),
                new PropertyMetadata(false));

        /// <summary>값이 바뀔 때 발생한다 (래퍼가 ValueChanged로 재노출).</summary>
        public event EventHandler ValueChanged;

        // TextChanged 재형식화 → Text 재할당 → TextChanged 재진입을 막는 가드.
        private bool updatingText;

        // 텍스트 입력이 Value를 갱신하는 동안, DP 콜백이 에디터 텍스트를
        // 다시 덮어써 캐럿이 튀는 것을 막는 가드.
        private bool syncingFromText;

        // 마지막으로 ValueChanged를 통지한 값 — 같은 값이면 이벤트를 삼킨다.
        private decimal? lastRaisedValue;

        public ModernNumericTextBoxControl()
        {
            this.InitializeComponent();
        }

        /// <summary>입력된 값. null은 미입력(전체 조회)을 의미한다.</summary>
        public decimal? Value
        {
            get { return (decimal?)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        /// <summary>허용할 소수 자릿수. 0이면 정수만.</summary>
        public int DecimalPlaces
        {
            get { return (int)this.GetValue(DecimalPlacesProperty); }
            set { this.SetValue(DecimalPlacesProperty, value); }
        }

        /// <summary>음수 입력 허용 여부.</summary>
        public bool AllowNegative
        {
            get { return (bool)this.GetValue(AllowNegativeProperty); }
            set { this.SetValue(AllowNegativeProperty, value); }
        }

        /// <summary>입력 전(빈 필드) 회색으로 표시되는 안내 텍스트.</summary>
        public string Placeholder
        {
            get { return (string)this.GetValue(PlaceholderProperty); }
            set { this.SetValue(PlaceholderProperty, value); }
        }

        /// <summary>필수 입력 필드 표시(값이 비어 있는 동안 빨간 점).</summary>
        public bool Required
        {
            get { return (bool)this.GetValue(RequiredProperty); }
            set { this.SetValue(RequiredProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernNumericTextBoxControl control = (ModernNumericTextBoxControl)d;

            // 코드에서 값이 바뀌면 에디터 표시를 동기화한다.
            // (텍스트 입력 경로에서는 캐럿을 지키기 위해 덮어쓰지 않는다.)
            if (!control.syncingFromText)
            {
                control.SetEditorText(control.FormatCanonical((decimal?)e.NewValue));
            }

            control.RaiseValueChangedIfNeeded();
        }

        private void RaiseValueChangedIfNeeded()
        {
            decimal? current = this.Value;

            if (current == this.lastRaisedValue)
            {
                return;
            }

            this.lastRaisedValue = current;

            if (this.ValueChanged != null)
            {
                this.ValueChanged(this, EventArgs.Empty);
            }
        }

        // 확정 값의 정규 표시: 천단위 콤마 + 소수 자릿수 패딩 (예: 1,234.50)
        private string FormatCanonical(decimal? value)
        {
            if (!value.HasValue)
            {
                return string.Empty;
            }

            return value.Value.ToString("N" + this.DecimalPlaces.ToString(), CultureInfo.InvariantCulture);
        }

        private void SetEditorText(string text)
        {
            this.updatingText = true;
            this.InnerTextBox.Text = text;
            this.InnerTextBox.CaretIndex = text.Length;
            this.updatingText = false;
        }

        // 입력의 모든 변경(타이핑·중간 수정·붙여넣기·삭제)을 유효 문자만 추려
        // 콤마 형식으로 재구성한다. 캐럿은 "앞에 있던 유효 문자 개수"를 기준으로
        // 복원되므로 어느 위치를 고쳐도 자연스럽게 이어서 입력할 수 있다.
        private void InnerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.updatingText)
            {
                return;
            }

            string rawText = this.InnerTextBox.Text;
            int caret = this.InnerTextBox.CaretIndex;

            int significantBeforeCaret = CountSignificant(rawText, caret);

            bool negative;
            string integerDigits;
            string decimalDigits;
            bool hasDot;
            this.SanitizeInput(rawText, out negative, out integerDigits, out decimalDigits, out hasDot);

            string formatted = BuildDisplay(negative, integerDigits, decimalDigits, hasDot);

            this.updatingText = true;
            this.InnerTextBox.Text = formatted;
            this.InnerTextBox.CaretIndex = CaretIndexAfterSignificant(formatted, significantBeforeCaret);
            this.updatingText = false;

            this.ApplyToValue(negative, integerDigits, decimalDigits);
        }

        // 유효 입력(숫자가 하나라도 있음)만 Value에 반영한다. "-"나 "." 만 있는
        // 중간 상태, 빈 입력은 null (조건 없음)로 둔다.
        private void ApplyToValue(bool negative, string integerDigits, string decimalDigits)
        {
            decimal? newValue = null;

            if (integerDigits.Length > 0 || decimalDigits.Length > 0)
            {
                string plain =
                    (negative ? "-" : string.Empty) +
                    (integerDigits.Length > 0 ? integerDigits : "0") +
                    (decimalDigits.Length > 0 ? "." + decimalDigits : string.Empty);

                decimal parsed;

                if (decimal.TryParse(plain, NumberStyles.Number, CultureInfo.InvariantCulture, out parsed))
                {
                    newValue = parsed;
                }
            }

            if (newValue != this.Value)
            {
                this.syncingFromText = true;
                this.Value = newValue;
                this.syncingFromText = false;
            }
        }

        // 포커스가 떠날 때 표시를 확정 값의 정규 형식으로 정리한다
        // (미완성 입력 "-", "." 등은 빈 필드로).
        private void InnerTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            this.SetEditorText(this.FormatCanonical(this.Value));
        }

        // 원본 문자열에서 유효 문자만 추려 부호/정수부/소수부로 분해한다.
        // - '-'는 AllowNegative이고 아직 숫자가 없을 때 첫 부호만 인정
        // - '.'는 DecimalPlaces > 0일 때 첫 번째 것만 인정
        // - 소수부는 DecimalPlaces 자리까지만, 정수부 선행 0은 정리
        private void SanitizeInput(
            string raw, out bool negative, out string integerDigits, out string decimalDigits, out bool hasDot)
        {
            negative = false;
            hasDot = false;
            StringBuilder integerPart = new StringBuilder();
            StringBuilder decimalPart = new StringBuilder();
            int decimalPlaces = this.DecimalPlaces;

            foreach (char ch in raw)
            {
                if (ch == '-')
                {
                    if (this.AllowNegative && !negative && integerPart.Length == 0 && decimalPart.Length == 0 && !hasDot)
                    {
                        negative = true;
                    }
                }
                else if (ch == '.')
                {
                    if (decimalPlaces > 0 && !hasDot)
                    {
                        hasDot = true;
                    }
                }
                else if (char.IsDigit(ch))
                {
                    if (hasDot)
                    {
                        if (decimalPart.Length < decimalPlaces)
                        {
                            decimalPart.Append(ch);
                        }
                    }
                    else
                    {
                        integerPart.Append(ch);
                    }
                }
            }

            // 선행 0 정리: "0012" → "12" (전부 0이면 "0" 하나만 남긴다)
            string integerText = integerPart.ToString().TrimStart('0');

            if (integerText.Length == 0 && integerPart.Length > 0)
            {
                integerText = "0";
            }

            integerDigits = integerText;
            decimalDigits = decimalPart.ToString();
        }

        // 부호/정수부(콤마 그룹)/소수부를 표시 문자열로 조립한다.
        private static string BuildDisplay(bool negative, string integerDigits, string decimalDigits, bool hasDot)
        {
            if (integerDigits.Length == 0 && decimalDigits.Length == 0 && !hasDot && !negative)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();

            if (negative)
            {
                builder.Append('-');
            }

            builder.Append(GroupDigits(integerDigits));

            if (hasDot)
            {
                builder.Append('.');
                builder.Append(decimalDigits);
            }

            return builder.ToString();
        }

        // 정수부 숫자에 뒤에서부터 3자리마다 콤마를 넣는다.
        private static string GroupDigits(string digits)
        {
            if (digits.Length <= 3)
            {
                return digits;
            }

            StringBuilder builder = new StringBuilder();
            int leading = digits.Length % 3;

            if (leading > 0)
            {
                builder.Append(digits, 0, leading);
            }

            for (int i = leading; i < digits.Length; i += 3)
            {
                if (builder.Length > 0)
                {
                    builder.Append(',');
                }

                builder.Append(digits, i, 3);
            }

            return builder.ToString();
        }

        // 유효 문자 = 숫자, '.', '-' (콤마는 표시용이라 제외)
        private static bool IsSignificant(char ch)
        {
            return char.IsDigit(ch) || ch == '.' || ch == '-';
        }

        private static int CountSignificant(string text, int endExclusive)
        {
            int count = 0;
            int limit = Math.Min(endExclusive, text.Length);

            for (int i = 0; i < limit; i++)
            {
                if (IsSignificant(text[i]))
                {
                    count++;
                }
            }

            return count;
        }

        // 형식화된 텍스트에서 n번째 유효 문자 바로 뒤의 캐럿 위치를 구한다.
        private static int CaretIndexAfterSignificant(string formatted, int significantCount)
        {
            if (significantCount <= 0)
            {
                return 0;
            }

            int seen = 0;

            for (int i = 0; i < formatted.Length; i++)
            {
                if (IsSignificant(formatted[i]))
                {
                    seen++;

                    if (seen == significantCount)
                    {
                        return i + 1;
                    }
                }
            }

            return formatted.Length;
        }
    }
}
