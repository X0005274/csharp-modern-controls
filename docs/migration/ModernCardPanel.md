# ModernCardPanel 도입 가이드

- **대체 대상**: 영역 구분용 `Panel`/`GroupBox` — 카드 룩(흰 표면·옅은 테두리·radius 8)으로 통일
- **네임스페이스**: `Modern.Lab.WinForms.Controls.Layout`

## 특징 — 순수 WinForms 컨테이너

이 컨트롤은 **ElementHost가 아니라 GDI+로 그리는 일반 WinForms `Panel` 파생**이다.
따라서 계약 룰 5(영역 레이아웃은 WinForms 담당)를 그대로 지키면서, 모든 WinForms
자식 — 모던 리프 컨트롤 포함 — 을 담을 수 있다. (ElementHost 기반 모던 컨테이너는
WinForms 자식을 담지 못하므로 만들지 않는다는 원칙과 충돌하지 않음.)

## 제공 멤버

`Panel`의 모든 멤버 그대로 (`Dock`, `Padding`, `Controls`, ...). 추가 멤버 없음 —
표면색/테두리/radius는 토큰 값(Surface/BorderSubtle/8)으로 고정.

기본 `Padding`: (12, 8, 12, 8). 배경은 흰색(Surface)이므로 자식 컨트롤 배경이
자연스럽게 이어진다.

## 사용 예시 — 조회조건 영역을 카드로

```csharp
using Modern.Lab.WinForms.Controls.Layout;

ModernCardPanel searchCard = new ModernCardPanel();
searchCard.Dock = DockStyle.Fill;          // TableLayoutPanel 셀에 배치
searchCard.Padding = new Padding(12, 9, 12, 9);

FlowLayoutPanel searchRow = new FlowLayoutPanel();
searchRow.Dock = DockStyle.Fill;
// ... 라벨/입력/버튼 추가 ...
searchCard.Controls.Add(searchRow);
```

## 함께 쓰는 패턴 — Flat 통계 컨트롤

`ModernKpiCard`/`ModernSummaryList`는 자체 카드 테두리를 갖고 있으므로, 카드 판넬
위에 올릴 때는 `Flat = true`로 개별 테두리를 끄고 판넬이 카드 역할을 하게 한다
(중첩 카드 방지). `EmployeeManagementForm` 하단 영역이 이 패턴의 예.

## 주의

| 항목 | 내용 |
|---|---|
| 모서리 배경 | 둥근 모서리 바깥은 부모 `BackColor`로 다시 칠함 — 부모 배경이 단색일 때 자연스러움 |
| `BackColor` | 기본 Surface(흰색). 변경하지 않는 것을 권장 (토큰 일관성) |
