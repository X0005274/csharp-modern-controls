# 설계 노트 — WPF-in-WinForms 하이브리드에서 배운 것들

이 문서는 이전 프로젝트(wpfControls)에서 겪은 문제와 그 분석을 기록한 것입니다.
새 컨트롤/래퍼를 설계하기 전에 반드시 읽어야 합니다.

## 1. ElementHost 래퍼의 디자인 타임 동작 원리

WinForms 폼 디자이너는 GDI 기반 디자인 표면이라 WPF 비주얼 트리를 안정적으로 렌더링하지
못합니다. 그래서 래퍼(ElementHost 상속)는 보통 다음 패턴을 씁니다:

- `[Designer(ControlDesigner)]` — 디자이너가 래퍼를 "불투명 컨트롤"로 취급하게 하여
  `ElementHost.Child`(WPF 트리)를 재직렬화하다 `.Designer.cs`를 오염시키는 사고를 차단.
- `Child` 속성을 `[Browsable(false)]` + `[DesignerSerializationVisibility(Hidden)]`으로 숨김.
- 디자인 타임에는 `Child`를 연결하지 않음 → 디자이너에서는 빈 박스로 보이고,
  실행해야 실제 UI가 보이는 것이 **의도된 동작**.

### 결론
"디자이너에서 안 보이는 것"은 버그가 아니라 이 아키텍처의 트레이드오프다.
단, 아래 2번처럼 **일관성 없이** 구현하면 진짜 버그가 된다.

## 2. [버그 교훈] 디자인 타임 가드는 생성자와 OnHandleCreated 양쪽에 걸어야 한다

이전 프로젝트의 `WpfElementHostBase<TWpf>`는 이렇게 되어 있었다:

```csharp
protected WpfElementHostBase()
{
    this.Wpf = new TWpf();
    if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
    {
        this.Child = this.Wpf;          // 디자인 타임엔 연결하지 않음 (의도)
    }
}

protected override void OnHandleCreated(System.EventArgs e)
{
    if (this.Child == null && this.Wpf != null)
    {
        this.Child = this.Wpf;          // ← 디자인 타임 검사가 없다!
    }
    base.OnHandleCreated(e);
}
```

**문제**: 디자인 표면의 컨트롤도 실제 Win32 핸들을 가지므로 `OnHandleCreated`는
디자이너에서도 실행된다. 결과적으로 핸들 생성 타이밍·폼 리로드·컨트롤 재생성 순서에
따라 WPF가 디자이너에 붙기도 하고 안 붙기도 하는 **비결정적(복불복) 동작**이 됐다.
"디자이너에서 보였다 안 보였다 한다"는 증상의 원인.

**새 프로젝트 규칙**: 디자인 타임 정책(보인다/안 보인다)을 하나로 정하고, 생성자와
`OnHandleCreated` **둘 다** 같은 가드를 적용한다. `OnHandleCreated` 시점에는
`this.DesignMode`(Site 기반)가 신뢰 가능하므로 그것을 쓰고, 생성자에서는
`LicenseManager.UsageMode`를 쓴다 (생성자에서는 `DesignMode`가 아직 false).

## 3. [버그 교훈] Control.Text를 `new`로 숨기지 말 것 — "버튼"으로만 보이던 문제

이전 프로젝트의 래퍼는 텍스트를 이렇게 노출했다:

```csharp
[DefaultValue("버튼")]
public new string Text                 // ← Control.Text를 hide
{
    get { return this.Wpf.Text; }
    set { this.Wpf.Text = value; }
}
```

WPF 쪽 DP 기본값이 `"버튼"`이라, 설정한 텍스트가 `Wpf.Text`까지 도달하지 못하면
기본값 "버튼"만 표시된다. `new` 숨김이 이를 유발하는 두 경로:

1. **`Localizable = true` 폼** (한국어 엔터프라이즈 앱에서 흔함): 디자이너가 속성을
   코드 대신 `.resx` + `resources.ApplyResources(...)`로 직렬화하는데,
   `ComponentResourceManager`는 리플렉션으로 `Text`를 찾는다. 파생(new)/베이스 양쪽에
   `Text`가 있으면 베이스 `Control.Text`(ElementHost엔 무의미)에 쓰거나
   `AmbiguousMatchException`이 난다 → WPF 텍스트는 기본값 그대로.
