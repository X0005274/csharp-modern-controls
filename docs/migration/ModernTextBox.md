# ModernTextBox 교체 가이드

- **대체 대상**: `System.Windows.Forms.TextBox` (단일행)
- **네임스페이스**: `Modern.Lab.WinForms.Controls.Input`

## 호환 제공 멤버

| 멤버 | 비고 |
|---|---|
| `Text` | `Control.Text` override, `[Localizable(true)]` |
| `TextChanged` | 표준 WinForms 이벤트 — 내부 WPF 텍스트 변경 시 발생. 기존 핸들러 그대로 동작. 한글 조합 중에는 음절 확정 시점에 발생 |
| `ReadOnly` | `TextBox.ReadOnly`와 동일 의미. 배경이 옅게 바뀜 |
| `AutoCompleteMode` | `None` 외의 값(`Suggest`/`SuggestAppend`/`Append`)은 모두 **제안 드롭다운(Suggest)** 으로 동작 |
| `AutoCompleteSource` | **`CustomSource`만 지원** — 다른 값은 자동완성 비활성 |
| `AutoCompleteCustomSource` | `AutoCompleteStringCollection` 그대로 수용. 포함(contains) 매칭 + **한글 초성 검색**("ㄱ" → 김민수, "김ㅎ" → 김하늘) + 조합 중간 상태 매칭("기" → 김민수), 영문 대소문자 무시, 최대 8건 표시. ↓/↑ 탐색, Enter 선택(선택 후 `EnterPressed` 발생), Esc 닫기, 클릭 선택 |
| `Enabled` | 전파됨 |

### 자동완성 예시 (기존 WinForms 코드 그대로)

```csharp
AutoCompleteStringCollection names = new AutoCompleteStringCollection();
names.Add("김민수");
names.Add("김하늘");

this.txtName.AutoCompleteMode = AutoCompleteMode.Suggest;
this.txtName.AutoCompleteSource = AutoCompleteSource.CustomSource;
this.txtName.AutoCompleteCustomSource = names;   // 재할당 시 후보 갱신
```

## 추가 멤버

| 멤버 | 설명 |
|---|---|
| `PlaceholderText` | 입력이 비었을 때 표시할 힌트. 한글 조합 시작(자음 입력)과 동시에 숨겨짐 — 표시 여부가 IME 조합 중에도 발생하는 내부 `TextChanged` 기반이라 바인딩 지연의 영향을 받지 않음 |
| `EnterPressed` | Enter 키 입력 시 발생. **주의: 기존 `KeyDown`으로 Enter를 잡던 코드는 이 이벤트로 옮겨야 함** — WPF 에디터 내부에서 처리된 키는 WinForms `KeyDown`으로 오지 않음 |

## 미지원 멤버와 대체 방법

| 기존 멤버 | 대체 |
|---|---|
| `KeyDown`/`KeyPress` (Enter 감지 용도) | `EnterPressed` 이벤트 |
| `MaxLength`, `PasswordChar`, `Multiline` | 미구현 — 필요 시 컨트롤 확장 요청 (비밀번호는 별도 ModernPasswordBox로) |
| `Font`, `BackColor` | 없음 — 토큰이 결정 |

## .Designer.cs 교체 예시

```csharp
// 변경 전
private System.Windows.Forms.TextBox txtEmpName;
this.txtEmpName = new System.Windows.Forms.TextBox();

// 변경 후
private Modern.Lab.WinForms.Controls.Input.ModernTextBox txtEmpName;
this.txtEmpName = new Modern.Lab.WinForms.Controls.Input.ModernTextBox();
this.txtEmpName.PlaceholderText = "이름 입력";
```

권장 크기: 200×32.
