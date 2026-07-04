using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Modern.Lab.Controls.Wpf.Input
{
    /// <summary>
    /// 모던 년월 선택 필드 (마스크 입력 + 월 선택 팝업).
    ///
    /// 직접 입력: 숫자 6자리를 치면 yyyy-MM 형식으로 자동 형식화된다.
    /// 팝업: 달력 버튼이 12개월 그리드(Year 뷰)를 열고 월 클릭이 곧 선택이 된다.
    /// SelectedMonth는 항상 해당 월의 1일로 정규화되며, null은 "미선택(전체)"을 의미한다.
    /// </summary>
    public partial class ModernMonthPickerControl : UserControl
    {
        private const string MonthFormat = "yyyy-MM";

        /// <summary>선택된 년월 (해당 월 1일로 정규화). null은 미선택(전체 조회).</summary>
        public static readonly DependencyProperty SelectedMonthProperty =
            DependencyProperty.Register(
                "SelectedMonth",
                typeof(DateTime?),
                typeof(ModernMonthPickerControl),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedMonthChanged));

        /// <summary>선택 가능한 최소 년월 (null = 제한 없음). 팝업 표시 범위를 제한한다.</summary>
        public static readonly DependencyProperty MinDateProperty =
            DependencyProperty.Register(
                "MinDate",
                typeof(DateTime?),
                typeof(ModernMonthPickerControl),
                new PropertyMetadata(null));

        /// <summary>선택 가능한 최대 년월 (null = 제한 없음). 팝업 표시 범위를 제한한다.</summary>
        public static readonly DependencyProperty MaxDateProperty =
            DependencyProperty.Register(
                "MaxDate",
                typeof(DateTime?),
                typeof(ModernMonthPickerControl),
                new PropertyMetadata(null));

        /// <summary>입력 전(빈 필드) 회색으로 표시되는 형식 안내 텍스트.</summary>
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(
                "Placeholder",
                typeof(string),
                typeof(ModernMonthPickerControl),
                new PropertyMetadata("yyyy-MM"));

        /// <summary>필수 입력 필드 표시 — 값이 비어 있는 동안 필드에 빨간 점을 표시한다.</summary>
        public static readonly DependencyProperty RequiredProperty =
            DependencyProperty.Register(
                "Required",
                typeof(bool),
                typeof(ModernMonthPickerControl),
                new PropertyMetadata(false));

        /// <summary>선택 년월이 바뀔 때 발생한다 (래퍼가 ValueChanged로 재노출).</summary>
        public event EventHandler ValueChanged;

        // TextChanged 재형식화 → Text 재할당 → TextChanged 재진입을 막는 가드.
        private bool updatingText;

        // 텍스트 입력이 SelectedMonth를 갱신하는 동안, DP 콜백이 에디터 텍스트를
        // 다시 덮어써 캐럿이 튀는 것을 막는 가드.
        private bool syncingFromText;

        // 마지막으로 ValueChanged를 통지한 값 — 같은 값이면 이벤트를 삼킨다.
        private DateTime? lastRaisedValue;

        public ModernMonthPickerControl()
        {
            this.InitializeComponent();
        }

        /// <summary>선택된 년월 (해당 월 1일로 정규화). null은 미선택(전체 조회).</summary>
        public DateTime? SelectedMonth
        {
            get { return (DateTime?)this.GetValue(SelectedMonthProperty); }
            set { this.SetValue(SelectedMonthProperty, value); }
        }

        /// <summary>선택 가능한 최소 년월 (null = 제한 없음).</summary>
        public DateTime? MinDate
        {
            get { return (DateTime?)this.GetValue(MinDateProperty); }
            set { this.SetValue(MinDateProperty, value); }
        }

        /// <summary>선택 가능한 최대 년월 (null = 제한 없음).</summary>
        public DateTime? MaxDate
        {
            get { return (DateTime?)this.GetValue(MaxDateProperty); }
            set { this.SetValue(MaxDateProperty, value); }
        }

        /// <summary>입력 전(빈 필드) 회색으로 표시되는 형식 안내 텍스트.</summary>
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

        private static void OnSelectedMonthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernMonthPickerControl control = (ModernMonthPickerControl)d;

            // 코드/팝업에서 값이 바뀌면 에디터 표시를 동기화한다.
            // (텍스트 입력 경로에서는 캐럿을 지키기 위해 덮어쓰지 않는다.)
            if (!control.syncingFromText)
            {
                control.SetEditorText(control.FormatMonth((DateTime?)e.NewValue));
            }

            control.RaiseValueChangedIfNeeded();
        }

        private void RaiseValueChangedIfNeeded()
        {
            DateTime? current = this.SelectedMonth;

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

        private string FormatMonth(DateTime? value)
        {
            if (!value.HasValue)
            {
                return string.Empty;
            }

            return value.Value.ToString(MonthFormat, CultureInfo.InvariantCulture);
        }

        private void SetEditorText(string text)
        {
            this.updatingText = true;
            this.InnerTextBox.Text = text;
            this.InnerTextBox.CaretIndex = text.Length;
            this.updatingText = false;
        }

        // 입력의 모든 변경(타이핑·중간 수정·붙여넣기·삭제)을 숫자만 추려
        // yyyy-MM으로 재형식화한다. 캐럿은 "앞에 있던 숫자 개수" 기준으로 복원된다.
        private void InnerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.updatingText)
            {
                return;
            }

            string rawText = this.InnerTextBox.Text;
            int caret = this.InnerTextBox.CaretIndex;

            int digitsBeforeCaret = CountDigits(rawText, caret);
            string digits = ExtractDigits(rawText, 6);
            string formatted = FormatDigits(digits);

            this.updatingText = true;
            this.InnerTextBox.Text = formatted;
            this.InnerTextBox.CaretIndex = CaretIndexAfterDigits(formatted, digitsBeforeCaret);
            this.updatingText = false;

            this.ApplyDigitsToSelectedMonth(digits);
        }

        // 6자리가 유효한 년월일 때만 SelectedMonth에 반영한다.
        // 미완성/무효 입력은 null (조건 없음)로 두며 입력을 막지 않는다.
        private void ApplyDigitsToSelectedMonth(string digits)
        {
            DateTime parsed;
            DateTime? newValue = null;

            if (digits.Length == 6 &&
                DateTime.TryParseExact(digits, "yyyyMM", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            {
                newValue = parsed;
            }

            if (newValue != this.SelectedMonth)
            {
                this.syncingFromText = true;
                this.SelectedMonth = newValue;
                this.syncingFromText = false;
            }
        }

        // 포커스가 떠날 때 표시를 확정 값과 동기화한다:
        // 유효한 년월이면 정규 형식으로, 미완성이면 빈 필드로 되돌린다.
        private void InnerTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            this.SetEditorText(this.FormatMonth(this.SelectedMonth));
        }

        private void CalendarButton_Click(object sender, RoutedEventArgs e)
        {
            this.PickerCalendar.DisplayMode = CalendarMode.Year;
            this.PickerCalendar.DisplayDate = this.SelectedMonth.HasValue ? this.SelectedMonth.Value : DateTime.Today;
            this.MonthPopup.IsOpen = true;
        }

        // Year 뷰에서 월을 클릭하면 Calendar가 Month 뷰로 내려가려 한다 —
        // 그 시점의 DisplayDate가 클릭된 월이므로 선택으로 확정하고 팝업을 닫는다.
        private void PickerCalendar_DisplayModeChanged(object sender, CalendarModeChangedEventArgs e)
        {
            if (e.NewMode != CalendarMode.Month)
            {
                return;
            }

            DateTime picked = this.PickerCalendar.DisplayDate;

            this.PickerCalendar.DisplayMode = CalendarMode.Year;
            this.SelectedMonth = new DateTime(picked.Year, picked.Month, 1);
            this.MonthPopup.IsOpen = false;

            // Calendar가 마우스 캡처를 쥔 채로 닫히면 다음 클릭이 삼켜진다.
            Mouse.Capture(null);
        }

        private static int CountDigits(string text, int endExclusive)
        {
            int count = 0;
            int limit = Math.Min(endExclusive, text.Length);

            for (int i = 0; i < limit; i++)
            {
                if (char.IsDigit(text[i]))
                {
                    count++;
                }
            }

            return count;
        }

        private static string ExtractDigits(string text, int maxCount)
        {
            StringBuilder builder = new StringBuilder(maxCount);

            foreach (char ch in text)
            {
                if (char.IsDigit(ch))
                {
                    builder.Append(ch);

                    if (builder.Length >= maxCount)
                    {
                        break;
                    }
                }
            }

            return builder.ToString();
        }

        // 숫자 나열을 진행형 마스크로 형식화한다: "2015" → "2015", "201507" → "2015-07"
        private static string FormatDigits(string digits)
        {
            if (digits.Length <= 4)
            {
                return digits;
            }

            return digits.Substring(0, 4) + "-" + digits.Substring(4);
        }

        // 형식화된 텍스트에서 n번째 숫자 바로 뒤의 캐럿 위치를 구한다.
        private static int CaretIndexAfterDigits(string formatted, int digitCount)
        {
            if (digitCount <= 0)
            {
                return 0;
            }

            int seen = 0;

            for (int i = 0; i < formatted.Length; i++)
            {
                if (char.IsDigit(formatted[i]))
                {
                    seen++;

                    if (seen == digitCount)
                    {
                        return i + 1;
                    }
                }
            }

            return formatted.Length;
        }
    }
}
