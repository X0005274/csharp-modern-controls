using System;
using System.Windows;
using System.Windows.Media;
using Modern.Lab.Theming;

namespace Modern.Lab.Controls.Wpf.Common
{
    /// <summary>
    /// WPF 컨트롤 장평(글자 가로 비율) 공용 첨부 속성.
    ///
    /// 사용 규약 (모든 WPF 모던 컨트롤 공통):
    /// 1. 컨트롤 루트에 <see cref="Initialize"/>를 한 번 호출한다 —
    ///    전역(ModernTheme.FontWidthRatio)과 재정의를 합성한 가로
    ///    <see cref="TextWidthTransformProperty"/>가 채워진다.
    ///    (ElementHost 래퍼는 WpfElementHostBase가 자동으로 호출한다.)
    /// 2. TextWidthTransform은 **상속(Inherits) 속성**이라 팝업·템플릿·아이템
    ///    내부까지 트리 전체로 전파된다. XAML의 텍스트 요소는 자기 자신이
    ///    물려받은 값을 바인딩한다:
    ///    <c>LayoutTransform="{Binding Path=(common:FontWidthScaling.TextWidthTransform),
    ///    RelativeSource={RelativeSource Self}}"</c>.
    ///    LayoutTransform은 측정에 반영되므로 줄어든/늘어난 폭 기준으로 배치도 맞는다.
    /// 3. WinForms 래퍼는 <see cref="SetFontWidthRatio"/>로 재정의 값을 넘긴다
    ///    (0 = 전역 사용, WpfElementHostBase.FontWidthRatio가 이 경로). 값이
    ///    바뀌면 변환이 자동 갱신된다.
    ///
    /// 장평 1.0(기본)이면 변환이 Identity라 렌더링 비용/결과 모두 기존과 같다.
    /// </summary>
    public static class FontWidthScaling
    {
        // 1.0에 충분히 가까우면 변환 없음(Identity)으로 처리한다.
        private const double ratioEpsilon = 0.001;

        /// <summary>장평 재정의 값 (0 = 전역 사용). 컨트롤 루트에 붙인다.</summary>
        public static readonly DependencyProperty FontWidthRatioProperty =
            DependencyProperty.RegisterAttached(
                "FontWidthRatio",
                typeof(double),
                typeof(FontWidthScaling),
                new PropertyMetadata(0d, OnFontWidthRatioChanged));

        /// <summary>
        /// 텍스트 요소에 걸 가로 변환 (읽기 전용 용도 — Initialize/재정의 변경 시 갱신).
        /// 장평 1.0이면 Identity. 상속 속성이라 컨트롤 루트에 설정하면 팝업·템플릿
        /// 내부의 텍스트 요소까지 값이 전파된다.
        /// </summary>
        public static readonly DependencyProperty TextWidthTransformProperty =
            DependencyProperty.RegisterAttached(
                "TextWidthTransform",
                typeof(Transform),
                typeof(FontWidthScaling),
                new FrameworkPropertyMetadata(Transform.Identity, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>장평 재정의 값을 읽는다.</summary>
        public static double GetFontWidthRatio(DependencyObject element)
        {
            return (double)element.GetValue(FontWidthRatioProperty);
        }

        /// <summary>장평 재정의 값을 설정한다 (0 = 전역 사용).</summary>
        public static void SetFontWidthRatio(DependencyObject element, double value)
        {
            element.SetValue(FontWidthRatioProperty, value);
        }

        /// <summary>현재 유효한 가로 변환을 읽는다.</summary>
        public static Transform GetTextWidthTransform(DependencyObject element)
        {
            return (Transform)element.GetValue(TextWidthTransformProperty);
        }

        /// <summary>가로 변환을 직접 설정한다 (일반적으로 Initialize/변경 콜백이 채운다).</summary>
        public static void SetTextWidthTransform(DependencyObject element, Transform value)
        {
            element.SetValue(TextWidthTransformProperty, value);
        }

        /// <summary>
        /// 컨트롤 생성 시 전역 장평을 반영한 변환을 채운다 —
        /// 재정의 없이 전역만 설정된 경우를 위해 생성자에서 반드시 호출한다.
        /// </summary>
        public static void Initialize(DependencyObject element)
        {
            SetTextWidthTransform(element, CreateTransform(GetFontWidthRatio(element)));
        }

        /// <summary>재정의 값(0 = 전역)을 합성해 가로 ScaleTransform을 만든다. 1.0이면 Identity.</summary>
        public static Transform CreateTransform(double localOverride)
        {
            double ratio = ModernTheme.ResolveFontWidthRatio(localOverride);

            if (Math.Abs(ratio - 1d) < ratioEpsilon)
            {
                return Transform.Identity;
            }

            ScaleTransform transform = new ScaleTransform(ratio, 1d);
            transform.Freeze();
            return transform;
        }

        private static void OnFontWidthRatioChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SetTextWidthTransform(d, CreateTransform((double)e.NewValue));
        }
    }
}
