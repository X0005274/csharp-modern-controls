# ModernRadioGroup 도입 가이드

- **대체 대상**: `GroupBox`/`Panel` + `RadioButton` 묶음 (배타 선택)
- **네임스페이스**: `Modern.Lab.WinForms.Controls.Selection`

RadioButton 여러 개를 폼에 늘어놓고 `CheckedChanged`를 각각 배선하는 대신,
하나의 컨트롤에 코드 테이블을 할당하고 `SelectedValue` 하나로 읽고 쓴다.

## 제공 멤버 (ComboBox 데이터 계약과 동일한 이름 체계)

| 멤버 | 설명 |
|---|---|
| `DataSource` | `DataTable`/`DataView`/`IList`/`IEnumerable`. 행마다 라디오 하나 |
| `DisplayMember` | 라디오 옆 표시 텍스트 컬럼/속성 이름 |
| `ValueMember` | 선택 값 컬럼/속성 이름 |
| `SelectedValue` | 선택된 값. **`null` = 미선택**. `DataSource`보다 먼저 설정해도 됨(계약 규칙 3) |
| `SelectedValueChanged` | 선택이 바뀔 때 1회 발생 (같은 값 재할당은 미발생) |
| `Vertical` | `true`면 세로 나열 (기본은 가로) |
| `Enabled` | 전파됨 |

## 계약 보장 동작

- `SelectedValue`를 `DataSource`보다 먼저 설정해도 목록 구성 시 적용됨
- `DataSource` 재할당 시 기존 값이 새 목록에 없으면 미선택(`null`)으로 초기화 + 이벤트 1회
- 배타성은 컨트롤이 보장 — 폼에서 개별 라디오를 관리할 필요 없음

## 사용 예시

```csharp
private Modern.Lab.WinForms.Controls.Selection.ModernRadioGroup radioSort;

// 코드 테이블 할당 (서버 코드 테이블 구조 그대로)
DataTable sortTable = new DataTable();
sortTable.Columns.Add("SORT_CODE", typeof(string));
sortTable.Columns.Add("SORT_NAME", typeof(string));
sortTable.Rows.Add("EMP_NO", "사번순");
sortTable.Rows.Add("HIRE_DATE", "입사일순");

this.radioSort.DisplayMember = "SORT_NAME";
this.radioSort.ValueMember = "SORT_CODE";
this.radioSort.DataSource = sortTable;
this.radioSort.SelectedValue = "EMP_NO";   // 기본 선택

// 선택 즉시 반응
this.radioSort.SelectedValueChanged += this.OnSortChanged;

// 조회 시 코드 사용
string sortColumn = this.radioSort.SelectedValue as string;
```

권장 크기: 항목 수에 맞는 폭 × 높이 24~32. 항목이 많으면 `Vertical = true`.
