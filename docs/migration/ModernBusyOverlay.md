# ModernBusyOverlay 도입 가이드

- **대체 대상**: 없음(신규 개념) — 조회/처리 중 로딩 표시
- **네임스페이스**: `Modern.Lab.WinForms.Controls.Display`

서버 조회처럼 시간이 걸리는 작업 동안 **가운데 뜨는 컴팩트 팝업 카드**(아크 스피너 +
메시지)를 표시한다. 대상 영역을 통째로 덮지 않고, 표시 시점에 부모 정중앙으로 배치된다.
기본은 숨김 상태이고 `Busy = true`일 때만 나타난다.

## 제공 멤버

| 멤버 | 설명 |
|---|---|
| `Busy` | `true` = 부모 중앙 배치 + 표시 + 맨 앞으로(BringToFront), `false` = 숨김. 기본 `false` |
| `Message` | 스피너 아래 주 안내 문구. 기본 `"처리 중..."`. `[Localizable(true)]` |
| `SubMessage` | 주 메시지 아래 보조 문구(선택). 비어 있으면 숨김. 기본 `""`. `[Localizable(true)]` |
| `Enabled` | 전파됨 |

## 배치 방법

컴팩트 팝업이라 **Dock/Anchor 없이** 고정 크기(기본 300×180)로 두면 된다 — `Busy = true`
시점에 부모 클라이언트 영역 정중앙으로 자동 배치된다. z-순서만 위(컨테이너에 먼저 Add =
index 0)로 둔다.

```csharp
// .Designer.cs — 오버레이를 그리드보다 먼저 Add해야 위(z-순서 0)에 놓인다
this.Controls.Add(this.busyOverlay);   // Dock/Anchor 불필요, 고정 크기(300x180)
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
| 반투명 | 불가 — ElementHost는 아래 형제 컨트롤이 비치는 반투명을 지원하지 않는다. 그래서 전체를 덮는 배경 대신 **가운데 작은 불투명 카드 팝업**으로 표시한다(카드 바깥은 흰색 배경) |
| 진행률(%) | 미지원 — 불확정(indeterminate) 스피너 전용. 진행률 바가 필요하면 별도 요청 |
