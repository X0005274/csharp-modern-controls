using System;
using System.ComponentModel;
using System.Drawing;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Input
{
    /// <summary>
    /// 금액/수량 입력용 숫자 전용 텍스트박스
    /// (WPF ModernNumericTextBoxControl을 ElementHost로 호스팅.
    /// TextBox + 수동 콤마 처리 또는 NumericUpDown을 대체).
    ///
    /// 제공 멤버: Value(decimal?, null=미입력) / ValueChanged /
    /// DecimalPlaces / AllowNegative / PlaceholderText / Required, Enabled.
    /// 숫자만 치면 천단위 콤마가 자동으로 붙고, 우측 정렬로 표시된다.
    /// </summary>
    [ToolboxItem(true)]
    public class ModernNumericTextBox : WpfElementHostBase<Modern.Lab.Controls.Wpf.Input.ModernNumericTextBoxControl>
    {
        // 디자인 타임 WPF 생성이 실패한 경우(Wpf == null)에도 속성 그리드가
        // 동작하도록 하는 폴백 저장소.
        private decimal? fallbackValue;
        private int fallbackDecimalPlaces;
        private bool fallbackAllowNegative;
        private string fallbackPlaceholder;
        private bool fallbackRequired;

        /// <summary>값이 바뀔 때 발생한다.</summary>
        public event EventHandler ValueChanged;

        /// <summary>적절한 기본 크기로 컨트롤을 생성한다.</summary>
        public ModernNumericTextBox()
        {
            this.Size = new Size(120, 32);
            this.fallbackValue = null;
            this.fallbackDecimalPlaces = 0;
            this.fallbackAllowNegative = true;
            this.fallbackPlaceholder = string.Empty;
            this.fallbackRequired = false;

            if (this.Wpf != null)
            {
                this.Wpf.ValueChanged += this.OnWpfValueChanged;
            }
        }

        /// <summary>
        /// 입력된 값. null은 미입력(전체 조회)을 의미한다.
        /// 조회 조건 초기화 시 null을 할당한다.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public decimal? Value
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

        /// <summary>허용할 소수 자릿수. 0이면 정수만 입력된다.</summary>
        [Category("모던 컨트롤")]
        [Description("허용할 소수 자릿수 (0 = 정수만)")]
        [DefaultValue(0)]
        public int DecimalPlaces
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.DecimalPlaces;
                }

                return this.fallbackDecimalPlaces;
            }
            set
            {
                this.fallbackDecimalPlaces = value;

                if (this.Wpf != null)
                {
                    this.Wpf.DecimalPlaces = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>음수 입력 허용 여부 (맨 앞 '-').</summary>
        [Category("모던 컨트롤")]
        [Description("음수 입력 허용 여부")]
        [DefaultValue(true)]
        public bool AllowNegative
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.AllowNegative;
                }

                return this.fallbackAllowNegative;
            }
            set
            {
                this.fallbackAllowNegative = value;

                if (this.Wpf != null)
                {
                    this.Wpf.AllowNegative = value;
                }
            }
        }

        /// <summary>입력 전(빈 필드) 회색으로 표시되는 안내 텍스트.</summary>
        [Category("모던 컨트롤")]
        [Description("입력 전 회색으로 표시할 안내 텍스트")]
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

        private void OnWpfValueChanged(object sender, EventArgs e)
        {
            if (this.ValueChanged != null)
            {
                this.ValueChanged(this, EventArgs.Empty);
            }
        }
    }
}
