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

## 6. WinForms 단독으로 "WPF 감성"이 어려운 이유 (하이브리드를 택한 근거)

- WinForms에는 스타일/토큰 시스템이 없다 — 토큰 하나 고치면 전 컨트롤이 따라오는
  구조를 만들려면 미니 UI 프레임워크를 자작해야 한다.
- 안티앨리어싱된 둥근 모서리·그림자·반투명은 GDI+에서 편법이 필요하고, WPF에선 기본.
- 호버/포커스 전환 애니메이션 파이프라인이 없다 (타이머로 프레임 직접 구동).
- GDI+는 래스터 기반이라 고DPI에서 커스텀 드로잉이 흐려진다. WPF는 벡터라 자동 선명.
- 그리드 셀 안 배지/아이콘/진행바 같은 템플릿 구성이 WinForms에선 전부 셀 페인팅 코드.

→ 결론: 새 UI는 순수 WPF로 만들고 ElementHost 래퍼로 기존 WinForms에 얹는
점진적 현대화가 유지보수 비용에서 압도적으로 유리하다. (서드파티 WinForms 스위트는
회사 정책상 금지.)
