# ModernMonthPicker 도입 가이드

- **대체 대상**: 년월 선택용 `DateTimePicker`(CustomFormat="yyyy-MM") 또는 년/월 콤보 2개 조합
- **네임스페이스**: `Modern.Lab.WinForms.Controls.Input`

## 제공 멤버

| 멤버 | 설명 |
|---|---|
| `Value` | `DateTime?`. **해당 월 1일로 정규화**되어 저장 (임의 날짜를 할당해도 1일로 맞춰짐). **`null` = 미선택(전체)** |
| `ValueChanged` | 년월이 바뀔 때 1회 발생 (팝업·타이핑·코드 할당 공통) |
| `MinDate` / `MaxDate` | 팝업에서 선택 가능한 범위 (`null` = 제한 없음) |
| `PlaceholderText` | 입력 전 회색 형식 안내. 기본 `"yyyy-MM"` |
| `Required` | 필수 입력 표시 — 값이 비어 있는 동안 빨간 점 (입력 컨트롤 공통 속성) |
| `Enabled` | 전파됨 |

## 입력 방식

- 달력 버튼 → **12개월 그리드 팝업**에서 월 클릭이 곧 선택
- 직접 타이핑: **숫자 6자리(`202107`)만 치면 `2021-07`로 자동 형식화** — 중간 수정 가능,
  무효 월(13월 등)은 오류 없이 값 미반영, 포커스 이탈 시 마지막 유효 값으로 정리
- 빈 필드 = `Value == null` = 조건 없음

## 사용 예시 (기준월 조회)

```csharp
private Modern.Lab.WinForms.Controls.Input.ModernMonthPicker monthHire;

DateTime? month = this.monthHire.Value;

if (month.HasValue)
{
    // 문자열 날짜(ISO)라면 접두 매칭, DateTime 컬럼이라면 구간 비교
    conditions.Add("HIRE_DATE LIKE '" + month.Value.ToString("yyyy-MM") + "%'");
}

// 초기화
this.monthHire.Value = null;
```

권장 크기: 110×32.
