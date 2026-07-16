# ModernSpreadGrid 교체 가이드

- **대체 대상**: FarPoint Spread 8 (COM/ActiveX, `AxFPSpread.AxfpSpread` 등 interop 클래스)
- **네임스페이스**: `Modern.Lab.WinForms.Controls.Data`
- **소스**: `Modern.Lab.Commons/WinForms/Data/ModernSpreadGrid.cs`

## 다른 모던 컨트롤과 다른 점 (먼저 읽을 것)

`ModernSpreadGrid`는 ElementHost/WPF 래퍼가 **아니다**. 회사에 등록된 Spread 8
OCX를 **상속**해서 모던 디자인 토큰(테마 색·폰트·행 높이)만 입히는 클래스다.
따라서:

- 폼의 **기존 Spread API 호출 코드는 전부 그대로 동작한다** (SetText, GetText,
  Row/Col, 이벤트 등). 코드비하인드를 고칠 필요가 없는 것이 기본이다.
- 이 파일은 개발 PC에 Spread interop이 없으면 컴파일되지 않으므로,
  라이브러리 원본 솔루션에서는 **빌드 제외** 상태다. 회사 솔루션에서 아래
  연결 절차를 거쳐야 사용할 수 있다.

## 최초 1회 연결 절차 (회사 환경)

1. 화면 프로젝트(또는 공용 컨트롤 프로젝트)에 Spread 8 OCX 참조가 있는지 확인
   — 있으면 interop 어셈블리(`AxFPSpread.dll`, `FPSpread.dll` 등)가 이미 생성돼 있다.
2. `ModernSpreadGrid.cs` 파일을 회사 컨트롤 프로젝트에 **포함(Compile)** 한다.
3. 파일 상단 `class ModernSpreadGrid : AxFPSpread.AxfpSpread` 의 base 클래스를
   실제 interop 클래스 이름에 맞게 고친다 (버전에 따라 `AxFPUSpread.AxfpSpread` 등).
4. 빌드한다. 오류가 나는 지점은 대부분 `// ※확인` 주석이 달린 멤버다 —
   회사 Spread 버전의 실제 멤버명으로 맞춘다 (아래 표 참고).
5. 성공하면 이후 폼 교체는 `.Designer.cs` 타입 교체만으로 끝난다.

### ※확인 멤버 (Spread 버전별 이름이 다를 수 있는 지점)

| 용도 | 코드의 가정 | 오류 시 확인할 것 |
|---|---|---|
| 데이터 행 수 | `MaxRows` | 행 개수 속성 |
| 활성 행 | `ActiveRow` (1 기반) | 활성 셀 행 속성 |
| 다시그리기 억제 | `ReDraw` | 재드로 토글 속성 |
| 셀 텍스트 | `SetText(col, row, text)` | 셀 값 설정 메서드 |
| 행 높이/열 폭 | `RowHeight(row, px)` / `ColWidth(col, w)` | 치수 메서드 |
| 외형 | `Appearance`/`BorderStyle`/`GridColor`/`SelBackColor`/`SelForeColor`/`OperationMode`/`Protect`/`TypeHAlign` | 대응 속성과 열거값 |

## 호환 제공 멤버 (ModernDataGrid와 동일 형태)

| 멤버 | 비고 |
|---|---|
| `DataSource` | **`DataTable`/`DataView`만** 지원 (그 외 타입은 명시적 예외). 할당하면 헤더+셀을 다시 채우고 스타일 재적용 |
| `ConfigureColumns(params ModernDataGridColumn[])` | 열 순서/캡션/폭/형식/정렬 지정. `DataSource` 할당 전에 호출. **`Kind`(체크박스/배지/버튼)는 미지원 — 텍스트 열만** |
| `RowCount` | 데이터 행 수 (헤더 제외) |
| `SelectedItem` | 선택 행의 `DataRowView` (미선택 시 null) |
| `SelectedIndex` | 0 기반, 미선택 시 -1 |
| `SelectionChanged` | **`DataSource` 할당 완료 시에만 발생** — 사용자 클릭에는 발생하지 않는다. 행 클릭 연동이 필요하면 기존 Spread 이벤트(Click/LeaveCell 등)를 그대로 쓴다 |
| `AlternatingRowColors` | 교차 행 배경. **기본 true** (기존 화면 외관 보존; `ModernDataGrid`는 기본 false) |

## 적용되는 모던 스타일

- 색은 전부 `ModernTheme` 팔레트에서 읽는다 — 앱 시작 시 `ModernTheme.Mode`만
  설정하면 7종 테마가 그대로 적용된다.
- 헤더 SemiBold + 옅은 파랑 배경, Segoe UI 9pt, 행 높이 32 / 헤더 36,
  전체 행 선택(액센트 배경), 행 헤더(열 0) 숨김, 평면(비3D) 외형.
- **`Protect = true`(읽기 전용)가 기본 적용된다** — 기존 화면이 셀을 직접
  편집한다면 `ApplyModernStyle()`의 해당 줄을 제거해야 한다 (파일 내 주석 참고).

## .Designer.cs 교체 예시

```csharp
// 변경 전
private AxFPSpread.AxfpSpread spdList;
this.spdList = new AxFPSpread.AxfpSpread();

// 변경 후
private Modern.Lab.WinForms.Controls.Data.ModernSpreadGrid spdList;
this.spdList = new Modern.Lab.WinForms.Controls.Data.ModernSpreadGrid();
```

- `AxHost` 파생이므로 디자이너의 `BeginInit`/`EndInit`/`OcxState` 줄은
  **그대로 둔다** (삭제하면 OCX 초기화가 깨진다).
- 폼의 기존 Spread 이벤트 연결(`+=`)도 그대로 둔다.

## 미지원/주의

| 항목 | 설명 |
|---|---|
| 셀 편집 | 기본 읽기 전용(`Protect = true`). 편집 화면은 위 "적용되는 모던 스타일" 참고 |
| `DataSource`에 IList/IEnumerable | 미지원 — `DataTable`/`DataView`로 변환 후 할당 |
| 체크박스/배지/버튼 열 | 미지원 — 필요하면 기존 Spread CellType 코드를 그대로 유지 |
| 디자인 타임 미리보기 | OCX 특성상 디자이너에서 스타일이 안 보일 수 있다 — 실행 화면 기준으로 확인 |
