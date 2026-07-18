using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using Modern.Lab.Controls.Wpf.Common;

namespace Modern.Lab.Controls.Wpf.Data
{
    /// <summary>
    /// ModernDataGridControl의 컬럼 정의(ModernDataGridColumn)를 실제
    /// DataGridColumn으로 만드는 팩토리. 컨트롤 인스턴스 상태(리소스 조회,
    /// 이벤트 핸들러, 헤더 체크박스 등록)는 전부 파라미터/콜백으로 받아
    /// 순수 생성만 담당한다.
    /// </summary>
    internal static class GridColumnFactory
    {
        // ===== 배지/버튼 셀이 쓰는 값 변환기 (공유 인스턴스) =====

        private static readonly IValueConverter badgeBackgroundConverter = new BadgeBackgroundConverter();
        private static readonly IValueConverter badgeForegroundConverter = new BadgeForegroundConverter();
        private static readonly IValueConverter truthyConverter = new TruthyToBooleanConverter();

        /// <summary>
        /// 컬럼 정의의 Kind에 따라 텍스트/체크박스/배지/버튼 컬럼을 만든다.
        /// </summary>
        /// <param name="definition">컬럼 정의.</param>
        /// <param name="widthRatio">장평(글자 가로 비율) — 1이 아니면 셀 텍스트에 가로 스케일 적용.</param>
        /// <param name="resourceSource">스타일/토큰 리소스를 조회할 기준 요소 (그리드 컨트롤).</param>
        /// <param name="headerCheckBoxClick">헤더 체크박스(전체 선택/해제) 클릭 콜백.</param>
        /// <param name="cellCheckBoxClick">셀 체크박스 클릭 콜백 (헤더 상태 재계산용).</param>
        /// <param name="cellButtonClick">버튼 셀 클릭 콜백 (CellButtonClick 이벤트 중계).</param>
        /// <param name="registerHeaderCheckBox">생성된 헤더 체크박스를 컨트롤 목록에 등록하는 콜백.</param>
        internal static DataGridColumn CreateColumn(
            ModernDataGridColumn definition,
            double widthRatio,
            FrameworkElement resourceSource,
            RoutedEventHandler headerCheckBoxClick,
            RoutedEventHandler cellCheckBoxClick,
            RoutedEventHandler cellButtonClick,
            Action<CheckBox> registerHeaderCheckBox)
        {
            switch (definition.Kind)
            {
                case GridColumnKind.CheckBox:
                    return CreateCheckBoxColumn(definition, resourceSource, headerCheckBoxClick, cellCheckBoxClick, registerHeaderCheckBox);
                case GridColumnKind.Badge:
                    return CreateBadgeColumn(definition, widthRatio, resourceSource);
                case GridColumnKind.Button:
                    return CreateButtonColumn(definition, widthRatio, resourceSource, cellButtonClick);
                default:
                    return CreateTextColumn(definition, widthRatio);
            }
        }

        /// <summary>장평 적용용 가로 ScaleTransform (Freeze해 컬럼/행 간 공유 가능).</summary>
        internal static ScaleTransform CreateWidthTransform(double widthRatio)
        {
            ScaleTransform transform = new ScaleTransform(widthRatio, 1d);
            transform.Freeze();
            return transform;
        }

        private static DataGridTextColumn CreateTextColumn(ModernDataGridColumn definition, double widthRatio)
        {
            DataGridTextColumn column = new DataGridTextColumn();
            column.Header = definition.HeaderText;

            Binding binding = new Binding(definition.DataPropertyName);

            // 표시 형식: "N0" 같은 단순 형식은 "{0:N0}"으로 해석된다(WPF 규칙).
            // 원본이 타입 컬럼(int/decimal/DateTime)일 때만 효과가 있다.
            if (!string.IsNullOrEmpty(definition.Format))
            {
                binding.StringFormat = definition.Format;
            }

            column.Binding = binding;
            ApplyColumnWidth(column, definition);

            Style elementStyle = new Style(typeof(TextBlock));

            if (definition.TextAlignment != GridTextAlignment.Left)
            {
                TextAlignment alignment = definition.TextAlignment == GridTextAlignment.Center
                    ? TextAlignment.Center
                    : TextAlignment.Right;

                elementStyle.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, alignment));
            }

            // 장평: 셀 TextBlock에 가로 LayoutTransform을 걸어 측정·배치까지
            // 줄어든/늘어난 폭 기준으로 동작하게 한다.
            if (Math.Abs(widthRatio - 1d) >= 0.001)
            {
                elementStyle.Setters.Add(
                    new Setter(FrameworkElement.LayoutTransformProperty, CreateWidthTransform(widthRatio)));
            }

