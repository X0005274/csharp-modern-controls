# 기존 폼 → 모던 컨트롤 변환 절차서

이 문서는 코딩 에이전트(Codex)가 기존 WinForms 폼을 Modern.Lab 모던 컨트롤로
변환할 때 따르는 표준 절차다. AGENTS.md의 절대 규칙이 항상 우선한다.

---

## 0. 전제 — 솔루션에 라이브러리가 통합되어 있어야 한다

폼 변환을 시작하기 전에 아래가 이미 되어 있는지 확인한다. 안 되어 있으면
변환을 시작하지 말고 이 목록을 보고한다 (통합은 보통 사람/IT가 1회 수행).

- [ ] 호스트 프로젝트가 `Modern.Lab.Commons`(프로젝트 참조 또는 DLL)를 참조한다.
- [ ] 호스트 프로젝트에 다음 어셈블리 참조가 있다:
      `WindowsFormsIntegration`, `PresentationCore`, `PresentationFramework`,
      `WindowsBase`, `System.Xaml`
- [ ] 프로그램 진입점(Main)에 `[STAThread]`가 있다 (WinForms 기본 템플릿이면 이미 있음).
- [ ] (선택) 다크/색상 테마를 쓰려면 앱 시작 시 `ModernTheme.Mode` 설정 —
      기본은 Light이며 아무 설정 없이 동작한다.

## 1. 변환 원칙 — 무엇을 바꾸고 무엇을 절대 안 바꾸나

모던 컨트롤 래퍼는 대체 대상 WinForms 컨트롤과 **드롭인 호환**되게 설계됐다
(`DataSource`/`DisplayMember`/`SelectedValue`/`SelectedIndexChanged` 등 이름과
의미가 같음). 따라서 이상적인 변환은 **`.Designer.cs`에서 선언 타입만 바꾸는 것**이고,
폼의 서버 요청/응답 코드는 전혀 바뀌지 않아야 한다.

**바꾸는 것**
- `.Designer.cs`: 컨트롤 필드의 선언 타입, `new` 생성 타입
- `.Designer.cs`: 모던 컨트롤이 지원하지 않는 속성 설정 줄 삭제 (아래 3-3단계)
- 코드비하인드(`.cs`): 미지원 멤버를 사용하는 줄의 **최소 대체** (각 컨트롤
  마이그레이션 문서의 "미지원 멤버와 대체 방법" 표를 그대로 따름)

**절대 안 바꾸는 것**
- 서버/DB/전문 통신 코드, 데이터 가공·계산·검증 로직, 이벤트 처리의 업무 내용
- 배치 컨테이너 구조(`TableLayoutPanel`/`Panel`/`SplitContainer`의 트리, Dock/Anchor, 크기)
- 폼 전환/오픈 흐름, 생성자 시그니처, public 멤버
- 변환 대상으로 지시받지 않은 폼

## 2. 컨트롤 매핑 표

각 행의 상세(호환 멤버, 미지원 멤버 대체, .Designer.cs 예시)는 반드시
`docs/migration/<모던컨트롤>.md`를 열어 확인한 뒤 적용한다.

### 2-1. 기존 컨트롤 1:1 교체 (기본 대상)

