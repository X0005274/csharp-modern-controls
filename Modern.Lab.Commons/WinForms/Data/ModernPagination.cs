using System;
using System.ComponentModel;
using System.Drawing;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Data
{
    /// <summary>
    /// 그리드 하단 페이지 바 (신규 개념; WPF ModernPaginationControl을
    /// ElementHost로 호스팅). 서버 페이징 화면에서 사용한다.
    ///
    /// 흐름: 조회 응답의 전체 건수를 TotalCount에 넣으면 페이지 수가 계산되고,
    /// 사용자가 페이지를 클릭하면 PageChanged가 발생한다 — 폼은 CurrentPage로
    /// 해당 페이지를 서버에 요청(또는 로컬 슬라이스)해서 그리드에 바인딩한다.
    /// </summary>
    [ToolboxItem(true)]
    public class ModernPagination : WpfElementHostBase<Modern.Lab.Controls.Wpf.Data.ModernPaginationControl>
    {
        // 디자인 타임 WPF 생성이 실패한 경우(Wpf == null)에도 속성 그리드가
        // 동작하도록 하는 폴백 저장소.
        private int fallbackPageSize;

        /// <summary>현재 페이지가 바뀔 때 발생한다.</summary>
        public event EventHandler PageChanged;

        /// <summary>적절한 기본 크기로 컨트롤을 생성한다.</summary>
        public ModernPagination()
        {
            this.Size = new Size(500, 32);
            this.fallbackPageSize = 20;

            if (this.Wpf != null)
            {
                this.Wpf.PageChanged += this.OnWpfPageChanged;
            }
        }

        /// <summary>전체 건수. 조회 응답을 받을 때마다 갱신한다.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TotalCount
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.TotalCount;
                }

                return 0;
            }
            set
            {
                if (this.Wpf != null)
                {
                    this.Wpf.TotalCount = value;
                }
            }
        }

        /// <summary>페이지당 건수 (기본 20).</summary>
        [Category("모던 컨트롤")]
        [Description("페이지당 건수")]
        [DefaultValue(20)]
        public int PageSize
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.PageSize;
                }

                return this.fallbackPageSize;
            }
            set
            {
                this.fallbackPageSize = value;

                if (this.Wpf != null)
                {
                    this.Wpf.PageSize = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>현재 페이지 (1부터). 범위를 벗어나면 자동 보정된다.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CurrentPage
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.CurrentPage;
                }

                return 1;
            }
            set
            {
                if (this.Wpf != null)
                {
                    this.Wpf.CurrentPage = value;
                }
            }
        }

        /// <summary>전체 페이지 수 (읽기 전용, 최소 1).</summary>
        [Browsable(false)]
        public int PageCount
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.PageCount;
                }

                return 1;
            }
        }

        private void OnWpfPageChanged(object sender, EventArgs e)
        {
            if (this.PageChanged != null)
            {
                this.PageChanged(this, EventArgs.Empty);
            }
        }
    }
}
