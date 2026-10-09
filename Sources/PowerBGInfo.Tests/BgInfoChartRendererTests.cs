using ChartForgeX;
using ChartForgeX.Core;
using ChartForgeX.Primitives;
using ChartForgeX.Typography;
using ChartForgeX.Themes;
using System.Xml.Linq;

namespace PowerBGInfo.Tests;

public class BgInfoChartRendererTests
{
    [Theory]
    [InlineData(BgInfoChartKind.Bar, "grid-y")]
    [InlineData(BgInfoChartKind.HorizontalBar, "grid-x")]
    public void GridDensityChangesExportedValueGuidesAndDisabledSettingsRemoveAllGuides(BgInfoChartKind kind, string valueGuideRole)
    {
        // Use separated preferred budgets; nice-number intervals need not change for adjacent settings.
        var chart = new BgInfoChart { Kind = kind, ShowGrid = true, GridLineCount = 2 };
        var values = new[] { 0d, 100d };
        var configuration = new BgInfoConfiguration();
        var sparse = XDocument.Parse(BgInfoChartRenderer.BuildChartForgeXChart(chart, values, configuration, 320, 180).ToSvg());
        chart.GridLineCount = 10;
        var dense = XDocument.Parse(BgInfoChartRenderer.BuildChartForgeXChart(chart, values, configuration, 320, 180).ToSvg());
        var sparseGuides = sparse.Descendants().Count(node => (string?)node.Attribute("data-cfx-role") == valueGuideRole);
        var denseGuides = dense.Descendants().Count(node => (string?)node.Attribute("data-cfx-role") == valueGuideRole);
        Assert.True(sparseGuides > 0);
        Assert.True(denseGuides > sparseGuides,
            $"{kind} value guides did not respond to the preferred density: sparse={sparseGuides}, dense={denseGuides}.");

        chart.GridLineCount = 0;
        var zero = XDocument.Parse(BgInfoChartRenderer.BuildChartForgeXChart(chart, values, configuration, 320, 180).ToSvg());
        Assert.DoesNotContain(zero.Descendants(), node => (string?)node.Attribute("data-cfx-role") is "grid-x" or "grid-y");
        chart.GridLineCount = 10;
        chart.ShowGrid = false;
        var disabled = XDocument.Parse(BgInfoChartRenderer.BuildChartForgeXChart(chart, values, configuration, 320, 180).ToSvg());
        Assert.DoesNotContain(disabled.Descendants(), node => (string?)node.Attribute("data-cfx-role") is "grid-x" or "grid-y");
    }

    [Theory]
    [InlineData(VisualThemeMode.Light)]
    [InlineData(VisualThemeMode.Dark)]
    public void ChartThemeUsesSharedRolesAndPreservesExplicitHostColors(VisualThemeMode mode)
    {
        var chart = new BgInfoChart { Kind = BgInfoChartKind.Donut, ThemeMode = mode };
        var values = new[] { 42d, 58d };
        var tokens = mode == VisualThemeMode.Light ? VisualDesignTokens.GraphiteLight() : VisualDesignTokens.GraphiteDark();
        var configuration = new BgInfoConfiguration();

        var rendered = BgInfoChartRenderer.BuildChartForgeXChart(chart, values, configuration, 320, 180);

        Assert.Equal(tokens.Palette, rendered.Options.Theme.Palette);
        Assert.Equal(tokens.Foreground, rendered.Options.TitleStyle.Color);
        Assert.Equal(tokens.Foreground, rendered.Options.DataLabelStyle.Color);
        Assert.Equal(VisualTheme.Graphite().Typography.LegendSize, rendered.Options.Theme.LegendFontSize);

        configuration.Color = ChartColors.Gold;
        configuration.ValueColor = ChartColors.Cyan;
        chart.Palette = new[] { ChartColors.Red, ChartColors.Green };
        rendered = BgInfoChartRenderer.BuildChartForgeXChart(chart, values, configuration, 320, 180);
        Assert.Equal(chart.Palette, rendered.Options.Theme.Palette);
        Assert.Equal(ChartColors.Gold, rendered.Options.TitleStyle.Color);
        Assert.Equal(ChartColors.Cyan, rendered.Options.DataLabelStyle.Color);
    }

