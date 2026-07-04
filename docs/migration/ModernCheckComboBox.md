# ModernCheckComboBox 도입 가이드

- **대체 대상**: 없음(신규) — WinForms에는 체크 콤보가 없어 서드파티(DevExpress 등)로 쓰던 자리를 대체.
  다중 조건 필터(직급 여러 개 선택 등)에 사용
- **네임스페이스**: `Modern.Lab.WinForms.Controls.Selection`

## 제공 멤버 (ComboBox 데이터 계약과 동일한 이름 체계)

| 멤버 | 설명 |
|---|---|
| `DataSource` | `DataTable`/`DataView`/`IList`/`IEnumerable`. 할당 시 체크가 초기화되고 `CheckedChanged` 1회 발생 |
| `DisplayMember` | 체크박스 옆 표시 텍스트 컬럼/속성 이름 |
| `ValueMember` | `CheckedValues`로 사용할 컬럼/속성 이름 |
| `CheckedValues` | 체크된 항목들의 값 배열. **`DataSource`보다 먼저 설정해도 됨**(보류 후 적용 — 계약 룰 3). null/빈 배열 = 전체 해제 |
| `CheckedItems` | 체크된 원본 행 배열 (`DataRowView` 등) — 읽기 전용 |
| `CheckedChanged` | 체크 상태 변경 시 발생 |
| `PlaceholderText` | 체크된 항목이 없을 때 표시할 힌트 (예: "직급 전체") — 미체크 = 전체 패턴 |
| `Text` | 체크된 항목 표시 텍스트를 ", "로 연결한 값 (읽기 전용) |
| `Enabled` | 전파됨 |

## 동작

- 필드 클릭 → 체크박스 목록 드롭다운. 체크해도 닫히지 않아 여러 개 선택 가능,
  바깥 클릭 시 닫힘
- 필드에는 체크된 항목들이 "부장, 과장"처럼 연결되어 표시 (넘치면 말줄임)
- 아무것도 체크 안 됨 = 플레이스홀더 표시 = 조회 코드에서 "전체"로 처리

## 사용 예시 — 직급 다중 필터

```csharp
private Modern.Lab.WinForms.Controls.Selection.ModernCheckComboBox cboRank;

this.cboRank.DisplayMember = "RANK_NAME";
this.cboRank.ValueMember = "RANK_CODE";
this.cboRank.DataSource = rankTable;      // 서버 응답 DataTable

// 조회 시
object[] rankCodes = this.cboRank.CheckedValues;
if (rankCodes != null && rankCodes.Length > 0)
{
    // POSITION IN ('부장', '과장') 형태로 조건 구성
}

// 초기화
this.cboRank.CheckedValues = null;
```

## 미지원 / 주의

| 항목 | 내용 |
|---|---|
| 단일 선택 멤버 (`SelectedValue` 등) | 없음 — 단일 선택은 `ModernComboBox` 사용 |
| 전체 선택/해제 헤더 항목 | v1 미제공 — 필요 시 요청 |
| 검색(타이핑 필터) | v1 미제공 — 항목이 많아지면 요청 |

권장 크기: 150×32 이상 (체크 항목이 많으면 표시 텍스트가 말줄임되므로 폭 여유).
