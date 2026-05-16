using System;
using System.Drawing;
using System.IO;
using ChartForgeX.Topology;
using ImagePlayground.Gdi;
using GdiImage = ImagePlayground.Gdi.Image;

namespace PowerBGInfo;

internal static class BgInfoTopologyRenderer {
    public static GdiImage Render(BgInfoTopology topology, BgInfoConfiguration config) {
        if (topology == null) {
            throw new ArgumentNullException(nameof(topology));
        }
        if (config == null) {
            throw new ArgumentNullException(nameof(config));
        }

        var width = Math.Max(1, topology.Width);
        var height = Math.Max(1, topology.Height);
        var chart = BuildTopology(topology, width, height);
        var options = BuildRenderOptions(topology);

        var image = new GdiImage();
        image.Create(string.Empty, width, height, Color.Transparent);
        DrawPng(image, chart.ToPng(options), 0, 0, width, height);
        return image;
    }

    private static TopologyChart BuildTopology(BgInfoTopology topology, int width, int height) {
        var chart = TopologyChart.Create()
            .WithViewport(width, height, 18)
            .WithLayout(topology.Layout, topology.Direction)
            .WithTheme(CreateTheme(topology));

        if (!string.IsNullOrWhiteSpace(topology.Title)) {
            chart.Title = topology.Title;
        }
        if (!string.IsNullOrWhiteSpace(topology.Subtitle)) {
            chart.Subtitle = topology.Subtitle;
        }

        chart.Groups.AddRange(topology.Groups);
        chart.Nodes.AddRange(topology.Nodes);
        chart.Edges.AddRange(topology.Edges);
        return chart;
    }

    private static TopologyRenderOptions BuildRenderOptions(BgInfoTopology topology) {
        return new TopologyRenderOptions {
            IncludeTitle = topology.ShowTitle,
            IncludeLegend = topology.ShowLegend,
            IncludeGroups = topology.ShowGroups,
            IncludeEdgeLabels = topology.ShowEdgeLabels,
            IncludeStatusBadges = topology.ShowStatusBadges,
            FitContentToViewport = topology.FitContentToViewport,
            NodeDisplayMode = topology.NodeDisplayMode,
            VisualStyle = topology.VisualStyle,
            CanvasSurfaceStyle = topology.Transparent ? TopologyCanvasSurfaceStyle.Plain : topology.CanvasSurfaceStyle,
            IncludeGroupStatusDots = true,
            EdgeCornerStyle = TopologyEdgeCornerStyle.Rounded,
            EdgeCornerRadius = 10
        };
    }

    private static TopologyTheme CreateTheme(BgInfoTopology topology) {
        var theme = topology.Theme.Equals("Light", StringComparison.OrdinalIgnoreCase)
            ? TopologyTheme.Light()
            : TopologyTheme.Dark();

        if (topology.Transparent) {
            theme.Background = topology.Theme.Equals("Light", StringComparison.OrdinalIgnoreCase) ? "#FFFFFF00" : "#0B112000";
        }

        return theme;
    }

    private static void DrawPng(GdiImage image, byte[] png, float x, float y, float width, float height) {
        image.WithGraphics(graphics => {
            using var stream = new MemoryStream(png);
            using var bitmap = new Bitmap(stream);
            graphics.DrawImage(bitmap, x, y, width, height);
        });
    }
}
