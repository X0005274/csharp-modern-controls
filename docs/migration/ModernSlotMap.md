# ModernSlotMap — 캐리어 수납 구조 슬롯 맵

`Modern.Lab.WinForms.Controls.Display.ModernSlotMap`

캐리어(운반체)의 수납 구조를 **실물 단면처럼** 그리는 표시/선택 컨트롤이다.
기존 폼에 대응물이 없는 **신규 개념 컨트롤**이라 변환 작업에서는 도입하지
않고, 사람이 명시적으로 요청한 화면(Split/Merge/Scrap 등 수납 편집)에 쓴다.

- 구획(`SlotMapSection`) 단위로 셀 격자를 그린다 — `Columns = 1`이면 세로
  사다리(FOUP 슬롯 스택), `Columns = 5`면 5×5 격자(TRAY LCC) 식.
- 구획의 **`Kind`(`SlotMapSectionKind`)가 실물 표현**을 정한다 — 상태(채움/빈/
  미리보기/선택)를 색이 아닌 **형태 차(도형 유무·점선·링)**로 인코딩해 어느
  테마에서도 같은 방식으로 읽힌다:
  - `WaferEdge`(FOUP): 카세트를 옆에서 본 단면 — 좌우 레일 사이 얇은 웨이퍼
    바(아이템색 + ID 인쇄)가 층층이 쌓이고, 빈 슬롯은 레일 홈만 남는다.
  - `PinStub`(STUB): 핀 스텁 탑 뷰 — 원형 금속 원판 위 사각 칩(안에 유닛 ID
    끝 4자리 표기, 전체 ID는 캡션/툴팁). 빈 스텁은 가운데 핀 자국만 있는
    맨 원판(칩의 유무 = 채움/빈).
  - `LamellaPost`(LCC): 베이스에서 솟은 포스트(핑거)에 라멜라 칩이 붙는다 —
    삽입 위치 Top/Left/Right가 **라멜라가 붙는 위치 그 자체**로 표현된다.
    A~E 눈금은 베이스 아래 고정이라 가려지지 않는다.
  - `Generic`(기본): 번호 칩 + 유닛 토큰(단일) / 핑거 도트 미니 행(복합) —
    Kind를 지정하지 않은 기존 화면 호환용.
- 채워진 셀 클릭 = `CellClicked` 이벤트만 발생(선택 상태는 폼이 관리). 상세는
  호버 툴팁. 선택 표시는 `SetSelectedKeys`(스테이징 강조)·`SetClickKey`(클릭
  강조)로 폼이 직접 준다 — 둘이 겹친 셀은 유닛/번호 글씨 색이 바뀌어 결합을 나타낸다.
- 미리보기는 `SetPreview`로 "자리 키 → 유닛 ID" 맵을 받아 그 자리에 **같은
  실물 도형을 점선 윤곽 + "→ ID"**로 그린다(점선 = 어느 테마든 실선과 구분).
  `SetPreviewMarkers`를 함께 주면 미리보기 라멜라도 Top/Left/Right 위치에
  붙는다(커밋과 같은 배치 규칙 → 미리보기 = 이동 결과). 계획된 자리가
  부족하면 구획 집계가 빨간 "need n more"가 된다.
- **드래그앤드롭**: 원본 맵 `EnableDragOut = true`(선택된 셀을 끌면 선택
  전체가 함께 감), 대상 맵 `AcceptDrops = true` → 놓으면 `UnitsDropped`
  (끌려온 키들 + 놓은 자리의 앵커 키)가 발생한다. 검증/이동은 폼이 서버
  호출로 한다 — 앵커는 "이 자리부터 채워 달라"는 의도 전달일 뿐이다.

## 속성 / 메서드 / 이벤트

| 멤버 | 설명 |
|---|---|
| `SetSections(SlotMapSection[])` | 구획/셀 모델을 통째로 다시 그린다 — 재조회 반영 경로. 기존 선택·미리보기는 초기화 |
| `AllowSelection` | 셀 클릭 선택 허용 (기본 true) — 대상(보기 전용) 맵은 false |
| `EnableDragOut` | 채워진 셀 드래그 시작 허용 (기본 false) — 원본 맵에서 켠다 |
| `AcceptDrops` | 드롭 수용 (기본 false) — 대상 맵에서 켠다 |
| `SelectedKeys` | 선택된 셀 키 배열 — 키는 폼이 `SlotMapCell.Key`에 부여한 값 그대로 (예: `"SLOT|7"`) |
| `SetSelectedKeys(string[])` | 지정 키들만 스테이징 강조(강한 액센트, 이벤트 없음) |
| `SetClickKey(string)` | 클릭 강조 셀 지정(약한 색, null이면 없음) — 스테이징 셀과 겹치면 유닛/번호 글씨 색이 바뀌어 결합을 나타낸다 |
| `ClearSelection()` | 선택 전체 해제 |
| `SetPreview(Dictionary<string,string>)` | "자리 키(`SLOT\|N` / `STUB\|N` / `LCC\|N\|핑거`) → 들어올 유닛 ID" 미리보기 맵 (null = 해제). 그 자리가 비면 "→ ID"와 옅은 틴트로 표기해 확정 전 계획을 구분하고, LCC 프레임은 액센트 실선으로 강조한다 — 화면이 서버 배치 계획을 그대로 넘겨 미리보기와 실제 이동 결과가 일치한다 |
| `SetPreviewMarkers(Dictionary<string,string>)` | LCC 미리보기 자리 키 → 삽입 위치(`Top`/`Left`/`Right`) 맵. `SetPreview` 뒤에 설정하면 미리보기 라멜라(점선)가 그 위치에 붙는다 — 지정이 없으면 Top 위치로 그린다 (null = 해제) |
| `CellClicked` | 채움 셀 클릭 시 — `e.Key`(클릭된 셀 키). 선택 상태는 바꾸지 않으니 폼이 `SetSelectedKeys`/`SetClickKey`로 표시를 관리한다 |
| `SelectionChanged` | 선택 변경 시 (재구성/프로그램 해제) |
| `UnitsDropped` | 드롭 수신 시 — `e.Keys`(끌려온 셀 키들) + `e.AnchorKey`(놓은 자리 셀 키; 셀 밖이면 빈 문자열 = 앞에서부터) |
| `CellRightClick` | 맵에서 오른쪽 클릭 시 — `e.Key`(커서 아래 채움 셀 키; 셀 밖이면 빈 문자열). 폼이 이동 컨텍스트 메뉴를 띄우는 데 쓴다 |