| 기존 WinForms | 모던 컨트롤 | 네임스페이스 | 비고 |
|---|---|---|---|
| `Button` | `ModernButton` | `Modern.Lab.WinForms.Controls.Input` | |
| `Label` | `ModernLabel` | `Modern.Lab.WinForms.Controls.Display` | |
| `TextBox` (단일행) | `ModernTextBox` | `Modern.Lab.WinForms.Controls.Input` | 여러 줄(Multiline) TextBox는 보류 항목으로 보고 |
| 숫자용 `TextBox` / `NumericUpDown` | `ModernNumericTextBox` | `Modern.Lab.WinForms.Controls.Input` | 콤마 표시 내장 |
| `CheckBox` | `ModernCheckBox` | `Modern.Lab.WinForms.Controls.Input` | 설정성 켬/끔이면 `ModernToggleSwitch`도 가능(사람 승인 필요) |
| `ComboBox` | `ModernComboBox` | `Modern.Lab.WinForms.Controls.Selection` | 검색형(DropDown)/선택전용(DropDownList) 모두 지원 |
| `DateTimePicker` (날짜) | `ModernDatePicker` | `Modern.Lab.WinForms.Controls.Input` | |
| `DateTimePicker` (yyyy-MM) / 년·월 콤보 2개 | `ModernMonthPicker` | `Modern.Lab.WinForms.Controls.Input` | |
| `GroupBox`/`Panel` + `RadioButton` 묶음 | `ModernRadioGroup` | `Modern.Lab.WinForms.Controls.Selection` | 라디오 여러 개 → 컨트롤 하나로 합침 |
| `DataGridView` (읽기 전용 조회) | `ModernDataGrid` | `Modern.Lab.WinForms.Controls.Data` | **셀 편집 화면은 부적합 — 3-1단계에서 판정**. `Kind = Badge`는 같은 컬럼의 가장 긴 표시값 기준으로 폭이 자동 통일되므로, 셀마다 여백 문자열이나 폭 보정 코드를 넣지 않는다 |
| FarPoint Spread 8 (`AxfpSpread`) | `ModernSpreadGrid` | `Modern.Lab.WinForms.Controls.Data` | 아래 2-3 참고 |
| `TreeView` (계층 선택) | `ModernTreeView` | `Modern.Lab.WinForms.Controls.Selection` | |
| `TabControl` | `ModernTabControl`(+`ModernTabPage`) | `Modern.Lab.WinForms.Controls.Layout` | |
| `GroupBox` | `ModernGroupBox` | `Modern.Lab.WinForms.Controls.Layout` | |
| `SplitContainer` | `ModernSplitContainer` | `Modern.Lab.WinForms.Controls.Layout` | |
| 영역 구분 `Panel`/`GroupBox` (카드 룩) | `ModernCardPanel` | `Modern.Lab.WinForms.Controls.Layout` | 시각적 판단 필요 — 사람 승인 후 적용 |
| 캡션/값 상세표 `TableLayoutPanel` | `ModernDetailTable` | `Modern.Lab.WinForms.Controls.Layout` | 사람 승인 후 적용 |
| 상태 표시용 색 `Label` | `ModernStatusBadge` | `Modern.Lab.WinForms.Controls.Display` | |
| `Button` + `ContextMenuStrip` | `ModernDropDownButton` | `Modern.Lab.WinForms.Controls.Input` | 캡션은 `ModernButton`과 같은 SemiBold가 기본이므로 별도 Font 지정 금지. 자동 갱신 화면은 `IsDropDownOpen`이 true인 동안 재바인딩을 보류 |
| 완료/안내용 `MessageBox.Show` | `ModernToast` | `Modern.Lab.WinForms.Controls.Display` | **확인(예/아니오)을 받는 MessageBox는 그대로 둔다** |

### 2-2. 신규 개념 컨트롤 — 변환 작업에서는 도입하지 않는다

`ModernBusyOverlay`(로딩), `ModernPagination`(페이지 바), `ModernKpiCard`,
`ModernSummaryList`, `ModernCheckComboBox`, `ModernStepIndicator`,
`ModernSlotMap`(캐리어 수납 구조 슬롯 맵) 등은 기존 폼에
대응물이 없는 **신규 컨트롤**이다. 변환 작업의 목표는 "동작 동일 + 외형 현대화"이므로,
사람이 명시적으로 요청한 경우에만 추가한다.

### 2-2-1. Carrier Editor 같은 수납 편집 화면 — 명시 요청 시에만

`ModernSlotMap`을 써야 하는 캐리어 수납 편집 화면은 반드시
`docs/migration/ModernSlotMap.md`를 먼저 읽고, 샘플
`Modern.Lab.Samples/CarrierEditForm`을 참조한다. 기존 화면을 단순 변환하는
작업에는 추가하지 않는다.

