using System.Collections.Generic;
using SixLabors.ImageSharp;
using DesktopManager;

namespace PowerBGInfo;

public class BgInfoConfiguration
{
    public string FilePath { get; set; } = string.Empty;
    public string ConfigurationDirectory { get; set; } = string.Empty;
    public string FontFamilyName { get; set; } = "Calibri";
    public Color Color { get; set; } = Color.Black;
    public float FontSize { get; set; } = 16f;
    public Color ValueColor { get; set; } = Color.Black;
    public float ValueFontSize { get; set; } = 16f;
    public string ValueFontFamilyName { get; set; } = "Calibri";
    public int SpaceBetweenLines { get; set; } = 10;
    public int SpaceBetweenColumns { get; set; } = 30;
    public float PositionX { get; set; } = 10;
    public float PositionY { get; set; } = 10;
    public int MonitorIndex { get; set; }
    public int SpaceX { get; set; } = 10;
    public int SpaceY { get; set; } = 10;
    public DesktopWallpaperPosition WallpaperFit { get; set; } = DesktopWallpaperPosition.Center;
    public string TextPosition { get; set; } = "TopLeft";
    public string Target { get; set; } = "Wallpaper";
    public bool UseScreenCoordinates { get; set; }
    public List<BgInfoEntry> Entries { get; } = new();
}
