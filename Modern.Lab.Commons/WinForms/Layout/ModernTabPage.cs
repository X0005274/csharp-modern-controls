using System.ComponentModel;
using System.Windows.Forms;

namespace Modern.Lab.WinForms.Controls.Layout
{
    /// <summary>
    /// <see cref="ModernTabControl"/>의 페이지 하나 — 표준 TabPage 대응.
    /// <see cref="Text"/>가 탭 헤더의 제목이 되고, 페이지 본문에는 어떤 WinForms
    /// 자식이든 담을 수 있다(순수 WinForms 컨테이너 — 계약 룰 5).
    ///
    /// 크기·위치·표시 여부는 부모 ModernTabControl이 선택 상태와 DisplayRectangle로
    /// 직접 관리한다. 폼 디자이너에서는 ModernTabControl의 "탭 추가" 동사로 만들고,
    /// 런타임에서는 ModernTabControl.AddTab이 내부적으로 이 타입을 생성한다.
    /// </summary>
    [ToolboxItem(false)]
    [DesignTimeVisible(false)]
    public class ModernTabPage : Panel
    {
        /// <summary>
        /// 탭 헤더에 표시할 제목. Panel이 숨긴 Control.Text를 다시 노출한다
        /// (override — 디자이너 직렬화·현지화 대상, 설계 노트 3장 규칙 준수).
        /// </summary>
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Bindable(true)]
        [Localizable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        /// <summary>
        /// 표시 여부는 부모 ModernTabControl이 선택 탭에 따라 제어하므로 디자이너
        /// 직렬화를 막는다(표준 TabPage와 동일한 처리). Control.Visible은 virtual이
        /// 아니라 new로 재선언하지만 값은 그대로 base에 전달되므로, Control 참조로
        /// 설정해도 동작 차이는 없다 — 순수 어트리뷰트 재장식이다.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool Visible
        {
            get { return base.Visible; }
            set { base.Visible = value; }
        }
    }
}