2. 호스트 앱의 공용 유틸(다국어 적용기 등)이 `Control`로 캐스팅해 `ctrl.Text = …`로
   설정하는 경우: `new` 속성은 캐스팅되면 무시되고 `base.Text`만 바뀐다.

**진단법**: 문제 폼의 `.Designer.cs`에서 해당 컨트롤이
`this.xxx.Text = "…";`(정상 경로)인지 `resources.ApplyResources(this.xxx, …)`(1번 경로)인지 확인.

**새 프로젝트 규칙**: `Control.Text`는 virtual이므로 `new` 대신 **`override`**로
재정의해 `Wpf.Text`로 라우팅하고 `[Localizable(true)]`를 붙인다. 다른 이름의 속성
(예: `Kind`, `IconGlyph`)은 숨김이 아니므로 그대로 CLR 속성으로 노출해도 된다.

```csharp
[Category("모던 컨트롤")]
[Localizable(true)]
[DefaultValue("버튼")]
public override string Text
{
    get { return this.Wpf.Text; }
    set { this.Wpf.Text = value; }
}
```

## 4. WPF 생성자는 디자이너 프로세스 안에서 실행된다

`Child` 연결을 건너뛰어도 `this.Wpf = new TWpf()`는 디자인 타임에도 실행된다.
즉 WPF 컨트롤의 생성자 + `InitializeComponent()`(XAML 파싱, `Tokens.xaml` 병합
딕셔너리 로드)가 **VS 디자이너 프로세스 안에서** 돈다. 여기서 예외가 나면 폼에
빨간 에러 박스가 뜨거나 폼 디자이너 로드 자체가 실패한다.

전형적 실패 시나리오:

1. **오래된 어셈블리 캐시**: VS는 로드한 컨트롤 DLL을 언로드하지 못한다.
   라이브러리 수정·리빌드 후에는 디자이너 문서를 닫고 VS를 재시작해야 확실하다.
2. **pack URI 해석 실패**: `/<어셈블리>;component/Themes/Tokens.xaml`은 디자이너가
   로드한 어셈블리 기준으로 해석된다. 빌드 전 상태거나 캐시가 꼬이면 리소스를 못 찾는다.
   폼 디자이너를 열기 전에 솔루션이 빌드되어 있어야 한다.
3. **런타임 전제 코드**: WPF 생성자/`Loaded`에서 `Application.Current` 접근, 파일/환경
   접근 등은 디자이너 프로세스에서 예외를 일으킨다.
4. **obj 캐시 꼬임(MC1000)**: `obj`/`bin` 삭제 후 리빌드.

**새 프로젝트 규칙**: 디자인 타임에는 `new TWpf()`를 try/catch로 감싸고, 실패 시
`Wpf = null` + 자리표시자 페인팅으로 폴백한다. 래퍼 속성 접근을 null-safe하게 만들어
"컨트롤 하나가 깨져도 폼 디자이너는 산다"를 보장한다.

## 5. 디자인 타임 미리보기 전략 비교

| 전략 | 실제 모습 | 디자이너 안전성 | 비고 |
|---|---|---|---|
| ① 디자인 타임에도 `Child` 연결 | O (라이브) | **나쁨** — WPF 예외가 폼 디자이너를 죽이고, 직렬화 오염·마우스 캡처 문제 | 비추천 |
| ② `RenderTargetBitmap` 스냅샷을 `OnPaint`로 그림 | O (정적) | 좋음 — 디자이너 입장에선 그림일 뿐. 실패 시 ③으로 폴백 | **권장**. Resize/속성 변경 시 `Invalidate()` 필요 |
| ③ 자리표시자(타입명 + 테두리) | X | 최고 | 최소 비용 |
| ④ 미리보기는 XAML 디자이너 + 샘플 갤러리 앱에서 | O (별도) | 최고 | 폼 디자이너에선 배치만. 이전 프로젝트의 방식 |

②의 골자 (베이스 클래스에 한 번만 구현하면 모든 래퍼가 상속):

```csharp
protected override void OnPaint(PaintEventArgs e)
{
    base.OnPaint(e);
    if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
    {
        this.PaintDesignTimePreview(e.Graphics);   // Wpf를 Measure/Arrange 후
    }                                              // RenderTargetBitmap → GDI+로 출력,
}                                                  // 예외 시 자리표시자 폴백
```

### 5-1. 스냅숏 렌더 전에 바인딩/DataTrigger 큐를 먼저 소화하고 재배치해야 한다 (2026-07-07)

