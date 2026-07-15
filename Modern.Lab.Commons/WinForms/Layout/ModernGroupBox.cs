using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Modern.Lab.WinForms.Controls.Layout
{
    /// <summary>
    /// 헤더 타이틀이 있는 카드 컨테이너 — System.Windows.Forms.GroupBox의 대체.
    ///
    /// ModernCardPanel과 마찬가지로 GDI+로 그리는 **순수 WinForms 패널**이라
    /// 모던 리프 컨트롤을 포함한 어떤 WinForms 자식도 담을 수 있다
    /// (계약 규칙 5: 영역 레이아웃은 WinForms 유지).
    ///
    /// 카드 표면 위쪽에 SemiBold 타이틀과 은은한 구분선을 그리고,
    /// 기본 Padding이 헤더 높이를 확보하므로 자식은 헤더 아래에 배치된다.
    /// </summary>
    [ToolboxItem(true)]
    public class ModernGroupBox : ModernCardPanel
    {
        // 제목/구분선 색은 중앙 팔레트(ModernTheme)에서 — 라이트/다크에 따라 바뀐다.
        private static Color TitleColor { get { return Modern.Lab.Theming.ModernTheme.TextPrimary; } }
        private static Color AccentTitleColor { get { return Modern.Lab.Theming.ModernTheme.Accent; } }
        private static Color SeparatorColor { get { return Modern.Lab.Theming.ModernTheme.BorderSubtle; } }
        private const int HeaderHeight = 32;

        private string titleText;
        private FontStyle titleFontStyle;
        private bool titleAccent;
        private double fontWidthRatio;

        /// <summary>헤더 높이를 확보한 기본 패딩으로 그룹박스를 생성한다.</summary>
        public ModernGroupBox()
        {
            this.titleText = "그룹";
            this.titleFontStyle = FontStyle.Regular;
            this.Padding = new Padding(12, HeaderHeight + 8, 12, 12);
        }

        /// <summary>헤더 타이틀에 적용할 폰트 스타일(Bold/Italic 조합 가능).</summary>
        [Category("모던 컨트롤")]
        [Description("헤더 타이틀에 적용할 폰트 스타일(Bold/Italic 조합 가능)")]
        [DefaultValue(FontStyle.Regular)]
        public FontStyle TitleFontStyle
        {
            get
            {
                return this.titleFontStyle;
            }
            set
            {
                this.titleFontStyle = value;
                this.Invalidate();
            }
        }

        /// <summary>장평(글자 가로 비율) 재정의. 0 = 전역(ModernTheme.FontWidthRatio) 사용.</summary>
        [Category("모던 컨트롤")]
        [Description("헤더 타이틀 장평(글자 가로 비율) 재정의 — 0 = 전역(ModernTheme.FontWidthRatio) 사용, 허용 0.8~1.2")]
        [DefaultValue(0d)]
        public double FontWidthRatio
        {
            get
            {
                return this.fontWidthRatio;
            }
            set
            {
                this.fontWidthRatio = value;
                this.Invalidate();
            }
        }

        /// <summary>헤더 타이틀을 액센트 색(#0078D4)으로 강조할지 여부.</summary>
        [Category("모던 컨트롤")]
        [Description("헤더 타이틀을 액센트 색으로 강조할지 여부")]
        [DefaultValue(false)]
        public bool TitleAccent
        {
            get
            {
                return this.titleAccent;
            }
            set
            {
                this.titleAccent = value;
                this.Invalidate();
            }
        }

        /// <summary>헤더에 표시되는 타이틀(GroupBox.Text와 동일한 의미).</summary>
        [Category("모던 컨트롤")]
        [Description("헤더에 표시할 타이틀")]
        [Browsable(true)]
        [Localizable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue("그룹")]
        public override string Text
        {
            get
            {
                return this.titleText;
            }
            set
            {
                this.titleText = value ?? string.Empty;
                this.Invalidate();
            }
        }

        /// <summary>카드 표면 위에 타이틀과 헤더 구분선을 그린다.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (this.Width <= 1 || this.Height <= HeaderHeight)
            {
                return;
            }

            // 타이틀: 구조 요소 규칙(SemiBold)을 따른다. Segoe UI Semibold가
            // 없으면 Bold로 폴백된다. TitleFontStyle로 Bold/Italic을 겹칠 수 있다.
            using (Font titleFont = new Font("Segoe UI Semibold", 9f, this.titleFontStyle))
            using (SolidBrush titleBrush = new SolidBrush(this.titleAccent ? AccentTitleColor : TitleColor))
            {
                SizeF textSize = e.Graphics.MeasureString(this.titleText, titleFont);
                float textY = (HeaderHeight - textSize.Height) / 2f + 1f;

                // 장평: DrawString(GDI+)은 변환을 따르므로 가로만 스케일해 그린다
                // (x 좌표는 변환 전 좌표계로 환산해 시작 위치 12px을 유지).
                double widthRatio = Modern.Lab.Theming.ModernTheme.ResolveFontWidthRatio(this.fontWidthRatio);

                if (System.Math.Abs(widthRatio - 1d) < 0.001)
                {
                    e.Graphics.DrawString(this.titleText, titleFont, titleBrush, 12f, textY);
                }
                else
                {
                    System.Drawing.Drawing2D.GraphicsState state = e.Graphics.Save();
                    e.Graphics.ScaleTransform((float)widthRatio, 1f);
                    e.Graphics.DrawString(this.titleText, titleFont, titleBrush, 12f / (float)widthRatio, textY);
                    e.Graphics.Restore(state);
                }
            }

            // 헤더 아래 은은한 구분선 (테두리 안쪽 1px 여백)
            using (Pen separatorPen = new Pen(SeparatorColor))
            {
                e.Graphics.DrawLine(separatorPen, 1, HeaderHeight, this.Width - 2, HeaderHeight);
            }
        }
    }
}
