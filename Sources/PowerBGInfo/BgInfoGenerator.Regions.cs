using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ChartForgeX.Primitives;

namespace PowerBGInfo;

public partial class BgInfoGenerator {
    /// <summary>Region slots resolved for one image, keyed by item reference and its collection index.</summary>
    internal sealed class RegionPlacements {
        private readonly Dictionary<object, Dictionary<int, ChartRect>> _slots = new(ReferenceComparer.Instance);

        public void Set(object item, int index, ChartRect slot) {
            if (!_slots.TryGetValue(item, out var occurrences)) _slots[item] = occurrences = new Dictionary<int, ChartRect>();
            occurrences[index] = slot;
        }

        public bool TryGet(object item, out ChartRect slot, int index = 0) {
            slot = default;
            return _slots.TryGetValue(item, out var occurrences) && occurrences.TryGetValue(index, out slot);
        }

        public int Count => _slots.Values.Sum(occurrences => occurrences.Count);

        private sealed class ReferenceComparer : IEqualityComparer<object> {
            public static readonly ReferenceComparer Instance = new();
            public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }

    /// <summary>Resolves every item that asked for a <see cref="BgInfoRegion"/> into a rectangle.</summary>
    internal static RegionPlacements ResolveRegionPlacements(int width, int height, BgInfoConfiguration config, ChartRect textBlock,
        IReadOnlyList<IReadOnlyList<double>>? chartValues = null) {
        var placements = new RegionPlacements();
        var items = new List<(object Item, int Index, BgInfoRegion Region, int Order, int Rank, int Width, int Height)>();
        var rank = 0;
        for (var index = 0; index < config.Charts.Count; index++) {
            var chart = config.Charts[index];
            if (chart.Region != BgInfoRegion.None && (chartValues == null || chartValues[index].Count > 0)) {
                items.Add((chart, index, chart.Region, chart.RegionOrder, rank++, chart.Width, chart.Height));
            }
        }
        for (var index = 0; index < config.Topologies.Count; index++) {
            var topology = config.Topologies[index];
            if (topology.Region != BgInfoRegion.None && topology.Nodes.Count > 0) {
                items.Add((topology, index, topology.Region, topology.RegionOrder, rank++, topology.Width, topology.Height));
            }
        }
        for (var index = 0; index < config.VisualCanvases.Count; index++) {
            var visual = config.VisualCanvases[index];
            if (visual.Region != BgInfoRegion.None && visual.Tiles.Count > 0) {
                var visualHeight = visual.Height > 0 ? visual.Height : BgInfoVisualCanvasRenderer.MeasureRegionHeight(visual, height);
                items.Add((visual, index, visual.Region, visual.RegionOrder, rank++, visual.Width, visualHeight));
            }
        }
        for (var index = 0; index < config.Widgets.Count; index++) {
            var widget = config.Widgets[index];
            if (widget.Region != BgInfoRegion.None) items.Add((widget, index, widget.Region, widget.RegionOrder, rank++, widget.Width, ResolveWidgetHeight(widget, config)));
        }
        if (items.Count == 0) return placements;

        var ordered = items.OrderBy(item => item.Order).ThenBy(item => item.Rank).ToList();
        var requests = ordered.Select(item => new BgInfoLayoutRequest(item.Region, item.Width, item.Height)).ToArray();
        var reserved = textBlock.Width > 0 && textBlock.Height > 0 ? textBlock : (ChartRect?)null;
        var slots = BgInfoLayoutEngine.Resolve(width, height, config.Layout ?? new BgInfoLayout(), reserved, requests);
        for (var i = 0; i < ordered.Count; i++) {
            if (slots[i].Width > 0 && slots[i].Height > 0) placements.Set(ordered[i].Item, ordered[i].Index, slots[i]);
        }
        return placements;
    }

    /// <summary>Unsized tables and lists in a column take their natural height instead of sharing the column.</summary>
    internal static int ResolveWidgetHeight(BgInfoWidget widget, BgInfoConfiguration config) {
        if (widget.Height > 0 || widget.NaturalBodyHeight <= 0) return widget.Height;
        if (widget.Region is BgInfoRegion.Top or BgInfoRegion.Bottom) return 0;
        var header = string.IsNullOrWhiteSpace(widget.Title) && string.IsNullOrWhiteSpace(widget.Note)
            ? 0
            : (widget.TitleFontSize ?? config.FontSize) * 1.35 + Math.Max(6, widget.Padding / 2d);
        return (int)Math.Ceiling(widget.NaturalBodyHeight * widget.Scale + header + widget.Padding * 2);
    }

    private static void RenderWidgets(BgInfoRasterImage image, BgInfoConfiguration config, RegionPlacements placements) {
        for (var index = 0; index < config.Widgets.Count; index++) {
            var widget = config.Widgets[index];
            if (widget == null) continue;
            int width, height;
            ChartPoint position;
            if (placements.TryGet(widget, out var slot, index)) {
                width = (int)slot.Width;
                height = (int)slot.Height;
                position = new ChartPoint(slot.X, slot.Y);
            } else {
                if (widget.Region != BgInfoRegion.None) continue;
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
