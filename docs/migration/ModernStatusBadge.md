# ModernStatusBadge 도입 가이드

- **대체 대상**: 상태 표시용 색 있는 `Label` (BackColor/ForeColor 직접 지정)
- **네임스페이스**: `Modern.Lab.WinForms.Controls.Display`

승인/반려/대기, 운영/개발 같은 상태를 색 있는 알약(pill)으로 표시한다.
배경색만 지정하면 **글자색은 배경과 같은 색상 계열로 자동 유도**된다
(요약 칩의 `ColorMember`와 동일한 규칙 — 연초록 배경이면 진초록 글씨).

## 제공 멤버

| 멤버 | 설명 |
|---|---|
| `Text` | 배지 텍스트. `Control.Text` override, `[Localizable(true)]` |
| `Color` | 배경색 문자열 (`"#DCFCE7"` hex 또는 `"SkyBlue"` 색 이름). 비우거나 파싱 불가면 중립 회색 |
| `Enabled` | 전파됨 |

## 사용 예시

```csharp
private Modern.Lab.WinForms.Controls.Display.ModernStatusBadge badgeStatus;

// 상태에 따라 색과 텍스트를 함께 변경 — 서버 코드 테이블의 색 컬럼을 그대로 써도 된다
this.badgeStatus.Text = "승인";
this.badgeStatus.Color = "#DCFCE7";   // 연초록 → 진초록 글씨 자동

this.badgeStatus.Text = "반려";
this.badgeStatus.Color = "#FEE2E2";   // 연빨강 → 진빨강 글씨 자동

this.badgeStatus.Text = "대기";
this.badgeStatus.Color = "";          // 중립 회색
```

권장 크기: 텍스트에 맞는 폭 × 24. 색 관례: 상태 색은 코드 테이블에 저장해
칩/배지가 같은 색을 공유하게 한다.
