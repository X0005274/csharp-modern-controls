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
    /// 페이지 구성 (표준 TabControl/TabPage 대응 — 계약 룰 1):
    /// - 폼 디자이너: <see cref="ModernTabPage"/>를 자식으로 직렬화한다.
    ///   전용 디자이너(ModernTabControlDesigner)가 "탭 추가/선택 탭 제거" 동사와
    ///   디자인 타임 헤더 클릭 전환을 제공한다.
    /// - 런타임 코드: 기존 <see cref="AddTab"/>(제목, 컨트롤)도 그대로 동작한다 —
    ///   내부에서 ModernTabPage를 만들어 감싼다.
    ///
    /// 페이지 배치는 Dock이 아니라 <see cref="DisplayRectangle"/>(헤더 제외 영역)로
    /// 컨트롤이 직접 잡는다 — 표준 TabControl과 같은 방식이라 디자이너가 페이지를
    /// 어떤 순서로 추가해도 레이아웃이 흔들리지 않는다.
    /// </summary>
    [ToolboxItem(true)]
    [Designer(typeof(Modern.Lab.WinForms.Design.ModernTabControlDesigner))]
    public class ModernTabControl : Panel
    {
        private const int HeaderHeight = 40;

        private readonly TabHeaderStrip header;
        private readonly List<ModernTabPage> pages = new List<ModernTabPage>();
        private double fontWidthRatio;

        /// <summary>선택 탭이 바뀔 때 발생.</summary>
        public event EventHandler SelectedIndexChanged;

        /// <summary>헤더 스트립(비직렬화 내부 자식)을 붙여 생성한다.</summary>
        public ModernTabControl()
        {
            // 헤더는 런타임 내부 자식이라 디자이너에 site되지 않으므로
            // .Designer.cs로 직렬화되지 않는다. 배치는 OnLayout에서 직접 잡는다.
            this.header = new TabHeaderStrip();
            this.header.SelectedIndexChanged += this.OnHeaderSelectedIndexChanged;
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

        /// <summary>장평(글자 가로 비율) 재정의. 0 = 전역(ModernTheme.FontWidthRatio) 사용.</summary>
        [Category("모던 컨트롤")]
        [Description("탭 헤더 장평(글자 가로 비율) 재정의 — 0 = 전역(ModernTheme.FontWidthRatio) 사용, 허용 0.8~1.2")]
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
                this.header.FontWidthRatioOverride = value;
                this.header.Invalidate();
            }
        }

        /// <summary>현재 선택된 페이지 (없으면 null).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ModernTabPage SelectedTab
        {
            get
            {
                int index = this.header.SelectedIndex;
                return index >= 0 && index < this.pages.Count ? this.pages[index] : null;
            }
        }

        /// <summary>헤더 스트립 영역(클라이언트 좌표) — 디자이너 히트테스트용.</summary>
        internal Rectangle HeaderBounds
        {
            get { return new Rectangle(0, 0, this.ClientSize.Width, HeaderHeight); }
        }

        /// <summary>
        /// 페이지가 차지하는 본문 영역: 헤더와 Padding을 제외한 나머지.
        /// 표준 TabControl처럼 이 값으로 페이지를 직접 배치한다.
        /// </summary>
        public override Rectangle DisplayRectangle
        {
            get
            {
                int width = this.ClientSize.Width - this.Padding.Horizontal;
                int height = this.ClientSize.Height - HeaderHeight - this.Padding.Vertical;
                return new Rectangle(
                        this.Padding.Left,
                        HeaderHeight + this.Padding.Top,
                        Math.Max(0, width),
                        Math.Max(0, height));
            }
        }

        /// <summary>
        /// 탭 추가 (런타임 코드 경로). content를 Dock=Fill로 담은 ModernTabPage를
        /// 만들어 붙인다. 첫 탭이 자동 선택되는 기존 동작을 유지한다.
        /// </summary>
        public void AddTab(string title, Control content)
        {
            ModernTabPage page = new ModernTabPage();
            page.Text = title;

            content.Dock = DockStyle.Fill;
            page.Controls.Add(content);

            this.Controls.Add(page);
        }

        /// <summary>탭 제목을 바꾼다 (예: "Unit History — IT10001.01"처럼 대상 표시).</summary>
        public void SetTabTitle(int index, string title)
        {
            if (index >= 0 && index < this.pages.Count)
            {
                this.pages[index].Text = title ?? string.Empty;
            }
        }

        /// <summary>ModernTabPage 자식이 붙으면 페이지로 등록한다 (디자이너/런타임 공통 경로).</summary>
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            ModernTabPage page = e.Control as ModernTabPage;
            if (page == null)
            {
                return;
            }

            page.TextChanged += this.OnPageTextChanged;
            this.SyncPagesFromControls();

            // 첫 페이지는 자동 선택 (기존 AddTab 동작 유지).
            if (this.header.SelectedIndex < 0 && this.pages.Count > 0)
            {
                this.header.SelectedIndex = 0;
            }

            this.ApplyPageVisibility();
            this.PerformLayout();
        }

        /// <summary>페이지가 빠지면 등록을 해제하고 선택을 유효 범위로 되돌린다.</summary>
        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);

            ModernTabPage page = e.Control as ModernTabPage;
            if (page == null)
            {
                return;
            }

            page.TextChanged -= this.OnPageTextChanged;
            this.SyncPagesFromControls();
            this.ApplyPageVisibility();
        }

        /// <summary>헤더를 상단 띠에, 모든 페이지를 본문 영역에 직접 배치한다.</summary>
        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            this.header.SetBounds(0, 0, this.ClientSize.Width, HeaderHeight);

            Rectangle display = this.DisplayRectangle;
            foreach (ModernTabPage page in this.pages)
            {
                page.Bounds = display;
            }

            // 직렬화/외부 코드가 페이지 Visible을 건드렸어도 선택 상태로 복원한다.
            this.ApplyPageVisibility();
        }

        /// <summary>자식 컬렉션 순서 그대로 페이지 목록·헤더 제목을 다시 만든다.</summary>
        private void SyncPagesFromControls()
        {
            this.pages.Clear();

            foreach (Control child in this.Controls)
            {
                ModernTabPage page = child as ModernTabPage;
                if (page != null)
                {
                    this.pages.Add(page);
                }
            }

            List<string> titles = new List<string>(this.pages.Count);
            foreach (ModernTabPage page in this.pages)
            {
                titles.Add(page.Text);
            }

            this.header.SetTitles(titles);
        }

        /// <summary>선택 탭만 보이게 한다 (헤더는 페이지가 아니므로 손대지 않는다).</summary>
        private void ApplyPageVisibility()
        {
            int selected = this.header.SelectedIndex;

            for (int index = 0; index < this.pages.Count; index++)
            {
                this.pages[index].Visible = index == selected;
            }
        }

        private void OnHeaderSelectedIndexChanged(object sender, EventArgs e)
        {
            this.ApplyPageVisibility();

            if (this.SelectedIndexChanged != null)
            {
                this.SelectedIndexChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>페이지 Text 변경 → 헤더 제목 동기화 (SetTabTitle도 이 경로를 탄다).</summary>
        private void OnPageTextChanged(object sender, EventArgs e)
        {
            List<string> titles = new List<string>(this.pages.Count);
            foreach (ModernTabPage page in this.pages)
            {
                titles.Add(page.Text);
            }

            this.header.SetTitles(titles);
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

            // 부모 ModernTabControl의 장평 재정의 값 (0 = 전역 사용).
            internal double FontWidthRatioOverride;

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

            /// <summary>제목 목록을 통째로 교체한다. 선택이 범위를 벗어나면 마지막 탭으로 되돌린다.</summary>
            public void SetTitles(List<string> newTitles)
            {
                this.titles.Clear();
                this.titles.AddRange(newTitles);

                if (this.selectedIndex >= this.titles.Count)
                {
                    // 마지막 페이지 제거 등 — 유효 범위로 조정 (빈 목록이면 -1).
                    this.selectedIndex = this.titles.Count - 1;

                    if (this.SelectedIndexChanged != null)
                    {
                        this.SelectedIndexChanged(this, EventArgs.Empty);
                    }
                }

                this.Invalidate();
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
                double widthRatio = Modern.Lab.Theming.ModernTheme.ResolveFontWidthRatio(this.FontWidthRatioOverride);

                using (Font normal = new Font("Segoe UI", 10f, FontStyle.Regular))
                using (Font selectedFont = new Font("Segoe UI Semibold", 10f, FontStyle.Regular))
                {
                    for (int i = 0; i < this.titles.Count; i++)
                    {
                        bool selected = i == this.selectedIndex;
                        Size textSize = Modern.Lab.WinForms.Rendering.ScaledTextRenderer.MeasureText(
                                this.titles[i], selectedFont,
                                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter, widthRatio);
                        int w = textSize.Width + PadX * 2;
                        Rectangle rect = new Rectangle(x, 0, w, this.Height);
                        this.rects.Add(rect);

                        Color fore = selected ? accent : (i == this.hoverIndex ? hoverInk : muted);
                        Modern.Lab.WinForms.Rendering.ScaledTextRenderer.DrawText(
                                g, this.titles[i], selected ? selectedFont : normal, rect, fore,
                                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter, widthRatio);

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
