# Modern.Lab 공통 컨트롤 사용법 레퍼런스

컨트롤별 주요 속성·이벤트·메서드의 사용법을 예제와 함께 정리한 문서다.
"기존 컨트롤을 무엇으로 어떻게 교체하는가"는 [migration/](migration/) 폴더의
컨트롤별 교체 가이드를 참고하고, 이 문서는 "교체한 뒤 어떻게 쓰는가"를 다룬다.

- 네임스페이스: 래퍼(WinForms) `Modern.Lab.WinForms.Controls.*`,
  컬럼 정의 등 공용 타입 `Modern.Lab.Controls.Wpf.*`
- 모든 컨트롤 공통: `Enabled` 전파, 디자이너에서 스냅샷 미리보기 표시,
  높이 32(컨트롤) / 카드류는 자유

## 목차

1. [공통 데이터 계약](#공통-데이터-계약)
2. [ModernLabel](#modernlabel) — 라벨
3. [ModernButton](#modernbutton) — 버튼
4. [ModernTextBox](#moderntextbox) — 텍스트 입력 + 자동완성
5. [ModernComboBox](#moderncombobox) — 콤보(검색형·멀티컬럼)
6. [ModernCheckComboBox](#moderncheckcombobox) — 체크 콤보(다중 선택)
7. [ModernDataGrid](#moderndatagrid) — 데이터 그리드
8. [ModernKpiCard / ModernSummaryList](#modernkpicard--modernsummarylist) — 통계 표시
9. [ModernCardPanel](#moderncardpanel) — 카드 판넬
10. [ModernDataGridColumn](#moderndatagridcolumn) — 컬럼 정의 (그리드/콤보 공용)

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

## ModernTextBox

단일행 텍스트 입력. 플레이스홀더·Enter 조회·자동완성(초성 검색 포함) 내장.

### 속성·이벤트

| 멤버 | 타입 | 설명 |
|---|---|---|
| `Text` | string | 입력 텍스트. 한글 조합 중에는 음절 확정 시점에 반영 |
| `PlaceholderText` | string | 비어 있을 때 회색 힌트. 자음 하나만 쳐도 즉시 사라짐 |
| `ReadOnly` | bool | 읽기 전용 (배경이 옅게 변함) |
| `TextChanged` | 이벤트 | 표준 WinForms 이벤트 |
| `EnterPressed` | 이벤트 | Enter 키 입력 시 발생 — **기존 `KeyDown`으로 Enter를 잡던 코드는 이 이벤트로 교체** (WPF 에디터가 처리한 키는 WinForms `KeyDown`으로 오지 않음) |
| `AutoCompleteMode` | `AutoCompleteMode` | `None` 외 값은 모두 제안 드롭다운(Suggest)으로 동작 |
| `AutoCompleteSource` | `AutoCompleteSource` | **`CustomSource`만 지원** |
| `AutoCompleteCustomSource` | `AutoCompleteStringCollection` | 후보 목록. 재할당으로 갱신 |

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
| `CheckAll()` / `UncheckAll()` | 메서드 | 전체 체크/해제 — 드롭다운 헤더와 동일 동작. 헤더 라벨은 상태에 따라 "전체 선택" ↔ "전체 해제"(전부 체크 시)로 바뀜 |

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
```

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
| `Title` | 칩 왼쪽 제목 (비우면 숨김) |
| `Flat` | 카드 테두리 제거 |

```csharp
this.listDeptCount.Title = "부서별";
this.listDeptCount.DisplayMember = "DEPT_NAME";
this.listDeptCount.ValueMember = "CNT";
this.listDeptCount.DataSource = deptCountTable;   // 조회할 때마다 재할당
// → [경영지원팀 4] [개발1팀 6] ... 칩으로 표시
```

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

## ModernDataGridColumn

그리드(`ConfigureColumns`)와 콤보 드롭다운(`ConfigureDropDownColumns`)이 공유하는
컬럼 정의. **나열하는 만큼 컬럼이 생긴다 — 3개, 4개, 그 이상도 가능.**

| 생성자/속성 | 설명 |
|---|---|
| `new ModernDataGridColumn(컬럼명, 헤더)` | 폭 생략 = 남은 공간 채움(star) |
| `new ModernDataGridColumn(컬럼명, 헤더, 폭)` | 픽셀 고정 폭 |
| `TextAlignment` | `Left`(기본) / `Center` / `Right` |

```csharp
// 원하는 개수만큼 나열
new ModernDataGridColumn("EMP_NO", "사번", 90),
new ModernDataGridColumn("EMP_NAME", "이름", 110),
new ModernDataGridColumn("SALARY", "급여", 100) { TextAlignment = GridTextAlignment.Right },
new ModernDataGridColumn("NOTE", "비고")   // 마지막은 폭 생략으로 채움
```
