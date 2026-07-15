# ModernTabControl 도입 가이드

- **대체 대상**: `System.Windows.Forms.TabControl` — 영역 안에서 여러 페이지 전환
- **네임스페이스**: `Modern.Lab.WinForms.Controls.Layout`

## 특징 — 순수 WinForms 컨테이너

`ModernCardPanel`과 마찬가지로 GDI+로 그리는 일반 WinForms 컨테이너다 (계약 룰 5).
언더라인(피벗) 스타일: 탭을 채우지 않고 텍스트만 두되, 선택 탭은 액센트색
SemiBold + 아래 액센트 밑줄, 헤더 하단에 옅은 구분선. 색은 그릴 때
`ModernTheme` 팔레트를 읽으므로 7개 테마 전부 자동 대응한다.

## 페이지 모델 — ModernTabPage (표준 TabPage 대응)

페이지는 `ModernTabPage`(=`TabPage` 대응, `Panel` 파생)로 구성한다.
`Text`가 탭 제목이며, 페이지의 크기/위치/표시 여부는 컨트롤이 선택 상태와
`DisplayRectangle`(헤더 제외 본문 영역)로 직접 관리한다.

**폼 디자이너 지원** (`ModernTabControlDesigner`):

- 컨트롤 우클릭/스마트 태그의 **"탭 추가" / "선택 탭 제거"** 동사로 페이지를 만든다.
- 디자인 타임에도 **헤더 클릭으로 탭 전환**이 동작한다.
- 페이지 위에 컨트롤을 드래그해 놓으면 그 페이지의 자식으로 직렬화된다.
- 직접 자식으로는 `ModernTabPage`만 허용된다 (일반 컨트롤은 페이지 안에).

## 제공 멤버

| 멤버 | 설명 |
|---|---|
| `Controls` (ModernTabPage) | 디자이너 직렬화 경로 — 페이지를 자식으로 추가 |
| `AddTab(title, content)` | 런타임 코드 경로 — content를 `Dock=Fill`로 담은 페이지를 만들어 추가 |
| `SelectedIndex` | 선택 탭 인덱스 (설정 시 전환) |
| `SelectedTab` | 현재 선택된 `ModernTabPage` (없으면 null) |
| `SelectedIndexChanged` | 선택 탭 변경 이벤트 |
| `SetTabTitle(index, title)` | 탭 제목 변경 — "Unit History — IT10001.01"처럼 대상 표시용. `page.Text` 변경과 동일 |
| `TabCount` | 탭 수 |

## `.Designer.cs` 직렬화 예시 — 하단 이력 영역을 탭 2개로

```csharp
this.tabHistory = new Modern.Lab.WinForms.Controls.Layout.ModernTabControl();
this.pageItemHistory = new Modern.Lab.WinForms.Controls.Layout.ModernTabPage();
this.pageUnitHistory = new Modern.Lab.WinForms.Controls.Layout.ModernTabPage();
// ...
this.tabHistory.Controls.Add(this.pageItemHistory);
this.tabHistory.Controls.Add(this.pageUnitHistory);
this.pageItemHistory.Controls.Add(this.gridHistory);      // 그리드는 페이지의 자식
this.pageItemHistory.Text = "Item History";
this.pageUnitHistory.Controls.Add(this.gridUnitHistory);
this.pageUnitHistory.Text = "Unit History";
```

코드 비하인드에서는 데이터만 갱신하고 탭은 전환하지 않는 패턴(사용자가 보던 탭 유지):

```csharp
this.gridUnitHistory.DataSource = unitHistory;
this.tabHistory.SetTabTitle(1, "Unit History — " + unitId);
```

런타임 전용 화면이라면 기존 `AddTab`도 그대로 쓸 수 있다:

```csharp
this.tabHistory.AddTab("Item History", this.gridHistory);
this.tabHistory.AddTab("Unit History", this.gridUnitHistory);
```

## 주의

| 항목 | 내용 |
|---|---|
| 페이지 배치 | 페이지는 `Dock`이 아니라 컨트롤이 `DisplayRectangle`로 직접 잡는다 — 페이지의 Location/Size를 손으로 맞출 필요 없음 |
| 페이지 Visible | 선택 상태로 컨트롤이 관리하므로 직렬화되지 않는다 (표준 TabPage와 동일) |
| 헤더 배경 | 부모의 부모(폼/패널) 배경색을 따라간다 — 폼 배경 위에 두는 것을 권장 |
