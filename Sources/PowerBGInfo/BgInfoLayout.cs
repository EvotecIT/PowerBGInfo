using System;
using System.Collections.Generic;
using System.Linq;
using ChartForgeX.Primitives;

namespace PowerBGInfo;

/// <summary>Named screen regions that panels flow into.</summary>
public enum BgInfoRegion {
    /// <summary>No region; the item uses its anchor, offsets or explicit position.</summary>
    None,
    /// <summary>Left column between the top and bottom bands. Items stack from the top.</summary>
    Left,
    /// <summary>Right column between the top and bottom bands. Items stack from the top.</summary>
    Right,
    /// <summary>Full-width band at the top of the safe area. Items flow left to right.</summary>
    Top,
    /// <summary>Full-width band at the bottom of the safe area. Items flow left to right.</summary>
    Bottom,
    /// <summary>Space between the columns and the bands. Items stack and are centered.</summary>
    Center
}

/// <summary>Where a stack sits inside a column when its panels do not fill it.</summary>
public enum BgInfoColumnAlignment {
    /// <summary>Stack from the top.</summary>
    Top,
    /// <summary>Center the stack vertically.</summary>
    Center,
    /// <summary>Stack against the bottom.</summary>
    Bottom
}

/// <summary>Safe-area margins and region sizes for <see cref="BgInfoRegion"/> placement.</summary>
public sealed class BgInfoLayout {
    /// <summary>Distance from the left and right screen edges.</summary>
    public int MarginX { get; set; } = 64;
    /// <summary>Distance from the top and bottom screen edges.</summary>
    public int MarginY { get; set; } = 56;
    /// <summary>Gap between panels and between regions.</summary>
    public int Gap { get; set; } = 24;
    /// <summary>Column width. Zero uses 24 % of the width, clamped to 340-620 pixels.</summary>
    public int ColumnWidth { get; set; }
    /// <summary>Default band height for items without a height. Zero uses 26 % of the height, clamped to 200-340 pixels.</summary>
    public int BandHeight { get; set; }
    /// <summary>Height kept free for the taskbar. A negative value estimates it from the image height (48 px at 1080, 64 px at 1440).</summary>
    public int TaskbarReserve { get; set; } = -1;
    /// <summary>Width kept free on the left for desktop icons. It also moves a left-aligned text block.</summary>
    public int IconAreaWidth { get; set; }
    /// <summary>Largest height an unsized panel takes when it shares a column. Zero uses 36 % of the image height, which keeps panels readable on portrait screens.</summary>
    public int MaximumPanelHeight { get; set; }
    /// <summary>Where Left and Right stacks sit when their panels do not fill the column. Center stacks are always centered.</summary>
    public BgInfoColumnAlignment ColumnAlignment { get; set; }
}

/// <summary>One item that asks for a place in a region.</summary>
public readonly struct BgInfoLayoutRequest {
    /// <summary>Creates a request. A zero width or height shares the remaining space of the region.</summary>
    public BgInfoLayoutRequest(BgInfoRegion region, int width, int height) {
        Region = region;
        Width = Math.Max(0, width);
        Height = Math.Max(0, height);
    }

    /// <summary>The requested region.</summary>
    public BgInfoRegion Region { get; }
    /// <summary>Requested width, or zero to fill.</summary>
    public int Width { get; }
    /// <summary>Requested height, or zero to fill.</summary>
    public int Height { get; }
}

/// <summary>Resolves region requests into rectangles for one image.</summary>
public static class BgInfoLayoutEngine {
    /// <summary>Returns the safe area: the image minus margins, the taskbar reserve and the icon area.</summary>
    public static ChartRect GetSafeArea(int width, int height, BgInfoLayout layout) {
        if (layout == null) throw new ArgumentNullException(nameof(layout));
        width = Math.Max(1, width);
        height = Math.Max(1, height);
        var taskbar = layout.TaskbarReserve >= 0 ? layout.TaskbarReserve : (int)Math.Round(height * 0.0444);
        var left = (int)Math.Min(width - 1L, (long)Math.Max(0, layout.MarginX) + Math.Max(0, layout.IconAreaWidth));
        var top = Math.Min(height - 1, Math.Max(0, layout.MarginY));
        var right = Math.Min(width - left - 1, Math.Max(0, layout.MarginX));
        var bottom = (int)Math.Min(height - top - 1L, Math.Max((long)Math.Max(0, layout.MarginY), taskbar > 0 ? (long)taskbar + 16 : 0));
        return new ChartRect(left, top, Math.Max(1, width - left - right), Math.Max(1, height - top - bottom));
    }