**증상**: 속성 그리드에서 값을 바꿔도(예: `ModernLabel.Kind`를 Body→Title) 디자이너
미리보기가 그 자리에서 바뀌지 않는다. 저장 후 폼을 닫았다 다시 열면 그제서야 반영된다.

**진짜 원인 — 바인딩 반영과 레이아웃의 순서**:

모던 컨트롤은 값에 따른 시각 변화를 XAML의 **ElementName 바인딩 + DataTrigger**로
처리한다. 예) `ModernLabelControl`의 `Kind` (당시 예시 — 라벨/배지는 이후 7장의
GDI+ 전환으로 WPF 버전이 제거됐지만, 이 교훈은 호스팅되는 모든 컨트롤에 유효하다):

```xml
<DataTrigger Binding="{Binding Kind, ElementName=RootControl}" Value="Title">
    <Setter Property="FontSize" Value="{StaticResource Font.Size.Title}" />
    ...
```

이런 ElementName 바인딩/DataTrigger는 소스 DP(`Wpf.Kind`)를 대입하는 즉시 반영되지
않고 **`DispatcherPriority.DataBind`로 비동기 반영**된다. 그런데 처음 구현한
`RenderPreviewBitmap`은 **Measure/Arrange/UpdateLayout → 렌더** 순서였다:

1. 측정/배치 — 이 시점엔 트리거가 아직 안 걸려 **Body 폰트로 레이아웃**
2. (뒤늦게) 디스패처가 DataBind를 소화하며 트리거 적용 → FontSize=Title, 레이아웃이
   다시 더러워짐 — 하지만 **재배치는 하지 않음**
3. `RenderTargetBitmap.Render` — 더럽거나 "변경 직전"(Body) 상태가 캡처됨

그 스냅숏이 캐시에 고정되고 이후 무효화가 없어 값 변경이 안 보였다. 재오픈 때만 맞게
보인 건, 폼 로드 중 여러 레이아웃/페인트/디스패처 사이클이 자연히 돌아 결국 최신
상태가 렌더됐기 때문이다. (WM_PAINT 재진입 문맥이 원인이라는 초기 가설은 틀렸다 —
문맥과 무관하게 **렌더 직전에 바인딩을 flush하고 다시 배치**하면 해결된다.)

**규칙**: `RenderPreviewBitmap`은 반드시 아래 순서로 렌더한다.

1. `Measure/Arrange` — 시각 트리를 활성화한다(바인딩/트리거가 살아난다).
2. `Dispatcher.Invoke(DispatcherPriority.DataBind, …)` — 대기 중인 바인딩/DataTrigger를
   소화한다(예: Kind→FontSize/FontWeight, Required/TitleBar→Visibility).
3. `Measure/Arrange/UpdateLayout` — 트리거가 바꾼 값으로 **다시** 배치한다.
4. `RenderTargetBitmap.Render`.

부가로, 값 변경이 "즉시" 보이려면 스냅숏을 `InvalidateDesignTimePreview`(속성 setter·
`OnResize` 경로)에서 미리 만들어 캐시에 담아 둔다. 이 수정은 베이스 클래스
(`WpfElementHostBase`)에 있으므로 모든 래퍼가 함께 고쳐진다.

### 5-2. WPF 디스패처 BeginInvoke는 디자인 타임에 실행되지 않는다 (2026-07-07)

한때 재렌더를 `Wpf.Dispatcher.BeginInvoke(DispatcherPriority.Background, …)`로 미루는
방식을 시도했으나 **디자인 타임에서는 동작하지 않는다**. 파일 로그 계측으로 확인한 사실:

- 디자인 표면에는 WPF 콘텐츠가 실제로 호스팅되지 않으므로(HwndSource 없음) WPF
  디스패처의 Background 큐를 펌프하는 주체가 없다.
- 큐에 넣은 콜백은 폼 로드·속성 변경 중에는 전혀 발화하지 않다가, **폼을 닫을 때
  (컨트롤 dispose 시점) 한꺼번에 뒤늦게** 발화했다.

반면 `RenderPreviewBitmap`은 어느 문맥에서 호출해도(속성 커밋 문맥 포함) 5-1의 순서
(배치 → `Dispatcher.Invoke(DataBind)` flush → 재배치 → 렌더)만 지키면 항상 바뀐 값이
올바르게 담긴 스냅숏을 만든다는 것도 같은 계측으로 확인됐다.

