# csharp-modern-controls (Modern.Lab)

엔터프라이즈 Windows 데스크톱용 **모던 WPF 컨트롤 라이브러리**입니다.
새 UI는 순수 WPF로 작성하고, 기존 WinForms 프로그램에는 `ElementHost` 래퍼로 올려
점진적으로 화면을 현대화하는 것을 목표로 합니다.

- 런타임: .NET Framework 4.8 · 언어: C# 7.3 · IDE: Visual Studio 2026
- 서드파티 UI 라이브러리 없음 — 순수 WPF만 사용
- 디자인 언어: Windows Fluent (액센트 `#0078D4`, Segoe UI + 맑은 고딕 폴백, 컨트롤 radius 4 / 카드 radius 8)

## 솔루션 구조

| 프로젝트 | 출력 | 역할 |
|---|---|---|
| `Modern.Lab.Commons` | 클래스 라이브러리 (`Modern.Lab.Commons.dll`) | 재사용 가능한 순수 WPF 컨트롤(`Controls/Wpf`), 디자인 토큰 테마(`Themes/Tokens.xaml`), WinForms 래퍼(`WinForms/…`). 다른 솔루션에 통합할 때는 이 프로젝트만 추가하면 됩니다. |
| `Modern.Lab.Samples` | 실행 파일 (`Modern.Lab.Samples.exe`) | 실행 가능한 예제 갤러리. 컨트롤이 추가될 때마다 샘플 화면이 등록됩니다. |

- 루트 네임스페이스: `Modern.Lab` (WPF 컨트롤 `Modern.Lab.Controls.Wpf.*`, 래퍼 `Modern.Lab.WinForms.Controls.*`)
- XAML pack URI: `/Modern.Lab.Commons;component/Themes/Tokens.xaml`

## 빌드 & 실행

```
msbuild Modern.Lab.sln /t:Build /p:Configuration=Debug
Modern.Lab.Samples\bin\Debug\Modern.Lab.Samples.exe
```

Visual Studio에서는 **Modern.Lab.Samples**를 시작 프로젝트로 설정하고 F5로 실행합니다.

## 설계 원칙

1. **디자인 토큰이 단일 진실 공급원** — 모든 색·크기·간격·radius는 `Themes/Tokens.xaml`의
   토큰으로만 사용합니다. 컨트롤에 hex/px 하드코딩 금지.
2. **동작은 WPF 컨트롤 한 곳에** — WinForms 래퍼는 로직 없이 속성/이벤트만 재노출합니다.
3. **디자이너 안전 + 결정적 디자인 타임 동작** — 래퍼는 `WpfElementHostBase`를 상속하며,
   디자인 타임에는 WPF를 호스팅하지 않고 스냅샷 미리보기(실패 시 자리표시자)를 그립니다.

전신 프로젝트(wpfControls)에서 배운 교훈과 상세 설계 근거는
[`docs/design-notes.md`](docs/design-notes.md)를 참고하세요.
Claude Code 작업 규칙은 [`CLAUDE.md`](CLAUDE.md)에 있습니다.

## 문서/코드 언어 규칙

- 소스 코드 주석: **영문 전용**
- README 및 `docs/` 문서: **한글**
- 사용자 노출 문자열(버튼 텍스트 등): 한글 허용
