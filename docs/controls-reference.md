# Modern.Lab 공통 컨트롤 사용법 레퍼런스

컨트롤별 주요 속성·이벤트·메서드의 사용법을 예제와 함께 정리한 문서다.
"기존 컨트롤을 무엇으로 어떻게 교체하는가"는 [migration/](migration/) 폴더의
컨트롤별 교체 가이드를 참고하고, 이 문서는 "교체한 뒤 어떻게 쓰는가"를 다룬다.

- 네임스페이스: 래퍼(WinForms) `Modern.Lab.WinForms.Controls.*`,
  컬럼 정의 등 공용 타입 `Modern.Lab.Controls.Wpf.*`
- 모든 컨트롤 공통: `Enabled` 전파, 디자이너에서 스냅샷 미리보기 표시,
  높이 32(컨트롤) / 카드류는 자유
- 텍스트 렌더링: 전 컨트롤이 부드러운 Ideal 포매팅 + ClearType (WPF 자체 렌더링이라
  MacType 등 GDI 후킹 도구와 무관하게 동일하게 보임)
- 입력 컨트롤 공통 (`ModernTextBox`/`ModernDatePicker`/`ModernComboBox`/`ModernCheckComboBox`):
  `Required = true`면 **값이 비어 있는 동안만 필드 오른쪽에 작은 빨간 점(●)** 이 표시되고
  입력/선택하면 사라짐 — "아직 안 채운 필수 항목"을 알려주는 상태 기반 표시
  (라벨의 `Required` 별표와 세트)

## 목차

