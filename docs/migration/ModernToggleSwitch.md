# ModernToggleSwitch 도입 가이드

- **대체 대상**: 설정성 켬/끔 용도의 `CheckBox`
- **네임스페이스**: `Modern.Lab.WinForms.Controls.Input`

`ModernCheckBox`와 **완전히 동일한 API**에 비주얼만 온/오프 스위치(알약 트랙 + 원형 썸).

## 용도 구분

| 상황 | 사용 |
|---|---|
| 다중 "선택/포함" (필터 조건, 항목 선택) | `ModernCheckBox` |
| 설정성 "켬/끔" (표시 옵션, 기능 활성화) | `ModernToggleSwitch` |

## 제공 멤버

| 멤버 | 비고 |
|---|---|
| `Text` | `Control.Text` override, `[Localizable(true)]` |
| `Checked` | 켬/끔 상태 |
| `CheckedChanged` | 상태가 바뀔 때 1회 발생 (같은 값 재할당은 미발생) |
| `Enabled` | 전파됨 |

## 사용 예시

```csharp
private Modern.Lab.WinForms.Controls.Input.ModernToggleSwitch tglShowEmail;

this.tglShowEmail = new Modern.Lab.WinForms.Controls.Input.ModernToggleSwitch();
this.tglShowEmail.Text = "이메일 표시";
this.tglShowEmail.Checked = true;
this.tglShowEmail.CheckedChanged += new System.EventHandler(this.OnShowEmailToggled);

private void OnShowEmailToggled(object sender, EventArgs e)
{
    this.ConfigureGridColumns();   // 켬/끔에 따라 화면 구성 변경
    this.ExecuteSearch();
}
```

권장 크기: 폭은 텍스트에 맞게, 높이 24.
