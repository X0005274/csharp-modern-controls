using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Theming;

// ─────────────────────────────────────────────────────────────────────────────
// ※ 중요 — 이 파일은 의도적으로 솔루션 빌드(.csproj Compile 목록)에서 제외되어 있다.
//
//   FarPoint Spread 8은 COM(ActiveX) 컨트롤이라 이 저장소의 순수 WPF/ElementHost
//   구조와 다른 계열이며, 빌드하려면 회사 PC에 등록된 Spread 8 OCX의 interop
//   어셈블리(AxImp/TlbImp 생성물)가 필요하다. 이 개발 머신에는 그 interop이 없어
//   컴파일 검증을 하지 못했다. 회사 환경에 가져가 아래 절차로 연결한 뒤 사용한다.
//
//   [회사 적용 절차]
//   1) Spread 8 OCX를 참조에 추가하면 VS가 interop 두 개를 생성한다:
//        - AxFPSpread.dll  (AxHost 래퍼, 폼에 올리는 클래스)
//        - FPSpread.dll     (원시 COM 타입/열거형)
//      ※ 네임스페이스/클래스 이름은 생성물에 따라 다를 수 있다
//        (예: AxFPSpread.AxfpSpread, AxFPUSpread.AxfpSpread 등).
//        아래 base 클래스와 using을 실제 생성물 이름에 맞춰 한 곳만 고치면 된다.
//   2) 이 파일을 회사 컨트롤 라이브러리에 포함(Compile)한다.
//   3) 폼의 기존 AxfpSpread 필드 선언 타입을 ModernSpreadGrid로 바꾸면
//      기존 Spread 코드는 그대로 두고 모던 스타일만 입혀진다(드롭인 교체).
//
//   [확인 필요 지점]  Spread 8 COM API는 버전/빌드에 따라 멤버명이 조금씩 다르다.
//   "// ※확인" 주석이 붙은 줄이 회사 Spread 버전에서 검증이 필요한 부분이다.
//   반대로 색상은 ModernTheme 팔레트(디자인 토큰의 GDI+ 미러)에서 읽으므로
//   6종 테마가 전부 적용되고, 폰트/치수 상수는 토큰 확정 값이다.
// ─────────────────────────────────────────────────────────────────────────────

namespace Modern.Lab.WinForms.Controls.Data
{
    /// <summary>
    /// FarPoint Spread 8(COM)을 현재 모던 컨트롤과 동일한 디자인 언어로 스타일링한
    /// 공통 그리드. ModernDataGrid(WPF)와 같은 드롭인 API를 제공하되 실체는
    /// Spread 8이므로, 기존 Spread 기반 화면/코드를 유지하면서 외형만 통일한다.
    ///
    /// 호환 멤버(ModernDataGrid와 동일): DataSource(DataTable/DataView),
    /// ConfigureColumns(...), RowCount, SelectedItem, SelectedIndex, SelectionChanged.
    ///
    /// 스타일: 헤더 SemiBold, 짝수행 교차색, 액센트 선택색, Segoe UI 9pt,
    /// 행 높이 32 / 헤더 36 — 색은 전부 ModernTheme 팔레트에서 읽으므로
    /// 6종 테마(Light/Dark/Gray/Purple/Orange/Tomato)가 그대로 적용된다
    /// (다른 컨트롤처럼 앱 시작 시 ModernTheme.Mode를 설정하면 끝).
    /// </summary>
    [ToolboxItem(true)]
    [DefaultEvent("SelectionChanged")]
    public class ModernSpreadGrid : AxFPSpread.AxfpSpread   // ※확인: 실제 interop 클래스명으로 교체
    {
        // ===== 디자인 토큰 (ModernTheme 팔레트 = Themes/Tokens.<테마>.xaml 미러) =====
        // static readonly가 아니라 속성인 이유: ModernTheme.Mode가 앱 시작 시
        // 설정된 뒤에 읽혀야 테마 색이 잡힌다 (필드는 타입 로드 시점에 고정됨).
        private static Color HeaderBackColor { get { return ModernTheme.GridHeaderBackground; } }
        private static Color HeaderForeColor { get { return ModernTheme.TextPrimary; } }
        private static Color GridLineColor { get { return ModernTheme.BorderSubtle; } }
        private static Color RowBackColor { get { return ModernTheme.Surface; } }
        private static Color RowAltBackColor { get { return ModernTheme.GridRowAlt; } }
        private static Color CellForeColor { get { return ModernTheme.TextPrimary; } }
        private static Color SelectBackColor { get { return ModernTheme.SelectionBackground; } }
        private static Color SelectForeColor { get { return ModernTheme.SelectedText; } }

