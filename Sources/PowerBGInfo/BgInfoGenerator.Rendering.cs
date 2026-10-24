using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ChartForgeX.Primitives;
using ChartForgeX.Typography;
using DesktopManager;

namespace PowerBGInfo;

/// <summary>
/// Generates BGInfo overlays and applies them to the configured target.
/// </summary>
public partial class BgInfoGenerator
{
    private static void RenderCharts(BgInfoRasterImage image, BgInfoConfiguration config, ChartRect textBlock, RegionPlacements placements, IReadOnlyList<IReadOnlyList<double>> chartValues)
    {
        if (config.Charts.Count == 0)
        {
            return;
        }

        for (int i = 0; i < config.Charts.Count; i++)
        {
            var chart = config.Charts[i];
            if (!placements.TryGet(chart, out var slot, i))
            {
                continue;
            }

            var values = chartValues[i];
            if (values.Count == 0)
            {
                continue;
            }

            using var regionImage = BgInfoChartRenderer.Render(chart, values, config, (int)slot.Width, (int)slot.Height);
            image.DrawImage(regionImage, slot.X, slot.Y);
        }

        if (config.ChartLayout == BgInfoChartLayoutMode.Stack)
        {
            RenderStackedCharts(image, config, textBlock, chartValues);
            return;
        }

        for (int i = 0; i < config.Charts.Count; i++)
        {
            var chart = config.Charts[i];
            if (chart.Region != BgInfoRegion.None)
            {
                continue;
            }

            var values = chartValues[i];
            if (values.Count == 0)
            {
                continue;
            }

            using var chartImage = BgInfoChartRenderer.Render(chart, values, config);
            var position = ResolveChartPosition(image, chart);
            image.DrawImage(chartImage, position.X, position.Y);
        }
    }

    private static void RenderTopologies(BgInfoRasterImage image, BgInfoConfiguration config, RegionPlacements placements)
    {
        if (config.Topologies.Count == 0)
        {
            return;
        }

        for (int i = 0; i < config.Topologies.Count; i++)
        {
            var topology = config.Topologies[i];
            if (topology.Nodes.Count == 0)
            {
                continue;
            }

            if (placements.TryGet(topology, out var slot, i))
            {
                using var regionImage = BgInfoTopologyRenderer.Render(topology, config, (int)slot.Width, (int)slot.Height);
                image.DrawImage(regionImage, slot.X, slot.Y);
                continue;
            }

            if (topology.Region != BgInfoRegion.None)
            {
                continue;
            }

            using var topologyImage = BgInfoTopologyRenderer.Render(topology, config);
            var position = ResolveTopologyPosition(image, topology);
            image.DrawImage(topologyImage, position.X, position.Y);
        }
    }

    private static void RenderVisualCanvases(BgInfoRasterImage image, BgInfoConfiguration config, RegionPlacements placements)
    {
        if (config.VisualCanvases.Count == 0)
        {
            return;
        }

        for (int i = 0; i < config.VisualCanvases.Count; i++)
        {
            var visual = config.VisualCanvases[i];
            if (placements.TryGet(visual, out var slot, i))
            {
                using var regionImage = BgInfoVisualCanvasRenderer.RenderRegion(visual, (int)slot.Width, (int)slot.Height, image.Height);
                image.DrawImage(regionImage, slot.X, slot.Y);
                continue;
            }

            if (visual.Region != BgInfoRegion.None)
            {
                continue;
            }

            using var visualImage = BgInfoVisualCanvasRenderer.Render(visual, config, image.Width, image.Height);
            image.DrawImage(visualImage, visual.PositionX, visual.PositionY);
        }
    }

    private static void RenderImages(BgInfoRasterImage image, BgInfoConfiguration config)
    {
        if (config.Images.Count == 0)
        {
            return;
        }

        foreach (var overlay in config.Images)
        {
            if (overlay == null || string.IsNullOrWhiteSpace(overlay.Path))
            {
                continue;
            }
            if (!File.Exists(overlay.Path))
            {
                throw new FileNotFoundException("BGInfo image overlay file was not found.", overlay.Path);
            }

            using var overlayImage = BgInfoRasterImage.Load(overlay.Path);
            var (width, height) = ResolveImageSize(overlay, overlayImage.Width, overlayImage.Height);
            var position = ResolveImagePosition(image, overlay, width, height);
            image.DrawImage(overlayImage.ToRgbaImage(), position.X, position.Y, width, height, overlay.Opacity, BgInfoImageFitMapper.ToVisualCanvasFit(overlay.Fit));
        }
    }

