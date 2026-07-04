# ModernComboBox 교체 가이드

- **대체 대상**: `System.Windows.Forms.ComboBox`
- **네임스페이스**: `Modern.Lab.WinForms.Controls.Selection`

## 호환 제공 멤버

| 멤버 | 비고 |
|---|---|
| `DataSource` | `DataTable`/`DataView`/`IList`/`IEnumerable` 수용. `DataTable`은 내부에서 `DefaultView`로 변환 |
| `DisplayMember` | 표시 텍스트 컬럼/속성 이름 |
| `ValueMember` | `SelectedValue` 컬럼/속성 이름 |
| `SelectedValue` | **`DataSource`보다 먼저 설정해도 됨** — 값을 보류했다가 데이터 도착 시 적용 (계약 룰 3) |
| `SelectedItem` | `DataTable` 소스일 때는 `DataRowView` 반환 (기존 WinForms 바인딩과 동일) |
| `SelectedIndex` | 미선택 시 -1 |
| `Items` | 수동 항목 컬렉션 (`Items.Add(...)`). `DataSource` 지정 시 `DataSource`가 우선 |
| `SelectedIndexChanged` | `DataSource` 할당 시 정확히 1회, 이후 사용자 선택 변경 시 발생 |
| `DropDownStyle` | **`DropDown`(기본, WinForms 기본값과 동일) = 검색형 콤보** — 입력하면 목록이 필터링되고(한글 초성 검색 포함, "ㄱ" → 개발1팀·개발2팀·경영지원팀) **입력을 모두 지우면 선택도 해제됨**(빈 입력 = 전체 패턴). `DropDownList` = 선택 전용 |
| `Text` | 현재 선택/입력 텍스트 (읽기). 쓰기는 DropDown/Simple에서만 동작, DropDownList에서는 무동작 |
| `Enabled` | 전파됨 |

## 계약 보장 동작 (docs/design-notes.md §6-1)

- `SelectedValue` → `DataSource` 순서로 설정해도 정상 동작 (보류 후 적용)
- `DataSource` 재할당 시 선택이 깨끗하게 초기화되고 이벤트는 1회만 발생
- 보류 값이 없으면 첫 행 자동 선택 (WinForms `ComboBox` 기본 동작과 일치)
- null/빈 데이터는 빈 목록으로 표시, 예외 없음
- 백그라운드 조회 후 UI 스레드 `Invoke` 할당 패턴 지원

## 추가 멤버

| 멤버 | 설명 |
|---|---|
| `PlaceholderText` | 미선택/미입력 상태에서 표시할 힌트 텍스트 — **`ModernTextBox`와 동일한 속성명**. "전체" 더미 행 대신 미선택(`SelectedIndex = -1`) + 플레이스홀더 패턴 권장: `DataSource` 할당 후 `SelectedIndex = -1`로 초기화하면 미선택 상태가 유지되고, 폼 조회 코드에서 `SelectedValue == null`을 "전체"로 처리 |

## 미지원 멤버와 대체 방법

| 기존 멤버 | 대체 |
|---|---|
| `Text` 쓰기로 항목 선택 (DropDownList) | `SelectedValue` 또는 `SelectedIndex`로 선택 |
| `AutoCompleteMode`/`AutoCompleteSource` | 불필요 — `DropDownStyle = DropDown`이면 검색형 필터링이 기본 동작 |
| `SelectedValueChanged`/`SelectionChangeCommitted` | `SelectedIndexChanged`로 통합 |
| `FormattingEnabled`, `FormatString` | 미구현 — 표시 문자열은 데이터 쪽에서 가공 |
| `Font`, `BackColor`, `FlatStyle` | 없음 — 토큰이 결정 |

## .Designer.cs 교체 예시

```csharp
// 변경 전
private System.Windows.Forms.ComboBox cboDept;
this.cboDept = new System.Windows.Forms.ComboBox();
this.cboDept.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

// 변경 후
private Modern.Lab.WinForms.Controls.Selection.ModernComboBox cboDept;
this.cboDept = new Modern.Lab.WinForms.Controls.Selection.ModernComboBox();
```

폼의 서버 request/reply 코드는 그대로 둔다:

```csharp
// 기존 코드 변경 없음
this.cboDept.DisplayMember = "DEPT_NAME";
this.cboDept.ValueMember = "DEPT_CODE";
this.cboDept.SelectedValue = "D002";   // DataSource보다 먼저여도 동작
this.cboDept.DataSource = replyTable;  // 서버 응답 DataTable
```

권장 크기: 200×32.