- LCC처럼 하나의 위치(`POS`)에 여러 핑거(`FINGER`)가 딸린 데이터는 **조회 행
  순서에 의존하지 말고 `POS`로 먼저 묶어** `SlotMapCell` 하나와 그 안의
  `SlotMapSubCell`들로 만든다.
- 이동 미리보기는 화면에서 빈 자리를 다시 계산하지 않는다. 서버가 확정한 배치
  계획을 `SetPreview`에 그대로 전달하고, LCC 삽입 방향은 같은 계획의
  `Top`/`Left`/`Right` 값을 `SetPreviewMarkers`에 전달한다. 즉 **미리보기와
  실제 이동 결과는 반드시 같아야 한다.**
- `SetPreviewMarkers`는 LCC 대상 핑거에만 쓴다. 단순 슬롯/STUB에는 별도 점선
  테두리나 방향 마커를 추가하지 않는다.

### 2-3. FarPoint Spread 8 → ModernSpreadGrid (특수)

- **반드시 `docs/migration/ModernSpreadGrid.md`를 먼저 읽는다.** 이 컨트롤은
  ElementHost 래퍼가 아니라 Spread 8 OCX를 **상속해 스타일만 입히는** 클래스라서
  기존 Spread API 호출 코드는 그대로 동작하며, `.Designer.cs`의 필드 타입 교체가
  기본이다 (`BeginInit`/`OcxState` 줄은 그대로 둔다).
- 최초 1회 "연결 절차"(파일 Compile 포함, base 클래스 이름 수정, `// ※확인`
  멤버 확인)가 끝나 있어야 한다 — 안 되어 있으면 변환을 멈추고 문서의
  연결 절차 체크리스트를 보고한다.
- 기본 스타일이 읽기 전용(`Protect = true`)이다 — 셀 편집 화면이면 3-1단계에서
  판정하고 문서의 주의 절을 따른다.

## 3. 폼 하나 변환 절차

### 3-1단계. 조사와 계획 (코드 수정 없음)

1. 대상 폼의 `.Designer.cs`와 `.cs`(코드비하인드)를 읽는다.
2. 폼에 놓인 모든 컨트롤의 인벤토리 표를 만든다:
   `필드명 | 현재 타입 | 매핑(2장 표) | 코드비하인드에서 쓰는 멤버 | 미지원 멤버 여부`
3. **적합성 판정**: 아래에 해당하면 그 컨트롤(또는 폼 전체)을 "보류"로 분류한다.
   - `DataGridView`에서 셀 직접 편집(`CellValueChanged`, `ReadOnly=false` 편집)을
     쓰는 화면 — `ModernDataGrid`는 읽기 전용(체크박스 컬럼 제외)
   - `MultiSelect` 다중 행 선택 의존 — 체크박스 컬럼으로 대체 가능한지 표시
   - Multiline `TextBox`, `RichTextBox`, `ListView` 등 대응 컨트롤이 없는 것
   - 서드파티 컨트롤 중 매핑 표에 없는 것
4. 인벤토리 표 + 보류 목록 + 예상 코드비하인드 보정 목록을 보고하고 **멈춘다.**
   사람의 승인을 받은 뒤 3-2단계로 간다.

### 3-2단계. .Designer.cs 타입 교체

각 컨트롤에 대해 (해당 컨트롤 마이그레이션 문서의 교체 예시 참고):

```csharp
// 변경 전
private System.Windows.Forms.DataGridView gridEmployee;
this.gridEmployee = new System.Windows.Forms.DataGridView();

// 변경 후
private Modern.Lab.WinForms.Controls.Data.ModernDataGrid gridEmployee;
this.gridEmployee = new Modern.Lab.WinForms.Controls.Data.ModernDataGrid();
```

- 필드 선언과 `new` 두 곳 모두 바꾼다. 네임스페이스는 **전체 경로**로 쓴다.
- `((System.ComponentModel.ISupportInitialize)(...)).BeginInit/EndInit` 줄이
  기존 컨트롤용으로 있었다면(DataGridView 등) 삭제한다.