    /// <summary>Resolves every request into a rectangle, in request order.</summary>
    /// <param name="width">Image width.</param>
    /// <param name="height">Image height.</param>
    /// <param name="layout">Margins and region sizes.</param>
    /// <param name="reserved">Optional rectangle, such as the text block, that regions flow around.</param>
    /// <param name="requests">Items in emission order.</param>
    public static ChartRect[] Resolve(int width, int height, BgInfoLayout layout, ChartRect? reserved, IReadOnlyList<BgInfoLayoutRequest> requests) {
        if (layout == null) throw new ArgumentNullException(nameof(layout));
        if (requests == null) throw new ArgumentNullException(nameof(requests));
        var result = new ChartRect[requests.Count];
        var safe = GetSafeArea(width, height, layout);
        var gap = Math.Max(0, layout.Gap);
        var text = reserved is { Width: > 0, Height: > 0 } r ? r : (ChartRect?)null;

        int[] Indexes(BgInfoRegion region) => Enumerable.Range(0, requests.Count).Where(i => requests[i].Region == region).ToArray();
        var top = Indexes(BgInfoRegion.Top);
        var bottom = Indexes(BgInfoRegion.Bottom);
        var left = Indexes(BgInfoRegion.Left);
        var right = Indexes(BgInfoRegion.Right);
        var center = Indexes(BgInfoRegion.Center);

        var autoBand = layout.BandHeight > 0 ? layout.BandHeight : Clamp(height * 0.26, 200, 340);
        var topHeight = BandHeight(requests, top, autoBand, safe.Height);
        var bottomHeight = BandHeight(requests, bottom, autoBand, safe.Height);
        var hasMiddleRegions = left.Length > 0 || right.Length > 0 || center.Length > 0;
        var separatorCount = hasMiddleRegions
            ? (top.Length > 0 ? 1 : 0) + (bottom.Length > 0 ? 1 : 0)
            : top.Length > 0 && bottom.Length > 0 ? 1 : 0;
        var bandTotal = topHeight + bottomHeight;
        var verticalRegionCount = (top.Length > 0 ? 1 : 0) + (bottom.Length > 0 ? 1 : 0) + (hasMiddleRegions ? 1 : 0);
        if (safe.Height < verticalRegionCount) return result;
        var middleReserve = hasMiddleRegions ? Math.Min(120, safe.Height) : 0;
        var bandReserve = Math.Min(120, topHeight) + Math.Min(120, bottomHeight);
        var bandGap = separatorCount > 0
            ? Math.Min(gap, Math.Max(0, safe.Height - bandReserve - middleReserve) / separatorCount)
            : 0;
        var bandBudget = Math.Max(0, safe.Height - bandGap * separatorCount - middleReserve);
        if (bandTotal > bandBudget && bandTotal > 0) {
            var bandScale = bandBudget / bandTotal;
            topHeight *= bandScale;
            bottomHeight *= bandScale;
        }
        var safeRight = safe.X + safe.Width;
        var safeBottom = safe.Y + safe.Height;

        if (top.Length > 0) {
            var span = BandSpan(safe.X, safeRight, safe.Y, topHeight, text, gap);
            if (span is { } available) FlowRow(requests, top, result, available.Start, available.End, safe.Y, topHeight, gap, alignBottom: false);
        }
        if (bottom.Length > 0) {
            var y = safeBottom - bottomHeight;
            var span = BandSpan(safe.X, safeRight, y, bottomHeight, text, gap);
            if (span is { } available) FlowRow(requests, bottom, result, available.Start, available.End, y, bottomHeight, gap, alignBottom: true);
        }

        var columnTop = Math.Min(safeBottom, Math.Max(safe.Y, safe.Y + (top.Length > 0 ? topHeight + bandGap : 0)));
        var columnBottom = Math.Min(safeBottom, Math.Max(safe.Y, safeBottom - (bottom.Length > 0 ? bottomHeight + bandGap : 0)));
        var columnWidth = layout.ColumnWidth > 0 ? layout.ColumnWidth : Clamp(width * 0.24, 340, 620);
        var sideColumns = (left.Length > 0 ? 1 : 0) + (right.Length > 0 ? 1 : 0);
        var columnGapCount = center.Length > 0 ? sideColumns : Math.Max(0, sideColumns - 1);
        var centerReserve = center.Length > 0 && sideColumns > 0 ? Math.Min(120, safe.Width) : 0;
        var columnGap = columnGapCount > 0
            ? Math.Min(gap, Math.Max(0, (safe.Width - centerReserve - sideColumns) / columnGapCount))
            : 0;
        var columnGaps = columnGapCount * columnGap;
        var columnLimit = sideColumns > 0
            ? Math.Max(1, (safe.Width - centerReserve - columnGaps) / sideColumns)
            : safe.Width;
        columnWidth = Math.Max(1, Math.Min(columnWidth, columnLimit));

        var leftX = safe.X;
        var rightX = safeRight - columnWidth;
        var maximumShare = layout.MaximumPanelHeight > 0 ? layout.MaximumPanelHeight : height * 0.36;
        if (left.Length > 0) FlowColumn(requests, left, result, leftX, columnWidth, columnTop, columnBottom, text, gap, CrossAlign.Start, layout.ColumnAlignment, maximumShare);
        if (right.Length > 0) FlowColumn(requests, right, result, rightX, columnWidth, columnTop, columnBottom, text, gap, CrossAlign.End, layout.ColumnAlignment, maximumShare);
        if (center.Length > 0) {
            var centerLeft = left.Length > 0 ? leftX + columnWidth + columnGap : safe.X;
            var centerRight = right.Length > 0 ? rightX - columnGap : safeRight;
            FlowColumn(requests, center, result, centerLeft, Math.Max(1, centerRight - centerLeft), columnTop, columnBottom, text, gap, CrossAlign.Middle, BgInfoColumnAlignment.Center, maximumShare);
        }

        return result;
    }

