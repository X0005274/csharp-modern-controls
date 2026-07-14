using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Modern.Lab.WinForms.Controls.Layout
{
    /// <summary>
    /// 언더라인(피벗) 스타일 탭 컨테이너 — System.Windows.Forms.TabControl의 대체.
    ///
    /// ModernCardPanel과 마찬가지로 GDI+로 그리는 **순수 WinForms 컨테이너**라
    /// (계약 규칙 5: 영역 레이아웃은 WinForms 담당) 각 탭 본문에 어떤 WinForms
    /// 자식(모던 그리드 포함)도 담을 수 있다.
    ///
    /// 시각은 토큰을 따른다: 탭을 채우지 않고 텍스트만 두되, 선택 탭은
    /// 액센트색 SemiBold + 아래 액센트 밑줄, 하단에 옅은 구분선. 색은 전부
    /// ModernTheme 팔레트를 그릴 때 읽으므로 6개 테마 전부 자동 대응한다.
    ///
    /// 사용법: <see cref="AddTab"/>으로 페이지를 붙인다. 페이지는 본문에
    /// Dock=Fill로 배치되고 선택 시에만 보인다. 탭 전환은 클릭 또는
    /// <see cref="SelectedIndex"/> 설정.
    /// </summary>
    [ToolboxItem(true)]
    public class ModernTabControl : Panel
    {
        private readonly TabHeaderStrip header;
        private readonly Panel body;
        private readonly List<Control> pages = new List<Control>();

        /// <summary>선택 탭이 바뀔 때 발생.</summary>
        public event EventHandler SelectedIndexChanged;

        /// <summary>헤더(Top) + 본문(Fill) 구조로 생성한다.</summary>
        public ModernTabControl()
        {
            // 본문(Fill)을 먼저 추가하고 헤더(Top)를 나중에 추가한다 — 이 순서라야
            // 헤더가 상단 띠를 차지하고 본문이 나머지를 채운다.
            this.body = new Panel();
            this.body.Dock = DockStyle.Fill;
            this.Controls.Add(this.body);

            this.header = new TabHeaderStrip();
            this.header.Dock = DockStyle.Top;
            this.header.Height = 40;
            this.header.SelectedIndexChanged += (sender, args) =>
            {
                this.ShowPage(this.header.SelectedIndex);
                if (this.SelectedIndexChanged != null)
                {
                    this.SelectedIndexChanged(this, EventArgs.Empty);
                }
            };
            this.Controls.Add(this.header);
        }

        /// <summary>선택 탭 인덱스 (없으면 -1).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get { return this.header.SelectedIndex; }
            set { this.header.SelectedIndex = value; }
        }

        /// <summary>탭 수.</summary>
        [Browsable(false)]
        public int TabCount
        {
            get { return this.pages.Count; }
        }

        /// <summary>탭 추가. content는 본문에 Dock=Fill로 배치되고 선택 시에만 보인다.</summary>
        public void AddTab(string title, Control content)
        {
            content.Dock = DockStyle.Fill;
            content.Visible = this.pages.Count == 0;
            this.body.Controls.Add(content);
            this.pages.Add(content);
            this.header.AddTab(title);

            if (this.pages.Count == 1)
            {
                this.header.SelectedIndex = 0;
            }
        }

        /// <summary>탭 제목을 바꾼다 (예: "Unit History — IT10001.01"처럼 대상 표시).</summary>
        public void SetTabTitle(int index, string title)
        {
            this.header.SetTitle(index, title);
        }

        private void ShowPage(int index)
        {
            for (int i = 0; i < this.pages.Count; i++)
            {
                this.pages[i].Visible = i == index;
            }
        }

        /// <summary>언더라인 스타일 탭 헤더 (직접 그리기 + 클릭 히트테스트).</summary>
        private sealed class TabHeaderStrip : Panel
        {
            private const int PadX = 16;
            private const int Gap = 4;
            private const int UnderlineThickness = 3;

            private readonly List<string> titles = new List<string>();
            private readonly List<Rectangle> rects = new List<Rectangle>();
            private int selectedIndex = -1;
            private int hoverIndex = -1;

            public event EventHandler SelectedIndexChanged;

            public TabHeaderStrip()
            {
                this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint
                        | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            }

            public int SelectedIndex
            {
                get
                {
                    return this.selectedIndex;
                }
                set
                {
                    if (value != this.selectedIndex && value >= 0 && value < this.titles.Count)
                    {
                        this.selectedIndex = value;
                        this.Invalidate();

                        if (this.SelectedIndexChanged != null)
                        {
                            this.SelectedIndexChanged(this, EventArgs.Empty);
                        }
                    }
                }
            }

            public void AddTab(string title)
            {
                this.titles.Add(title);
                this.Invalidate();
            }

            public void SetTitle(int index, string title)
            {
                if (index >= 0 && index < this.titles.Count)
                {
                    this.titles[index] = title ?? string.Empty;
                    this.Invalidate();
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;

                // 배경은 부모(폼/패널) 색을 따라간다 — 카드/폼 어디에 두어도 자연스럽다.
                Color canvas = this.Parent != null && this.Parent.Parent != null
                        ? this.Parent.Parent.BackColor
                        : Modern.Lab.Theming.ModernTheme.Background;
                g.Clear(canvas);

                int bottom = this.Height - 1;
                using (Pen dividerPen = new Pen(Modern.Lab.Theming.ModernTheme.BorderSubtle))
                {
                    g.DrawLine(dividerPen, 0, bottom, this.Width, bottom);
                }

                Color accent = Modern.Lab.Theming.ModernTheme.Accent;
                Color muted = Modern.Lab.Theming.ModernTheme.TextSecondary;
                Color hoverInk = Modern.Lab.Theming.ModernTheme.TextPrimary;

                this.rects.Clear();
                int x = 2;

                // 선택 탭은 구조 요소 규칙(SemiBold)을 따른다. 폭은 항상 SemiBold
                // 기준으로 재서 선택이 바뀌어도 탭 위치가 흔들리지 않게 한다.
                using (Font normal = new Font("Segoe UI", 10f, FontStyle.Regular))
                using (Font selectedFont = new Font("Segoe UI Semibold", 10f, FontStyle.Regular))
                {
                    for (int i = 0; i < this.titles.Count; i++)
                    {
                        bool selected = i == this.selectedIndex;
                        Size textSize = TextRenderer.MeasureText(this.titles[i], selectedFont);
                        int w = textSize.Width + PadX * 2;
                        Rectangle rect = new Rectangle(x, 0, w, this.Height);
                        this.rects.Add(rect);

                        Color fore = selected ? accent : (i == this.hoverIndex ? hoverInk : muted);
                        TextRenderer.DrawText(g, this.titles[i], selected ? selectedFont : normal, rect, fore,
                                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                        if (selected)
                        {
                            using (SolidBrush underline = new SolidBrush(accent))
                            {
                                g.FillRectangle(underline,
                                        x + PadX, this.Height - UnderlineThickness,
                                        w - PadX * 2, UnderlineThickness);
                            }
                        }

                        x += w + Gap;
                    }
                }
            }

            protected override void OnMouseClick(MouseEventArgs e)
            {
                base.OnMouseClick(e);

                for (int i = 0; i < this.rects.Count; i++)
                {
                    if (this.rects[i].Contains(e.Location))
                    {
                        this.SelectedIndex = i;
                        return;
                    }
                }
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);

                int hit = -1;
                for (int i = 0; i < this.rects.Count; i++)
                {
                    if (this.rects[i].Contains(e.Location))
                    {
                        hit = i;
                        break;
                    }
                }

                if (hit != this.hoverIndex)
                {
                    this.hoverIndex = hit;
                    this.Cursor = hit >= 0 ? Cursors.Hand : Cursors.Default;
                    this.Invalidate();
                }
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);

                if (this.hoverIndex != -1)
                {
                    this.hoverIndex = -1;
                    this.Invalidate();
                }
            }
        }
    }
}