        private const string FontName = "Segoe UI";   // Font.Family (한글 폴백은 OS가 처리)
        private const float FontSize = 9f;             // Font.Size.Body 12 DIU = 9pt
        private const int RowHeightPx = 32;            // Size.RowHeight
        private const int HeaderHeightPx = 36;         // Size.GridHeaderHeight

        // Spread는 헤더 행(0)과 데이터 행(1..N)을 함께 다룬다. -1은 "모든 행/열".
        private const int HeaderRow = 0;
        private const int AllCellsIndex = -1;

        // 명시적 컬럼 정의(ConfigureColumns). null이면 DataSource 컬럼을 자동 사용.
        private ModernDataGridColumn[] columns;

        // 폴백 저장소 — 데이터/컬럼은 바인딩 시 Spread 셀로 채운다.
        private object dataSource;

        /// <summary>행 선택이 바뀔 때 발생한다(ModernDataGrid와 동일 이름).</summary>
        public event EventHandler SelectionChanged;

        public ModernSpreadGrid()
        {
            this.columns = null;
            this.dataSource = null;
        }

        // ===== 드롭인 API (ModernDataGrid와 동일 형태) =====

        /// <summary>
        /// 데이터 소스: DataTable 또는 DataView. 할당하면 Spread 셀을 다시 채우고
        /// 스타일을 재적용한다. null이면 데이터 영역을 비운다.
        /// (Spread 8 COM의 ADO DataSource와 달리 ADO.NET을 셀에 직접 채우므로
        ///  타입·형식 처리가 예측 가능하다.)
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object DataSource
        {
            get { return this.dataSource; }
            set
            {
                this.dataSource = value;
                this.BindData();
            }
        }

        /// <summary>현재 데이터 행 수(헤더 제외). Spread의 MaxRows와 동일.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int RowCount
        {
            get { return this.MaxRows; }   // ※확인: 데이터 행 개수 속성(MaxRows)
        }

        /// <summary>선택된 데이터 행의 원본(DataRowView). 미선택/범위밖이면 null.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedItem
        {
            get
            {
                DataView view = this.ToView(this.dataSource);
                int index = this.SelectedIndex;

                if (view == null || index < 0 || index >= view.Count)
                {
                    return null;
                }

                return view[index];
            }
        }

        /// <summary>선택된 데이터 행 인덱스(0 기반, 미선택이면 -1).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get
            {
                // Spread 활성 셀의 행(ActiveRow)은 1 기반, 헤더가 0 → 데이터 인덱스는 -1.
                int activeRow = this.ActiveRow;   // ※확인: 활성 행 속성(ActiveRow)
                return activeRow >= 1 ? activeRow - 1 : -1;
            }
            set
            {
                if (value >= 0 && value < this.MaxRows)
                {
                    this.Row = value + 1;         // ※확인: 활성 셀 이동(Row/Col 지정)
                    this.Col = 1;
                    this.Action = 0;              // ※확인: ActionActiveCell 등 셀 이동 액션
                }
            }
        }

        /// <summary>
        /// 컬럼을 명시적 정의로 지정한다(ModernDataGrid.ConfigureColumns와 동일 시그니처).
        /// DataSource 할당 전에 호출한다.
        /// </summary>
        public void ConfigureColumns(params ModernDataGridColumn[] definitions)
        {
            this.columns = definitions;
        }

        // ===== 스타일 적용 (★ Spread 8 COM 연결 지점) =====

