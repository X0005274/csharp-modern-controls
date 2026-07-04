# ModernBusyOverlay 도입 가이드

- **대체 대상**: 없음(신규 개념) — 조회/처리 중 로딩 표시
- **네임스페이스**: `Modern.Lab.WinForms.Controls.Display`

서버 조회처럼 시간이 걸리는 작업 동안 대상 영역(보통 그리드)을 스피너 + 메시지
패널로 덮는다. 기본은 숨김 상태이고 `Busy = true`일 때만 나타난다.

## 제공 멤버

| 멤버 | 설명 |
|---|---|
| `Busy` | `true` = 표시 + 맨 앞으로(BringToFront), `false` = 숨김. 기본 `false` |
| `Message` | 스피너 아래 안내 문구. 기본 `"처리 중..."`. `[Localizable(true)]` |
| `Enabled` | 전파됨 |

## 배치 방법

덮을 영역과 **같은 Dock/Bounds**로 배치하고 z-순서를 위(컨테이너에 먼저 Add =
index 0)로 둔다. 그리드가 `Dock = Fill`이면 오버레이도 `Dock = Fill` — 두 Fill
컨트롤은 같은 영역을 차지하므로 정확히 겹친다.

```csharp
// .Designer.cs — 오버레이를 그리드보다 먼저 Add해야 위(z-순서 0)에 놓인다
this.Controls.Add(this.busyOverlay);
this.Controls.Add(this.gridEmployee);
```

## 사용 예시 (백그라운드 조회)

```csharp
private void OnSearchClick(object sender, EventArgs e)
{
    this.busyOverlay.Busy = true;

    // 백그라운드 조회 후 UI 스레드로 복귀해 반영 (계약: 백그라운드 조회 + Invoke)
    System.Threading.ThreadPool.QueueUserWorkItem(delegate(object state)
    {
        DataTable result = CallServer();   // 서버 request/reply

        this.Invoke(new System.Windows.Forms.MethodInvoker(delegate
        {
            this.gridEmployee.DataSource = result;
            this.busyOverlay.Busy = false;
        }));
    });
}
```

## 주의

| 항목 | 내용 |
|---|---|
| 반투명 | 불가 — ElementHost는 아래 형제 컨트롤이 비치는 반투명을 지원하지 않아 **불투명 패널**로 덮는다 |
| 진행률(%) | 미지원 — 불확정(indeterminate) 스피너 전용. 진행률 바가 필요하면 별도 요청 |