    [Theory]
    [InlineData(VisualThemeMode.Light)]
    [InlineData(VisualThemeMode.Dark)]
    public void InheritedChartFontsReachPreparedTextAndNativeHeaders(VisualThemeMode mode)
    {
        var chart = new BgInfoChart {
            Title = "Memory working set",
            Kind = BgInfoChartKind.Bar,
            ThemeMode = mode,
            Values = new[] { 42d, 65d },
            ShowDataLabels = true,
            Width = 320,
            Height = 180
        };
        var configuration = new BgInfoConfiguration();
        var tokens = mode == VisualThemeMode.Light ? VisualDesignTokens.GraphiteLight() : VisualDesignTokens.GraphiteDark();
        Assert.Equal("Calibri", configuration.FontFamilyName); // Classic entries retain their default.
        Assert.Equal("Calibri", configuration.ValueFontFamilyName);
        var plot = BgInfoChartRenderer.BuildChartForgeXChart(chart, chart.Values, configuration, 320, 180);
        var labels = XDocument.Parse(plot.ToSvg()).Descendants().Where(node => node.Name.LocalName == "text").ToArray();
        Assert.NotEmpty(labels);
        Assert.All(labels, label => Assert.Equal(tokens.FontFamily, (string?)label.Attribute("font-family")));

        using var inherited = BgInfoChartRenderer.Render(chart, chart.Values, configuration);
        using var explicitTheme = BgInfoChartRenderer.Render(chart, chart.Values, new BgInfoConfiguration {
            FontFamilyName = tokens.FontFamily,
            ValueFontFamilyName = tokens.FontFamily
        });
        Assert.Equal(explicitTheme.ToRgbaImage().Pixels, inherited.ToRgbaImage().Pixels);

        configuration.FontFamilyName = "Calibri"; // An explicit default remains an override.
        configuration.ValueFontFamilyName = "Consolas";
        plot = BgInfoChartRenderer.BuildChartForgeXChart(chart, chart.Values, configuration, 320, 180);
        Assert.Equal("Calibri", plot.Options.TitleStyle.FontFamily);
        Assert.Equal("Consolas", plot.Options.DataLabelStyle.FontFamily);
        labels = XDocument.Parse(plot.ToSvg()).Descendants().Where(node => node.Name.LocalName == "text").ToArray();
        Assert.NotEmpty(labels);
        Assert.All(labels, label => Assert.Equal("Consolas", (string?)label.Attribute("font-family")));
    }

    [Fact]
    public void LongChartTitleDoesNotPaintAcrossTheCompleteLatestValue()
    {
        var chart = new BgInfoChart {
            Title = "Memory use on the primary operational workstation",
            Kind = BgInfoChartKind.Bar,
            Width = 240,
            Height = 150,
            Values = new[] { 13.25 },
            ValueSuffix = " GiB",
            TitleColor = ChartColors.Red,
            ValueColor = ChartColors.Cyan,
            LineColor = ChartColors.Black,
            TitleFontSize = 20,
            ValueFontSize = 20
        };

        using var rendered = BgInfoChartRenderer.Render(chart, chart.Values, new BgInfoConfiguration());
        var pixels = rendered.ToRgbaImage();
        var titleRows = Enumerable.Range(0, pixels.Width * pixels.Height)
            .Where(i => pixels.Pixels[i * 4] > 180 && pixels.Pixels[i * 4 + 1] < 80 && pixels.Pixels[i * 4 + 2] < 80)
            .Select(i => i / pixels.Width).ToArray();
        var valueRows = Enumerable.Range(0, pixels.Width * pixels.Height)
            .Where(i => pixels.Pixels[i * 4] < 80 && pixels.Pixels[i * 4 + 1] > 180 && pixels.Pixels[i * 4 + 2] > 180)
            .Select(i => i / pixels.Width).ToArray();

        Assert.NotEmpty(titleRows);
        Assert.NotEmpty(valueRows);
        Assert.True(titleRows.Max() < valueRows.Min());
        Assert.Equal("Memory use on the primary operational workstation", chart.Title);
        Assert.Equal(" GiB", chart.ValueSuffix);
    }

