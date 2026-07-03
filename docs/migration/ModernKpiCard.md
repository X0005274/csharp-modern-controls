# ModernKpiCard 도입 가이드

- **대체 대상**: 직접 조합해 쓰던 `Label` 2개(제목 + 값) 패턴 — 1:1 대체 대상 컨트롤 없음(신규)
- **네임스페이스**: `Modern.Lab.WinForms.Controls.Display`

## 제공 멤버

| 멤버 | 설명 |
|---|---|
| `Title` | 값 위에 표시할 제목 (예: "조회 건수"). `[Localizable(true)]` |
| `Value` | 강조 표시할 값 텍스트. 조회 후 갱신 (예: `grid.RowCount.ToString()`) |
| `Flat` | 카드 테두리/배경 제거 — `ModernCardPanel` 위에 평면 배치할 때 사용 |
| `Enabled` | 전파됨 |

## 사용 예시

```csharp
private Modern.Lab.WinForms.Controls.Display.ModernKpiCard cardCount;

this.cardCount = new Modern.Lab.WinForms.Controls.Display.ModernKpiCard();
this.cardCount.Title = "조회 건수";
this.cardCount.Value = "0";

// 조회 완료 후
this.cardCount.Value = this.gridEmployee.RowCount.ToString();
```

## 미지원 / 주의

| 항목 | 내용 |
|---|---|
| 숫자 포맷 | `Value`는 문자열 — 포맷(천단위 콤마 등)은 폼에서 처리 |
| `Font`, 색상 | 없음 — 토큰이 결정 (카드 radius 8, Heading 타입 램프) |

권장 크기: 150×76.
