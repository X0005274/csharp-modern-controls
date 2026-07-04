# ModernPagination 도입 가이드

- **대체 대상**: 없음(신규 개념) — 서버 페이징 화면의 그리드 하단 페이지 바
- **네임스페이스**: `Modern.Lab.WinForms.Controls.Data`

좌측에 "총 N건", 우측에 ◀ 1 2 3 ▶ 페이지 버튼(현재 페이지 중심 최대 7개)이 표시된다.

## 제공 멤버

| 멤버 | 설명 |
|---|---|
| `TotalCount` | 전체 건수. 조회 응답을 받을 때마다 갱신 — 페이지 수가 자동 계산됨 |
| `PageSize` | 페이지당 건수 (기본 20) |
| `CurrentPage` | 현재 페이지 (1부터). 범위를 벗어나게 할당해도 자동 보정, 결과가 줄면 자동으로 마지막 페이지로 |
| `PageCount` | 전체 페이지 수 (읽기 전용, 최소 1) |
| `PageChanged` | 현재 페이지가 바뀔 때 1회 발생 (버튼 클릭·코드 할당 공통) |
| `Enabled` | 전파됨 |

## 사용 예시 (서버 페이징)

```csharp
private Modern.Lab.WinForms.Controls.Data.ModernPagination pagerEmployee;

// 조회 완료 시: 전체 건수를 알려주고 1페이지로
this.pagerEmployee.TotalCount = reply.TotalCount;
this.pagerEmployee.CurrentPage = 1;
this.gridEmployee.DataSource = reply.PageData;

// 페이지 이동: 해당 페이지를 서버에 요청
private void OnPagerPageChanged(object sender, EventArgs e)
{
    RequestPage(this.pagerEmployee.CurrentPage, this.pagerEmployee.PageSize);
}
```

로컬 페이징(전체 결과를 이미 받은 경우)이라면 저장해 둔 DataTable에서
`(CurrentPage - 1) * PageSize` 구간을 잘라 다시 바인딩하면 된다
(샘플 `EmployeeManagementForm.BindCurrentPage` 참고).

## 페이지 크기를 화면 높이에 맞추기 (자동 PageSize)

`ModernDataGrid.VisibleRowCapacity`(현재 높이에서 스크롤 없이 보이는 행 수)와
`VisibleRowCapacityChanged` 이벤트를 연동하면 페이지 크기가 폼 크기를 따라간다:

```csharp
private void OnGridCapacityChanged(object sender, EventArgs e)
{
    int capacity = this.gridEmployee.VisibleRowCapacity;

    if (this.pagerEmployee.PageSize != capacity)
    {
        this.pagerEmployee.PageSize = capacity;   // CurrentPage는 자동 보정
        this.BindCurrentPage();                   // 서버 페이징이면 재요청
    }
}
```

서버 페이징에서는 리사이즈 중 연속 재요청을 피하려면 `Form.ResizeEnd` 시점에만
반영하는 것을 권장.

## 배치

그리드 아래 `Dock = Bottom`, 높이 32. 통계(건수 카드·칩)는 전체 결과 기준으로
유지하고 그리드에만 페이지를 적용하는 것이 관례.