    private static void RenderStackedCharts(BgInfoRasterImage image, BgInfoConfiguration config, ChartRect textBlock, IReadOnlyList<IReadOnlyList<double>> chartValues)
    {
        var area = ResolveChartStackArea(image, config, textBlock);
        if (area.Width <= 0 || area.Height <= 0)
        {
            area = new ChartRect(0, 0, image.Width, image.Height);
        }

        int offsetX = config.ChartStackOffsetX;
        int offsetY = config.ChartStackOffsetY;
        double spacing = Math.Max(0, config.ChartStackSpacing);
        bool outside = config.ChartStackAlignToTextBlock && config.ChartStackOutsideTextBlock && textBlock.Width > 0 && textBlock.Height > 0;
        double? cursorX = null;
        double? cursorY = null;

        for (int i = 0; i < config.Charts.Count; i++)
        {
            var chart = config.Charts[i];
            if (chart.Region != BgInfoRegion.None)
            {
                continue;
            }

            var values = chartValues[i];
            if (values.Count == 0)
            {
                continue;
            }

            int width = Math.Max(1, chart.Width);
            int height = Math.Max(1, chart.Height);
            var basePosition = outside
                ? ResolveChartPositionOutsideTextBlock(textBlock, width, height, config.ChartStackAnchor, offsetX, offsetY)
                : ResolveChartPosition(area, width, height, config.ChartStackAnchor, offsetX, offsetY);

            double positionX = cursorX ?? basePosition.X;
            double positionY = cursorY ?? basePosition.Y;
            if (config.ChartStackDirection == BgInfoChartStackDirection.Vertical)
            {
                var direction = ResolveStackDirection(config.ChartStackAnchor, vertical: true);
                if (outside) direction *= -1;
                positionX = basePosition.X;
                cursorY = positionY + (height + spacing) * direction;
            }
            else
            {
                var direction = ResolveStackDirection(config.ChartStackAnchor, vertical: false);
                if (outside) direction *= -1;
                positionY = basePosition.Y;
                cursorX = positionX + (width + spacing) * direction;
            }

            var position = new ChartPoint(positionX, positionY);
            using var chartImage = BgInfoChartRenderer.Render(chart, values, config);
            image.DrawImage(chartImage, position.X, position.Y);
        }
    }

    private static ChartRect ResolveChartStackArea(BgInfoRasterImage image, BgInfoConfiguration config, ChartRect textBlock)
    {
        if (!config.ChartStackAlignToTextBlock)
        {
            return new ChartRect(0, 0, image.Width, image.Height);
        }

        if (textBlock.Width <= 0 || textBlock.Height <= 0)
        {
            return new ChartRect(0, 0, image.Width, image.Height);
        }

        return textBlock;
    }

    private static ChartPoint ResolveChartPositionOutsideTextBlock(ChartRect textBlock, int chartWidth, int chartHeight, BgInfoTextPosition anchor, int offsetX, int offsetY)
    {
        double x;
        double y;
        switch (anchor)
        {
            case BgInfoTextPosition.TopLeft:
                x = textBlock.X + offsetX;
                y = textBlock.Y - chartHeight - offsetY;
                break;
            case BgInfoTextPosition.TopCenter:
                x = textBlock.X + (textBlock.Width - chartWidth) / 2f + offsetX;
                y = textBlock.Y - chartHeight - offsetY;
                break;
            case BgInfoTextPosition.TopRight:
                x = textBlock.X + textBlock.Width - chartWidth - offsetX;
                y = textBlock.Y - chartHeight - offsetY;
                break;
            case BgInfoTextPosition.MiddleLeft:
                x = textBlock.X - chartWidth - offsetX;
                y = textBlock.Y + (textBlock.Height - chartHeight) / 2f + offsetY;
                break;
            case BgInfoTextPosition.MiddleCenter:
                x = textBlock.X + (textBlock.Width - chartWidth) / 2f + offsetX;
                y = textBlock.Y + textBlock.Height + offsetY;
                break;
            case BgInfoTextPosition.MiddleRight:
                x = textBlock.X + textBlock.Width + offsetX;
                y = textBlock.Y + (textBlock.Height - chartHeight) / 2f + offsetY;
                break;
            case BgInfoTextPosition.BottomLeft:
                x = textBlock.X + offsetX;
                y = textBlock.Y + textBlock.Height + offsetY;
                break;
            case BgInfoTextPosition.BottomCenter:
                x = textBlock.X + (textBlock.Width - chartWidth) / 2f + offsetX;
                y = textBlock.Y + textBlock.Height + offsetY;
                break;
            case BgInfoTextPosition.BottomRight:
                x = textBlock.X + textBlock.Width - chartWidth - offsetX;
                y = textBlock.Y + textBlock.Height + offsetY;
                break;
            default:
                x = textBlock.X + offsetX;
                y = textBlock.Y + textBlock.Height + offsetY;
                break;
        }

        return new ChartPoint(x, y);
    }

