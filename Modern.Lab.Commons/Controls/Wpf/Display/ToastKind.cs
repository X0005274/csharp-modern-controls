namespace Modern.Lab.Controls.Wpf.Display
{
    /// <summary>토스트 알림의 종류 — 아이콘과 색을 결정한다.</summary>
    public enum ToastKind
    {
        /// <summary>정보 (파랑, i 아이콘).</summary>
        Info,

        /// <summary>성공 (초록, 체크 아이콘).</summary>
        Success,

        /// <summary>경고 (주황, 느낌표 아이콘).</summary>
        Warning,

        /// <summary>오류 (빨강, X 아이콘).</summary>
        Error
    }
}