- 이벤트 연결(`+=`)은 이름이 호환되므로 그대로 둔다
  (예: `SelectedIndexChanged`, `SelectionChanged`, `Click`).

### 3-3단계. .Designer.cs 속성 정리

교체한 컨트롤의 속성 설정 블록에서:

- **유지**: `Name`, `Location`, `Size`, `Dock`, `Anchor`, `TabIndex`, `Enabled`,
  `Visible`, `Text`(캡션), 데이터 바인딩 관련(`DisplayMember` 등)
- **삭제**: `BackColor`, `ForeColor`, `Font`, `FlatStyle`, `BorderStyle`,
  `UseVisualStyleBackColor` 등 **외형 지정 전부** — 모던 컨트롤은 테마가 외형을
  결정하며 이런 속성은 존재하지 않거나 무시된다. 컴파일 오류가 나는 줄은 모두
  이 범주다.
- **삭제 후 보고**: `DataGridView`의 `Columns` 디자이너 정의처럼 통째로 사라지는
  블록은 코드비하인드의 `ConfigureColumns(...)` 호출로 옮기고 보고에 명시한다.

### 3-4단계. 코드비하인드 보정

1. 컴파일 오류가 나는 멤버 사용부를 마이그레이션 문서의
   **"미지원 멤버와 대체 방법"** 표에 있는 대체로 바꾼다. 표에 없는 멤버는
   임의 구현하지 말고 보류 항목으로 보고한다.
   **같은 멤버에 대해 서로 다른 해법을 2회 시도해도 해결되지 않으면** 추가
   시도를 멈추고, 그 멤버 관련 변경을 되돌린 뒤 보류 항목으로 보고한다 —
   표에 없는 우회를 계속 발명하지 않는다.
2. 대표 패턴:
   - `grid.CurrentRow.DataBoundItem` → `grid.SelectedItem`
   - `grid.Rows[i].Cells["X"].Value` → `((DataRowView)grid.SelectedItem)["X"]`
   - `DataGridViewButtonColumn` + `CellContentClick` → `Kind = Button` 컬럼 + `CellButtonClick`
   - `ConfigureColumns` 작성 시: 앱 시작 시 Common의
     `GridCaptionDictionary.RegisterAll()`을 호출해 뒀다면 표준 캡션 컬럼은 `new ModernDataGridColumn("ITEM_ID")`처럼
     캡션 인자를 생략하고, 화면 문맥상 다른 표현만 headerText로 명시한다
   - 그리드 내용 엑셀 저장(기존 Excel COM/CSV 내보내기) →
     `grid.ExportXlsx(path, sheetName, dataTable)` — 화면 컬럼 정의 그대로 저장,
     내보내기용 컬럼 목록 코드는 삭제한다
   - 완료 안내 `MessageBox.Show("저장되었습니다")` → `toast.Show("저장되었습니다", ToastKind.Success)`
     (§단, 폼에 `ModernToast`를 새로 놓아야 하므로 사람 승인 후)
3. 업무 로직(무엇을 조회하고 무엇을 저장하는지)은 어떤 경우에도 바꾸지 않는다.

### 3-5단계. 빌드와 자가 검증

1. AGENTS.md의 빌드 명령으로 빌드한다. 오류가 없어질 때까지 3-3/3-4를 반복한다.
2. 흔한 오류:
   - **MSB3027 (파일 잠김)**: 실행 중인 프로그램을 종료한 뒤 다시 빌드
   - **MC1000 / XAML 캐시**: 라이브러리 쪽 `obj`/`bin` 삭제 후 재빌드
   - **타입/멤버 없음**: 3-3단계에서 지워야 할 외형 속성이 남았거나,
     네임스페이스 전체 경로 누락
3. 빌드 성공 후 아래 완료 기준을 스스로 점검한다.

### 3-6단계. 완료 기준 (Definition of Done)

- [ ] 빌드 성공 (경고 증가 없음)
- [ ] 대상 폼 외의 파일 수정 없음 — 수정 파일 목록으로 확인
      (git 환경이면 `git diff`, 아니면 `_backup` 사본과 대조)
