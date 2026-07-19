# ModernDropDownButton 도입 가이드

- **대체 대상**: `Button` + `ContextMenuStrip` 조합 (버튼 하나에 여러 동작)
- **네임스페이스**: `Modern.Lab.WinForms.Controls.Input`

`ToolStripMenuItem`을 코드로 쌓고 이벤트를 개별 배선하는 대신, 메뉴 항목을
코드/명칭 테이블로 할당하고 `ItemClicked` 하나에서 코드로 분기한다.

## 제공 멤버

| 멤버 | 설명 |
|---|---|
| `Text` | 버튼 캡션. 셰브런(▼)은 자동. `Control.Text` override, `[Localizable(true)]` |
| `DataSource` | 메뉴 항목: `DataTable`/`DataView`/`IList`/`IEnumerable` |
| `DisplayMember` / `ValueMember` | 메뉴 표시 텍스트 / 항목 값 컬럼 |
| `EnabledMember` | 항목 실행 가능 여부 컬럼 (bool 또는 `"Y"`/`"true"`/`"1"`; 비우면 전부 활성). 비활성 항목은 회색 + 클릭 불가 — `ToolStripMenuItem.Enabled` 대체 |
| `Kind` | 버튼 시각 종류 — `Secondary`(기본) / `Execute`(초록 채움) / `Excel`(초록 아웃라인; 엑셀 내보내기 포인트) — `ModernButton.Kind`와 같은 문법. 캡션 굵기도 `ModernButton`과 동일한 SemiBold |
| `IsDropDownOpen` | 읽기 전용. 메뉴 팝업이 열려 있으면 true — 자동 갱신/재바인딩 화면은 true 동안 갱신을 보류해 사용 중인 메뉴를 유지 |
| `ItemClicked` | 항목 클릭 시 발생 — `DropDownItemClickedEventArgs.Value`/`DisplayText` |
| `Enabled` | 전파됨 |

## 사용 예시

```csharp
private Modern.Lab.WinForms.Controls.Input.ModernDropDownButton ddExcel;

DataTable exportTable = new DataTable();
exportTable.Columns.Add("EXPORT_CODE", typeof(string));
exportTable.Columns.Add("EXPORT_NAME", typeof(string));
exportTable.Rows.Add("PAGE", "현재 페이지 내보내기");
exportTable.Rows.Add("ALL", "전체 결과 내보내기");

this.ddExcel.Text = "엑셀";
this.ddExcel.DisplayMember = "EXPORT_NAME";
this.ddExcel.ValueMember = "EXPORT_CODE";
this.ddExcel.DataSource = exportTable;
this.ddExcel.ItemClicked += new System.EventHandler<Modern.Lab.Controls.Wpf.Input.DropDownItemClickedEventArgs>(this.OnExcelItemClicked);

private void OnExcelItemClicked(object sender, Modern.Lab.Controls.Wpf.Input.DropDownItemClickedEventArgs e)
{
    switch (e.Value as string)
    {
        case "PAGE": /* 현재 페이지 내보내기 */ break;
        case "ALL": /* 전체 내보내기 */ break;
    }
}
```

## 미지원 / 주의

| 항목 | 내용 |
|---|---|
| 버튼 Kind(Primary 등) | 없음 — 보조(Secondary) 스타일 고정 |
| 구분선/아이콘/중첩 메뉴 | 미지원 — 평면 항목 목록만 |
| 기본 동작 분리(SplitButton) | 미지원 — 버튼 전체가 메뉴 열기 |

권장 크기: 100~130×32.
