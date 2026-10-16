using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ChartForgeX.Primitives;

namespace PowerBGInfo;

public partial class BgInfoGenerator {
    /// <summary>Region slots resolved for one image, keyed by item reference.</summary>
    internal sealed class RegionPlacements {
        private readonly Dictionary<object, ChartRect> _slots = new(ReferenceComparer.Instance);

        public void Set(object item, ChartRect slot) => _slots[item] = slot;

        public bool TryGet(object item, out ChartRect slot) => _slots.TryGetValue(item, out slot);

        public int Count => _slots.Count;

        private sealed class ReferenceComparer : IEqualityComparer<object> {
            public static readonly ReferenceComparer Instance = new();
            public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }

    /// <summary>Resolves every item that asked for a <see cref="BgInfoRegion"/> into a rectangle.</summary>
    internal static RegionPlacements ResolveRegionPlacements(int width, int height, BgInfoConfiguration config, ChartRect textBlock) {
        var placements = new RegionPlacements();
        var items = new List<(object Item, BgInfoRegion Region, int Order, int Rank, int Width, int Height)>();
        var rank = 0;
        foreach (var chart in config.Charts) {
            if (chart.Region != BgInfoRegion.None) items.Add((chart, chart.Region, chart.RegionOrder, rank++, chart.Width, chart.Height));
        }
        foreach (var topology in config.Topologies) {
            if (topology.Region != BgInfoRegion.None) items.Add((topology, topology.Region, topology.RegionOrder, rank++, topology.Width, topology.Height));
        }
        foreach (var visual in config.VisualCanvases) {
            if (visual.Region != BgInfoRegion.None) {
                items.Add((visual, visual.Region, visual.RegionOrder, rank++, visual.Width, BgInfoVisualCanvasRenderer.MeasureRegionHeight(visual, height)));
            }
        }
        foreach (var widget in config.Widgets) {
            if (widget.Region != BgInfoRegion.None) items.Add((widget, widget.Region, widget.RegionOrder, rank++, widget.Width, widget.Height));
        }
        if (items.Count == 0) return placements;

        var ordered = items.OrderBy(item => item.Order).ThenBy(item => item.Rank).ToList();
        var requests = ordered.Select(item => new BgInfoLayoutRequest(item.Region, item.Width, item.Height)).ToArray();
        var reserved = textBlock.Width > 0 && textBlock.Height > 0 ? textBlock : (ChartRect?)null;
        var slots = BgInfoLayoutEngine.Resolve(width, height, config.Layout ?? new BgInfoLayout(), reserved, requests);
        for (var i = 0; i < ordered.Count; i++) placements.Set(ordered[i].Item, slots[i]);
        return placements;
    }

    private static void RenderWidgets(BgInfoRasterImage image, BgInfoConfiguration config, RegionPlacements placements) {
        foreach (var widget in config.Widgets) {
            if (widget == null) continue;
            int width, height;
            ChartPoint position;
            if (placements.TryGet(widget, out var slot)) {
                width = (int)slot.Width;
                height = (int)slot.Height;
                position = new ChartPoint(slot.X, slot.Y);
            } else {
                width = Math.Max(1, widget.Width);
                height = Math.Max(1, widget.Height);
                position = widget.PositionX.HasValue && widget.PositionY.HasValue
                    ? new ChartPoint(widget.PositionX.Value, widget.PositionY.Value)
                    : ResolveChartPosition(new ChartRect(0, 0, image.Width, image.Height), width, height, widget.Anchor, widget.OffsetX, widget.OffsetY);
            }

            using var panel = BgInfoWidgetRenderer.Render(widget, config, width, height);
            image.DrawImage(panel, position.X, position.Y);
        }
    }
}
