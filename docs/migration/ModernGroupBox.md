# ModernGroupBox 교체 가이드

- **대체 대상**: `System.Windows.Forms.GroupBox`
- **네임스페이스**: `Modern.Lab.WinForms.Controls.Layout`

헤더 타이틀(SemiBold) + 은은한 구분선이 있는 카드 컨테이너.
`ModernCardPanel`을 상속한 **순수 WinForms 패널**이라 모던 리프 컨트롤을 포함한
어떤 WinForms 자식도 담을 수 있다 (ElementHost 아님 — 계약 규칙 5).

## 호환 제공 멤버

| 멤버 | 비고 |
|---|---|
| `Text` | 헤더 타이틀. `Control.Text` override, `[Localizable(true)]` |
| `TitleFontStyle` | 헤더 타이틀 폰트 스타일(`FontStyle` — Bold/Italic 조합). 기본 `Regular` |
| `TitleAccent` | `true`면 헤더 타이틀을 액센트 색(#0078D4)으로 강조. 기본 `false` |
| `TitleRightText` | 헤더 **오른쪽 끝**에 표시할 보조 텍스트 (기본 빈 문자열 = 표시 없음). 조회 기준 일시("Days as of ...") 같은 메타 정보를 보조 텍스트 색으로 작게 표기 |
| `Controls` / `Padding` / `Dock` 등 | 일반 Panel과 동일. 기본 Padding이 헤더 높이(40px 상단)를 확보 |
| `Enabled` | 전파됨 |

## GroupBox와의 차이

| 항목 | 내용 |
|---|---|
| 테두리 | 카드 스타일 (흰 표면, radius 8, 은은한 테두리) — 파인 선 테두리 아님 |
| 타이틀 위치 | 카드 안쪽 헤더 영역 (테두리에 걸치지 않음) + 아래 구분선 |
| `FlatStyle` | 없음 |

## .Designer.cs 교체 예시

```csharp
// 변경 전
private System.Windows.Forms.GroupBox grpStats;
this.grpStats = new System.Windows.Forms.GroupBox();
this.grpStats.Text = "조회 통계";

// 변경 후
private Modern.Lab.WinForms.Controls.Layout.ModernGroupBox grpStats;
this.grpStats = new Modern.Lab.WinForms.Controls.Layout.ModernGroupBox();
this.grpStats.Text = "조회 통계";
// 자식 배치는 그대로 — 기본 Padding(12, 40, 12, 12)이 헤더 아래 공간을 확보
```

팁: 내부에 KpiCard/SummaryList를 올릴 때는 `Flat = true`. 헤더가 필요 없는 영역은
`ModernCardPanel` 사용.
