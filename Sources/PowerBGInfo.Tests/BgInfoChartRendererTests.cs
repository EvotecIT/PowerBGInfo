using ChartForgeX.Core;
using ChartForgeX.Primitives;
using ChartForgeX.Typography;

namespace PowerBGInfo.Tests;

public class BgInfoChartRendererTests
{
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