1. [공통 데이터 계약](#공통-데이터-계약)
2. [ModernLabel](#modernlabel) — 라벨
3. [ModernButton](#modernbutton) — 버튼
4. [ModernDropDownButton](#moderndropdownbutton) — 드롭다운 버튼(버튼+메뉴)
5. [ModernCheckBox](#moderncheckbox) — 체크박스
6. [ModernToggleSwitch](#moderntoggleswitch) — 온/오프 토글 스위치
7. [ModernTextBox](#moderntextbox) — 텍스트 입력 + 자동완성
8. [ModernNumericTextBox](#modernnumerictextbox) — 숫자/금액 입력
9. [ModernDatePicker](#moderndatepicker) — 날짜 선택
10. [ModernMonthPicker](#modernmonthpicker) — 년월 선택
11. [ModernComboBox](#moderncombobox) — 콤보(검색형·멀티컬럼)
12. [ModernCheckComboBox](#moderncheckcombobox) — 체크 콤보(다중 선택)
13. [ModernRadioGroup](#modernradiogroup) — 라디오 그룹(배타 선택)
14. [ModernTreeView](#moderntreeview) — 트리(조직도/계층 선택)
15. [ModernDataGrid](#moderndatagrid) — 데이터 그리드
16. [ModernPagination](#modernpagination) — 페이지 바
17. [ModernKpiCard / ModernSummaryList](#modernkpicard--modernsummarylist) — 통계 표시
17-1. [ModernStepIndicator](#modernstepindicator) — 진행 단계 표시
18. [ModernStatusBadge](#modernstatusbadge) — 상태 배지(pill)
19. [ModernBusyOverlay](#modernbusyoverlay) — 로딩 오버레이
20. [ModernToast](#moderntoast) — 자동 소멸 알림
21. [ModernCardPanel](#moderncardpanel) — 카드 판넬
22. [ModernGroupBox](#moderngroupbox) — 타이틀 있는 카드(그룹박스)
22-1. [ModernSplitContainer](#modernsplitcontainer) — 크기 조절 스플리터
22-2. [ModernTabControl](#moderntabcontrol) — 언더라인 탭
23. [ModernDataGridColumn](#moderndatagridcolumn) — 컬럼 정의 (그리드/콤보 공용)
24. [테마 (ModernTheme)](#테마-moderntheme--라이트다크--틴트-4종) — 라이트/다크/그레이/퍼플/오렌지/토마토

---

## 공통 데이터 계약

데이터 바인딩 컨트롤(콤보·체크콤보·그리드·요약 목록)은 전부 같은 규칙을 따른다.

| 규칙 | 의미 |
|---|---|
| `DataSource` 수용 형식 | `DataTable` / `DataView` / `IList` / `IEnumerable` — 서버 응답을 그대로 할당 |
| 순서 내성 | `SelectedValue`(또는 `CheckedValues`)를 `DataSource`보다 **먼저** 설정해도 됨 — 보류했다가 데이터 도착 시 적용 |
| 재할당 리셋 | `DataSource`를 다시 할당하면 선택/체크가 깨끗하게 초기화되고 변경 이벤트는 정확히 1회 발생 |
| null 안전 | null/빈 데이터는 빈 목록으로 표시, 예외 없음 |
| 스레드 | 백그라운드 조회 후 `this.Invoke(...)`로 UI 스레드에서 할당하는 기존 패턴 그대로 동작 |
| 수동 데이터 | 폼이 데이터를 조회해서 할당한다 — 컨트롤은 서버를 모른다 |

```csharp
// 전형적인 서버 조회 패턴 — 기존 폼 코드와 동일
private void OnSearchClick(object sender, EventArgs e)
{
    DataTable reply = this.CallServer("EMP_LIST", this.BuildRequest());  // 폼의 기존 통신 코드
    this.gridEmployee.DataSource = reply;                                // 할당만 하면 끝
}
```

---

## ModernLabel

텍스트 라벨. `Font`/`ForeColor`를 직접 지정하는 대신 **역할(Kind)** 을 고른다.

### 속성

| 속성 | 타입 | 기본값 | 설명 |
|---|---|---|---|
| `Text` | string | "레이블" | 표시 텍스트. `Control.Text` override라 기존 코드/리소스 그대로 동작 |
| `Kind` | `LabelKind` | `Body` | 타이포그래피 역할 — 크기·굵기·색이 자동 결정 |
| `Required` | bool | false | true면 텍스트 뒤에 빨간 별표(\*) 표시 (필수 입력 표시) |
| `TitleBar` | bool | false | `Kind=Title`일 때 텍스트 왼쪽에 액센트색 세로 타이틀 바 표시. Title이 아니면 무시됨 |

`Kind` 값:

| 값 | 모양 | 용도 |
|---|---|---|
| `Body` | 12px Regular, 진한 색 | 일반 텍스트 |
| `Title` | 16px SemiBold | 섹션 제목 |
| `Label` | 12px SemiBold, 회색 | 입력 필드 앞 라벨 |
| `Helper` | 12px Regular, 회색 | 보조 설명 |

### 예제

```csharp
this.lblName.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;
this.lblName.Text = "이름";
this.lblName.Required = true;   // "이름 *" 로 표시

this.lblPageTitle.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Title;
this.lblPageTitle.TitleBar = true;   // "▎직원관리" 처럼 왼쪽에 파란 세로 바
this.lblPageTitle.Text = "직원관리";
```

---

## ModernButton

버튼. 색을 직접 칠하는 대신 **위계(Kind)** 를 고른다.

### 속성·이벤트

| 멤버 | 타입 | 설명 |
|---|---|---|
| `Text` | string | 캡션. `Control.Text` override |
| `Kind` | `ButtonKind` | 버튼 위계 (아래 표) |
| `IconGlyph` | string | Segoe MDL2 Assets 글리프 (예: `""` 저장 아이콘). 비우면 아이콘 없음 |
| `Click` | 이벤트 | 표준 WinForms 이벤트 — 기존 핸들러 그대로 |

`Kind` 위계 (화면당 `Primary`와 `Execute`는 각각 1개 권장):

| 값 | 모양 | 용도 |
|---|---|---|
| `Primary` | 파랑 채움 | 화면의 대표 동작 (조회) |
| `Execute` | 초록 채움 | 중요 실행 동작 (실행/승인) |
| `Secondary` | 흰 배경 + 회색 외곽선 | 일반 동작 (신규/저장/수정) |
| `Danger` | 빨강 글자·외곽선 | 파괴적 동작 (삭제) |
| `Subtle` | 테두리 없는 텍스트 | 저강조 동작 (초기화/엑셀) |

### 예제

```csharp
this.btnSearch.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Primary;
this.btnSearch.Text = "조회";
this.btnSearch.Click += this.OnSearchClick;

this.btnDelete.Kind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Danger;
this.btnDelete.Text = "삭제";
```

---

## ModernDropDownButton

버튼 + 메뉴 (`Button`+`ContextMenuStrip` 조합 대체). 클릭하면 항목 메뉴가 열리고,
항목 클릭 시 `ItemClicked`가 발생한다 — 엑셀 내보내기 옵션처럼 한 버튼에 여러 동작을
묶을 때 사용. 캡션 옆 셰브런(▼)은 자동으로 붙는다.

### 속성 / 이벤트

| 멤버 | 설명 |
|---|---|
| `Text` | 버튼 캡션 (`Control.Text` override, localizable) |
| `DataSource` / `DisplayMember` / `ValueMember` | 메뉴 항목 — 공통 데이터 계약과 동일 |
| `ItemClicked` | 항목 클릭 시 발생. `e.Value`(코드) / `e.DisplayText`(명칭) 제공 |

```csharp
this.ddExcel.Text = "엑셀";
this.ddExcel.DisplayMember = "EXPORT_NAME";
this.ddExcel.ValueMember = "EXPORT_CODE";
this.ddExcel.DataSource = exportTable;   // PAGE 현재 페이지 / ALL 전체 / CSV ...
this.ddExcel.ItemClicked += this.OnExcelItemClicked;

private void OnExcelItemClicked(object sender, DropDownItemClickedEventArgs e)
{
    string code = e.Value as string;   // 분기 처리
}
```

버튼 모양은 보조(Secondary) 스타일 고정. 권장 크기: 100~130×32.

---

## ModernCheckBox

단독 체크박스 (`CheckBox` 대체). 둥근 사각 박스 + 액센트 채움 + 흰 체크 글리프 —
체크콤보 드롭다운 항목과 동일한 비주얼.

### 속성 / 이벤트

| 멤버 | 타입 | 설명 |
|---|---|---|
| `Text` | string | 박스 옆 레이블 (`Control.Text` override, localizable) |
| `Checked` | bool | 체크 상태 |
| `CheckedChanged` | event | 상태가 바뀔 때 1회 발생 (같은 값 재할당은 미발생) |

```csharp
this.chkRecentOnly.Text = "2020년 이후 입사";
this.chkRecentOnly.CheckedChanged += this.OnRecentOnlyCheckedChanged;

if (this.chkRecentOnly.Checked)
{
    conditions.Add("HIRE_DATE >= '2020-01-01'");
}
```

3상태(Indeterminate)는 미지원. 권장 크기: 폭은 텍스트에 맞게, 높이 24.

---

## ModernToggleSwitch

온/오프 토글 스위치 — `ModernCheckBox`와 **동일한 API**(`Text`/`Checked`/`CheckedChanged`)에
비주얼만 알약 트랙 + 원형 썸. **설정성 "켬/끔"** 용도 (다중 선택/포함에는 체크박스).

```csharp
this.tglShowEmail.Text = "이메일 표시";
this.tglShowEmail.Checked = true;
this.tglShowEmail.CheckedChanged += this.OnShowEmailToggled;   // 켬/끔에 따라 화면 구성 변경
```

권장 크기: 폭은 텍스트에 맞게, 높이 24.

---

## ModernTextBox

단일행 텍스트 입력. 플레이스홀더·Enter 조회·자동완성(초성 검색 포함) 내장.

### 속성·이벤트

| 멤버 | 타입 | 설명 |
|---|---|---|
| `Text` | string | 입력 텍스트. 한글 조합 중에는 음절 확정 시점에 반영 |
| `PlaceholderText` | string | 비어 있을 때 회색 힌트. 자음 하나만 쳐도 즉시 사라짐 |
| `ReadOnly` | bool | 읽기 전용 (배경이 옅게 변함) |
| `CharacterCasing` | `CharacterCasing` | 입력 즉시 대문자/소문자 강제 (WinForms 호환 이름, 기본 Normal). 코드/ID 입력용 |
| `AllowedCharacters` | string | 허용 문자 집합 (빈 값 = 제한 없음). 지정하면 타이핑·붙여넣기·IME 무관하게 그 외 문자가 제거됨. 예: `"ABC…XYZ0123456789.-"` |
| `TextChanged` | 이벤트 | 표준 WinForms 이벤트 |
| `EnterPressed` | 이벤트 | Enter 키 입력 시 발생 — **기존 `KeyDown`으로 Enter를 잡던 코드는 이 이벤트로 교체** (WPF 에디터가 처리한 키는 WinForms `KeyDown`으로 오지 않음) |
| `AutoCompleteMode` | `AutoCompleteMode` | `None` 외 값은 모두 제안 드롭다운(Suggest)으로 동작 |
| `AutoCompleteSource` | `AutoCompleteSource` | **`CustomSource`만 지원** |
| `AutoCompleteCustomSource` | `AutoCompleteStringCollection` | 후보 목록. 재할당으로 갱신 — 입력 중(포커스 상태) 재할당하면 드롭다운이 즉시 다시 필터링되므로, `TextChanged` + 디바운스 + 서버 조회로 **typeahead** 구현 가능 |

### 예제 — 검색 입력 + 자동완성

```csharp
this.txtName.PlaceholderText = "이름 검색";
this.txtName.EnterPressed += this.OnSearchClick;   // Enter로 바로 조회

// 자동완성: 기존 WinForms TextBox와 동일한 3종 세트
AutoCompleteStringCollection names = new AutoCompleteStringCollection();
foreach (DataRow row in employeeTable.Rows)
{
    names.Add(row["EMP_NAME"].ToString());
}
this.txtName.AutoCompleteMode = AutoCompleteMode.Suggest;
this.txtName.AutoCompleteSource = AutoCompleteSource.CustomSource;
this.txtName.AutoCompleteCustomSource = names;
// → "ㄱ"만 쳐도 김민수/김하늘/강태민 제안 (초성 검색), ↓/↑ 탐색, Enter 선택
```

---

## ModernNumericTextBox

금액/수량 입력 필드 (`NumericUpDown`·숫자용 TextBox 대체). 숫자만 치면 **천단위 콤마가
자동 삽입**되고(`1234567` → `1,234,567`) 우측 정렬로 표시된다. 중간 수정·붙여넣기에도
즉시 재형식화되며 숫자 외 문자는 무시된다.

### 속성 / 이벤트

| 멤버 | 타입 | 설명 |
|---|---|---|
| `Value` | `decimal?` | 입력 값. **`null` = 미입력(전체 조회)** — 초기화 시 `null` 할당 |
| `DecimalPlaces` | int | 허용 소수 자릿수 (기본 0 = 정수만). blur 시 자릿수 패딩(`1,234.50`) |
| `AllowNegative` | bool | 음수 허용 (기본 true) |
| `PlaceholderText` | string | 입력 전 회색 안내 (예: "만원") |
| `Required` | bool | 필수 입력 표시 (값이 비어 있는 동안 빨간 점) |
| `ValueChanged` | event | 값이 바뀔 때 1회 발생 |

```csharp
this.numSalaryFrom.PlaceholderText = "만원";
decimal? from = this.numSalaryFrom.Value;   // 조회 시
this.numSalaryFrom.Value = null;            // 초기화
```

권장 크기: 100~140×32.

---

## ModernDatePicker

날짜 선택 필드 (`DateTimePicker` 대체, 날짜 전용). 달력 팝업 또는 직접 타이핑으로 입력한다.
직접 입력은 **마스크 방식**: 숫자만 치면 `yyyy-MM-dd`로 자동 형식화되고(구분자 입력 불필요),
중간 위치를 수정해도 즉시 재형식화되며 무효 입력은 오류 없이 값 미반영으로 처리된다.

### 속성 / 이벤트

| 멤버 | 타입 | 설명 |
|---|---|---|
| `Value` | `DateTime?` | 선택 날짜. **`null` = 미선택(전체 조회)** — 초기화 시 `null` 할당 |
| `MinDate` / `MaxDate` | `DateTime?` | 달력에서 선택 가능한 범위 (`null` = 제한 없음) |
| `PlaceholderText` | string | 입력 전 회색으로 표시되는 형식 안내 (기본 `"yyyy-MM-dd"`) |
| `Required` | bool | 필수 입력 표시 (값이 비어 있는 동안 필드 오른쪽 빨간 점) |
| `ValueChanged` | event | 날짜가 바뀔 때 1회 발생 (달력·타이핑·코드 할당 공통) |

### 예제 — 구간(범위) 조회 조건

```csharp
DateTime? from = this.dtHireFrom.Value;
DateTime? to = this.dtHireTo.Value;

if (from.HasValue) { conditions.Add("HIRE_DATE >= '" + from.Value.ToString("yyyy-MM-dd") + "'"); }
if (to.HasValue) { conditions.Add("HIRE_DATE <= '" + to.Value.ToString("yyyy-MM-dd") + "'"); }

// 초기화
this.dtHireFrom.Value = null;
this.dtHireTo.Value = null;
```

권장 크기: 130×32. 표시 형식은 `yyyy-MM-dd` 고정 (문화권 무관).

---

## ModernMonthPicker

년월 선택 필드 (`yyyy-MM`). 정산·마감 등 기준월 조회용. 달력 버튼은 **12개월 그리드
팝업**을 열고 월 클릭이 곧 선택이다. 직접 입력은 숫자 6자리(`202107`) 마스크 방식.

### 속성 / 이벤트

| 멤버 | 타입 | 설명 |
|---|---|---|
| `Value` | `DateTime?` | 선택 년월 — **해당 월 1일로 정규화**. `null` = 미선택(전체) |
| `MinDate` / `MaxDate` | `DateTime?` | 팝업 선택 가능 범위 |
| `PlaceholderText` | string | 입력 전 회색 안내 (기본 `"yyyy-MM"`) |
| `Required` | bool | 필수 입력 표시 (값이 비어 있는 동안 빨간 점) |
| `ValueChanged` | event | 년월이 바뀔 때 1회 발생 |

```csharp
DateTime? month = this.monthHire.Value;
if (month.HasValue) { conditions.Add("HIRE_DATE LIKE '" + month.Value.ToString("yyyy-MM") + "%'"); }
this.monthHire.Value = null;   // 초기화
```

권장 크기: 110×32.

---

## ModernComboBox

단일 선택 콤보. 기본이 **검색형(DropDown)** 이고, 멀티컬럼 드롭다운을 지원한다.

### 속성·이벤트

| 멤버 | 타입 | 설명 |
|---|---|---|
| `DataSource` | object | 공통 데이터 계약 참고 |
| `DisplayMember` | string | 화면 표시 명칭 컬럼 |
| `ValueMember` | string | 값(코드) 컬럼 |
| `SelectedValue` | object | 선택 값(코드). `DataSource`보다 먼저 설정 가능 |
| `SelectedItem` | object | 선택 행 (`DataTable` 소스면 `DataRowView`) |
| `SelectedIndex` | int | 미선택 = -1. **-1 할당 = 선택 해제**(플레이스홀더 표시) |
| `Items` | IList | 수동 항목 (`Items.Add("사원")`). `DataSource` 지정 시 무시 |
| `DropDownStyle` | `ComboBoxStyle` | `DropDown`(기본) = 검색형: 타이핑으로 필터(초성 포함), **입력을 지우면 선택 해제**. `DropDownList` = 선택 전용 |
| `PlaceholderText` | string | 미선택/미입력 시 힌트 — "전체" 더미 행 대신 사용 권장 |
| `Text` | string | 현재 선택/입력 텍스트 (쓰기는 DropDown 모드만) |
| `SelectedIndexChanged` | 이벤트 | `DataSource` 할당 시 1회 + 사용자 선택 변경 시 |
| `ConfigureDropDownColumns(...)` | 메서드 | 멀티컬럼 드롭다운 구성 (아래) |

### 예제 — 코드/명칭 콤보 + "전체" 패턴

```csharp
// 서버 부서 테이블: (DEPT_CODE, DEPT_NAME)
this.cboDept.DisplayMember = "DEPT_NAME";
this.cboDept.ValueMember = "DEPT_CODE";
this.cboDept.PlaceholderText = "부서 전체";
this.cboDept.DataSource = deptTable;
this.cboDept.SelectedIndex = -1;            // 미선택 = 전체 (플레이스홀더 표시)

// 조회 시: 미선택이면 null → 조건 생략
string deptCode = this.cboDept.SelectedValue as string;
if (!string.IsNullOrEmpty(deptCode)) { /* DEPT_CODE = 'D3' 조건 추가 */ }
```

### 예제 — 멀티컬럼 드롭다운 (컬럼 수 제한 없음)

`ModernDataGridColumn`을 나열하는 만큼 컬럼이 생긴다. 3개든 4개든 가능:

```csharp
using Modern.Lab.Controls.Wpf.Data;

// 4컬럼 예: 코드 | 부서명 | 사업장 | 사용여부
this.cboDept.ConfigureDropDownColumns(
    new ModernDataGridColumn("DEPT_CODE", "코드", 60),
    new ModernDataGridColumn("DEPT_NAME", "부서명", 110),
    new ModernDataGridColumn("SITE_NAME", "사업장", 90),
    new ModernDataGridColumn("USE_YN", "사용", 40) { TextAlignment = GridTextAlignment.Center });
this.cboDept.DisplayMember = "DEPT_NAME";   // 선택 후 필드에는 명칭만 표시
this.cboDept.ValueMember = "DEPT_CODE";
this.cboDept.DataSource = deptTable;        // 구성 후에 DataSource 할당
```

- 드롭다운에 헤더 행 표시, 폭은 컬럼 합계만큼 자동 확장
- 검색형 모드의 타이핑 필터는 **모든 컬럼 대상** ("D3" 코드로도, "개발"/"ㄱ" 명칭으로도)

---

## ModernCheckComboBox

체크박스 다중 선택 콤보. 미체크 = "전체" 패턴에 최적.

### 속성·이벤트·메서드

| 멤버 | 타입 | 설명 |
|---|---|---|
| `DataSource` / `DisplayMember` / `ValueMember` | | 콤보와 동일 — 화면엔 명칭, 값은 코드 |
| `CheckedValues` | object[] | 체크된 코드 배열. `DataSource`보다 먼저 설정 가능. null 할당 = 전체 해제 |
| `CheckedItems` | object[] | 체크된 원본 행 (읽기 전용) |
| `ItemStyle` | `CheckItemStyle` | `CheckBox`(기본) / `Switch`(온오프 토글) |
| `PlaceholderText` | string | 미체크 시 힌트 (예: "직급 전체") |
| `Text` | string | 체크 항목을 ", "로 연결한 표시 텍스트 (읽기 전용) |
| `CheckedChanged` | 이벤트 | 체크 상태 변경 시 (일괄 변경은 1회) |
| `CheckAll()` / `UncheckAll()` | 메서드 | 전체 체크/해제 — 드롭다운 헤더와 동일 동작. 헤더 라벨은 상태에 따라 "Select all" ↔ "Deselect all"(전부 체크 시)로 바뀜 |

### 예제 — 직급 다중 필터 (서버에 코드 전송)

```csharp
// 서버 직급 테이블: (RANK_CODE, RANK_NAME) — 예: (R1, 부장)
this.cboRank.DisplayMember = "RANK_NAME";
this.cboRank.ValueMember = "RANK_CODE";
this.cboRank.PlaceholderText = "직급 전체";
this.cboRank.DataSource = rankTable;

// 조회 시 — 체크된 코드 배열을 그대로 서버 조건으로
object[] rankCodes = this.cboRank.CheckedValues;      // 예: { "R1", "R2" }
if (rankCodes != null && rankCodes.Length > 0)
{
    // RANK_CODE IN ('R1', 'R2') 조건 구성
}

// 프로그램에서 미리 체크해 두기 (DataSource보다 먼저여도 동작)
this.cboRank.CheckedValues = new object[] { "R1", "R2" };

// 초기화
this.cboRank.CheckedValues = null;
```

---

## ModernRadioGroup

배타 선택 라디오 그룹 (`GroupBox`+`RadioButton` 묶음 대체). 코드 테이블을 할당하면
행마다 모던 라디오(원형 + 액센트 점)가 가로(기본)/세로로 나열된다.

### 속성 / 이벤트

| 멤버 | 설명 |
|---|---|
| `DataSource` / `DisplayMember` / `ValueMember` | 공통 데이터 계약과 동일 |
| `SelectedValue` | 선택 값 (`null` = 미선택). `DataSource`보다 먼저 설정 가능 |
| `SelectedValueChanged` | 선택 변경 시 1회 발생 (같은 값 재할당은 미발생) |
| `Vertical` | `true`면 세로 나열 |

```csharp
this.radioSort.DisplayMember = "SORT_NAME";
this.radioSort.ValueMember = "SORT_CODE";
this.radioSort.DataSource = sortTable;          // EMP_NO 사번순 / HIRE_DATE 입사일순 ...
this.radioSort.SelectedValue = "EMP_NO";        // 기본 선택
this.radioSort.SelectedValueChanged += this.OnSortChanged;
```

배타성은 컨트롤이 보장한다 — 개별 라디오 관리 코드가 필요 없다.

---

## ModernTreeView

조직도·분류 계층 선택 트리 (`TreeView` 대체). **평면 자기참조 테이블**(키/부모키/명칭)을
그대로 할당하면 트리가 구성된다 — `TreeNode` 쌓기 코드가 필요 없다.

### 속성 / 이벤트

| 멤버 | 설명 |
|---|---|
| `DataSource` / `DisplayMember` | 공통 데이터 계약과 동일 |
| `IdMember` / `ParentIdMember` | 키/부모 키 컬럼 (부모 없음 = 루트) |
| `SelectedValue` | 선택 노드 키 (`null` = 미선택). 설정 시 조상 자동 펼침 |
| `SelectedItem` | 선택 노드의 원본 행 (읽기 전용) |
| `SelectedValueChanged` | 선택 변경 시 1회 발생 |
| `ForeColorMember` | 노드 텍스트 색 컬럼 (선택). 값은 `"#DC2626"` 같은 색 문자열 — 비었거나 해석 불가면 기본색. Scrap 등 상태 강조용 |
| `ExpandAll()` / `CollapseAll()` | 전체 펼침/접기 |

```csharp
this.treeOrg.IdMember = "ORG_CODE";
this.treeOrg.ParentIdMember = "PARENT_CODE";
this.treeOrg.DisplayMember = "ORG_NAME";
this.treeOrg.DataSource = orgTable;                    // 서버 조직 테이블 그대로
this.treeOrg.SelectedValueChanged += this.OnOrgTreeSelectionChanged;
```

노드 체크박스·지연 로딩·편집은 미지원(선택 전용). 권장 배치: 그리드 왼쪽 카드, 폭 180~240.

---

## ModernDataGrid

읽기 전용 데이터 그리드. 헤더 클릭 정렬, 컬럼 폭 마우스 조절 지원.

### 속성·이벤트·메서드

| 멤버 | 타입 | 설명 |
|---|---|---|
| `DataSource` | object | 할당 시 첫 행 자동 선택 + `SelectionChanged` 1회 |
| `ConfigureColumns(...)` | 메서드 | 명시적 컬럼 정의 — 개수 제한 없음. `DataSource` 할당 전에 호출 |
| `AutoGenerateColumns` | bool | 기본 true. `ConfigureColumns` 호출 시 자동 false |
| `RowCount` | int | 현재 행 수 — 조회 건수 표시 연동 |
| `SelectedItem` | object | 선택 행 (`DataRowView`) — 기존 `CurrentRow.DataBoundItem` 대체 |
| `SelectedIndex` | int | 미선택 = -1 |
| `SelectionChanged` | 이벤트 | 행 선택 변경 시 |
| `ShowStatusBar` | bool | 기본 false. true면 그리드 하단에 상태바 표시 — 왼쪽에 행 수 자동 표기 |
| `StatusCountFormat` | string | 상태바 행 수 형식. 기본 `"{0:N0} rows"` — `{0}`에 현재 행 수 |
| `StatusText` | string | 상태바 오른쪽 자유 텍스트 (선택 대상·조회 조건 등) |
| `RowColorMember` | string | 행 배경색 컬럼 (선택). 값은 `"#FEE2E2"` 같은 색 문자열 — 비었거나 해석 불가한 행은 기본 교차색 유지. 상태별 행 강조(예: Scrap 빨강)용. 트리 `ForeColorMember`와 짝 |

### 예제 — 컬럼 정의와 선택 행 사용

```csharp
using Modern.Lab.Controls.Wpf.Data;

// 폼 로드 시 1회
this.gridEmployee.ConfigureColumns(
    new ModernDataGridColumn("EMP_NO", "사번", 90),
    new ModernDataGridColumn("EMP_NAME", "이름", 110),
    new ModernDataGridColumn("HIRE_DATE", "입사일", 110) { TextAlignment = GridTextAlignment.Center },
    new ModernDataGridColumn("EMAIL", "이메일"));          // 폭 생략 = 남은 공간 채움

// 조회 후
this.gridEmployee.DataSource = replyTable;
this.lblCount.Text = "조회 " + this.gridEmployee.RowCount + "건";

// 선택 행 값 읽기
DataRowView row = this.gridEmployee.SelectedItem as DataRowView;
if (row != null)
{
    string empNo = row["EMP_NO"].ToString();
}

// 상태바 — 행 수는 자동, 오른쪽 텍스트는 조회 시마다 갱신
this.gridEmployee.ShowStatusBar = true;
this.gridEmployee.StatusCountFormat = "조회 {0:N0}건";
this.gridEmployee.StatusText = "부서: 개발1팀";   // 비워두면 표시 없음
```

---

## ModernPagination

그리드 하단 페이지 바. 좌측 "총 N건" + 우측 ◀ 1 2 3 ▶ (현재 페이지 중심 최대 7개).

| 멤버 | 설명 |
|---|---|
| `TotalCount` | 전체 건수 — 조회 응답마다 갱신하면 페이지 수 자동 계산 |
| `PageSize` | 페이지당 건수 (기본 20) |
| `CurrentPage` | 현재 페이지 (1부터, 범위 자동 보정) |
| `TotalCountFormat` | 좌측 건수 표기 형식. 기본 `"총 {0:N0}건"` — 영문 화면이면 `"{0:N0} records"` 등으로 |
| `PageChanged` | 페이지가 바뀔 때 1회 발생 — 해당 페이지를 조회해서 그리드에 바인딩 |

**그리드 상태바(`ModernDataGrid.ShowStatusBar`)와의 역할 구분**: 둘 다 그리드 하단에
건수를 표시하므로 **한 그리드에는 하나만** 쓴다. 페이징이 필요한 그리드 → 페이지 바
(건수 + 페이지 이동), 페이징 없는 단순 목록 → 상태바 (건수 + 문맥 텍스트).

```csharp
// 조회 완료 시
this.pagerEmployee.TotalCount = reply.TotalCount;
this.pagerEmployee.CurrentPage = 1;

// 페이지 이동 시
private void OnPagerPageChanged(object sender, EventArgs e)
{
    RequestPage(this.pagerEmployee.CurrentPage, this.pagerEmployee.PageSize);
}
```

배치: 그리드 아래 `Dock = Bottom`, 높이 32. 통계는 전체 결과 기준 유지가 관례.

**자동 PageSize**: `ModernDataGrid.VisibleRowCapacity`(스크롤 없이 보이는 행 수) +
`VisibleRowCapacityChanged` 이벤트와 연동하면 페이지 크기가 폼 높이를 따라간다.

---

## ModernKpiCard / ModernSummaryList

하단 통계 영역용 표시 컨트롤.

### ModernKpiCard — 제목 + 강조 값 카드

| 멤버 | 설명 |
|---|---|
| `Title` | 값 위 제목 (예: "조회 건수") |
| `Value` | 강조 값 텍스트 — 포맷은 폼에서 (`count.ToString("N0")` 등) |
| `Flat` | true면 카드 테두리 제거 — `ModernCardPanel` 위에 올릴 때 (중첩 카드 방지) |

```csharp
this.cardCount.Title = "조회 건수";
this.cardCount.Value = this.gridEmployee.RowCount.ToString();
```

### ModernSummaryList — 구분·건수 칩 목록

| 멤버 | 설명 |
|---|---|
| `DataSource` | 구분 컬럼 + 건수 컬럼을 가진 행 목록 (서버 GROUP BY 결과) |
| `DisplayMember` / `ValueMember` | 칩 라벨 컬럼 / 건수 컬럼 |
| `ColorMember` | 칩 배경색 컬럼 (선택; hex/색 이름 문자열, 비우면 기본색). 글자색은 배경과 같은 색상 계열로 자동 산출 (연파랑 → 진파랑 글씨) |
| `Title` | 칩 왼쪽 제목 (비우면 숨김) |
| `Flat` | 카드 테두리 제거 |

```csharp
this.listDeptCount.Title = "부서별";
this.listDeptCount.DisplayMember = "DEPT_NAME";
this.listDeptCount.ValueMember = "CNT";
this.listDeptCount.ColorMember = "COLOR";         // 선택 — 행별 "#DBEAFE" 같은 값
this.listDeptCount.DataSource = deptCountTable;   // 조회할 때마다 재할당
// → [경영지원팀 4] [개발1팀 6] ... 칩으로 표시 (COLOR 컬럼이 있으면 부서마다 다른 색)
```

---

## ModernStepIndicator

가로 진행 단계 표시(스텝 인디케이터). 공정/처리 흐름을 한 줄로 보여 "지금 어디까지
왔는지"를 한눈에 파악하게 한다. 직접 대응하는 WinForms 컨트롤은 없다.

| 멤버 | 설명 |
|---|---|
| `DataSource` | 단계 행 목록 (DataTable/DataView/IList/IEnumerable) |
| `DisplayMember` | 단계 이름 컬럼/속성 |
| `StateMember` | 단계 상태 컬럼/속성 — 값은 문자열 `Completed` / `Current` / `Pending` / `Failed` (대소문자 무시) |

상태별 모양: `Completed` 액센트 채움+체크, `Current` 흰 원+액센트 테두리(강조),
`Pending` 회색 번호, `Failed` 빨강+X. 색·글리프는 전부 디자인 토큰에서 온다.

```csharp
this.stepFlow.DisplayMember = "LABEL";
this.stepFlow.StateMember = "STATE";

DataTable steps = new DataTable();
steps.Columns.Add("LABEL", typeof(string));
steps.Columns.Add("STATE", typeof(string));
steps.Rows.Add("Created", "Completed");
steps.Rows.Add("Released", "Completed");
steps.Rows.Add("JobEnd", "Current");     // 마지막 = 현재 단계
this.stepFlow.DataSource = steps;         // → ●─●─◎ 진행 바
```

배치: 상세 카드/그리드 위에 `Dock = Top` 스트립(높이 약 56). 데이터의 실제 이벤트를
시간순으로 넣으면 마지막을 `Current`(또는 중단이면 `Failed`)로 표시하는 식으로 쓴다.

---

## ModernStatusBadge

상태 표시 pill 배지 (색 있는 `Label` 대체). 배경색만 주면 **글자색이 자동 유도**된다.

| 멤버 | 설명 |
|---|---|
| `Text` | 배지 텍스트 (`Control.Text` override, localizable) |
| `Color` | 배경색 (hex/색 이름 문자열; 비우면 중립 회색) |

```csharp
this.badgeStatus.Text = "승인";
this.badgeStatus.Color = "#DCFCE7";   // 연초록 배경 + 진초록 글씨(자동)
```

권장 크기: 텍스트에 맞는 폭 × 24.

---

## ModernBusyOverlay

조회/처리 중 대상 영역을 덮는 로딩 패널 (스피너 + 메시지). 기본 숨김.

| 멤버 | 설명 |
|---|---|
| `Busy` | `true` = 표시 + 맨 앞으로, `false` = 숨김 |
| `Message` | 안내 문구 (기본 `"처리 중..."`) |

배치: 덮을 영역과 같은 `Dock`으로, **그리드보다 먼저 `Controls.Add`** (z-순서 위).

```csharp
this.busyOverlay.Busy = true;                  // 조회 시작
// ... 백그라운드 조회 → Invoke로 결과 반영 ...
this.busyOverlay.Busy = false;                 // 완료
```

반투명은 ElementHost 제약으로 불가 — 불투명 패널로 덮는다.

---

## ModernToast

자동 소멸 알림 (완료/안내용 `MessageBox` 대체). 부모 우하단에 표시 후 자동 숨김.

```csharp
this.toastMain.Show("저장했습니다.", Modern.Lab.Controls.Wpf.Display.ToastKind.Success);
this.toastMain.Show("먼저 선택하세요.", Modern.Lab.Controls.Wpf.Display.ToastKind.Warning);
```

종류: `Info`/`Success`/`Warning`/`Error` (색 아이콘). `DurationMs` 기본 2500.
예/아니오 확인은 계속 `MessageBox` 사용 — 토스트는 통지 전용.

---

## ModernCardPanel

영역 그룹핑용 카드 컨테이너(흰 표면·옅은 테두리·radius 8). **순수 WinForms Panel**이라
어떤 WinForms 자식이든 담을 수 있다 — 조회조건 영역, 하단 영역을 감싸는 용도.

```csharp
ModernCardPanel searchCard = new ModernCardPanel();
searchCard.Dock = DockStyle.Fill;            // 또는 Location/Size/Anchor
searchCard.Padding = new Padding(12, 9, 12, 9);
searchCard.Controls.Add(this.lblName);       // 자식은 일반 Panel처럼 추가
```

팁: KpiCard/SummaryList를 카드 판넬 위에 올릴 때는 `Flat = true`로 개별 테두리를 끈다.

---

## ModernGroupBox

헤더 타이틀이 있는 카드 (`GroupBox` 대체). `ModernCardPanel` 상속의 **순수 WinForms
패널**이라 어떤 WinForms 자식이든 담을 수 있다.

```csharp
this.grpStats.Text = "조회 통계";              // 헤더 타이틀 (SemiBold + 구분선)
this.grpStats.Controls.Add(this.listDept);    // 자식은 일반 Panel처럼
// 기본 Padding(12, 40, 12, 12)이 헤더 아래 공간을 확보
```

헤더가 필요 없으면 `ModernCardPanel`, 헤더가 필요하면 `ModernGroupBox`.

---

## ModernSplitContainer

좌/우(상/하) 영역 크기를 드래그로 조절하는 스플리터 — `SplitContainer`의 대체
(순수 WinForms 컨테이너, GDI+). API는 `SplitContainer` 그대로이며 시각만 다르다:
거터는 부모 배경색(색 띠가 아닌 "간격"), 중앙에 그립 필 — 평상시 `BorderSubtle`,
오버/드래그 중 `Accent`. 드래그 후 점선 포커스 사각형이 남지 않는다.

드래그 방식 `DeferredDrag`(기본 true): 드래그 중 가는 액센트 가이드 라인만
움직이고 놓을 때 한 번 적용 — ElementHost가 많은 화면(리사이즈 스텝당 수백 ms)
에서도 드래그가 매끄럽다. 가벼운 화면은 `false`로 실시간 리플로우 가능.

```csharp
this.splitMain.Orientation = Orientation.Vertical;   // 좌/우 분할
this.splitMain.Panel1.Controls.Add(this.leftZone);   // 트리/목록 등
this.splitMain.Panel2.Controls.Add(this.rightZone);  // 상세 영역
this.splitMain.Panel1MinSize = 240;
this.splitMain.Panel2MinSize = 480;
this.splitMain.SplitterDistance = 340;
```

주의: 코드로 직접 배치할 때는 디자이너처럼 **`ISupportInitialize.BeginInit()/EndInit()`로
감싸야 한다** — 초기 크기보다 큰 `MinSize`/`SplitterDistance`를 그냥 설정하면 예외.
자세한 내용과 `.Designer.cs` 예시는 `migration/ModernSplitContainer.md`.

---

## ModernTabControl

언더라인(피벗) 스타일 탭 컨테이너 — `TabControl`의 대체 (순수 WinForms, GDI+).
선택 탭은 액센트색 SemiBold + 밑줄, 색은 팔레트를 읽어 6개 테마 자동 대응.
`TabPage` 모델 대신 `AddTab(제목, 컨트롤)`로 페이지를 붙인다.

```csharp
this.tabHistory.AddTab("Item History", this.gridHistory);
this.tabHistory.AddTab("Unit History", this.gridUnitHistory);
this.tabHistory.SetTabTitle(1, "Unit History — " + unitId);  // 데이터만 갱신, 전환 없음
this.tabHistory.SelectedIndexChanged += this.OnHistoryTabChanged;
```

자세한 멤버/주의는 `migration/ModernTabControl.md`.

---

## ModernDataGridColumn

그리드(`ConfigureColumns`)와 콤보 드롭다운(`ConfigureDropDownColumns`)이 공유하는
컬럼 정의. **나열하는 만큼 컬럼이 생긴다 — 3개, 4개, 그 이상도 가능.**

| 생성자/속성 | 설명 |
|---|---|
| `new ModernDataGridColumn(컬럼명, 헤더)` | 폭 생략 = 남은 공간 채움(star) |
| `new ModernDataGridColumn(컬럼명, 헤더, 폭)` | 픽셀 고정 폭 |
| `TextAlignment` | `Left`(기본) / `Center` / `Right` |
| `Format` | 표시 형식 — 숫자 `"N0"`/`"N2"`, 날짜 `"yyyy-MM-dd"` 등. **원본이 타입 컬럼(int/decimal/DateTime)일 때만 적용**되고, 정렬은 형식과 무관하게 원본 값 기준 |

```csharp
// 원하는 개수만큼 나열
new ModernDataGridColumn("EMP_NO", "사번", 90),
new ModernDataGridColumn("EMP_NAME", "이름", 110),
new ModernDataGridColumn("SALARY", "급여", 100) { TextAlignment = GridTextAlignment.Right, Format = "N0" },
new ModernDataGridColumn("NOTE", "비고")   // 마지막은 폭 생략으로 채움
```

---

## 테마 (ModernTheme) — 라이트/다크 + 틴트 4종

전 컨트롤 공통의 테마 (`Modern.Lab.Theming.ModernTheme`). 기본은 라이트이며,
**앱 시작 시 첫 컨트롤을 만들기 전에 한 번** `Mode`를 설정하는 opt-in 방식이다 —
설정하지 않으면 기존과 완전히 동일하므로, 이 라이브러리를 쓰는 다른 시스템에는
영향이 없다.

| 테마 (`ThemeMode`) | 특징 |
|---|---|
| `Light` (기본) | 기존 라이트 Fluent — 블루 액센트 `#0078D4` |
| `Dark` | 어두운 배경 전체 반전 |
| `Gray` | **어두운 슬레이트 회색** (다크 계열 dim — Dark보다 한 톤 밝음), 슬레이트 블루그레이 액센트 |
| `Purple` | 라벤더 **파스텔** — 흰 카드 + 뚜렷한 파스텔 배경/테두리, 바이올렛 액센트 `#7C3AED` |
| `Orange` | 피치 **파스텔** — 흰 카드 + 뚜렷한 파스텔 배경/테두리, 오렌지 액센트 `#EA580C` |
| `Tomato` | 로즈 **파스텔** — 흰 카드 + 뚜렷한 파스텔 배경/테두리, 토마토 액센트 `#D9442B` |

파스텔 3종(Purple/Orange/Tomato)은 라이트 기반이다: 카드 표면은 흰색, 텍스트·
시맨틱(성공/경고/오류) 색은 라이트와 공유하고, 배경·테두리·선택 강조·액센트가
파스텔 톤으로 바뀐다. Gray는 다크 계열이라 텍스트/시맨틱까지 어두운 면 기준으로
재정의된다. 적용 방법은 모두 동일하다 (`Mode` 설정 + 화면당 `Apply(this)`).

| 멤버 | 설명 |
|---|---|
| `ModernTheme.Mode` | 위 6가지 중 하나. 시작 시 한 번만 설정 |
| `ModernTheme.IsDark` | Dark 여부 (읽기 전용) |
| `ModernTheme.IsDarkBased` | 어두운 표면 계열 여부 — Dark/Gray (다크 타이틀바 등 공통 처리 기준) |
| `Surface` / `Background` / `Border` / `TextPrimary` / `TextSecondary` / `Accent` / `SelectionBackground` / `SurfaceAlt` 등 | 중앙 팔레트 색(`System.Drawing.Color`) — 현재 테마에 맞는 값을 돌려주므로, 폼 배경 등 라이브러리 밖 요소를 칠할 때 사용 |

```csharp
// Program.cs — 반드시 첫 폼 생성(Application.Run) 전에
ModernTheme.ThemeMode mode;
if (!Enum.TryParse(settings.ThemeName, true, out mode))   // "dark", "purple", ...
{
    mode = ModernTheme.ThemeMode.Light;
}
ModernTheme.Mode = mode;
Application.Run(new MainForm());
```

동작 원리 (통합자는 몰라도 됨):

- WPF(ElementHost) 컨트롤 — Light가 아니면 `Tokens.<테마>.xaml`이 `Tokens.xaml`
  뒤에 병합돼 같은 토큰 키를 테마 값으로 덮는다 (다크는 전체, 틴트는 액센트/배경만).
- 순수 GDI+ 컨트롤(`ModernLabel`/`ModernStatusBadge`/`ModernCardPanel`/`ModernGroupBox`)
  — XAML을 읽을 수 없으므로 `ModernTheme` 팔레트 색을 직접 읽는다.

주의:

- **런타임 토글은 지원하지 않는다** — WPF StaticResource가 로드 시 확정되기 때문.
  테마 전환 UI는 설정을 저장한 뒤 **앱 재시작**으로 반영한다.
- 일반 WinForms 컨트롤(폼 배경, 기본 `Button`/`TextBox` 등)은 자동으로 어두워지지
  않는다 — 폼 쪽에서 `ModernTheme` 팔레트 색으로 직접 칠해야 한다.
- **카드 배경색은 디자이너에 직렬화되지 않는다** (v0.4.1) — VS 디자이너는 항상
  라이트 모드로 돌기 때문에, 과거에는 `ModernCardPanel`/`ModernGroupBox`의
  `BackColor`(흰색)가 `.Designer.cs`에 저장돼 다크 테마에서 카드가 라이트로
  남는 문제가 있었다. v0.4.1부터 `BackColor` 직렬화를 차단하고, 이미 저장돼 있는
  값도 런타임(핸들 생성 시점)에 테마 표면색으로 복구하므로 **기존 폼의
  `.Designer.cs`는 수정할 필요가 없다**.

### 테마 적용 체크리스트 (기존 앱 — 다크/틴트 공통)

1. **DLL 교체** — `Modern.Lab.Commons` v0.6.0 이상 (다크만이면 v0.5.0 이상).
2. **Program.cs** — `Application.Run(...)` **직전**(첫 폼 생성 전)에 한 번:
   ```csharp
   // Dark 자리에 Gray / Purple / Orange / Tomato를 넣으면 해당 테마
   Modern.Lab.Theming.ModernTheme.Mode = Modern.Lab.Theming.ModernTheme.ThemeMode.Dark;
   ```
3. **각 폼** — 생성자에서 `InitializeComponent()` **직후** 한 줄:
   ```csharp
   Modern.Lab.Theming.ModernThemeWinForms.Apply(this);
   ```
4. `.Designer.cs`는 손대지 않는다 — 옛 `BackColor` 직렬화 줄이 남아 있어도
   런타임에 복구/치환된다.

동작 확인은 샘플 갤러리로: `Modern.Lab.Samples.exe --dark` 또는 `--theme=purple` 등.

### ModernThemeWinForms.Apply(root) — 화면 테마 적용 헬퍼 (v0.5.0)

`Modern.Lab.Theming.ModernThemeWinForms`. Light가 아닌 테마(다크/틴트)일 때만
동작하고 기본 라이트에서는 완전한 no-op이므로 조건문 없이 항상 호출해도 안전하다.
치환 결과는 현재 테마의 팔레트 값 — 아래 표의 "다크" 열은 Dark 기준 예시이고,
틴트 테마에서는 같은 규칙으로 그 테마의 색이 들어간다.

`Apply(Control root)` — **root는 Form이 아니어도 된다.** 화면이 UserControl이나
사내/서드파티 프레임워크의 베이스 컨트롤이면 그 루트를 그대로 넘긴다. root가
Form이 아니면 타이틀바는 건드리지 않으므로, 최상위 폼에서
`ApplyDarkTitleBar(mainForm)`을 한 번 따로 호출한다.

| 하는 일 | 내용 |
|---|---|
| 타이틀바 | root가 Form일 때만 — OS 타이틀바를 다크로 (DWM immersive dark mode, Win10 1809+; 미지원 OS는 조용히 무시) |
| 루트 배경 | `ModernTheme.Background`로 설정 |
| 자식 컨트롤 | 전체 재귀 순회하며 아래 표의 "알려진 라이트 색"과 **정확히 일치**하는 `BackColor`/`ForeColor`만 다크 팔레트로 치환 — 상태색(빨강/초록 등) 등 의도적인 색은 보존 |

색 치환 표:

| 하드코딩돼 있던 라이트 색 | 치환되는 다크 팔레트 |
|---|---|
| `Color.White` (255,255,255) | `Surface` |
| (249,250,251) / (247,248,250) | `SurfaceAlt` |
| (243,244,246) / `SystemColors.Control` | `Background` |
| (17,24,39) / `SystemColors.ControlText` | `TextPrimary` |
| (107,114,128) | `TextSecondary` |
| (55,65,81) | `NeutralText` |

- WPF(ElementHost) 컨트롤은 건너뛴다 — `Tokens.Dark.xaml`이 스스로 처리.
- 런타임에 동적으로 추가한 컨트롤은 추가 후 `Apply(root)`를 다시 호출하면 된다
  (화면 생성 시 1회 호출 기준으로 설계 — 반복 호출을 전제로 하지는 말 것).
- **커스텀 페인트에는 닿지 않는다** — `Paint`/`CellPaint` 핸들러 안에서 색을
  하드코딩해 직접 그리는 코드는 Apply가 바꿀 수 없다. 그런 코드는
  `ModernTheme` 팔레트 색으로 그리도록 고친다 (예: 샘플
  `ItemHistoryForm.OnDetailCellPaint` — 캡션 `SurfaceAlt`, 괘선 `BorderSubtle`).
- 타이틀바만 필요하면 `ModernThemeWinForms.ApplyDarkTitleBar(form)` 개별 사용 가능.
- 알려진 한계: `ModernSpreadGrid`(FarPoint COM)는 내부 셀 색이 라이트로 고정 —
  다크 지원은 추후 과제.