    [Theory]
    [InlineData(BgInfoChartKind.Bar, 240, 48, 42)]
    [InlineData(BgInfoChartKind.Line, 24, 80, 14)]
    public void ShortChartTilesWithTallTitlesRenderAtTheirAuthoredDimensions(BgInfoChartKind kind, int width, int height, int titleFontSize)
    {
        var chart = new BgInfoChart {
            Title = "CPU usage",
            Kind = kind,
            Values = new[] { 25d, 42d, 30d },
            Width = width,
            Height = height,
            TitleFontSize = titleFontSize,
            ValueFontSize = titleFontSize,
            TitleColor = ChartColors.White,
            ValueColor = ChartColors.Cyan
        };

        using var rendered = BgInfoChartRenderer.Render(chart, chart.Values, new BgInfoConfiguration());
        var pixels = rendered.ToRgbaImage();

        Assert.Equal(width, pixels.Width);
        Assert.Equal(height, pixels.Height);
        Assert.Contains(Enumerable.Range(0, width * height), index => pixels.Pixels[index * 4 + 3] > 0);
    }

    [Fact]
    public void ChartTextStylesFlowToEveryChartForgeXTextRoleWithoutInflatingRoleDefaults()
    {
        var chart = new BgInfoChart {
            Title = "CPU usage",
            Kind = BgInfoChartKind.Bar,
            Values = new[] { 42d },
            TitleColor = ChartColors.Gold,
            ValueColor = ChartColors.Cyan,
            FontFamilyName = "Consolas",
            TitleFontWeight = 800,
            TitleItalic = true,
            TitleUnderlineStyle = TextDecorationStyle.Double,
            TitleStrikethroughStyle = TextDecorationStyle.Dashed,
            TitleBaseline = TextBaseline.Superscript,
            TitleTextCase = TextCaseTransform.TitleCase,
            ValueFontWeight = 300,
            ValueItalic = true,
            ValueUnderlineStyle = TextDecorationStyle.Wavy,
            ValueStrikethroughStyle = TextDecorationStyle.Dotted,
            ValueBaseline = TextBaseline.Subscript,
            ValueTextCase = TextCaseTransform.ToggleCase
        };

        var rendered = BgInfoChartRenderer.BuildChartForgeXChart(chart, chart.Values, new BgInfoConfiguration(), 320, 180);

        Assert.Equal(ChartColors.Gold, rendered.Options.TitleStyle.Color);
        Assert.Equal("Consolas", rendered.Options.TitleStyle.FontFamily);
        Assert.Equal("800", rendered.Options.TitleStyle.FontWeight);
        Assert.True(rendered.Options.TitleStyle.Italic);
        Assert.Equal(TextDecorationStyle.Double, rendered.Options.TitleStyle.UnderlineStyle);
        Assert.Equal(TextDecorationStyle.Dashed, rendered.Options.TitleStyle.StrikethroughStyle);
        Assert.Equal(TextBaseline.Superscript, rendered.Options.TitleStyle.Baseline);
        Assert.Equal(TextCaseTransform.TitleCase, rendered.Options.TitleStyle.TextCase);
        Assert.Null(rendered.Options.TitleStyle.FontSize);

        foreach (var style in new[] {
                     rendered.Options.DataLabelStyle,
                     rendered.Options.LegendStyle,
                     rendered.Options.TickLabelStyle
                 }) {
            Assert.Equal(ChartColors.Cyan, style.Color);
            Assert.Equal("Consolas", style.FontFamily);
            Assert.Equal("300", style.FontWeight);
            Assert.True(style.Italic);
            Assert.Equal(TextDecorationStyle.Wavy, style.UnderlineStyle);
            Assert.Equal(TextDecorationStyle.Dotted, style.StrikethroughStyle);
            Assert.Equal(TextBaseline.Subscript, style.Baseline);
            Assert.Equal(TextCaseTransform.ToggleCase, style.TextCase);
            Assert.Null(style.FontSize);
        }
    }

