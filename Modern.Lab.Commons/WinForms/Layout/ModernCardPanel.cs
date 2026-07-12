using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Modern.Lab.WinForms.Controls.Layout
{
    /// <summary>
    /// 화면 영역(검색 바, 하단 통계 등)을 일관된 표면으로 묶는 카드 모양
    /// 컨테이너 패널.
    ///
    /// 이 컨트롤은 GDI+로 그리는 순수 WinForms Panel이며 ElementHost가
    /// 아니다 — 따라서 모던 리프 컨트롤을 포함한 어떤 WinForms 자식도
    /// 호스팅할 수 있다(계약 규칙 5: 영역 레이아웃은 WinForms 유지;
    /// ElementHost는 WinForms 자식을 호스팅할 수 없지만 네이티브 패널은
    /// 가능하다).
    ///
    /// 외형은 카드 토큰을 따른다: 흰 표면, 은은한 테두리, 반경 8.
    /// </summary>
    [ToolboxItem(true)]
    public class ModernCardPanel : Panel
    {
        // 카드 표면/테두리 색은 중앙 팔레트(ModernTheme)에서 — 라이트/다크에 따라 바뀐다.
        // (Brush.Surface / Brush.BorderSubtle / Radius.Lg 미러링.)
        private static Color SurfaceColor { get { return Modern.Lab.Theming.ModernTheme.Surface; } }
        private static Color BorderColor { get { return Modern.Lab.Theming.ModernTheme.BorderSubtle; } }
        private const int CardCornerRadius = 8;

        /// <summary>카드 표면과 기본 패딩으로 패널을 생성한다.</summary>
        public ModernCardPanel()
        {
            // 자식이 BackColor를 상속하므로 패널 자체를 표면 색으로 두고,
            // 모서리 영역만 부모 색으로 다시 칠한다.
            this.BackColor = SurfaceColor;
            this.Padding = new Padding(12, 8, 12, 8);
            this.SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.ResizeRedraw,
                true);
        }

        /// <summary>둥근 카드 표면과 테두리를 그린다.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (this.Width <= 1 || this.Height <= 1)
            {
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath cardPath = CreateRoundedPath(
                new Rectangle(0, 0, this.Width - 1, this.Height - 1), CardCornerRadius))
            {
                // 둥근 사각형 바깥 영역을 부모 배경색으로 다시 칠해
                // 모서리가 흰 사각형으로 보이지 않게 한다.
                Color outsideColor = this.Parent != null ? this.Parent.BackColor : SystemColors.Control;

                using (Region outside = new Region(new Rectangle(0, 0, this.Width, this.Height)))
                {
                    outside.Exclude(cardPath);

                    using (SolidBrush outsideBrush = new SolidBrush(outsideColor))
                    {
                        e.Graphics.FillRegion(outsideBrush, outside);
                    }
                }

                using (Pen borderPen = new Pen(BorderColor))
                {
                    e.Graphics.DrawPath(borderPen, cardPath);
                }
            }
        }

        private static GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();

            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180f, 90f);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270f, 90f);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0f, 90f);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90f, 90f);
            path.CloseFigure();

            return path;
        }
    }
}