        /// <summary>
        /// OCX 핸들 생성 직후 한 번 모던 스타일을 적용한다. 디자이너에서 OCX 생성이
        /// 실패해도 폼이 죽지 않도록 예외를 삼킨다(라이브러리 공통 방어 정책).
        /// </summary>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            try
            {
                this.ApplyModernStyle();
            }
            catch (Exception)
            {
                // 디자인 타임/미등록 OCX 등에서의 실패는 무시(외형만 기본값으로 남음).
            }
        }

        // 그리드 전역 외형을 디자인 토큰으로 맞춘다. 아래 Spread 속성명들은
        // 버전에 따라 다를 수 있어 "// ※확인"으로 표시했다.
        private void ApplyModernStyle()
        {
            // 평면(비3D) 외형 + 얇은 테두리 — 모던 룩의 기본.
            this.Appearance = 0;      // ※확인: 0 = Flat
            this.BorderStyle = 0;     // ※확인: 0 = None(카드가 테두리를 담당)

            // 격자선: 은은한 회색 실선.
            this.GridColor = OleColor(GridLineColor);   // ※확인: 격자색(GridColor, OLE_COLOR)

            // 데이터 영역 기본 셀: 흰 배경 + 진한 텍스트 + Segoe UI 9pt.
            this.Row = AllCellsIndex;
            this.Col = AllCellsIndex;
            this.BackColor = RowBackColor;               // 활성 범위(전체) 배경
            this.ForeColor = CellForeColor;
            this.FontName = FontName;                    // ※확인: 셀 폰트명(FontName)
            this.FontSize = FontSize;                    // ※확인: 셀 폰트 크기(FontSize)
            this.FontBold = false;                       // 데이터는 Regular

            // 행 높이 32 / 헤더 높이 36.
            this.RowHeight(AllCellsIndex, RowHeightPx);  // ※확인: 행 높이 설정(RowHeight)
            this.RowHeight(HeaderRow, HeaderHeightPx);

            // 열 헤더(행 0): 옅은 파랑 배경 + SemiBold(굵게) + 진한 텍스트.
            this.Row = HeaderRow;
            this.Col = AllCellsIndex;
            this.BackColor = HeaderBackColor;
            this.ForeColor = HeaderForeColor;
            this.FontBold = true;                        // 구조 요소는 SemiBold → 굵게

            // 행 헤더(열 0)는 모던 룩에서 숨긴다(폭 0). 필요 시 표시로 되돌린다.
            this.ColWidth(0, 0);                         // ※확인: 열 폭 설정(ColWidth)

            // 선택 강조: 액센트 계열 배경 + 진한 파랑 텍스트, 전체 행 선택.
            this.SelBackColor = OleColor(SelectBackColor);   // ※확인: 선택 배경(SelBackColor)
            this.SelForeColor = OleColor(SelectForeColor);   // ※확인: 선택 글자(SelForeColor)
            this.OperationMode = 2;                          // ※확인: 2 = Row(전체 행 선택)

            // 읽기 전용(ModernDataGrid는 편집 불가). 편집이 필요하면 이 줄을 제거한다.
            this.Protect = true;                             // ※확인: 셀 보호(Protect)
        }

        // ===== 데이터 → Spread 셀 채우기 =====

        // DataTable/DataView를 헤더 + 셀로 채우고 스타일을 재적용한다.
        private void BindData()
        {
            DataView view = this.ToView(this.dataSource);

            // 갱신 중 화면 깜빡임/재계산 억제.
            this.ReDraw = false;   // ※확인: 다시그리기 억제(ReDraw)

            try
            {
                if (view == null)
                {
                    this.MaxRows = 0;
                    return;
                }

                ColumnPlan[] plan = this.BuildColumnPlan(view.Table);

                this.MaxCols = plan.Length;
                this.MaxRows = view.Count;

                this.FillHeaders(plan);
                this.FillCells(view, plan);
                this.ApplyColumnLayout(plan);
                this.ApplyAlternatingRows(view.Count);
            }
            finally
            {
                this.ReDraw = true;

                try
                {
                    this.ApplyModernStyle();
                }
                catch (Exception)
                {
                    // 런타임 이전(핸들 미생성 등)에는 OnHandleCreated에서 다시 적용된다.
                }

                this.RaiseSelectionChanged();
            }
        }

        // 열 순서/헤더/폭/정렬 계획을 만든다. ConfigureColumns가 있으면 그 정의를,
        // 없으면 DataTable의 모든 컬럼을 그대로 사용한다.
        private ColumnPlan[] BuildColumnPlan(DataTable table)
        {
            if (this.columns != null && this.columns.Length > 0)
            {
                ColumnPlan[] plan = new ColumnPlan[this.columns.Length];

                for (int index = 0; index < this.columns.Length; index++)
                {
                    ModernDataGridColumn definition = this.columns[index];
                    plan[index] = new ColumnPlan
                    {
                        FieldName = definition.DataPropertyName,
                        HeaderText = definition.HeaderText,
                        Width = definition.Width,
                        Format = definition.Format,
                        Alignment = definition.TextAlignment
                    };
                }

                return plan;
            }

            ColumnPlan[] autoPlan = new ColumnPlan[table.Columns.Count];

            for (int index = 0; index < table.Columns.Count; index++)
            {
                DataColumn column = table.Columns[index];
                autoPlan[index] = new ColumnPlan
                {
                    FieldName = column.ColumnName,
                    HeaderText = column.Caption,
                    Width = 0d,
                    Format = null,
                    Alignment = GridTextAlignment.Left
                };
            }

            return autoPlan;
        }

        private void FillHeaders(ColumnPlan[] plan)
        {
            for (int index = 0; index < plan.Length; index++)
            {
                // Spread 열 헤더는 (열=index+1, 행=0)에 텍스트를 넣는다.
                this.SetText(index + 1, HeaderRow, plan[index].HeaderText ?? string.Empty); // ※확인: SetText(col,row,text)
            }
        }

        private void FillCells(DataView view, ColumnPlan[] plan)
        {
            for (int rowIndex = 0; rowIndex < view.Count; rowIndex++)
            {
                DataRowView rowView = view[rowIndex];

                for (int colIndex = 0; colIndex < plan.Length; colIndex++)
                {
                    string text = this.FormatValue(rowView, plan[colIndex]);
                    this.SetText(colIndex + 1, rowIndex + 1, text);   // 데이터는 행 1부터
                }
            }
        }

        // 컬럼 폭과 가로 정렬을 적용한다.
        private void ApplyColumnLayout(ColumnPlan[] plan)
        {
            for (int index = 0; index < plan.Length; index++)
            {
                int spreadCol = index + 1;

                if (plan[index].Width > 0d)
                {
                    this.ColWidth(spreadCol, (float)plan[index].Width);   // ※확인: ColWidth(col, width)
                }

                // 정렬: 헤더(0)와 데이터 셀 모두에 적용.
                this.Col = spreadCol;
                this.Row = AllCellsIndex;
                this.TypeHAlign = ToSpreadHAlign(plan[index].Alignment);  // ※확인: 가로 정렬(TypeHAlign)
            }
        }

        // 짝수 데이터 행에 교차 배경색을 넣어 ModernDataGrid의 AlternatingRowBackground를 재현한다.
        private void ApplyAlternatingRows(int rowCount)
        {
            for (int rowIndex = 1; rowIndex <= rowCount; rowIndex++)
            {
                this.Row = rowIndex;
                this.Col = AllCellsIndex;
                this.BackColor = (rowIndex % 2 == 0) ? RowAltBackColor : RowBackColor;
            }
        }

        // ===== 헬퍼 =====

        // 셀 표시 문자열: 컬럼에 Format이 지정되면 그 형식으로, 아니면 ToString.
        private string FormatValue(DataRowView rowView, ColumnPlan column)
        {
            if (!rowView.Row.Table.Columns.Contains(column.FieldName))
            {
                return string.Empty;
            }

            object value = rowView[column.FieldName];

            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(column.Format) && value is IFormattable)
            {
                return ((IFormattable)value).ToString(column.Format, null);
            }

            return value.ToString();
        }

        // 입력 데이터 소스를 DataView로 정규화한다. Spread 8 실사용의 대부분인
        // DataTable/DataView를 지원한다(그 외 타입은 명시적 예외로 알린다).
        private DataView ToView(object source)
        {
            if (source == null)
            {
                return null;
            }

            DataView asView = source as DataView;

            if (asView != null)
            {
                return asView;
            }

            DataTable asTable = source as DataTable;

            if (asTable != null)
            {
                return asTable.DefaultView;
            }

            throw new NotSupportedException(
                "ModernSpreadGrid.DataSource는 DataTable 또는 DataView만 지원합니다. " +
                "받은 형식: " + source.GetType().FullName);
        }

        // System.Drawing.Color → OLE_COLOR(0x00BBGGRR). Spread COM의 색 속성은
        // 대개 OLE_COLOR(uint)를 받으므로 변환해 넘긴다.
        private static uint OleColor(Color color)
        {
            return (uint)(color.R | (color.G << 8) | (color.B << 16));
        }

        // ModernDataGrid의 정렬 열거형 → Spread의 가로 정렬 코드.
        private static int ToSpreadHAlign(GridTextAlignment alignment)
        {
            // ※확인: Spread TypeHAlign 코드 (0=Left, 1=Right, 2=Center 로 가정)
            switch (alignment)
            {
                case GridTextAlignment.Center:
                    return 2;
                case GridTextAlignment.Right:
                    return 1;
                default:
                    return 0;
            }
        }

        private void RaiseSelectionChanged()
        {
            if (this.SelectionChanged != null)
            {
                this.SelectionChanged(this, EventArgs.Empty);
            }
        }

        // 열 렌더링 계획 — DataTable 컬럼 또는 ConfigureColumns 정의에서 만든다.
        private sealed class ColumnPlan
        {
            public string FieldName;
            public string HeaderText;
            public double Width;
            public string Format;
            public GridTextAlignment Alignment;
        }
    }
}
