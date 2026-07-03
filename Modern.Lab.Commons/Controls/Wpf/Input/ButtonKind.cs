namespace Modern.Lab.Controls.Wpf.Input
{
    /// <summary>Visual kind (color / emphasis level) of a button.</summary>
    public enum ButtonKind
    {
        /// <summary>Primary action (e.g. query). Accent-blue background, white text. One per screen.</summary>
        Primary,

        /// <summary>Execute-tier action (new/save/edit). White background, gray border.</summary>
        Secondary,

        /// <summary>Destructive action (delete etc.). Outlined red — red text/border on white.</summary>
        Danger,

        /// <summary>Low-emphasis action (reset/export etc.). Borderless text button with hover background.</summary>
        Subtle
    }
}
