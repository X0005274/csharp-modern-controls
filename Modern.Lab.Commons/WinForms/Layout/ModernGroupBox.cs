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
        // Themes/Tokens.xaml에서 미러링한 토큰(GDI+는 XAML 리소스를 읽을 수 없다):
        // Brush.TextPrimary / Brush.BorderSubtle / 구조 요소 SemiBold 9pt.
        private static readonly Color TitleColor = Color.FromArgb(17, 24, 39);
        private static readonly Color SeparatorColor = Color.FromArgb(229, 231, 235);
        private const int HeaderHeight = 32;

        private string titleText;

        /// <summary>헤더 높이를 확보한 기본 패딩으로 그룹박스를 생성한다.</summary>
        public ModernGroupBox()
        {
            this.titleText = "그룹";
            this.Padding = new Padding(12, HeaderHeight + 8, 12, 12);
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
            // 없으면 Bold로 폴백된다.
            using (Font titleFont = new Font("Segoe UI Semibold", 9f, FontStyle.Regular))
            using (SolidBrush titleBrush = new SolidBrush(TitleColor))
            {
                SizeF textSize = e.Graphics.MeasureString(this.titleText, titleFont);
                float textY = (HeaderHeight - textSize.Height) / 2f + 1f;
                e.Graphics.DrawString(this.titleText, titleFont, titleBrush, 12f, textY);
            }

            // 헤더 아래 은은한 구분선 (테두리 안쪽 1px 여백)
            using (Pen separatorPen = new Pen(SeparatorColor))
            {
                e.Graphics.DrawLine(separatorPen, 1, HeaderHeight, this.Width - 2, HeaderHeight);
            }
        }
    }
}