    private enum CrossAlign { Start, Middle, End }

    private static double BandHeight(IReadOnlyList<BgInfoLayoutRequest> requests, int[] indexes, double automatic, double limit) {
        if (indexes.Length == 0) return 0;
        var height = indexes.Max(i => requests[i].Height > 0 ? requests[i].Height : automatic);
        return Math.Min(height, limit);
    }

    private static (double Start, double End)? BandSpan(double start, double end, double y, double height, ChartRect? text, double gap) {
        if (text is not { } block || !Overlaps(y, y + height, block.Y, block.Y + block.Height)
            || !Overlaps(start, end, block.X, block.X + block.Width)) return (start, end);
        var before = block.X - gap - start;
        var after = end - (block.X + block.Width + gap);
        if (before <= 0 && after <= 0) return null;
        return after >= before ? (block.X + block.Width + gap, end) : (start, block.X - gap);
    }

    private static void FlowRow(IReadOnlyList<BgInfoLayoutRequest> requests, int[] indexes, ChartRect[] result, double start, double end, double y, double height, double gap, bool alignBottom) {
        if (height <= 0 || end <= start) return;
        var available = Math.Max(1, end - start);
        var effectiveGap = FitGap(available, indexes.Length, gap);
        var widths = Share(indexes.Select(i => (double)requests[i].Width).ToArray(), available, effectiveGap);
        // A row of fixed-width panels that does not fill the band is centered in it.
        var used = widths.Sum() + effectiveGap * Math.Max(0, indexes.Length - 1);
        var x = start + Math.Max(0, available - used) / 2;
        for (var n = 0; n < indexes.Length; n++) {
            var request = requests[indexes[n]];
            var itemHeight = request.Height > 0 ? Math.Min(request.Height, height) : height;
            var itemY = alignBottom ? y + height - itemHeight : y;
            result[indexes[n]] = Round(x, itemY, widths[n], itemHeight);
            x += widths[n] + effectiveGap;
        }
    }

