using ChartForgeX;
using ChartForgeX.Core;
using ChartForgeX.Primitives;
using ChartForgeX.Themes;
using System.Xml.Linq;

namespace PowerBGInfo.Tests;

public sealed class BgInfoMarkLabelContrastTests
{
    [Theory]
    [InlineData(BgInfoChartKind.Bar, VisualThemeMode.Light)]
    [InlineData(BgInfoChartKind.Bar, VisualThemeMode.Dark)]
    [InlineData(BgInfoChartKind.HorizontalBar, VisualThemeMode.Light)]
    [InlineData(BgInfoChartKind.HorizontalBar, VisualThemeMode.Dark)]
    public void DefaultInsideLabelsUseContrastingInkAndAuthoredColorsStillWin(BgInfoChartKind kind, VisualThemeMode mode)
    {
        var definition = new BgInfoChart { Kind = kind, ThemeMode = mode, ShowDataLabels = true, LineColor = ChartColors.Cyan };
        var config = new BgInfoConfiguration();
        var plot = BgInfoChartRenderer.BuildChartForgeXChart(definition, new[] { 42d, 73d }, config, 400, 180)
            .WithDataLabelPlacement(ChartDataLabelPlacement.Inside);
        var labels = LabelNodes(plot.ToSvg());
        Assert.Equal(2, labels.Length);
        Assert.All(labels, label => Assert.True(Contrast(ChartColor.FromHex((string)label.Attribute("fill")!), ChartColors.Cyan) >= 4.5));
        var captureRoot = Environment.GetEnvironmentVariable("PBG_CAPTURE_ROOT");
        if (!string.IsNullOrWhiteSpace(captureRoot))
        {
            Directory.CreateDirectory(captureRoot);
            File.WriteAllText(Path.Combine(captureRoot, $"{kind}-{mode}.svg"), plot.ToSvg());
            File.WriteAllBytes(Path.Combine(captureRoot, $"{kind}-{mode}.png"), plot.ToPng());
        }

        config.ValueColor = ChartColors.Magenta;
        plot = BgInfoChartRenderer.BuildChartForgeXChart(definition, new[] { 42d, 73d }, config, 400, 180)
            .WithDataLabelPlacement(ChartDataLabelPlacement.Inside);
        Assert.All(LabelNodes(plot.ToSvg()), label => Assert.Equal(ChartColors.Magenta, ChartColor.FromHex((string)label.Attribute("fill")!)));

        definition.ValueColor = ChartColors.Gold;
        plot = BgInfoChartRenderer.BuildChartForgeXChart(definition, new[] { 42d, 73d }, config, 400, 180)
            .WithDataLabelPlacement(ChartDataLabelPlacement.Inside);
        Assert.All(LabelNodes(plot.ToSvg()), label => Assert.Equal(ChartColors.Gold, ChartColor.FromHex((string)label.Attribute("fill")!)));
    }

    private static XElement[] LabelNodes(string svg) => XDocument.Parse(svg).Descendants()
        .Where(node => (string?)node.Attribute("data-cfx-role") == "data-label")
        .SelectMany(node => node.DescendantsAndSelf().Where(child => child.Name.LocalName == "text")).ToArray();

    private static double Contrast(ChartColor first, ChartColor second)
    {
        static double Linear(byte channel) { var value = channel / 255d; return value <= .04045 ? value / 12.92 : Math.Pow((value + .055) / 1.055, 2.4); }
        static double Luminance(ChartColor color) => .2126 * Linear(color.R) + .7152 * Linear(color.G) + .0722 * Linear(color.B);
        var a = Luminance(first); var b = Luminance(second);
        return (Math.Max(a, b) + .05) / (Math.Min(a, b) + .05);
    }
}
