# ModernDataGrid 교체 가이드

- **대체 대상**: `System.Windows.Forms.DataGridView` (읽기 전용 목록 조회 화면)
- **네임스페이스**: `Modern.Lab.WinForms.Controls.Data`

## 호환 제공 멤버

| 멤버 | 비고 |
|---|---|
| `DataSource` | `DataTable`/`DataView`/`IList`/`IEnumerable` 수용. `DataTable`은 내부에서 `DefaultView`로 변환 |
| `AutoGenerateColumns` | 기본 true. `ConfigureColumns` 호출 시 자동으로 false |
| `RowCount` | 현재 표시 중인 행 수 (하단 조회 건수 연동용) |
| `SelectedIndex` | 미선택 시 -1 |
| `SelectionChanged` | `DataSource` 할당 시 정확히 1회, 이후 사용자 선택 변경 시 발생 |
| `Enabled` | 전파됨 |

## 계약 보장 동작 (docs/design-notes.md §6-1)

- `DataSource` 할당 시 선택이 초기화되고, 데이터가 있으면 **첫 행이 자동 선택**됨
  (DataGridView의 최초 CurrentRow 동작과 일치) — 이벤트는 1회만 발생
- null/빈 데이터는 빈 그리드로 표시, 예외 없음
- 백그라운드 조회 후 UI 스레드 `Invoke` 할당 패턴 지원
- 컬럼 헤더 클릭 정렬, 컬럼 폭 마우스 조절 지원

## 추가 멤버

| 멤버 | 설명 |
|---|---|
| `ConfigureColumns(params ModernDataGridColumn[])` | 명시적 컬럼 정의. `DataSource` 할당 전에 호출. `ModernDataGridColumn(dataPropertyName[, headerText[, width]])` — headerText 생략 시 캡션 용어사전 참조. 앱 시작 시 `Modern.Lab.Captions.GridCaptionDictionary.RegisterAll()`을 한 번 호출하면 Common의 회사 표준 캡션을 쓴다. 화면 전용 용어는 `GridCaptionCatalog.Register`/`RegisterRange`로 추가·재정의한다. width 생략/음수는 남은 폭 채움(star). `TextAlignment`(Left/Center/Right) 지정 가능 |
| `ExportXlsx(path, sheetName, data)` | 화면 컬럼 정의 그대로(순서·캡션·`Format`) 데이터 전체를 진짜 .xlsx로 저장 — 외부 라이브러리 없음, 내보내기용 컬럼/헤더 목록을 폼이 따로 관리하지 않는다. CheckBox/Button 컬럼 자동 제외. `data`는 그리드 `DataSource`가 아니라 인자 — 페이지 화면에서도 전체 결과 저장 |
| `ColumnDefinitions` | `ConfigureColumns`로 선언한 정의의 복사본 — 화면과 동일한 컬럼 구성(순서·캡션·형식)으로 커스텀 파생 출력을 만들 때 단일 원천 (엑셀 저장은 `ExportXlsx`가 이미 해준다) |
| `EmptyText` | 데이터 0건일 때 데이터 영역 가운데 표시할 안내 문구 (기본 `"No data"`, 빈 문자열 = 끔) |
| `ModernDataGridColumn.Kind` | 셀 표시 종류 — `Text`(기본) / `CheckBox`(bool 양방향 체크박스, 벌크 대상 지정; OS 기본 룩이 아닌 모던 비주얼 — 둥근 사각 + 액센트 채움 + 흰 체크 글리프, ModernCheckBox와 동일) / `Badge`(`BadgeColorMember` 색 레티클 배지, 같은 컬럼은 가장 긴 표시값 기준 동일 폭) / `Button`(`ButtonText` 캡션, `ButtonEnabledMember`로 행별 활성 제어; ModernButton Secondary와 같은 문법 — 평상시 흰 배경 + 회색 테두리, hover 시 옅은 파랑 틴트 + 액센트 테두리/글자. 반복 행 액션의 캡션은 Label 크기 일반 굵기) |
| `ModernDataGridColumn.HeaderCheckBox` | CheckBox 컬럼 전용 (기본 false). true면 헤더 캡션 대신 **전체 선택/해제 체크박스**가 올라간다 — 클릭 시 현재 그리드에 표시 중인 모든 행(페이지 화면이면 현재 페이지)의 값을 일괄 설정하고, 행 값 상태에 따라 체크(전체)/해제(없음)/중간(일부)으로 자동 갱신. `DataGridView` 헤더 체크박스 커스텀 그리기 코드 대체 |
| `CellButtonClick` | 버튼 컬럼 셀 클릭 이벤트 — `e.Item`(클릭 행 `DataRowView`) + `e.DataPropertyName`(버튼 컬럼 이름). `DataGridView`의 `CellContentClick` + 버튼 컬럼 대체 |
| 행 우클릭 + `ContextMenuStrip` | 행 위에서 우클릭하면 **그 행이 먼저 현재 행으로 선택**된 뒤 컨트롤에 지정한 `ContextMenuStrip`이 커서 위치에 뜬다 — 메뉴 핸들러는 `SelectedItem`을 대상으로 처리하면 된다. 행 밖(헤더/빈 영역) 우클릭에는 뜨지 않는다 |
| `AllowColumnFilters` | 컬럼 헤더 깔때기 값 필터(엑셀식 고유 값 체크리스트, **기본 true**). 팝업 검색창은 자동완성 후보와 한글 초성 매칭을 제공하고, 검색 중에도 기존 체크 선택은 유지된다. 화면 뷰만 거르고 원본 데이터는 그대로 — `DataGridView`에서 직접 구현하던 헤더 필터 커스텀 코드 대체. 재조회(`DataSource` 재할당) 후에도 필터 선택 유지. 깔때기는 헤더 셀 맨 오른쪽 고정, 정렬 글리프가 그 왼쪽 |
| `FilterValueSource` / `ColumnFiltersChanged` / `MatchesColumnFilters(row)` | **페이지 슬라이스 화면의 필터 연동 3종** — 페이지 조각을 바인딩하는 화면은 ① `FilterValueSource`에 전체 결과 `DataTable`을 지정해 깔때기 체크리스트에 전체 값이 나오게 하고, ② `ColumnFiltersChanged` 이벤트에서 전체 결과를 `MatchesColumnFilters(row)`로 걸러 표시 행 목록과 페이지 수를 재계산한다 (Samples의 `LogisticsRequestForm` 참고). 페이징 없는 화면은 셋 다 필요 없다 |
| `SelectedItem` | 선택 행 (`DataTable` 소스일 때 `DataRowView`) — 기존 `CurrentRow.DataBoundItem` 대체 |
| `AutoFitColumns` | true면 각 컬럼 너비를 **헤더 캡션과 데이터 내용 중 더 넓은 쪽**에 맞춰 자동 계산 (`ConfigureColumns` 컬럼에만 적용, 컬럼 정의의 `Width`는 무시 — 단 CheckBox/Button/Badge 컬럼은 정의 폭·캡션 기준). `DataSource`가 바뀔 때마다 재계산되며 하한 48px / 상한 600px. 사용자의 마우스 폭 조절은 그대로 가능 |

