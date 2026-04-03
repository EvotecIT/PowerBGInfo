namespace PowerBGInfo;

/// <summary>
/// Defines where generated BGInfo output is applied.
/// </summary>
[Flags]
public enum BgInfoTarget {
    /// <summary>Apply as wallpaper only.</summary>
    Wallpaper = 1,
    /// <summary>Apply as logon/lock screen wallpaper.</summary>
    LogonScreen = 2,
    /// <summary>Write output file only.</summary>
    File = 4,
    /// <summary>Apply as wallpaper and logon/lock screen wallpaper.</summary>
    Both = Wallpaper | LogonScreen
}
