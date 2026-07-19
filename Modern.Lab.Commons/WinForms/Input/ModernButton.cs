using System;
using System.ComponentModel;
using System.Drawing;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Input
{
    /// <summary>
    /// System.Windows.Forms.Button의 드롭인 대체 컨트롤
    /// (WPF ModernButtonControl을 ElementHost로 호스팅).
    ///
    /// 호환 멤버: Text(override, localizable), Click, Enabled.
    /// Text는 `new`로 숨기지 않고 Control.Text를 override한다 — 숨기면
    /// Localizable 폼에서 ComponentResourceManager.ApplyResources가 깨지고,
    /// Control 참조로 Text를 할당하는 호스트 코드도 깨진다
    /// (docs/design-notes.md 3절).
    /// </summary>
    [ToolboxItem(true)]
    [DefaultEvent("Click")]
    public class ModernButton : WpfElementHostBase<Modern.Lab.Controls.Wpf.Input.ModernButtonControl>
    {
        // 디자인 타임 WPF 생성이 실패한 경우(Wpf == null)에도 속성 그리드가
        // 동작하도록 하는 폴백 저장소.
        private string fallbackText;
        private Modern.Lab.Controls.Wpf.Input.ButtonKind fallbackKind;
        private string fallbackIconGlyph;
        private double fallbackFontSize;
        private string fallbackTopLabel;

        /// <summary>적절한 기본 크기로 컨트롤을 생성한다.</summary>
        public ModernButton()
        {
            this.Size = new Size(120, 32);
            this.fallbackText = "버튼";
            this.fallbackKind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Primary;
            this.fallbackIconGlyph = string.Empty;
            this.fallbackFontSize = 0d;
            this.fallbackTopLabel = string.Empty;

            if (this.Wpf != null)
            {
                // WPF 클릭을 표준 WinForms Click 이벤트로 전달한다.
                this.Wpf.Click += this.OnWpfButtonClick;
            }
        }

        /// <summary>버튼에 표시되는 캡션.</summary>
        [Category("모던 컨트롤")]
        [Description("버튼에 표시할 텍스트")]
        [Browsable(true)]
        [Localizable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue("버튼")]
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

        /// <summary>시각적 종류 (Primary/Secondary/Danger).</summary>
        [Category("모던 컨트롤")]
        [Description("버튼 종류(Primary/Secondary/Danger)")]
        [DefaultValue(Modern.Lab.Controls.Wpf.Input.ButtonKind.Primary)]
        public Modern.Lab.Controls.Wpf.Input.ButtonKind Kind
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

        /// <summary>캡션 앞에 표시되는 아이콘 글리프(Segoe MDL2 Assets).</summary>
        [Category("모던 컨트롤")]
        [Description("버튼 글자 앞 아이콘 글리프(Segoe MDL2 Assets)")]
        [DefaultValue("")]
        public string IconGlyph
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.IconGlyph;
                }

                return this.fallbackIconGlyph;
            }
            set
            {
                this.fallbackIconGlyph = value;

                if (this.Wpf != null)
                {
                    this.Wpf.IconGlyph = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>캡션/아이콘 글자 크기 재정의(px). 0 이하이면 토큰 기본값
        /// (Font.Size.Body)을 쓴다 — 화살표·기호 같은 아이콘형 캡션을 크게
        /// 보이게 할 때 쓴다.</summary>
        [Category("모던 컨트롤")]
        [Description("캡션/아이콘 글자 크기 재정의(px) — 0 = 토큰 기본값")]
        [DefaultValue(0d)]
        public double GlyphSize
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.FontSizeOverride;
                }

                return this.fallbackFontSize;
            }
            set
            {
                this.fallbackFontSize = value;

                if (this.Wpf != null)
                {
                    this.Wpf.FontSizeOverride = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>글리프/캡션 위에 붙는 아주 작은 상단 라벨(비어 있으면 숨김) —
        /// 아이콘형 버튼에 "All"/"Selected" 같은 부가 설명을 얹을 때 쓴다.</summary>
        [Category("모던 컨트롤")]
        [Description("글리프/캡션 위 상단 소형 라벨 (비어 있으면 숨김)")]
        [DefaultValue("")]
        public string TopLabel
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.TopLabel;
                }

                return this.fallbackTopLabel;
            }
            set
            {
                this.fallbackTopLabel = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.TopLabel = value ?? string.Empty;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        private void OnWpfButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            this.OnClick(EventArgs.Empty);
        }
    }
}
