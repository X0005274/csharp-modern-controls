using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Modern.Lab.Controls.Wpf.Input
{
    /// <summary>
    /// 모던 날짜 선택 필드 (마스크 입력 + 달력 팝업).
    ///
    /// 직접 입력: 숫자만 치면 yyyy-MM-dd 형식으로 자동 형식화된다.
    /// - 구분자(-)는 코드가 삽입하므로 사용자는 숫자 8자리만 입력하면 된다.
    /// - 중간 위치를 수정해도 즉시 재형식화되며 입력이 막히지 않는다.
    /// - 8자리가 유효한 날짜면 SelectedDate에 반영, 아니면 null 유지.
    /// - 포커스가 떠날 때 미완성/무효 입력은 마지막 유효 값으로 되돌린다.
    ///
    /// SelectedDate = null 은 "미선택(전체)"을 의미한다.
    /// </summary>
    public partial class ModernDatePickerControl : UserControl
    {
        private const string DateFormat = "yyyy-MM-dd";

        // 진행형 마스크의 그룹 자릿수: yyyy-MM-dd → 4-2-2.
        private static readonly int[] maskGroupLengths = new int[] { 4, 2, 2 };

        /// <summary>선택된 날짜. null은 미선택(전체 조회)을 의미한다.</summary>
        public static readonly DependencyProperty SelectedDateProperty =
            DependencyProperty.Register(
                "SelectedDate",
                typeof(DateTime?),
                typeof(ModernDatePickerControl),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedDateChanged));

        /// <summary>선택 가능한 최소 날짜 (null = 제한 없음). 달력 표시 범위를 제한한다.</summary>
        public static readonly DependencyProperty MinDateProperty =
            DependencyProperty.Register(
                "MinDate",
                typeof(DateTime?),
                typeof(ModernDatePickerControl),
                new PropertyMetadata(null));

        /// <summary>선택 가능한 최대 날짜 (null = 제한 없음). 달력 표시 범위를 제한한다.</summary>
        public static readonly DependencyProperty MaxDateProperty =
            DependencyProperty.Register(
                "MaxDate",
                typeof(DateTime?),
                typeof(ModernDatePickerControl),
                new PropertyMetadata(null));

        /// <summary>필수 입력 필드 표시 — hover/포커스 시 필드 왼쪽에 옅은 빨간 세로 바.</summary>
        public static readonly DependencyProperty RequiredProperty =
            DependencyProperty.Register(
                "Required",
                typeof(bool),
                typeof(ModernDatePickerControl),
                new PropertyMetadata(false));

        /// <summary>입력 전(빈 필드) 회색으로 표시되는 형식 안내 텍스트.</summary>
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(
                "Placeholder",
                typeof(string),
                typeof(ModernDatePickerControl),
                new PropertyMetadata("yyyy-MM-dd"));

        /// <summary>선택 날짜가 바뀔 때 발생한다 (래퍼가 ValueChanged로 재노출).</summary>
        public event EventHandler ValueChanged;

        // TextChanged 재형식화 → Text 재할당 → TextChanged 재진입을 막는 가드.
        private bool updatingText;

        // 텍스트 입력이 SelectedDate를 갱신하는 동안, DP 콜백이 에디터 텍스트를
        // 다시 덮어써 캐럿이 튀는 것을 막는 가드.
        private bool syncingFromText;

        // 마지막으로 ValueChanged를 통지한 값 — 같은 값이면 이벤트를 삼킨다.
        private DateTime? lastRaisedValue;

        public ModernDatePickerControl()
        {
            this.InitializeComponent();
        }

        /// <summary>선택된 날짜. null은 미선택(전체 조회)을 의미한다.</summary>
        public DateTime? SelectedDate
        {
            get { return (DateTime?)this.GetValue(SelectedDateProperty); }
            set { this.SetValue(SelectedDateProperty, value); }
        }

        /// <summary>선택 가능한 최소 날짜 (null = 제한 없음).</summary>
        public DateTime? MinDate
        {
            get { return (DateTime?)this.GetValue(MinDateProperty); }
            set { this.SetValue(MinDateProperty, value); }
        }

        /// <summary>선택 가능한 최대 날짜 (null = 제한 없음).</summary>
        public DateTime? MaxDate
        {
            get { return (DateTime?)this.GetValue(MaxDateProperty); }
            set { this.SetValue(MaxDateProperty, value); }
        }

        /// <summary>필수 입력 필드 표시(hover/포커스 시 옅은 빨간 세로 바).</summary>
        public bool Required
        {
            get { return (bool)this.GetValue(RequiredProperty); }
            set { this.SetValue(RequiredProperty, value); }
        }

        /// <summary>입력 전(빈 필드) 회색으로 표시되는 형식 안내 텍스트.</summary>
        public string Placeholder
        {
            get { return (string)this.GetValue(PlaceholderProperty); }
            set { this.SetValue(PlaceholderProperty, value); }
        }

        private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernDatePickerControl control = (ModernDatePickerControl)d;

            // 코드/달력에서 값이 바뀌면 에디터 표시를 동기화한다.
            // (텍스트 입력 경로에서는 캐럿을 지키기 위해 덮어쓰지 않는다.)
            if (!control.syncingFromText)
            {
                control.SetEditorText(control.FormatDate((DateTime?)e.NewValue));
            }

            control.RaiseValueChangedIfNeeded();
        }

        private void RaiseValueChangedIfNeeded()
        {
            DateTime? current = this.SelectedDate;

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

        private string FormatDate(DateTime? value)
        {
            if (!value.HasValue)
            {
                return string.Empty;
            }

            return value.Value.ToString(DateFormat, CultureInfo.InvariantCulture);
        }

        private void SetEditorText(string text)
        {
            this.updatingText = true;
            this.InnerTextBox.Text = text;
            this.InnerTextBox.CaretIndex = text.Length;
            this.updatingText = false;
        }

        // 입력의 모든 변경(타이핑·중간 수정·붙여넣기·삭제)을 숫자만 추려
        // yyyy-MM-dd로 재형식화한다. 캐럿은 "앞에 있던 숫자 개수"를 기준으로
        // 복원되므로 어느 위치를 고쳐도 자연스럽게 이어서 입력할 수 있다.
        private void InnerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.updatingText)
            {
                return;
            }

            string rawText = this.InnerTextBox.Text;
            int caret = this.InnerTextBox.CaretIndex;

            int digitsBeforeCaret = DigitMaskHelper.CountDigits(rawText, caret);
            string digits = DigitMaskHelper.ExtractDigits(rawText, 8);
            string formatted = FormatDigits(digits);

            this.updatingText = true;
            this.InnerTextBox.Text = formatted;
            this.InnerTextBox.CaretIndex = DigitMaskHelper.CaretIndexAfterDigits(formatted, digitsBeforeCaret);
            this.updatingText = false;

            this.ApplyDigitsToSelectedDate(digits);
        }

        // 8자리가 유효한 날짜일 때만 SelectedDate에 반영한다.
        // 미완성/무효 입력은 SelectedDate = null (조건 없음)로 둔다 —
        // 입력을 막거나 오류를 띄우지 않는다.
        private void ApplyDigitsToSelectedDate(string digits)
        {
            DateTime parsed;
            DateTime? newValue = null;

            if (digits.Length == 8 &&
                DateTime.TryParseExact(digits, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            {
                newValue = parsed;
            }

            if (newValue != this.SelectedDate)
            {
                this.syncingFromText = true;
                this.SelectedDate = newValue;
                this.syncingFromText = false;
            }
        }

        // 포커스가 떠날 때 표시를 확정 값과 동기화한다:
        // 유효한 날짜면 정규 형식으로, 미완성이면 빈 필드로 되돌린다.
        private void InnerTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            this.SetEditorText(this.FormatDate(this.SelectedDate));
        }

        private void CalendarButton_Click(object sender, RoutedEventArgs e)
        {
            this.PickerCalendar.SelectedDate = this.SelectedDate;
            this.PickerCalendar.DisplayDate = this.SelectedDate.HasValue ? this.SelectedDate.Value : DateTime.Today;
            this.CalendarPopup.IsOpen = true;
        }

        private void PickerCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.PickerCalendar.SelectedDate.HasValue)
            {
                this.SelectedDate = this.PickerCalendar.SelectedDate;
                this.CalendarPopup.IsOpen = false;

                // Calendar가 마우스 캡처를 쥔 채로 닫히면 다음 클릭이 삼켜진다 —
                // 캡처를 명시적으로 해제한다.
                Mouse.Capture(null);
            }
        }

        // 숫자 나열을 진행형 마스크로 형식화한다:
        // "2015" → "2015", "201507" → "2015-07", "20150713" → "2015-07-13"
        private static string FormatDigits(string digits)
        {
            return DigitMaskHelper.FormatGroups(digits, '-', maskGroupLengths);
        }
    }
}
