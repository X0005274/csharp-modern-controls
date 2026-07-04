# ModernToast 도입 가이드

- **대체 대상**: 확인이 필요 없는 완료/안내용 `MessageBox.Show(...)`
- **네임스페이스**: `Modern.Lab.WinForms.Controls.Display`

"저장되었습니다" 같은 알림을 확인 버튼 없이 부모 우하단에 잠깐 표시했다가
자동으로 없앤다. **사용자의 결정(예/아니오)이 필요한 확인 대화상자는 계속
`MessageBox`를 쓴다** — 토스트는 흐름을 끊지 않는 통지 전용.

## 제공 멤버

| 멤버 | 설명 |
|---|---|
| `Show(message)` | Info 종류로 표시 |
| `Show(message, kind)` | 종류 지정: `Info`(파랑 i) / `Success`(초록 체크) / `Warning`(주황 !) / `Error`(빨강 X) |
| `HideToast()` | 즉시 숨김 |
| `DurationMs` | 표시 유지 시간(밀리초, 기본 2500) |

## 동작

- 메시지 길이에 맞게 폭이 자동 조정되고 부모 우하단(여백 16px)에 표시된다
- 표시 중 다시 `Show`하면 내용이 바뀌고 타이머가 재시작된다
- 폼에 하나만 배치하면 된다 (위치는 Show가 재계산하므로 아무 곳에나)

## MessageBox → Toast 교체 예시

```csharp
private Modern.Lab.WinForms.Controls.Display.ModernToast toastMain;

// 변경 전
MessageBox.Show(this, "직원 21건을 저장했습니다.", "직원관리");

// 변경 후
this.toastMain.Show("직원 21건을 저장했습니다.",
    Modern.Lab.Controls.Wpf.Display.ToastKind.Success);

// 검증 안내는 Warning으로
this.toastMain.Show("삭제할 직원을 먼저 선택하세요.",
    Modern.Lab.Controls.Wpf.Display.ToastKind.Warning);
```

## 주의

| 항목 | 내용 |
|---|---|
| 확인 대화상자 | 대체 불가 — 예/아니오 결정은 `MessageBox` 유지 (샘플의 삭제 확인 참고) |
| 다중 스택 | 미지원 — 마지막 알림 하나만 표시 (연속 호출 시 교체) |
| 폼 밖 표시 | 미지원 — 부모 컨테이너 안에서만 표시 |
