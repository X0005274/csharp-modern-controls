using System;
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

        /// <summary>
        /// 카드 표면색 — 테마(ModernTheme)가 결정하므로 디자이너 직렬화를 차단한다.
        /// 디자이너는 항상 라이트 모드로 돌기 때문에, 이 속성이 직렬화되면
        /// .Designer.cs에 흰색이 박혀 다크 테마에서 생성자가 설정한 표면색을
        /// InitializeComponent가 도로 덮어쓰는 사고가 난다(다크 모드 무력화의 주범).
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color BackColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

        /// <summary>
        /// Light가 아닌 테마일 때 카드 표면색을 재적용한다. 과거 버전에서 디자이너가
        /// .Designer.cs에 직렬화해 둔 라이트 색(흰색)이 InitializeComponent에서
        /// 생성자 값을 덮어쓰므로, 그보다 늦은 핸들 생성 시점에 되돌린다 —
        /// 덕분에 기존 폼의 .Designer.cs는 한 줄도 고칠 필요가 없다.
        /// (파스텔 테마는 표면이 흰색이라 사실상 no-op, 다크 계열에서 실효.)
        /// 기본 라이트에서는 아무 것도 하지 않는다(디자인 서피스 포함, 기존 동작 보존).
        /// </summary>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (Modern.Lab.Theming.ModernTheme.Mode != Modern.Lab.Theming.ModernTheme.ThemeMode.Light)
            {
                this.BackColor = SurfaceColor;
            }
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

            // Win11 카드 엘리베이션: 카드 본체를 2px 위로 올리고 아래 여백에
            // 옅은 그림자 라인 두 줄을 깐다 (알파 블랙 — 어떤 배경에서도 자연스럽고,
            // DropShadowEffect 같은 비용 없이 스텝당 그리기 비용이 사실상 0이다).
            using (GraphicsPath cardPath = CreateRoundedPath(
                new Rectangle(0, 0, this.Width - 1, this.Height - 3), CardCornerRadius))
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

                // 하단 그림자: 카드 바로 아래 1px(진하게) + 그 아래 1px(옅게).
                // 모서리 반경만큼 안쪽에서 시작해 둥근 모서리와 자연스럽게 만난다.
                using (Pen shadowNear = new Pen(Color.FromArgb(26, 0, 0, 0)))
                {
                    e.Graphics.DrawLine(shadowNear,
                            CardCornerRadius, this.Height - 2,
                            this.Width - 1 - CardCornerRadius, this.Height - 2);
                }

                using (Pen shadowFar = new Pen(Color.FromArgb(10, 0, 0, 0)))
                {
                    e.Graphics.DrawLine(shadowFar,
                            CardCornerRadius + 2, this.Height - 1,
                            this.Width - 3 - CardCornerRadius, this.Height - 1);
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
