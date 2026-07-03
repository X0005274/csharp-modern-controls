# ModernSummaryList 도입 가이드

- **대체 대상**: 없음(신규 개념) — 부서별/직급별 인원 수 같은 구분·건수 목록을 칩(pill) 형태로 표시
- **네임스페이스**: `Modern.Lab.WinForms.Controls.Display`

## 제공 멤버 (ComboBox 데이터 계약과 동일한 이름 체계)

| 멤버 | 설명 |
|---|---|
| `DataSource` | `DataTable`/`DataView`/`IList`/`IEnumerable`. 구분 컬럼 + 건수 컬럼을 가진 행 목록 (서버 GROUP BY 결과 등). null이면 칩 목록이 비워짐 |
| `DisplayMember` | 칩 라벨로 사용할 컬럼/속성 이름 |
| `ValueMember` | 칩 건수로 사용할 컬럼/속성 이름 |
| `Title` | 칩 목록 왼쪽 제목 (비우면 숨김). `[Localizable(true)]` |
| `Flat` | 카드 테두리/배경 제거 — `ModernCardPanel` 위에 평면 배치할 때 사용 |
| `Enabled` | 전파됨 |

## 계약 보장 동작 (docs/design-notes.md §6-1)

- `DisplayMember`/`ValueMember`와 `DataSource`의 설정 순서는 무관 — 어느 쪽이 나중에 와도 칩이 다시 구성됨
- null/빈 데이터, 존재하지 않는 컬럼 이름은 빈 표시로 처리, 예외 없음
- 백그라운드 조회 후 UI 스레드 `Invoke` 할당 패턴 지원

## 사용 예시

```csharp
private Modern.Lab.WinForms.Controls.Display.ModernSummaryList listDeptCount;

this.listDeptCount = new Modern.Lab.WinForms.Controls.Display.ModernSummaryList();
this.listDeptCount.Title = "부서별 인원";
this.listDeptCount.DisplayMember = "DEPT_NAME";
this.listDeptCount.ValueMember = "CNT";

// 조회 완료 후 (서버 GROUP BY 응답 또는 로컬 집계 DataTable)
this.listDeptCount.DataSource = deptCountTable;
```

## 미지원 / 주의

| 항목 | 내용 |
|---|---|
| 칩 클릭 이벤트 | 표시 전용 — 필터링 연동이 필요하면 별도 요청 |
| 데이터 내부 변경 감지 | `DataTable` 행을 직접 수정해도 자동 갱신되지 않음 — 조회 후 `DataSource`를 다시 할당 |

권장 크기: 300×76 이상 (칩이 많으면 줄바꿈되므로 높이 여유 필요).