정렬 글리프(▲/▼)는 헤더 텍스트 옆이 아니라 **헤더 셀 오른쪽 끝에 고정** 표시된다 —
컬럼 너비와 무관하게 위치가 일정해 정렬 상태를 훑어보기 쉽다.

## 미지원 멤버와 대체 방법

| 기존 멤버 | 대체 |
|---|---|
| `CurrentRow`, `SelectedRows`, `Rows[i].Cells[...]` | `SelectedItem`(`DataRowView`)으로 값 접근: `((DataRowView)grid.SelectedItem)["EMP_NO"]` |
| `Columns` 컬렉션 (디자이너 정의 포함) | `ConfigureColumns(...)` 코드 정의 |
| 셀 편집 (`ReadOnly = false`, `CellValueChanged`) | 일반 셀 편집은 미지원(읽기 전용 조회 전용). 단 **체크박스 컬럼**(`Kind = CheckBox`)과 **콤보 컬럼**(`Kind = Combo`)은 예외로 양방향 갱신된다 |
| `DataGridViewCheckBoxColumn` | `ConfigureColumns`에 `Kind = GridColumnKind.CheckBox` 컬럼 (bool 컬럼 바인딩) |
| `DataGridViewButtonColumn` + `CellContentClick` | `Kind = GridColumnKind.Button` 컬럼 + `CellButtonClick` 이벤트 |
| `DataGridViewComboBoxColumn` | `Kind = GridColumnKind.Combo` 컬럼 — `ComboItems`(고정 선택지 `string[]`, 예: `{"SUCC","FAIL"}`) 중 하나를 고르면 원본 행 컬럼 값이 즉시 갱신된다. `ComboEnabledMember` 컬럼 값(bool/`"Y"`/`"true"`/`"1"`)으로 행별 입력 가능 여부 제어 (비활성 행은 회색 잠금). `ComboItemColors`(선택지와 같은 순서의 색 배열)를 주면 선택 값/드롭다운 항목이 **레티클(둥근 사각) 배지**로 표시되고 필드 표면 전체도 선택 값의 배지 색으로 칠해진다 (미선택은 기본 필드). 입력분만 서버로 보내려면 바인딩 직전 `table.AcceptChanges()` 후 `table.GetChanges()`를 전송. 판정/등급 입력용 |
| `CellClick`/`CellDoubleClick` | `SelectionChanged`로 선택 처리. 더블클릭 이벤트가 필요하면 별도 요청 |
| `MultiSelect` | 단일 행 선택만 지원 — 벌크 작업 대상은 체크박스 컬럼으로 지정 |
| `DefaultCellStyle`, `Font`, 색상 계열 | 없음 — 토큰이 결정 |

## .Designer.cs 교체 예시

```csharp
// 변경 전
private System.Windows.Forms.DataGridView gridEmployee;
this.gridEmployee = new System.Windows.Forms.DataGridView();

// 변경 후
private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridEmployee;
this.gridEmployee = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
```

폼 코드에서 컬럼 정의 후 서버 응답을 그대로 할당한다:

```csharp
using Modern.Lab.Controls.Wpf.Data; // ModernDataGridColumn

this.gridEmployee.ConfigureColumns(
    new ModernDataGridColumn("EMP_NO", "사번", 90),
    new ModernDataGridColumn("EMP_NAME", "이름", 110),
    new ModernDataGridColumn("HIRE_DATE", "입사일") { TextAlignment = GridTextAlignment.Center });

this.gridEmployee.DataSource = replyTable;   // 서버 응답 DataTable
this.lblCount.Text = "조회 " + this.gridEmployee.RowCount + "건";
```

권장 크기: 영역을 채우는 컨트롤이므로 `Dock = Fill` 또는 `Anchor` 4방향 지정.