    [Fact]
    public void ExplicitChartFontSizesOverrideChartForgeXRoleDefaults()
    {
        var chart = new BgInfoChart {
            Kind = BgInfoChartKind.Bar,
            Values = new[] { 42d },
            TitleFontSize = 23,
            ValueFontSize = 17
        };

        var rendered = BgInfoChartRenderer.BuildChartForgeXChart(chart, chart.Values, new BgInfoConfiguration(), 320, 180);

        Assert.Equal(23, rendered.Options.TitleStyle.FontSize);
        Assert.Equal(23, rendered.Options.AxisTitleStyle.FontSize);
        Assert.Equal(17, rendered.Options.DataLabelStyle.FontSize);
        Assert.Equal(17, rendered.Options.LegendStyle.FontSize);
        Assert.Equal(17, rendered.Options.TickLabelStyle.FontSize);
    }

    [Theory]
    [InlineData(BgInfoChartKind.Line, false)]
    [InlineData(BgInfoChartKind.Area, true)]
    [InlineData(BgInfoChartKind.Sparkline, true)]
    public void DenseTrendSeriesUseWidthAwareDecimationWithoutChangingTheirStyle(BgInfoChartKind kind, bool expectedSmooth)
    {
        const int sourcePointCount = 10_000;
        const int spikeIndex = sourcePointCount / 2;
        var values = new double[sourcePointCount];
        values[spikeIndex] = 1000;
        var chart = new BgInfoChart {
            Kind = kind,
            Width = 240,
            Height = 90
        };

        var rendered = BgInfoChartRenderer.BuildChartForgeXChart(chart, values, new BgInfoConfiguration(), chart.Width, chart.Height);

        var series = Assert.Single(rendered.Series);
        Assert.True(series.IsDecimated);
        Assert.Equal(sourcePointCount, series.SourcePointCount);
        Assert.Equal(ChartDecimationMode.LargestTriangleThreeBuckets, series.DecimationMode);
        Assert.True(series.Points.Count <= ChartResolutionPolicy.Trend().ResolvePointBudget(chart.Width));
        Assert.Equal(0, series.SourcePointIndices[0]);
        Assert.Equal(sourcePointCount - 1, series.SourcePointIndices[series.SourcePointIndices.Count - 1]);
        Assert.Contains(spikeIndex, series.SourcePointIndices);
        Assert.Equal(expectedSmooth, series.Smooth);
    }

    [Fact]
    public void DenseCategoricalSeriesKeepEveryExactValue()
    {
        var values = Enumerable.Range(0, 2_000).Select(value => (double)value).ToArray();
        var chart = new BgInfoChart {
            Kind = BgInfoChartKind.Bar,
            Width = 240,
            Height = 90
        };

        var rendered = BgInfoChartRenderer.BuildChartForgeXChart(chart, values, new BgInfoConfiguration(), chart.Width, chart.Height);

        var series = Assert.Single(rendered.Series);
        Assert.False(series.IsDecimated);
        Assert.Null(series.DecimationMode);
        Assert.Equal(values.Length, series.Points.Count);
        Assert.Equal(values.Length, series.SourcePointCount);
    }

    [Fact]
    public void ShortTrendSeriesKeepEveryExactValue()
    {
        var values = Enumerable.Range(0, 30).Select(value => Math.Sin(value / 3d)).ToArray();
        var chart = new BgInfoChart {
            Kind = BgInfoChartKind.Area,
            Width = 240,
            Height = 90
        };

        var rendered = BgInfoChartRenderer.BuildChartForgeXChart(chart, values, new BgInfoConfiguration(), chart.Width, chart.Height);

        var series = Assert.Single(rendered.Series);
        Assert.False(series.IsDecimated);
        Assert.Null(series.DecimationMode);
        Assert.Equal(values.Length, series.Points.Count);
        Assert.True(series.Smooth);
    }
}
