using ChartForgeX.Core;

namespace PowerBGInfo.Tests;

public class BgInfoChartRendererTests
{
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
        Assert.True(series.Points.Count <= BgInfoChartRenderer.ResolveTrendPointBudget(chart.Width));
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

    [Theory]
    [InlineData(-10, 64)]
    [InlineData(1, 64)]
    [InlineData(240, 480)]
    [InlineData(int.MaxValue, int.MaxValue)]
    public void TrendPointBudgetIsBoundedAndWidthAware(int width, int expected)
    {
        Assert.Equal(expected, BgInfoChartRenderer.ResolveTrendPointBudget(width));
    }
}
