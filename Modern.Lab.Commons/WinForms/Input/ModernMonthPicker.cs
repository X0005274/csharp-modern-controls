using System;
using System.ComponentModel;
using System.Drawing;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Input
{
    /// <summary>
    /// 년월 선택 필드 (yyyy-MM; DateTimePicker의 년월 커스텀 포맷 사용을 대체.
    /// WPF ModernMonthPickerControl을 ElementHost로 호스팅).
    ///
    /// 제공 멤버: Value(DateTime?, 해당 월 1일로 정규화, null=미선택) /
    /// ValueChanged / MinDate / MaxDate / PlaceholderText / Required, Enabled.
    /// 숫자 6자리 마스크 입력 또는 12개월 그리드 팝업으로 선택한다.
    /// </summary>
    [ToolboxItem(true)]
    public class ModernMonthPicker : WpfElementHostBase<Modern.Lab.Controls.Wpf.Input.ModernMonthPickerControl>
    {
        // 디자인 타임 WPF 생성이 실패한 경우(Wpf == null)에도 속성 그리드가
        // 동작하도록 하는 폴백 저장소.
        private DateTime? fallbackValue;
        private DateTime? fallbackMinDate;
        private DateTime? fallbackMaxDate;
        private string fallbackPlaceholder;
        private bool fallbackRequired;

        /// <summary>선택 년월이 바뀔 때 발생한다.</summary>
        public event EventHandler ValueChanged;

        /// <summary>적절한 기본 크기로 컨트롤을 생성한다.</summary>
        public ModernMonthPicker()
        {
            this.Size = new Size(110, 32);
            this.fallbackValue = null;
            this.fallbackMinDate = null;
            this.fallbackMaxDate = null;
            this.fallbackPlaceholder = "yyyy-MM";
            this.fallbackRequired = false;

            if (this.Wpf != null)
            {
                this.Wpf.ValueChanged += this.OnWpfValueChanged;
            }
        }

        /// <summary>
        /// 선택된 년월 (해당 월 1일로 정규화). null은 미선택(전체 조회)을 의미한다.
        /// 임의의 날짜를 할당해도 그 달의 1일로 정규화된다.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime? Value
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.SelectedMonth;
                }

                return this.fallbackValue;
            }
            set
            {
                DateTime? normalized = Normalize(value);
                this.fallbackValue = normalized;

                if (this.Wpf != null)
                {
                    this.Wpf.SelectedMonth = normalized;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>선택 가능한 최소 년월 (null = 제한 없음).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime? MinDate
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.MinDate;
                }

                return this.fallbackMinDate;
            }
            set
            {
                this.fallbackMinDate = value;

                if (this.Wpf != null)
                {
                    this.Wpf.MinDate = value;
                }
            }
        }

        /// <summary>선택 가능한 최대 년월 (null = 제한 없음).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime? MaxDate
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.MaxDate;
                }

                return this.fallbackMaxDate;
            }
            set
            {
                this.fallbackMaxDate = value;

                if (this.Wpf != null)
                {
                    this.Wpf.MaxDate = value;
                }
            }
        }

        /// <summary>입력 전(빈 필드) 회색으로 표시되는 형식 안내 텍스트.</summary>
        [Category("모던 컨트롤")]
        [Description("입력 전 회색으로 표시할 형식 안내 텍스트")]
        [Localizable(true)]
        [DefaultValue("yyyy-MM")]
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

        /// <summary>필수 입력 표시 — 값이 비어 있는 동안 필드에 빨간 점을 표시한다.</summary>
        [Category("모던 컨트롤")]
        [Description("필수 입력 표시(값이 비어 있는 동안 빨간 점)")]
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

        private static DateTime? Normalize(DateTime? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            return new DateTime(value.Value.Year, value.Value.Month, 1);
        }

        private void OnWpfValueChanged(object sender, EventArgs e)
        {
            if (this.ValueChanged != null)
            {
                this.ValueChanged(this, EventArgs.Empty);
            }
        }
    }
}
