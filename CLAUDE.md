# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

Modern WPF control library for enterprise Windows desktop apps. Reusable UI is **pure WPF**, hosted in existing WinForms programs through `ElementHost` wrappers. This repository is a fresh start; hard-won design lessons from the predecessor project are recorded in `docs/design-notes.md` — **read it before designing any WinForms wrapper or design-time behavior.**

## Environment

- IDE: Visual Studio 2026 (VS 18) · Runtime: .NET Framework 4.8 · Language: C# (7.3)
- Use only syntax supported by .NET Framework 4.8 / C# 7.3.
- MSBuild on this machine:
  `"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" Modern.Lab.sln /t:Build /p:Configuration=Debug /v:minimal /clp:ErrorsOnly`
- Run the demo host: `Modern.Lab.Samples/bin/Debug/Modern.Lab.Samples.exe`

## Solution structure (Modern.Lab.sln, 2 projects)

| Project | Output | Role |
|---|---|---|
| `Modern.Lab.Commons` (`Modern.Lab.Commons/…`, assembly `Modern.Lab.Commons`) | WPF + WinForms class library | Reusable **pure-WPF** UserControls + design-token theme (`Controls/Wpf`, `Themes/Tokens.xaml`) and the WinForms **wrappers** (`WinForms/…`). Integrators add only this project. |
| `Modern.Lab.Samples` (`Modern.Lab.Samples/…`, assembly `Modern.Lab.Samples`) | WinExe | Runnable example gallery (`SampleShellForm`). References `Modern.Lab.Commons`. |

- Root namespace is `Modern.Lab` in both projects. Namespaces follow folders: `Modern.Lab.Controls.Wpf.*` (WPF controls), `Modern.Lab.WinForms.Controls.*` (wrappers), `Modern.Lab.Samples` (gallery).
- XAML pack URIs use the commons assembly name: `/Modern.Lab.Commons;component/Themes/Tokens.xaml`.
- Both `.csproj` files are classic (non-SDK) with explicit item lists — every new `.cs` needs a `Compile` entry, every new `.xaml` a `Page` (+ code-behind `Compile` with `DependentUpon`).
- A running demo/sample exe locks build output (MSB3027) — kill it before rebuilding.
- After renaming/moving projects, stale `obj/` markup-compile cache can cause MC1000 "could not find … .xaml"; delete `obj`/`bin` and rebuild.
- Korean comments are common; files are UTF-8 without BOM. Do not edit with tools that re-encode (e.g. PowerShell `Get-Content`/`Set-Content` without explicit `UTF8Encoding($false)`).

## Architecture principles (carried over from the predecessor project)

### Design tokens are the single source of truth
Keep a `Themes/Tokens.xaml` `ResourceDictionary` holding every color, font size, spacing, radius, and elevation value. **Controls consume tokens and never hardcode hex / px / font-weight.** Design language: Windows Fluent — accent `#0078D4`, Segoe UI, control radius 4 / card radius 8, fixed type ramp, structural-elements-SemiBold / body-Regular. When a value recurs, add a token rather than a literal.

### Two parallel control sets, one source of behavior
Each control has a WPF control (`Controls/Wpf/...`) and a WinForms wrapper (same name minus the `Control` suffix). The wrapper holds no real logic — behavior and styling live in the WPF control so every host inherits them. The wrapper only re-exposes inner `DependencyProperty`s as CLR properties and surfaces events (via `DependencyPropertyDescriptor.AddValueChanged`).

### Wrapper / ElementHost pattern must be designer-safe AND deterministic
Wrappers derive from a common `WpfElementHostBase<TWpf>` marked `[Designer(ControlDesigner)]` so the VS form designer treats the wrapper as an opaque control and never serializes the WPF `Child`. Critical rules learned from the predecessor (full analysis in `docs/design-notes.md`):

- Design-time behavior must be **deterministic**: the design-time guard must be enforced consistently in the constructor **and** `OnHandleCreated` (handles are created on the design surface too — guarding only the constructor gives flip-flopping visibility).
- Never hide `Control.Text` with `new`. **`override` it** and add `[Localizable(true)]` — `new` breaks `ComponentResourceManager.ApplyResources` on `Localizable = true` forms (text silently stays at the WPF default) and breaks any host code that sets `Text` through a `Control` reference.
- Guard design-time WPF construction with try/catch + placeholder fallback so one broken control cannot kill the host form's designer.

## Absolute rules

- Never use `var`. Use explicit types everywhere.
- No third-party UI libraries (MahApps, ModernWpf, MaterialDesign, DevExpress, Telerik, Syncfusion, etc.). Pure WPF only, unless the user explicitly requests otherwise.
- Output complete, compilable code only — no pseudocode, placeholders, TODOs, or omitted sections.
- **Pretendard font is not allowed** (corporate policy). Use Segoe UI with Malgun Gothic fallback for Korean via the `Font.Family` token.

## C# style

- Explicit access modifier on every type and member.
- PascalCase for types/methods/properties/public members; camelCase for locals/parameters/private fields.
- Braces on every control block — no brace-less one-liners.
- One public type per file; file name matches the type name (and `x:Class` for controls).
- Split long initialization into named private methods.

## WPF component rules

- Base every reusable component on `UserControl`.
- Expose every bindable value as a `DependencyProperty` with a CLR wrapper. Register two-way values with `FrameworkPropertyMetadataOptions.BindsTwoWayByDefault`.
- Expose secrets (e.g. password text) through a plain CLR property, never a `DependencyProperty`.
- `ItemsSource` DPs are typed `IEnumerable`; `SelectedItem`/`SelectedValue` are typed `object`. Bound collections are `ObservableCollection<T>`; UI item models implement `INotifyPropertyChanged`.
- Keep styling in XAML and visual trees simple; conservative enterprise look (no flashy gradients/shadows/animation).
- Every component must render and function when hosted in WinForms via `ElementHost`.

Canonical DependencyProperty:

```csharp
public static readonly DependencyProperty TitleProperty =
    DependencyProperty.Register(
        "Title", typeof(string), typeof(MyControl), new PropertyMetadata(string.Empty));

public string Title
{
    get { return (string)this.GetValue(TitleProperty); }
    set { this.SetValue(TitleProperty, value); }
}
```

## WinForms integration

Host project must reference `WindowsFormsIntegration` plus `PresentationCore`, `PresentationFramework`, `WindowsBase`, `System.Xaml`, and this control library. Run WPF on an STA thread (`[STAThread]`). Give each `ElementHost` an explicit size (`Dock`/`Height`/`Anchor`).

## XAML & build correctness — verify before finishing

- `x:Class`, code-behind namespace + class name, file name, and folder all match exactly; code-behind class is `partial`.
- Each `.xaml` uses Build Action `Page` with `<Generator>MSBuild:Compile</Generator>`; the `.csproj` has both the `Page` entry and the matching `Compile` + `DependentUpon` entry. New `.cs` files need a `Compile` entry too when using classic, non-SDK `.csproj` with explicit item lists.
- Each `x:Name` is unique and does not collide with a DependencyProperty's CLR property name.
- No `var`; no third-party UI dependency.

## Response behavior

- Ask one short clarifying question only when requirements are genuinely ambiguous; otherwise proceed.
- Lead with code; keep prose brief and practical.
- When a build or integration failure is likely, name the exact failure point and the fix.
