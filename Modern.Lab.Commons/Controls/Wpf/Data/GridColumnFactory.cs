using System;
using System.Globalization;
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
                case GridColumnKind.Combo:
                    return CreateComboColumn(definition, resourceSource);
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

            // 잘린 셀에만 전체 텍스트 툴팁 — 컬럼 폭이 좁아 "…"로 잘리면 호버 시
            // 전체 값을 보여 준다. ToolTip은 셀 자신의 Text에 바인딩하고, 실제로
            // 잘리지 않았으면 ToolTipOpening에서 취소해 불필요한 툴팁을 막는다.
            elementStyle.Setters.Add(new Setter(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis));
            elementStyle.Setters.Add(new Setter(
                TextBlock.ToolTipProperty,
                new Binding("Text") { RelativeSource = new RelativeSource(RelativeSourceMode.Self) }));
            elementStyle.Setters.Add(new EventSetter(
                ToolTipService.ToolTipOpeningEvent, new ToolTipEventHandler(OnCellToolTipOpening)));

            if (elementStyle.Setters.Count > 0)
            {
                column.ElementStyle = elementStyle;
            }

            return column;
        }

        // 셀 텍스트가 실제로 잘렸을 때만 툴팁을 연다 — 안 잘렸으면 취소.
        private static void OnCellToolTipOpening(object sender, ToolTipEventArgs e)
        {
            TextBlock text = sender as TextBlock;

            if (text == null || !IsTextTrimmed(text))
            {
                e.Handled = true;
            }
        }

        // TextBlock의 전체 텍스트 폭이 실제 표시 폭보다 크면(=잘림) true.
        private static bool IsTextTrimmed(TextBlock text)
        {
            if (string.IsNullOrEmpty(text.Text) || text.ActualWidth <= 0d)
            {
                return false;
            }

            Typeface typeface = new Typeface(
                text.FontFamily, text.FontStyle, text.FontWeight, text.FontStretch);

            FormattedText formatted = new FormattedText(
                text.Text, CultureInfo.CurrentCulture, text.FlowDirection, typeface,
                text.FontSize, Brushes.Black, VisualTreeHelper.GetDpi(text).PixelsPerDip);

            double available = text.ActualWidth - text.Padding.Left - text.Padding.Right;
            return formatted.WidthIncludingTrailingWhitespace > available + 0.5d;
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

        // 배지 컬럼: 값 텍스트를 BadgeColorMember 색의 레티클(둥근 사각,
        // Radius.Sm) 배지로 감싼다. 글자색은 배경색에서 자동 유도
        // (ChipColorHelper.DeriveForeground)한다.
        private static DataGridColumn CreateBadgeColumn(
            ModernDataGridColumn definition,
            double widthRatio,
            FrameworkElement resourceSource)
        {
            DataGridTemplateColumn column = new DataGridTemplateColumn();
            column.Header = definition.HeaderText;
            column.SortMemberPath = definition.DataPropertyName;

            // 여백 10,2 — 셀 버튼보다 한 단계 낮은 컴팩트 배지(약 20px)로
            // 32px 행의 세로 중앙에 앉는다. 배지/버튼 컬럼이 나란히 있어도
            // 배지가 버튼을 넘지 않는다. 모양은 레티클(둥근 사각) — 배지 관례와 통일.
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.CornerRadiusProperty, resourceSource.FindResource("Radius.Sm"));
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
            text.SetValue(TextBlock.FontWeightProperty, FontWeights.Normal);
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

        // 콤보 입력 컬럼: ComboItems의 고정 선택지 중 하나를 고르면 원본 행
        // 컬럼 값이 즉시 갱신된다 (체크박스 컬럼과 같은 양방향 규칙 — 그리드가
        // 읽기 전용이어도 CellTemplate 안의 콤보는 살아 있다). 행별 입력 가능
        // 여부는 ComboEnabledMember 컬럼 값(bool/Y/N)이 제어한다.
        private static DataGridColumn CreateComboColumn(
            ModernDataGridColumn definition,
            FrameworkElement resourceSource)
        {
            DataGridTemplateColumn column = new DataGridTemplateColumn();
            column.Header = definition.HeaderText;
            column.SortMemberPath = definition.DataPropertyName;

            FrameworkElementFactory combo = new FrameworkElementFactory(typeof(ComboBox));
            combo.SetValue(FrameworkElement.StyleProperty, resourceSource.FindResource("Parts.GridComboBoxStyle"));
            combo.SetValue(ItemsControl.ItemsSourceProperty, definition.ComboItems ?? new string[0]);
            combo.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            combo.SetValue(FrameworkElement.MarginProperty, new Thickness(2d, 0d, 2d, 0d));

            // 배지형 콤보 — 선택지별 색이 지정되면 선택 값과 드롭다운 항목을
            // 레티클 배지로 그리고 (배지 컬럼과 같은 글자색 유도 규칙),
            // **필드 표면 전체**도 선택 값의 배지 색으로 칠한다 — 미선택이면
            // 기본 필드(Surface)로 떨어진다.
            if (definition.ComboItemColors != null && definition.ComboItemColors.Length > 0)
            {
                combo.SetValue(
                        ItemsControl.ItemTemplateProperty,
                        BuildComboBadgeTemplate(definition, resourceSource));

                Binding surfaceBinding = new Binding(definition.DataPropertyName);
                surfaceBinding.Converter = new ComboItemBadgeConverter(
                        definition.ComboItems, definition.ComboItemColors, false);
                surfaceBinding.FallbackValue = resourceSource.FindResource("Brush.Surface");
                combo.SetBinding(Control.BackgroundProperty, surfaceBinding);
            }

            Binding valueBinding = new Binding(definition.DataPropertyName);
            valueBinding.Mode = BindingMode.TwoWay;
            valueBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            combo.SetBinding(Selector.SelectedItemProperty, valueBinding);

            if (!string.IsNullOrEmpty(definition.ComboEnabledMember))
            {
                Binding enabledBinding = new Binding(definition.ComboEnabledMember);
                enabledBinding.Converter = truthyConverter;
                combo.SetBinding(UIElement.IsEnabledProperty, enabledBinding);
            }

            DataTemplate template = new DataTemplate();
            template.VisualTree = combo;
            column.CellTemplate = template;

            ApplyColumnWidth(column, definition);
            return column;
        }

        // 배지형 콤보의 항목 템플릿 — 항목 문자열을 (ComboItems ↔
        // ComboItemColors) 매핑 색의 레티클(둥근 사각, Radius.Sm) 배지로
        // 감싼다. 배지는 행 전체 폭을 채워(가운데 텍스트) 필드 표면 색과
        // 이어져 보인다. ItemTemplate 하나로 선택 값 표시(SelectionBox)와
        // 드롭다운 항목이 함께 배지가 된다.
        private static DataTemplate BuildComboBadgeTemplate(
            ModernDataGridColumn definition,
            FrameworkElement resourceSource)
        {
            IValueConverter backgroundConverter = new ComboItemBadgeConverter(
                    definition.ComboItems, definition.ComboItemColors, false);
            IValueConverter foregroundConverter = new ComboItemBadgeConverter(
                    definition.ComboItems, definition.ComboItemColors, true);

            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.CornerRadiusProperty, resourceSource.FindResource("Radius.Sm"));
            border.SetValue(Border.PaddingProperty, new Thickness(10d, 1d, 10d, 1d));
            border.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            border.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

            Binding backgroundBinding = new Binding();
            backgroundBinding.Converter = backgroundConverter;
            border.SetBinding(Border.BackgroundProperty, backgroundBinding);

            FrameworkElementFactory text = new FrameworkElementFactory(typeof(TextBlock));
            text.SetBinding(TextBlock.TextProperty, new Binding());
            text.SetValue(TextBlock.FontSizeProperty, resourceSource.FindResource("Font.Size.Label"));
            text.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold);
            text.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            text.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

            Binding foregroundBinding = new Binding();
            foregroundBinding.Converter = foregroundConverter;
            text.SetBinding(TextBlock.ForegroundProperty, foregroundBinding);

            border.AppendChild(text);

            DataTemplate template = new DataTemplate();
            template.VisualTree = border;
            return template;
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
            // 좌우 10px — 행마다 반복되는 버튼이라 컬럼 폭을 잡아먹지 않게
            // 일반 버튼(14px)보다 한 단계 좁힌다.
            style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(10d, 4d, 10d, 4d)));
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