**규칙**: 디자인 타임 스냅숏 재렌더는 절대 WPF 디스패처에 미루지 말고,
`InvalidateDesignTimePreview` 안에서 **동기로** 수행해 캐시에 담는다.

### 5-3. VS 디자이너 화면은 컨트롤의 WM_PAINT로 갱신되지 않는다 — 크기 1px 흔들기 (2026-07-07)

5-1·5-2를 다 고쳐 "올바른 스냅숏을 그 자리에서 동기로 캐시"해도, **속성 그리드에서
값을 바꾸면 디자이너 화면이 그 자리에서 안 바뀌는** 최종 증상이 남았다. 파일 로그
계측으로 확인한 사실:

- 값 커밋 직후 새(Title) 스냅숏이 정상 렌더되고, 컨트롤 자신의 `Invalidate+Update`로
  한 번, 루트(디자이너 프레임) 영역 무효화 경유의 실제 WM_PAINT로 또 한 번 — **새
  모습이 두 번이나 실제로 그려졌는데도 화면은 옛 모습 그대로**였다.
- 부모/루트 무효화 후에도 화면 반영은 없었고, **실제 크기 변경(리사이즈)** 때만
  반영됐다. 크기 변경은 WM_WINDOWPOSCHANGED를 통해 디자이너(ControlDesigner/
  BehaviorService)의 표면 재동기화를 강제하기 때문이다.
- 즉 이 환경(VS 18)의 디자이너 화면 갱신을 코드에서 확실히 일으키는 경로는 "실제
  크기 변경"뿐이다.

**규칙**: `InvalidateDesignTimePreview`는 동기 재렌더 후 다음 순서로 화면을 갱신한다.

1. 자기 자신 `Invalidate()` + `Update()` (자기 창의 캐시 blit).
2. 컨트롤 트리 최상위 루트에서 이 컨트롤 영역을 `Invalidate(rect, true)` + `Update()`.
3. **크기 1px 흔들기**: 폭을 +1 했다 즉시 되돌린다(폭이 Dock 등으로 고정이면 높이로
   시도). 이것이 실제로 화면을 갱신하는 결정적 단계다. 최종 크기는 원래 값이므로
   `.Designer.cs` 직렬화에는 변화가 없다. 흔들기가 유발하는 `OnResize` 재진입은
   `isNudgingBounds` 가드로 차단한다.

이 처리는 전부 베이스 클래스(`WpfElementHostBase`)에 있으므로 모든 래퍼가 함께
동작한다. 검증: `ModernLabel.Kind`를 Body→Title로 바꾸면 리사이즈·포커스 이동 없이
그 자리에서 즉시 반영됨을 확인(2026-07-07).

## 6-1. 컨트롤 설계 계약 (2026-07-04 합의)

모든 모던 컨트롤은 아래 계약을 지킨다. 목표: **기존 WinForms 폼에서 컨트롤 교체가
".Designer.cs의 타입 선언 한 줄 바꾸기"로 수렴**하고, 서버 request/reply 코드는
한 글자도 바뀌지 않는 것.

### 룰 1 — Drop-in 호환
래퍼는 교체 대상 WinForms 컨트롤의 API를 같은 이름·같은 의미로 제공한다.
- 버튼: `Text`, `Click`, `Enabled`
- 콤보박스: `DataSource`, `DisplayMember`, `ValueMember`, `SelectedValue`,
  `SelectedIndexChanged`, `Items`
- 숨김(`new`) 금지 — 베이스에 있는 멤버는 `override`(3장 참고).

### 룰 2 — 컨트롤은 데이터 출처를 모른다
`Modern.Lab.Commons`에는 통신/DB 코드를 절대 넣지 않는다. 컨트롤은 `DataSource`로
받은 것을 표시할 뿐이다. 대신 `DataSource`는 관대하게 받는다: `DataTable`,
`DataView`, `IList`, `IEnumerable` 모두 허용하고 내부에서 표준 아이템 모델로 변환.
(집=더미 데이터, 회사=실제 서버 응답을 그대로 꽂는 구조의 전제.)

### 룰 3 — 순서·상태에 관대하게
- `SelectedValue`를 `DataSource`보다 먼저 설정해도 동작한다(값을 보류했다가
  데이터 도착 시 적용). 표준 ComboBox의 조용한 무시 동작을 재현하지 않는다.
- `DataSource` 재할당(재조회) 시 선택 상태를 깨끗하게 초기화하고 이벤트가
  중복 발생하지 않는다.
