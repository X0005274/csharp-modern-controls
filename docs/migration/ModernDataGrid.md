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
| `ConfigureColumns(params ModernDataGridColumn[])` | 명시적 컬럼 정의. `DataSource` 할당 전에 호출. `ModernDataGridColumn(dataPropertyName, headerText[, width])` — width 생략/음수는 남은 폭 채움(star). `TextAlignment`(Left/Center/Right) 지정 가능 |
| `ModernDataGridColumn.Kind` | 셀 표시 종류 — `Text`(기본) / `CheckBox`(bool 양방향 체크박스, 벌크 대상 지정) / `Badge`(`BadgeColorMember` 색 알약) / `Button`(`ButtonText` 캡션, `ButtonEnabledMember`로 행별 활성 제어) |
| `CellButtonClick` | 버튼 컬럼 셀 클릭 이벤트 — `e.Item`(클릭 행 `DataRowView`) + `e.DataPropertyName`(버튼 컬럼 이름). `DataGridView`의 `CellContentClick` + 버튼 컬럼 대체 |
| `SelectedItem` | 선택 행 (`DataTable` 소스일 때 `DataRowView`) — 기존 `CurrentRow.DataBoundItem` 대체 |
| `AutoFitColumns` | true면 각 컬럼 너비를 **헤더 캡션과 데이터 내용 중 더 넓은 쪽**에 맞춰 자동 계산 (`ConfigureColumns` 컬럼에만 적용, 컬럼 정의의 `Width`는 무시 — 단 CheckBox/Button/Badge 컬럼은 정의 폭·캡션 기준). `DataSource`가 바뀔 때마다 재계산되며 하한 48px / 상한 600px. 사용자의 마우스 폭 조절은 그대로 가능 |

정렬 글리프(▲/▼)는 헤더 텍스트 옆이 아니라 **헤더 셀 오른쪽 끝에 고정** 표시된다 —
컬럼 너비와 무관하게 위치가 일정해 정렬 상태를 훑어보기 쉽다.

## 미지원 멤버와 대체 방법

| 기존 멤버 | 대체 |
|---|---|
| `CurrentRow`, `SelectedRows`, `Rows[i].Cells[...]` | `SelectedItem`(`DataRowView`)으로 값 접근: `((DataRowView)grid.SelectedItem)["EMP_NO"]` |
| `Columns` 컬렉션 (디자이너 정의 포함) | `ConfigureColumns(...)` 코드 정의 |
| 셀 편집 (`ReadOnly = false`, `CellValueChanged`) | 일반 셀 편집은 미지원(읽기 전용 조회 전용). 단 **체크박스 컬럼**(`Kind = CheckBox`)은 예외로 양방향 토글된다 |
| `DataGridViewCheckBoxColumn` | `ConfigureColumns`에 `Kind = GridColumnKind.CheckBox` 컬럼 (bool 컬럼 바인딩) |
| `DataGridViewButtonColumn` + `CellContentClick` | `Kind = GridColumnKind.Button` 컬럼 + `CellButtonClick` 이벤트 |
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