            // 컬럼 강조색: 지정된 경우 컬럼 전체 텍스트에 적용한다 (해석 불가하면 기본색).
            if (!string.IsNullOrEmpty(definition.TextColor))
            {
                Brush textBrush = ChipColorHelper.TryCreateBrush(definition.TextColor);

                if (textBrush != null)
                {
                    elementStyle.Setters.Add(new Setter(TextBlock.ForegroundProperty, textBrush));
                }
            }

            // 컬럼 굵기 강조: 파생 지표를 색+굵기로 강조할 때 쓴다.
            if (definition.TextSemiBold)
            {
                elementStyle.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.SemiBold));
            }

            if (elementStyle.Setters.Count > 0)
            {
                column.ElementStyle = elementStyle;
            }

            return column;
        }

        // 체크박스 컬럼: bool 컬럼에 양방향 바인딩. 그리드가 읽기 전용이어도
        // CellTemplate 안의 체크박스는 살아 있으므로 한 번의 클릭으로 토글되고,
        // UpdateSourceTrigger=PropertyChanged로 원본 행 값이 즉시 갱신된다.
        private static DataGridColumn CreateCheckBoxColumn(
            ModernDataGridColumn definition,
            FrameworkElement resourceSource,
            RoutedEventHandler headerCheckBoxClick,
            RoutedEventHandler cellCheckBoxClick,
            Action<CheckBox> registerHeaderCheckBox)
        {
            DataGridTemplateColumn column = new DataGridTemplateColumn();
            column.CanUserSort = false;

            // 헤더 체크박스(전체 선택/해제): HeaderCheckBox 옵션이 켜지면 헤더
            // 캡션 대신 체크박스를 올린다. 클릭 시 현재 표시 중인 모든 행의 값을
            // 일괄 설정하고, 행 값 상태(전체/일부/없음)를 중간 상태로 되비춘다.
            if (definition.HeaderCheckBox)
            {
                CheckBox headerCheck = new CheckBox();
                headerCheck.Tag = definition.DataPropertyName;
                headerCheck.Style = (Style)resourceSource.FindResource("Parts.GridCheckBoxStyle");
                headerCheck.HorizontalAlignment = HorizontalAlignment.Center;
                headerCheck.VerticalAlignment = VerticalAlignment.Center;

                // 헤더 템플릿의 오른쪽 정렬 글리프 예약(여백 6px + 세로 구분선 1px)
                // 만큼 왼쪽 여백을 줘 셀 체크박스와 세로 중심을 맞춘다.
                headerCheck.Margin = new Thickness(7d, 0d, 0d, 0d);
                headerCheck.Click += headerCheckBoxClick;
                column.Header = headerCheck;
                registerHeaderCheckBox(headerCheck);

                // 좁은 체크박스 컬럼에서 헤더 기본 좌우 패딩(Pad.Field)이 내용
                // 영역을 잠식해 체크박스 오른쪽이 잘린다 — 이 컬럼만 패딩을 없애
                // 체크박스가 온전히 가운데 오게 한다.
                Style headerStyle = new Style(typeof(DataGridColumnHeader));
                headerStyle.BasedOn = (Style)resourceSource.FindResource("ModernGridColumnHeaderStyle");
                headerStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(0d)));
                column.HeaderStyle = headerStyle;
            }
            else
            {
                column.Header = definition.HeaderText;
            }

            Binding binding = new Binding(definition.DataPropertyName);
            binding.Mode = BindingMode.TwoWay;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            FrameworkElementFactory check = new FrameworkElementFactory(typeof(CheckBox));
            check.SetBinding(CheckBox.IsCheckedProperty, binding);
            check.SetValue(FrameworkElement.StyleProperty, resourceSource.FindResource("Parts.GridCheckBoxStyle"));
            check.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            check.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

            // 셀 토글이 헤더 체크박스 상태에 즉시 반영되도록 클릭을 구독한다.
            check.AddHandler(ButtonBase.ClickEvent, cellCheckBoxClick);

            DataTemplate template = new DataTemplate();
            template.VisualTree = check;
            column.CellTemplate = template;

            ApplyColumnWidth(column, definition);
            return column;
        }

        // 배지 컬럼: 값 텍스트를 BadgeColorMember 색의 알약(Border)으로 감싼다.
        // 글자색은 배경색에서 자동 유도(ChipColorHelper.DeriveForeground)한다.
        private static DataGridColumn CreateBadgeColumn(
            ModernDataGridColumn definition,
            double widthRatio,
            FrameworkElement resourceSource)
        {
            DataGridTemplateColumn column = new DataGridTemplateColumn();
            column.Header = definition.HeaderText;
            column.SortMemberPath = definition.DataPropertyName;

            // 알약 여백은 SummaryList 칩과 동일한 10,2 — 세로 2px 여백으로 알약이
            // 정확히 20px(= Radius.Pill 10의 지름)가 되어 완전한 캡슐로 그려지고,
            // 32px 행의 세로 중앙에 위아래 균등하게 앉는다. 글자도 답답하지 않다.
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.CornerRadiusProperty, resourceSource.FindResource("Radius.Pill"));
            border.SetValue(Border.PaddingProperty, new Thickness(10d, 2d, 10d, 2d));
            border.SetValue(FrameworkElement.HorizontalAlignmentProperty, ToHorizontalAlignment(definition.TextAlignment));
            border.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

            if (!string.IsNullOrEmpty(definition.BadgeColorMember))
            {
                Binding backgroundBinding = new Binding(definition.BadgeColorMember);
                backgroundBinding.Converter = badgeBackgroundConverter;
                border.SetBinding(Border.BackgroundProperty, backgroundBinding);
            }

            Binding textBinding = new Binding(definition.DataPropertyName);

            if (!string.IsNullOrEmpty(definition.Format))
            {
                textBinding.StringFormat = definition.Format;
            }

            FrameworkElementFactory text = new FrameworkElementFactory(typeof(TextBlock));
            text.SetBinding(TextBlock.TextProperty, textBinding);
            text.SetValue(TextBlock.FontSizeProperty, resourceSource.FindResource("Font.Size.Label"));
            text.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold);
            text.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

            // 장평: 배지 텍스트에도 같은 가로 스케일을 적용한다 (알약은 텍스트 폭을 따라간다).
            if (Math.Abs(widthRatio - 1d) >= 0.001)
            {
                text.SetValue(FrameworkElement.LayoutTransformProperty, CreateWidthTransform(widthRatio));
            }

            if (!string.IsNullOrEmpty(definition.BadgeColorMember))
            {
                Binding foregroundBinding = new Binding(definition.BadgeColorMember);
                foregroundBinding.Converter = badgeForegroundConverter;
                text.SetBinding(TextBlock.ForegroundProperty, foregroundBinding);
            }

            border.AppendChild(text);

            DataTemplate template = new DataTemplate();
            template.VisualTree = border;
            column.CellTemplate = template;

            ApplyColumnWidth(column, definition);
            return column;
        }

        // 버튼 컬럼: 행 단위 액션 버튼. 클릭은 CellButtonClick 이벤트로 전달되고,
        // ButtonEnabledMember 컬럼 값(bool/Y/N)이 행별 활성화를 제어한다.
        private static DataGridColumn CreateButtonColumn(
            ModernDataGridColumn definition,
            double widthRatio,
            FrameworkElement resourceSource,
            RoutedEventHandler cellButtonClick)
        {
            DataGridTemplateColumn column = new DataGridTemplateColumn();
            column.Header = definition.HeaderText;
            column.CanUserSort = false;

            FrameworkElementFactory button = new FrameworkElementFactory(typeof(Button));

            // 캡션은 TextBlock으로 감싸 장평(가로 스케일)을 적용한다 —
            // 버튼 테두리는 스케일하지 않고 글자만 조절된다.
            FrameworkElementFactory caption = new FrameworkElementFactory(typeof(TextBlock));
            caption.SetValue(TextBlock.TextProperty, definition.ButtonText);

            if (Math.Abs(widthRatio - 1d) >= 0.001)
            {
                caption.SetValue(FrameworkElement.LayoutTransformProperty, CreateWidthTransform(widthRatio));
            }

            button.AppendChild(caption);
            button.SetValue(FrameworkElement.TagProperty, definition.DataPropertyName);
            button.SetValue(FrameworkElement.HorizontalAlignmentProperty, ToHorizontalAlignment(definition.TextAlignment));
            button.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            button.SetValue(FrameworkElement.StyleProperty, BuildCellButtonStyle(resourceSource));

            if (!string.IsNullOrEmpty(definition.ButtonEnabledMember))
            {
                Binding enabledBinding = new Binding(definition.ButtonEnabledMember);
                enabledBinding.Converter = truthyConverter;
                button.SetBinding(UIElement.IsEnabledProperty, enabledBinding);
            }

            button.AddHandler(ButtonBase.ClickEvent, cellButtonClick);

            DataTemplate template = new DataTemplate();
            template.VisualTree = button;
            column.CellTemplate = template;

            ApplyColumnWidth(column, definition);
            return column;
        }

        // 셀 안 버튼: ModernButton Secondary와 같은 문법의 차분한 버튼 —
        // 평상시 흰 배경 + 회색 테두리 + 진한 글자, hover는 옅은 파랑 틴트 +
        // 액센트 테두리/글자, pressed는 한 단계 진한 틴트. 행마다 반복 노출되는
        // 액션이라 평상시엔 중립을 유지하고 상호작용 시에만 액센트가 드러난다.
        // 캡션이 갑갑하지 않도록 상하 패딩을 확보해 행 높이(32) 안에서 위아래
        // 중앙에 온다. 비활성 행은 회색 채움/텍스트로 눌 수 없음을 드러낸다.
        private static Style BuildCellButtonStyle(FrameworkElement resourceSource)
        {
            Style style = new Style(typeof(Button));
            style.Setters.Add(new Setter(Control.ForegroundProperty, resourceSource.FindResource("Brush.TextPrimary")));
            style.Setters.Add(new Setter(Control.BackgroundProperty, resourceSource.FindResource("Brush.Surface")));
            style.Setters.Add(new Setter(Control.BorderBrushProperty, resourceSource.FindResource("Brush.Border")));
            style.Setters.Add(new Setter(Control.FontSizeProperty, resourceSource.FindResource("Font.Size.Label")));
            style.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.SemiBold));
            style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(14d, 4d, 14d, 4d)));
            style.Setters.Add(new Setter(FrameworkElement.CursorProperty, System.Windows.Input.Cursors.Hand));

            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.CornerRadiusProperty, resourceSource.FindResource("Radius.Sm"));
            border.SetValue(Border.BorderThicknessProperty, new Thickness(1d));
            border.SetBinding(Border.BackgroundProperty, TemplateBinding(Control.BackgroundProperty));
            border.SetBinding(Border.BorderBrushProperty, TemplateBinding(Control.BorderBrushProperty));
            border.SetBinding(Border.PaddingProperty, TemplateBinding(Control.PaddingProperty));

            FrameworkElementFactory presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            presenter.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            border.AppendChild(presenter);
            template.VisualTree = border;
            style.Setters.Add(new Setter(Control.TemplateProperty, template));

            Trigger hover = new Trigger();
            hover.Property = UIElement.IsMouseOverProperty;
            hover.Value = true;
            hover.Setters.Add(new Setter(Control.BackgroundProperty, resourceSource.FindResource("Brush.InfoBackground")));
            hover.Setters.Add(new Setter(Control.BorderBrushProperty, resourceSource.FindResource("Brush.Accent")));
            hover.Setters.Add(new Setter(Control.ForegroundProperty, resourceSource.FindResource("Brush.Accent")));
            style.Triggers.Add(hover);

            Trigger pressed = new Trigger();
            pressed.Property = ButtonBase.IsPressedProperty;
            pressed.Value = true;
            pressed.Setters.Add(new Setter(Control.BackgroundProperty, resourceSource.FindResource("Brush.SelectedBackground")));
            pressed.Setters.Add(new Setter(Control.BorderBrushProperty, resourceSource.FindResource("Brush.AccentHover")));
            pressed.Setters.Add(new Setter(Control.ForegroundProperty, resourceSource.FindResource("Brush.AccentHover")));
            style.Triggers.Add(pressed);

            Trigger disabled = new Trigger();
            disabled.Property = UIElement.IsEnabledProperty;
            disabled.Value = false;
            disabled.Setters.Add(new Setter(Control.BackgroundProperty, resourceSource.FindResource("Brush.DisabledBackground")));
            disabled.Setters.Add(new Setter(Control.ForegroundProperty, resourceSource.FindResource("Brush.DisabledText")));
            disabled.Setters.Add(new Setter(Control.BorderBrushProperty, resourceSource.FindResource("Brush.BorderSubtle")));
            style.Triggers.Add(disabled);

            return style;
        }

        // 코드 생성 템플릿에서 TemplateBinding과 같은 효과를 내는 바인딩 헬퍼.
        private static Binding TemplateBinding(DependencyProperty property)
        {
            Binding binding = new Binding(property.Name);
            binding.RelativeSource = RelativeSource.TemplatedParent;
            return binding;
        }

        // 정의 폭(양수)이 있으면 고정 픽셀, 없으면 별(*) 너비로 채운다.
        private static void ApplyColumnWidth(DataGridColumn column, ModernDataGridColumn definition)
        {
            if (definition.Width > 0d)
            {
                column.Width = new DataGridLength(definition.Width);
            }
            else
            {
                column.Width = new DataGridLength(1d, DataGridLengthUnitType.Star);
            }
        }

        // GridTextAlignment → WPF HorizontalAlignment 변환.
        private static HorizontalAlignment ToHorizontalAlignment(GridTextAlignment alignment)
        {
            if (alignment == GridTextAlignment.Center)
            {
                return HorizontalAlignment.Center;
            }

            if (alignment == GridTextAlignment.Right)
            {
                return HorizontalAlignment.Right;
            }

            return HorizontalAlignment.Left;
        }
    }
}
