# ModernTabControl 도입 가이드

- **대체 대상**: `System.Windows.Forms.TabControl` — 영역 안에서 여러 페이지 전환
- **네임스페이스**: `Modern.Lab.WinForms.Controls.Layout`

## 특징 — 순수 WinForms 컨테이너

`ModernCardPanel`과 마찬가지로 GDI+로 그리는 일반 WinForms 컨테이너다 (계약 룰 5).
언더라인(피벗) 스타일: 탭을 채우지 않고 텍스트만 두되, 선택 탭은 액센트색
SemiBold + 아래 액센트 밑줄, 헤더 하단에 옅은 구분선. 색은 그릴 때
`ModernTheme` 팔레트를 읽으므로 7개 테마 전부 자동 대응한다.

## 제공 멤버

| 멤버 | 설명 |
|---|---|
| `AddTab(title, content)` | 탭 추가 — content는 본문에 `Dock=Fill`로 배치되고 선택 시에만 보임 |
| `SelectedIndex` | 선택 탭 인덱스 (설정 시 전환) |
| `SelectedIndexChanged` | 선택 탭 변경 이벤트 |
| `SetTabTitle(index, title)` | 탭 제목 변경 — "Unit History — IT10001.01"처럼 대상 표시용 |
| `TabCount` | 탭 수 |

## 사용 예시 — 하단 이력 영역을 탭 2개로

```csharp
using Modern.Lab.WinForms.Controls.Layout;

// Designer에는 컨테이너만 배치(Dock=Fill)하고, 페이지는 코드에서 붙인다.
this.tabHistory.AddTab("Item History", this.gridHistory);
this.tabHistory.AddTab("Unit History", this.gridUnitHistory);

// 데이터만 갱신하고 탭은 전환하지 않는 패턴 (사용자가 보던 탭 유지):
this.gridUnitHistory.DataSource = unitHistory;
this.tabHistory.SetTabTitle(1, "Unit History — " + unitId);
```

## 주의

| 항목 | 내용 |
|---|---|
| TabPage 없음 | WinForms `TabPage` 컬렉션 모델이 아니라 `AddTab(제목, 컨트롤)` 방식 — 디자이너 드래그 배치 대신 코드로 페이지를 구성한다 |
| 헤더 배경 | 부모의 부모(폼/패널) 배경색을 따라간다 — 폼 배경 위에 두는 것을 권장 |