- null/빈 데이터는 예외 없이 빈 목록으로 표시한다.
- 백그라운드 조회 후 UI 스레드 할당(`Invoke`) 패턴이 그대로 동작한다.

### 룰 4 — 데이터 연결은 수동형
폼이 조회해서 `DataSource`에 할당한다. 컨트롤이 데이터를 요청하는 능동형
(`DataRequested` 이벤트, `IDataProvider`)은 만들지 않는다 — 조회 시점이 컨트롤
내부로 숨어 디버깅이 어려워지고, 기존 코드를 컨트롤 규격에 맞춰야 하기 때문.

### 룰 5 — 레이아웃은 WinForms 컨테이너로
영역 배치(상/중/하단, 분할)는 표준 WinForms 컨테이너(`Panel`,
`TableLayoutPanel`, `SplitContainer`)로 한다. 모던 컨트롤은 **말단 위젯만**
담당한다. ElementHost는 WinForms 자식 컨트롤을 담을 수 없으므로 "모던 레이아웃
컨테이너"는 만들지 않는다. 이것이 폼 디자이너에서 가장 안정적인 구성이다.

### 룰 6 — 컨트롤마다 교체 체크리스트 문서
컨트롤 하나가 완성되면 `docs/migration/<컨트롤>.md`에 기록한다: 대응 WinForms
컨트롤, 호환 제공 멤버, 미지원 멤버와 대체 방법, `.Designer.cs` 교체 예시.

### 배포 룰
- 안정 시점마다 `v0.x` 태그. 회사에서는 태그를 받아 `Modern.Lab.Commons`
  프로젝트만 기존 솔루션에 추가한다(Samples는 개발 참고용).
- 외부 의존성 0 유지 (순수 .NET Framework 4.8 참조만).

## 6-2. WinForms 단독으로 "WPF 감성"이 어려운 이유 (하이브리드를 택한 근거)

- WinForms에는 스타일/토큰 시스템이 없다 — 토큰 하나 고치면 전 컨트롤이 따라오는
  구조를 만들려면 미니 UI 프레임워크를 자작해야 한다.
- 안티앨리어싱된 둥근 모서리·그림자·반투명은 GDI+에서 편법이 필요하고, WPF에선 기본.
- 호버/포커스 전환 애니메이션 파이프라인이 없다 (타이머로 프레임 직접 구동).
- GDI+는 래스터 기반이라 고DPI에서 커스텀 드로잉이 흐려진다. WPF는 벡터라 자동 선명.
- 그리드 셀 안 배지/아이콘/진행바 같은 템플릿 구성이 WinForms에선 전부 셀 페인팅 코드.

→ 결론: 새 UI는 순수 WPF로 만들고 ElementHost 래퍼로 기존 WinForms에 얹는
점진적 현대화가 유지보수 비용에서 압도적으로 유리하다. (서드파티 WinForms 스위트는
회사 정책상 금지.)

## 7. [성능 교훈] ElementHost는 "개수"가 성능이다 — 표시 전용 컨트롤은 GDI+로 (2026-07-09)

회사 환경에서 Item History 폼의 리사이즈가 굉장히 버겁다는 보고로 실측한 결과:

- **ElementHost 1개당 리사이즈 1스텝에 ~20ms의 고정 비용**이 든다. 내용 복잡도와
  거의 무관하다(16컬럼 그리드 22ms ≈ 6컬럼 그리드 19ms ≈ 트리 100노드 19ms;
  빈 WinForms 폼은 1ms). HwndSource 리사이즈 시 WPF가 동기로 재배치·재렌더하는
  비용이 지배적이기 때문이다.
- 라벨/배지처럼 **한 폼에 수십 개씩 놓이는 표시 전용 컨트롤을 전부 ElementHost로
  만들면 이 고정 비용이 개수만큼 누적**된다. Item History 폼은 자식 HWND 87개,
  리사이즈 1스텝 평균 556ms였다(집 PC 기준 — 회사 저사양/RDP에선 더 나쁨).

**규칙**: 상호작용 없는 표시 전용 + 다수 배치 컨트롤(라벨, 배지)은 WPF를 호스팅하지
말고 ModernGroupBox/ModernCardPanel처럼 **토큰을 미러링해 GDI+로 직접 그린다**.
ModernLabel·ModernStatusBadge를 GDI+로 전환한 뒤 같은 폼이 평균 ~300ms로 개선됐다
(남은 비용은 그리드·트리 등 꼭 필요한 WPF 섬들). 입력/그리드/트리처럼 WPF의 템플릿·
가상화가 실제로 필요한 컨트롤만 ElementHost를 유지한다.

