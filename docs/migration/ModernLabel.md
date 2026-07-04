# ModernLabel 교체 가이드

- **대체 대상**: `System.Windows.Forms.Label`
- **네임스페이스**: `Modern.Lab.WinForms.Controls.Display`

## 호환 제공 멤버

| 멤버 | 비고 |
|---|---|
| `Text` | `Control.Text` override, `[Localizable(true)]` |
| `Enabled` | 전파됨 |

## 추가 멤버

| 멤버 | 설명 |
|---|---|
| `Kind` | **타이포그래피 역할** — 이 라벨이 화면에서 맡는 역할을 고르면 글자 크기·굵기·색이 토큰 타입 램프에 따라 자동 결정됨. 아래 표 참고 |
| `Required` | `true`면 텍스트 뒤에 **빨간 별표(\*)** 를 붙여 필수 입력 필드임을 표시 |

### Kind 값별 의미 (직접 Font/색을 지정하는 대신 역할을 선택)

| Kind | 모양 | 어디에 쓰나 |
|---|---|---|
| `Body` (기본) | 12px Regular, 진한 글자색 | 일반 본문/데이터 텍스트 |
| `Title` | 16px SemiBold, 진한 글자색 | 섹션/영역 제목 |
| `Label` | 12px SemiBold, 회색 | 입력 필드 앞 라벨 (이름·부서 등) |
| `Helper` | 12px Regular, 회색 | 입력 아래 보조 설명·안내문 |

## 미지원 멤버와 대체 방법

| 기존 멤버 | 대체 |
|---|---|
| `AutoSize` | 없음 — 명시적 `Size` 지정 필요. 넘치는 텍스트는 말줄임(…) 처리 |
| `Font`, `ForeColor` | 없음 — 타이포는 `Kind`가 결정 (토큰 단일 소스 원칙) |
| `TextAlign` | 세로 중앙 고정. 가로 정렬이 필요하면 배치로 해결 |

## .Designer.cs 교체 예시

```csharp
// 변경 전
private System.Windows.Forms.Label lblName;
this.lblName = new System.Windows.Forms.Label();

// 변경 후
private Modern.Lab.WinForms.Controls.Display.ModernLabel lblName;
this.lblName = new Modern.Lab.WinForms.Controls.Display.ModernLabel();
this.lblName.Kind = Modern.Lab.Controls.Wpf.Display.LabelKind.Label;  // 필드 라벨이면
```

권장 크기: 폭은 내용에 맞게, 높이 24 (Title은 32).
