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
| `ItemStyle` | 항목 표시 스타일: `CheckBox`(모던 체크박스, 기본) / `Switch`(온오프 토글 스위치). 다중 "선택/포함" 의미의 필터에는 체크박스 권장, 스위치는 설정성 켬/끔 목록용 |
| `CheckAll()` / `UncheckAll()` | 전체 체크/해제 (이벤트 1회 발생) — 드롭다운 상단 "전체 선택" 헤더와 동일 동작 |
| `Text` | 체크된 항목 표시 텍스트를 ", "로 연결한 값 (읽기 전용) |
| `Enabled` | 전파됨 |

## 동작

- 필드 클릭 → 체크 목록 드롭다운. 체크해도 닫히지 않아 여러 개 선택 가능,
  바깥 클릭 시 닫힘
- 드롭다운 상단 **전체 선택/해제 헤더**: 전부 체크 = 체크 표시 + 라벨 "전체 해제",
  일부만 = 부분 선택(대시), 없음 = 해제 — 클릭 시 전체 체크 ↔ 전체 해제 토글
- 필드에는 체크된 항목들이 "부장, 과장"처럼 연결되어 표시 (넘치면 말줄임)
- 아무것도 체크 안 됨 = 플레이스홀더 표시 = 조회 코드에서 "전체"로 처리
- **코드/명칭 분리**: 화면에는 `DisplayMember`(명칭)가 보이고 `CheckedValues`는
  `ValueMember`(코드) 배열을 반환 — 서버 호출에 코드를 그대로 전송

## 사용 예시 — 직급 다중 필터 (코드/명칭 구조)

```csharp
private Modern.Lab.WinForms.Controls.Selection.ModernCheckComboBox cboRank;

// 서버 직급 테이블: (RANK_CODE, RANK_NAME) — 예: (R1, 부장)
this.cboRank.DisplayMember = "RANK_NAME";  // 화면 표시는 명칭
this.cboRank.ValueMember = "RANK_CODE";    // 값은 코드
this.cboRank.DataSource = rankTable;       // 서버 응답 DataTable

// 조회 시 — 서버에 코드를 그대로 전송
object[] rankCodes = this.cboRank.CheckedValues;   // 예: { "R1", "R2" }
if (rankCodes != null && rankCodes.Length > 0)
{
    // RANK_CODE IN ('R1', 'R2') 형태로 조건 구성
}

// 초기화
this.cboRank.CheckedValues = null;
```

## 미지원 / 주의

| 항목 | 내용 |
|---|---|
| 단일 선택 멤버 (`SelectedValue` 등) | 없음 — 단일 선택은 `ModernComboBox` 사용 |
| 검색(타이핑 필터) | v1 미제공 — 항목이 많아지면 요청 |

권장 크기: 150×32 이상 (체크 항목이 많으면 표시 텍스트가 말줄임되므로 폭 여유).