## LCC(복합 셀) 표시 옵션

- SubCells가 있는 구획(LCC) 헤더에는 **"Lamella ID" 스위치**가 자동으로 붙는다.
  - **켬(기본)**: 포스트+라멜라 아래에 핑거당 유닛 ID 행("A · LM-…")이 붙는다.
  - **끔**: 포스트+라멜라만 보인다 — 셀이 압축되고 유닛 ID는 호버 툴팁으로
    확인한다.
- 복합 셀(LCC) 툴팁은 채워진 핑거를 정렬된 미니 표(Finger/Unit ID/Insert/Item)로
  보여 준다.

## 모델 (Modern.Lab.Controls.Wpf.Display)

| 타입 | 멤버 |
|---|---|
| `SlotMapSection` | `Title`, `Columns`(격자 열 수, 1 = 사다리), `Kind`(실물 표현 — `WaferEdge`/`PinStub`/`LamellaPost`/`Generic`(기본)), `Cells` |
| `SlotMapCell` | `Key`(선택 키), `Label`(자리 번호), `UnitId`(단일 수납), `Color`(채움색 — 소속 아이템별 색 구분용), `ToolTip`(재정의 — "유닛 — 아이템" 표기), `SubCells`(복합 수납 — null이면 단일) |
| `SlotMapSubCell` | `Name`(핑거 눈금 글자, "A"~"E"), `UnitId`, `Marker`(삽입 위치 — "Top"/"Left"/"Right", 라멜라가 포스트에 붙는 위치), `Color`(핑거별 채움색 — 셀 색보다 우선), `Detail`(툴팁 부가 — 소속 아이템 ID 등) |

유닛의 **소속 아이템 표현**: 아이템 ID별로 팔레트 색을 배정해 `Color`에 주면
같은 아이템의 유닛이 같은 색으로 묶여 보인다 — 화면에 아이템 범례(색 배지
"IT-W01 · 12")를 함께 두면 색↔아이템 대응이 읽힌다 (참조 구현의 범례 참고).

## 사용 예 (FOUP 25슬롯 + TRAY STUB/LCC)

```csharp
// FOUP: 슬롯 사다리 한 구획 — 웨이퍼 에지 뷰
SlotMapSection slots = new SlotMapSection();
slots.Title = "Slots";
slots.Columns = 1;
slots.Kind = SlotMapSectionKind.WaferEdge;
// ... 자리마다 SlotMapCell { Key = "SLOT|7", Label = "7", UnitId = "WF-01.07" }
this.mapSource.SetSections(new SlotMapSection[] { slots });

// TRAY: STUB(핀 스텁 탑 뷰) + LCC(포스트+라멜라) 두 구획
// stubs.Kind = SlotMapSectionKind.PinStub; lccs.Kind = SlotMapSectionKind.LamellaPost;
// LCC 셀: SubCells에 핑거 A~E — SlotMapSubCell { Name="A", UnitId="CHIP-…", Marker="Top" }

// 선택 → 대상 맵 미리보기 (자리 키 → 들어올 유닛 ID; 서버 배치 계획을
// 그대로 넘긴다 → 미리보기와 실제 이동 결과가 일치)
this.mapTarget.AllowSelection = false;
System.Collections.Generic.Dictionary<string, string> preview =
        new System.Collections.Generic.Dictionary<string, string>();
preview["STUB|3"] = "CHIP-…";     // STUB 3 자리로
preview["LCC|2|A"] = "CHIP-…";    // LCC 2번 핑거 A 자리로 (LCC는 빈 LCC로 통째)
this.mapTarget.SetPreview(preview);

// 처리 대상 수집 (클릭 셀은 CellClicked로 폼이 추적)
string[] keys = this.mapSource.SelectedKeys;   // "STUB|3", "LCC|12" …
```

참조 구현: 샘플 갤러리 **Carrier Editor** 화면(`CarrierEditForm`) — 조회
테이블(KIND/POS/FINGER/INS_POS/UNIT_ID)을 구획 모델로 변환(`BuildSections`)하고
Split/Merge/Scrap과 미리보기를 배선한 전체 흐름이 있다.