    private static int ResolveStackDirection(BgInfoTextPosition anchor, bool vertical)
    {
        if (vertical)
        {
            return anchor switch
            {
                BgInfoTextPosition.BottomLeft => -1,
                BgInfoTextPosition.BottomCenter => -1,
                BgInfoTextPosition.BottomRight => -1,
                _ => 1
            };
        }

        return anchor switch
        {
            BgInfoTextPosition.TopRight => -1,
            BgInfoTextPosition.MiddleRight => -1,
            BgInfoTextPosition.BottomRight => -1,
            _ => 1
        };
    }

    private static ChartPoint ResolveChartPosition(BgInfoRasterImage image, BgInfoChart chart)
    {
        if (chart.PositionX.HasValue && chart.PositionY.HasValue)
        {
            return new ChartPoint(chart.PositionX.Value, chart.PositionY.Value);
        }

        double chartWidth = Math.Max(1, chart.Width);
        double chartHeight = Math.Max(1, chart.Height);
        return ResolveChartPosition(new ChartRect(0, 0, image.Width, image.Height),
            (int)chartWidth, (int)chartHeight, chart.Anchor, chart.OffsetX, chart.OffsetY);
    }

    private static ChartPoint ResolveTopologyPosition(BgInfoRasterImage image, BgInfoTopology topology)
    {
        if (topology.PositionX.HasValue && topology.PositionY.HasValue)
        {
            return new ChartPoint(topology.PositionX.Value, topology.PositionY.Value);
        }

        return ResolveChartPosition(new ChartRect(0, 0, image.Width, image.Height),
            Math.Max(1, topology.Width), Math.Max(1, topology.Height), topology.Anchor, topology.OffsetX, topology.OffsetY);
    }

    private static ChartPoint ResolveImagePosition(BgInfoRasterImage image, BgInfoImage overlay, int width, int height)
    {
        var anchored = ResolveChartPosition(new ChartRect(0, 0, image.Width, image.Height),
            width, height, overlay.Anchor, overlay.OffsetX, overlay.OffsetY);
        return new ChartPoint(overlay.PositionX ?? anchored.X, overlay.PositionY ?? anchored.Y);
    }

    private static (int Width, int Height) ResolveImageSize(BgInfoImage overlay, int sourceWidth, int sourceHeight)
    {
        var width = overlay.Width;
        var height = overlay.Height;
        if (width <= 0 && height <= 0)
        {
            return (Math.Max(1, sourceWidth), Math.Max(1, sourceHeight));
        }
        if (width > 0 && height <= 0)
        {
            height = (int)Math.Round(sourceHeight * (width / (double)Math.Max(1, sourceWidth)));
        }
        if (height > 0 && width <= 0)
        {
            width = (int)Math.Round(sourceWidth * (height / (double)Math.Max(1, sourceHeight)));
        }

        return (Math.Max(1, width), Math.Max(1, height));
    }

    internal static ChartPoint ResolveChartPosition(ChartRect area, int chartWidth, int chartHeight, BgInfoTextPosition anchor, int offsetX, int offsetY)
    {
        double x = area.X + offsetX;
        double y = area.Y + offsetY;
        switch (anchor)
        {
            case BgInfoTextPosition.TopCenter:
                x = area.X + (area.Width - chartWidth) / 2f + offsetX;
                break;
            case BgInfoTextPosition.TopRight:
                x = area.X + area.Width - chartWidth - offsetX;
                break;
            case BgInfoTextPosition.MiddleLeft:
                y = area.Y + (area.Height - chartHeight) / 2f + offsetY;
                break;
            case BgInfoTextPosition.MiddleCenter:
                x = area.X + (area.Width - chartWidth) / 2f + offsetX;
                y = area.Y + (area.Height - chartHeight) / 2f + offsetY;
                break;
            case BgInfoTextPosition.MiddleRight:
                x = area.X + area.Width - chartWidth - offsetX;
                y = area.Y + (area.Height - chartHeight) / 2f + offsetY;
                break;
            case BgInfoTextPosition.BottomLeft:
                y = area.Y + area.Height - chartHeight - offsetY;
                break;
            case BgInfoTextPosition.BottomCenter:
                x = area.X + (area.Width - chartWidth) / 2f + offsetX;
                y = area.Y + area.Height - chartHeight - offsetY;
                break;
            case BgInfoTextPosition.BottomRight:
                x = area.X + area.Width - chartWidth - offsetX;
                y = area.Y + area.Height - chartHeight - offsetY;
                break;
            case BgInfoTextPosition.TopLeft:
            default:
                break;
        }

        return new ChartPoint(x, y);
    }

}
