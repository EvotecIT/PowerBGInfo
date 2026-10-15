using ChartForgeX.Primitives;
using ChartForgeX.Topology;
using ChartForgeX.Typography;
using static PowerBGInfo.Tests.BgInfoBackgroundTests;

namespace PowerBGInfo.Tests;

public class BgInfoPanelTests
{
    private static readonly ChartColor PanelFill = ChartColor.FromRgb(12, 18, 34);
    private static readonly ChartColor PanelBorder = ChartColor.FromRgb(240, 240, 240);

    [Fact]
    public void ChartPanelRoundsCornersAndDrawsItsBorder()
    {
        var chart = new BgInfoChart {
            Title = string.Empty,
            Kind = BgInfoChartKind.Area,
            Width = 200,
            Height = 120,
            Values = new double[] { 1, 2, 3 },
            ShowLatestValue = false,
            BackgroundColor = PanelFill,
            BorderColor = PanelBorder,
            CornerRadius = 24
        };

        using var rendered = BgInfoChartRenderer.Render(chart, chart.Values, new BgInfoConfiguration());
        var pixels = rendered.ToRgbaImage();

        Assert.Equal(0, Pixel(pixels, 1, 1).A);
        Assert.True(Pixel(pixels, 100, 0).R > 150, "The top edge must carry the border.");
        AssertNear(PanelFill, Pixel(pixels, 12, 60));
    }

    [Fact]
    public void SquareChartPanelKeepsTheLegacySolidFill()
    {
        var chart = new BgInfoChart {
            Title = string.Empty,
            Width = 120,
            Height = 80,
            Values = new double[] { 1, 2 },
            ShowLatestValue = false,
            BackgroundColor = PanelFill
        };

        using var rendered = BgInfoChartRenderer.Render(chart, chart.Values, new BgInfoConfiguration());

        AssertNear(PanelFill, Pixel(rendered.ToRgbaImage(), 0, 0));
    }

    [Fact]
    public void TopologyPanelDrawsTheSharedSurfaceAroundTheDiagram()
    {
        var topology = CreateTopology();
        topology.BackgroundColor = PanelFill;
        topology.BorderColor = PanelBorder;
        topology.CornerRadius = 18;
        topology.Title = "Admin path";

        using var rendered = BgInfoTopologyRenderer.Render(topology, new BgInfoConfiguration());
        var pixels = rendered.ToRgbaImage();

        Assert.Equal(topology.Width, pixels.Width);
        Assert.Equal(topology.Height, pixels.Height);
        Assert.Equal(0, Pixel(pixels, 1, 1).A);
        AssertNear(PanelFill, Pixel(pixels, 8, topology.Height - 8));
    }

    [Fact]
    public void TopologyScaleRendersAtTheRequestedSize()
    {
        var topology = CreateTopology();
        topology.Scale = 2;

        using var rendered = BgInfoTopologyRenderer.Render(topology, new BgInfoConfiguration());

        Assert.Equal(topology.Width, rendered.Width);
        Assert.Equal(topology.Height, rendered.Height);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void TopologyScaleOutsideOneToFourIsRejected(int scale)
    {
        var topology = CreateTopology();
        topology.Scale = scale;

        Assert.Throws<ArgumentOutOfRangeException>(() => BgInfoTopologyRenderer.Render(topology, new BgInfoConfiguration()));
    }

    [Fact]
    public void SectionHeadingsSpanColumnsInsteadOfWideningTheLabelColumn()
    {
        var rows = new[] {
            (IsHeading: true, LabelWidth: 600d, ValueWidth: 0d),
            (IsHeading: false, LabelWidth: 80d, ValueWidth: 200d),
            (IsHeading: false, LabelWidth: 120d, ValueWidth: 150d)
        };

        var (labelColumn, total) = BgInfoGenerator.MeasureTextColumns(rows, 30);

        Assert.Equal(120, labelColumn);
        Assert.Equal(600, total);
    }

    [Fact]
    public void ValueColumnsDefineTheWidthWhenTheyAreWiderThanHeadings()
    {
        var rows = new[] {
            (IsHeading: true, LabelWidth: 100d, ValueWidth: 0d),
            (IsHeading: false, LabelWidth: 80d, ValueWidth: 200d)
        };

        var (labelColumn, total) = BgInfoGenerator.MeasureTextColumns(rows, 30);

        Assert.Equal(80, labelColumn);
        Assert.Equal(310, total);
    }

    [Fact]
    public void PanelSettingsRoundTripThroughJson()
    {
        var path = Path.Combine(Path.GetTempPath(), "bginfo-panel-" + Path.GetRandomFileName() + ".json");
        try
        {
            var configuration = new BgInfoConfiguration();
            configuration.Charts.Add(new BgInfoChart {
                Id = "cpu",
                Values = new double[] { 1 },
                CornerRadius = 18,
                BorderColor = PanelBorder
            });
            var topology = CreateTopology();
            topology.Scale = 2;
            topology.BackgroundColor = PanelFill;
            topology.BorderColor = PanelBorder;
            topology.CornerRadius = 18;
            topology.Padding = 24;
            topology.TitleColor = PanelBorder;
            topology.TitleFontSize = 15;
            topology.TitleFontWeight = 700;
            topology.TitleTextCase = TextCaseTransform.Uppercase;
            configuration.Topologies.Add(topology);

            BgInfoConfigurationJson.Save(configuration, path);
            var loaded = BgInfoConfigurationJson.Load(path);

            var chart = Assert.Single(loaded.Charts);
            Assert.Equal(18, chart.CornerRadius);
            Assert.Equal(PanelBorder.ToHexRgba(), chart.BorderColor!.Value.ToHexRgba());
            var diagram = Assert.Single(loaded.Topologies);
            Assert.Equal(2, diagram.Scale);
            Assert.True(diagram.HasPanel);
            Assert.Equal(PanelFill.ToHexRgba(), diagram.BackgroundColor!.Value.ToHexRgba());
            Assert.Equal(18, diagram.CornerRadius);
            Assert.Equal(24, diagram.Padding);
            Assert.Equal(15f, diagram.TitleFontSize);
            Assert.Equal(700, diagram.TitleFontWeight);
            Assert.Equal(TextCaseTransform.Uppercase, diagram.TitleTextCase);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static BgInfoTopology CreateTopology()
    {
        var topology = new BgInfoTopology { Width = 480, Height = 260, Title = "Path" };
        topology.Nodes.Add(new TopologyNode { Id = "a", Label = "Alpha", Kind = TopologyNodeKind.Server });
        topology.Nodes.Add(new TopologyNode { Id = "b", Label = "Beta", Kind = TopologyNodeKind.Server });
        topology.Edges.Add(new TopologyEdge { Id = "a-b", SourceNodeId = "a", TargetNodeId = "b" });
        return topology;
    }
}
