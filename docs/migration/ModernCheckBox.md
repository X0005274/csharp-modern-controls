# ModernCheckBox 교체 가이드

- **대체 대상**: `System.Windows.Forms.CheckBox`
- **네임스페이스**: `Modern.Lab.WinForms.Controls.Input`

## 호환 제공 멤버

| 멤버 | 비고 |
|---|---|
| `Text` | `Control.Text` override, `[Localizable(true)]` |
| `Checked` | 체크 상태 |
| `CheckedChanged` | 체크 상태가 바뀔 때 1회 발생 (클릭·코드 할당 공통; 같은 값 재할당은 미발생) |
| `Enabled` | 전파됨 |

## 미지원 멤버와 대체 방법

| 기존 멤버 | 대체 |
|---|---|
| `ThreeState` / `CheckState.Indeterminate` | 없음 — 3상태가 필요하면 별도 요청 |
| `Appearance.Button` | 없음 — 토글 버튼이 필요하면 ModernToggleSwitch(예정) 사용 |
| `CheckAlign` / `TextAlign` | 고정 (박스 왼쪽 + 텍스트 오른쪽, 세로 중앙) |

## .Designer.cs 교체 예시

```csharp
// 변경 전
private System.Windows.Forms.CheckBox chkRecentOnly;
this.chkRecentOnly = new System.Windows.Forms.CheckBox();

// 변경 후
private Modern.Lab.WinForms.Controls.Input.ModernCheckBox chkRecentOnly;
this.chkRecentOnly = new Modern.Lab.WinForms.Controls.Input.ModernCheckBox();
this.chkRecentOnly.Text = "2020년 이후 입사";
this.chkRecentOnly.CheckedChanged += new System.EventHandler(this.OnRecentOnlyCheckedChanged);
```

권장 크기: 폭은 텍스트에 맞게, 높이 24.