전환 시 주의:

- 기존 `.Designer.cs`가 직렬화해 둔 `xxx.Child = null;` 라인이 컴파일되도록
  무동작 `Child` 속성(object, Browsable false)을 남긴다 — 타입 선언만 바꾸면 되는
  drop-in 계약(6-1 룰 1) 유지.
- GDI+ 텍스트는 `TextRenderer` + `TextFormatFlags.NoPadding`으로 그린다.
  NoPadding이 없으면 GDI 기본 좌우 여백 때문에 같은 크기에서 WPF에는 들어가던
  텍스트가 말줄임된다(직원관리 "이름" 라벨 잘림으로 발견).

부수 발견 2건:

- **WPF는 MergedDictionaries의 Source 사전을 캐시하지 않는다** — 컨트롤 인스턴스마다
  Tokens.xaml(BAML)을 재파싱해 브러시를 새로 만든다. 모든 컨트롤 XAML은 일반
  ResourceDictionary 대신 `common:SharedResourceDictionary`(프로세스 캐시)를 병합한다.
- **숨겨진 ElementHost 안의 무한 Storyboard는 계속 돈다** — WinForms `Visible=false`는
  WPF `Visibility`를 바꾸지 않으므로 Loaded+Forever 애니메이션(BusyOverlay 스피너)이
  숨긴 뒤에도 디스패처 렌더 틱을 영원히 소모한다. 표시/숨김을 아는 래퍼가
  `IsSpinning` 같은 DP로 명시적으로 켜고 꺼야 한다.

### 7-1. "전체적으로 무겁다"는 느낌의 정밀 감사 (2026-07-09, Debug 빌드·집 PC 기준)

느낌이 아니라 **사실**이며, 무거움의 실체는 "WPF 섬 개수에 비례하는 고정 비용"이다.
항목별 실측:

| 항목 | 실측값 | 판정 |
|---|---|---|
| 컨트롤 생성+핸들 (개당) | WPF 호스팅 6~10ms vs 순정 0.5~3.5ms | **WPF 섬당 ~3~15배** |
| GDI+ 전환한 라벨/배지 (개당) | 0.45~0.5ms | 순정 Label과 동일 |
| WPF 스택 최초 기동 | 첫 컨트롤 1개에 ~300ms | 프로세스당 1회 |
| 폼 오픈 (Item History) | ctor 70 + show/load 220 + dispose ~20 = **~310ms** | 서버 호출 없음 — 순수 UI 비용 |
| 폼 오픈 (직원관리) | **~520ms** (WPF 섬 20여 개) | 데이터 전부 인메모리 — 순수 UI 비용 |
| 콜드 스타트(창 응답까지) | ~720ms | 양호 |
| 유휴 CPU (데이터 로드 상태 10초) | **0.00%** | 애니메이션/타이머 누수 없음 |
| 그리드 휠 스크롤 | 체감 지연 없음 | 문제 없음 |
| 메모리 | WorkingSet ~190~215MB, 폼 전환 반복에도 증가 없음 | WPF 런타임 포함(순정 대비 +150MB급), 누수 없음 |

결론:

- 스크롤·유휴·메모리 누수는 깨끗하다. 무거움은 **폼 오픈(0.3~0.5s)과 리사이즈**에
  집중되어 있고, 두 경우 모두 원인은 동일하다 — WPF 섬 1개당 생성 ~10ms,
  리사이즈 스텝당 ~20ms의 고정 비용.
- 라벨/배지 GDI+ 전환으로 개수가 가장 많던 덩어리는 이미 제거했다. 남은 섬(입력,
  그리드, 트리, 콤보)은 WPF의 템플릿·팝업·가상화가 실제로 필요한 컨트롤들이다.
- 회사 저사양/RDP에서는 이 고정 비용이 몇 배로 늘어날 수 있다(소프트웨어 렌더링).
- 추가로 줄여야 하면 다음 단계 후보: ① ModernButton처럼 상태가 단순한 인터랙션
  컨트롤의 GDI+ 전환, ② 자주 여닫는 화면은 폼을 dispose하지 말고 숨겨 재사용
  (오픈 비용이 최초 1회로 줄어든다), ③ Release 빌드 배포.