    private static void FlowColumn(IReadOnlyList<BgInfoLayoutRequest> requests, int[] indexes, ChartRect[] result, double x, double width, double top, double bottom, ChartRect? text, double gap, CrossAlign align, BgInfoColumnAlignment alignment, double maximumShare) {
        if (text is { } block && Overlaps(x, x + width, block.X, block.X + block.Width) && Overlaps(top, bottom, block.Y, block.Y + block.Height)) {
            var middle = (top + bottom) / 2;
            if (block.Y + block.Height / 2 <= middle) top = Math.Max(top, block.Y + block.Height + gap);
            else bottom = Math.Min(bottom, block.Y - gap);
        }

        if (top >= bottom) return;

        var available = Math.Max(1, bottom - top);
        var effectiveGap = FitGap(available, indexes.Length, gap);
        var heights = Share(indexes.Select(i => (double)requests[i].Height).ToArray(), available, effectiveGap, maximumShare);
        var used = heights.Sum() + effectiveGap * Math.Max(0, indexes.Length - 1);
        var spare = Math.Max(0, available - used);
        var y = alignment switch {
            BgInfoColumnAlignment.Center => top + spare / 2,
            BgInfoColumnAlignment.Bottom => top + spare,
            _ => top
        };
        for (var n = 0; n < indexes.Length; n++) {
            var request = requests[indexes[n]];
            var itemWidth = request.Width > 0 ? Math.Min(request.Width, width) : width;
            var itemX = align switch {
                CrossAlign.End => x + width - itemWidth,
                CrossAlign.Middle => x + (width - itemWidth) / 2,
                _ => x
            };
            result[indexes[n]] = Round(itemX, y, itemWidth, heights[n]);
            y += heights[n] + effectiveGap;
        }
    }

    /// <summary>Keeps fixed sizes when they fit, scales them proportionally when their combined size and gaps exceed the region, and splits remaining space among zero-size items.</summary>
    internal static double[] Share(double[] requested, double available, double gap, double maximumShare = double.MaxValue) {
        var sizes = new double[requested.Length];
        if (requested.Length == 0) return sizes;
        gap = FitGap(available, requested.Length, gap);
        var fixedTotal = requested.Where(v => v > 0).Sum();
        var automatic = requested.Count(v => v <= 0);
        var fixedBudget = Math.Max(0, available - gap * (requested.Length - 1) - automatic);
        var fixedScale = fixedTotal > fixedBudget && fixedTotal > 0 ? fixedBudget / fixedTotal : 1;
        var remaining = available - Math.Min(fixedTotal, fixedBudget) - gap * (requested.Length - 1);
        var share = automatic > 0 ? Math.Max(1, Math.Min(maximumShare, remaining / automatic)) : 0;
        for (var i = 0; i < requested.Length; i++) sizes[i] = requested[i] > 0 ? Math.Min(requested[i] * fixedScale, available) : share;
        return sizes;
    }

    private static double FitGap(double available, int count, double requestedGap) {
        if (count <= 1) return Math.Max(0, requestedGap);
        var maximumGap = Math.Max(0, (Math.Max(1, available) - count) / (count - 1));
        return Math.Min(Math.Max(0, requestedGap), maximumGap);
    }

    private static bool Overlaps(double start, double end, double otherStart, double otherEnd) => start < otherEnd && otherStart < end;

    private static double Clamp(double value, double min, double max) => Math.Max(min, Math.Min(max, value));

    private static ChartRect Round(double x, double y, double width, double height) {
        var left = Math.Round(x);
        var top = Math.Round(y);
        var right = Math.Round(x + width);
        var bottom = Math.Round(y + height);
        return new ChartRect(left, top, Math.Max(1, right - left), Math.Max(1, bottom - top));
    }
}
