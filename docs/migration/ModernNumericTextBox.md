# ModernNumericTextBox 교체 가이드

- **대체 대상**: 숫자 입력용 `TextBox`(+수동 콤마 처리 코드), `NumericUpDown`
- **네임스페이스**: `Modern.Lab.WinForms.Controls.Input`

## 제공 멤버

| 멤버 | 설명 |
|---|---|
| `Value` | `decimal?`. **`null` = 미입력(전체 조회)**. 초기화 시 `null` 할당 |
| `ValueChanged` | 값이 바뀔 때 1회 발생 (타이핑·코드 할당 공통) |
| `DecimalPlaces` | 허용 소수 자릿수 (기본 0 = 정수만). 초과 자릿수는 입력 단계에서 잘림 |
| `AllowNegative` | 음수 허용 여부 (기본 true). 맨 앞 `-`만 인정 |
| `PlaceholderText` | 입력 전 회색 안내 텍스트 (예: "만원") |
| `Required` | 필수 입력 표시 — 값이 비어 있는 동안 필드에 빨간 점 (입력 컨트롤 공통 속성) |
| `Enabled` | 전파됨 |

## 입력 방식 (콤마 마스크)

- 숫자만 치면 **천단위 콤마가 자동 삽입** (`1234567` → `1,234,567`), 우측 정렬 표시
- 중간 위치를 수정해도 즉시 재형식화되어 입력이 막히지 않음
- 붙여넣기에서 숫자 외 문자는 무시 (`abc12,34x5` → `12,345`)
- 선행 0 자동 정리 (`0012` → `12`)
- 포커스가 떠날 때 정규 형식으로 정리 — `DecimalPlaces=2`면 `1,234.5` → `1,234.50`
- 빈 필드 = `Value == null` = 조건 없음

## NumericUpDown과의 차이

| 기존 멤버 | 대체 |
|---|---|
| `Minimum` / `Maximum` | 없음 — 범위 검증은 폼의 저장/조회 로직에서 수행 |
| 증가/감소 버튼(스핀) | 없음 — 직접 입력 전용 |
| `ThousandsSeparator` | 항상 켜짐 |

## 사용 예시 (구간 조회)

```csharp
private Modern.Lab.WinForms.Controls.Input.ModernNumericTextBox numSalaryFrom;

this.numSalaryFrom.PlaceholderText = "만원";

// 조회 조건
decimal? from = this.numSalaryFrom.Value;

if (from.HasValue)
{
    conditions.Add("SALARY >= " + from.Value.ToString(CultureInfo.InvariantCulture));
}

// 초기화
this.numSalaryFrom.Value = null;
```

권장 크기: 100~140×32 (자릿수에 맞게).