- [ ] 서버 요청/응답·업무 로직 코드 무변경 (diff에 해당 줄이 없어야 함)
- [ ] 배치 컨테이너 구조·Dock/Anchor 무변경
- [ ] 색/폰트 하드코딩 새로 추가 없음
- [ ] 보류 항목이 전부 보고서에 기재됨
- [ ] AGENTS.md의 "완료 보고 형식"으로 보고 작성 — 특히 **"실행 확인 필요 항목"**에
      화면에서 사람이 눌러봐야 할 동작(조회, 선택, 저장, 콤보 변경 등)을 구체적으로 나열.
      **교체한 컨트롤마다 최소 1개 이상** 써야 하며, 화면 전체에서 항목이 5개
      미만이면 부실로 간주하고 다시 작성한다

## 4. 실행 검증은 사람이 한다

에이전트는 GUI를 직접 조작하지 않는다. 대신 보고서의 "실행 확인 필요 항목"을
충실히 작성해 사람이 프로그램을 켜고 그대로 따라 확인할 수 있게 한다.
항목은 "◯◯ 콤보에서 값을 바꾸면 그리드가 다시 조회된다"처럼
**동작 단위**로 쓴다.

## 5. 메인 프레임 통합 — 화면 오픈 지점

데모 갤러리의 셸(`SampleShellForm`)은 **회사로 가져가지 않는다** — 홈 데모
전용이다. 회사에는 이미 화면 폼을 여는 메인 프레임(메뉴/MDI)이 있고, 변환한
폼은 평범한 `Form`이라 기존 오픈 방식(`Show()`/`ShowDialog()`, 또는
`TopLevel=false` 임베드) 그대로 동작한다. 오픈 흐름 자체는 바꾸지 않는다
(§1 원칙).

다만 셸에서 검증한 두 패턴은 회사 프레임의 **폼 오픈 지점**에 이식할 가치가
있다.

### 5-1. 테마 적용 한 줄 (선택 — 다크/색상 테마를 쓸 때만)

폼 생성자에서 `InitializeComponent()` 직후 한 줄. 라이트 모드에서는 no-op이라
넣어 두어도 무해하다.

```csharp
public PendingRequestForm()
{
    this.InitializeComponent();
    Modern.Lab.Theming.ModernThemeWinForms.Apply(this);   // 테마 한 줄 적용
}
```

### 5-2. 로딩 커버 한 줄 (선택 — 폼이 열릴 때 깜빡이면)

폼이 **보이는 상태로** 열리면(특히 패널 임베드), ElementHost의 WPF 콘텐츠
생성과 그리드 AutoFit 컬럼 계산이 화면에 노출되어 컨트롤들이 크기를 잡아가는
중간 레이아웃이 그대로 깜빡인다.

라이브러리의 `ModernLoadCover` 헬퍼가 이를 한 줄로 가린다 — 폼 배경색 커버
패널을 덮어 두었다가 폼 표시 후 WPF 초기 레이아웃이 끝나는 시점(디스패처
유휴)에 걷어서, 완성된 화면만 한 번에 보이게 한다. **폼 스스로 커버를 덮는
방식이라 메인 프레임(폼을 여는 쪽)을 고칠 수 없어도 적용된다** — 별도
창(`Show`/`ShowDialog`)이든 패널 임베드(`TopLevel=false`)든 여는 방식과
무관하다.

폼 생성자에서 `InitializeComponent()` 직후 한 줄:

```csharp
public PendingRequestForm()
{
    this.InitializeComponent();

    // 로딩 커버 한 줄 — 오픈 시 깜빡임을 폼 스스로 가린다.
    Modern.Lab.WinForms.Controls.Hosting.ModernLoadCover.Attach(this);
}
```

별도 창으로 여는 폼은 창 표시 전에 Load와 바인딩이 끝나 깜빡임이 거의 없다 —
그 경우 커버는 무해하게 곧바로 걷히므로, 여는 방식을 모르면 그냥 넣어 둔다.
참조 구현: 데모 갤러리의 모든 샘플 폼 생성자.
