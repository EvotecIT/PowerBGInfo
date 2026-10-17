using System;
using Color = ChartForgeX.Primitives.ChartColor;
using ChartForgeX.Topology;
using ChartForgeX.Themes;

namespace PowerBGInfo;

internal static class BgInfoTopologyRenderer {
    public static BgInfoRasterImage Render(BgInfoTopology topology, BgInfoConfiguration config) {
        if (topology == null) throw new ArgumentNullException(nameof(topology));
        return Render(topology, config, topology.Width, topology.Height);
    }

    /// <summary>Renders the diagram at a resolved size, for example a layout region slot.</summary>
    public static BgInfoRasterImage Render(BgInfoTopology topology, BgInfoConfiguration config, int requestedWidth, int requestedHeight) {
        if (topology == null) {
            throw new ArgumentNullException(nameof(topology));
        }
        if (config == null) {
            throw new ArgumentNullException(nameof(config));
        }

        var width = Math.Max(1, requestedWidth);
        var height = Math.Max(1, requestedHeight);
        var scale = BgInfoTopology.ValidateScale(topology.Scale);
        var image = BgInfoPanel.Create(width, height, topology.BackgroundColor, topology.BorderColor, topology.CornerRadius);

        double left = 0, top = 0, diagramWidth = width, diagramHeight = height;
        if (topology.HasPanel) {
            var padding = Math.Max(0, topology.Padding);
            left = padding;
            top = padding;
            diagramWidth = Math.Max(1, width - padding * 2d);
            if (topology.ShowTitle) {
                var header = BgInfoPanel.DrawHeader(image, topology.Title, topology.Subtitle,
                    CreateHeaderStyle(topology, config), left, top, diagramWidth);
                if (header > 0) top += header + 8;
            }
            diagramHeight = Math.Max(1, height - top - padding);
        }

        var diagramPixelWidth = Math.Max(1, (int)Math.Round(diagramWidth));
        var diagramPixelHeight = Math.Max(1, (int)Math.Round(diagramHeight));
        var chart = BuildTopology(topology, Math.Max(1, diagramPixelWidth / scale), Math.Max(1, diagramPixelHeight / scale));
        var options = BuildRenderOptions(topology);
        options.PngOutputScale = scale;

        image.DrawImage(chart.ToRgbaImage(options), left, top, diagramWidth, diagramHeight);
        return image;
    }

    private static TopologyChart BuildTopology(BgInfoTopology topology, int width, int height) {
        var chart = TopologyChart.Create()
            .WithViewport(width, height, 18)
            .WithLayout(topology.Layout, topology.Direction)
            .WithTheme(CreateTheme(topology), ResolveThemeMode(topology));

        // A panel draws its own header, so the diagram keeps the full remaining area.
        if (!topology.HasPanel && !string.IsNullOrWhiteSpace(topology.Title)) {
            chart.Title = topology.Title;
        }
        if (!topology.HasPanel && !string.IsNullOrWhiteSpace(topology.Subtitle)) {
            chart.Subtitle = topology.Subtitle;
        }

        chart.Groups.AddRange(topology.Groups);
        chart.Nodes.AddRange(topology.Nodes);
        chart.Edges.AddRange(topology.Edges);
        return chart;
    }

    private static TopologyRenderOptions BuildRenderOptions(BgInfoTopology topology) {
        return new TopologyRenderOptions {
            IncludeTitle = topology.ShowTitle && !topology.HasPanel,
            IncludeLegend = topology.ShowLegend,
            IncludeGroups = topology.ShowGroups,
            IncludeEdgeLabels = topology.ShowEdgeLabels,
            IncludeStatusBadges = topology.ShowStatusBadges,
            FitContentToViewport = topology.FitContentToViewport,
            NodeDisplayMode = topology.NodeDisplayMode,
            VisualStyle = topology.VisualStyle,
            CanvasSurfaceStyle = topology.Transparent || topology.HasPanel ? TopologyCanvasSurfaceStyle.Plain : topology.CanvasSurfaceStyle,
            IncludeGroupStatusDots = true,
            EdgeCornerStyle = TopologyEdgeCornerStyle.Rounded,
            EdgeCornerRadius = 10
        };
    }

    private static ChartForgeX.Typography.TextStyle CreateHeaderStyle(BgInfoTopology topology, BgInfoConfiguration config) {
        var tokens = ResolveThemeMode(topology) == VisualThemeMode.Dark ? VisualDesignTokens.GraphiteDark() : VisualDesignTokens.GraphiteLight();
        return BgInfoRasterImage.CreateTextStyle(
            topology.TitleFontSize ?? config.FontSize,
            config.CustomFontFamilyName ?? VisualTheme.Graphite().Typography.Family,
            topology.TitleColor ?? config.CustomColor ?? tokens.MutedForeground,
            topology.TitleFontWeight ?? 600,
            false,
            ChartForgeX.Typography.TextDecorationStyle.None,
            ChartForgeX.Typography.TextDecorationStyle.None,
            ChartForgeX.Typography.TextBaseline.Normal,
            topology.TitleTextCase ?? ChartForgeX.Typography.TextCaseTransform.None);
    }

    private static VisualThemeMode ResolveThemeMode(BgInfoTopology topology) =>
        string.Equals(topology.Theme, "Light", StringComparison.OrdinalIgnoreCase) ? VisualThemeMode.Light : VisualThemeMode.Dark;

    // Transparent overlays and panels both let the wallpaper or panel show through the diagram canvas.
    private static VisualTheme CreateTheme(BgInfoTopology topology) {
        var theme = VisualTheme.Graphite();
        if (!string.IsNullOrWhiteSpace(topology.FontFamilyName)) theme = theme.WithFontFamily(topology.FontFamilyName!);
        return theme.Configure(builder => {
            var colors = builder.Colors(ResolveThemeMode(topology));
            if (topology.Accent.HasValue) colors.Accent = topology.Accent.Value;
            if (topology.Transparent || topology.HasPanel) colors.Background = Color.Transparent;
        });
    }

}
