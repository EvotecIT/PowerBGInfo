using System;
using ChartForgeX.Composition;
using ChartForgeX.Raster;

namespace PowerBGInfo;

internal static partial class BgInfoWidgetRenderer {
    // Includes the owner's two working samples per output sample.
    private const double RowWorkingPixelBudget = 16000000;
    private const int RowsPerBatch = 24;
    private const int BatchLogicalWidth = 1024;

    private static bool CanRenderRowsDirectly(BgInfoWidget widget, int width, int height, int? outputScale = null) {
        var (w, h, scale) = Density(widget, width, height);
        scale = outputScale ?? scale;
        return (double)w * h * scale * scale * 4 <= RowWorkingPixelBudget;
    }

    private static RgbaImage RenderBatchedRows(BgInfoWidget widget, BgInfoWidgetStyle style, int width, int height) {
        var table = widget as BgInfoTableWidget;
        var list = widget as BgInfoListWidget;
        var count = table?.Rows.Count ?? list!.Items.Count;
        var rowHeight = table != null ? (table.Dense ? 24d : 31d) : (list!.Dense ? 25d : 33d);
        var header = table is { ShowHeader: true } ? (table.Dense ? 30d : 36d) : 0;
        var (requestedWidth, _, _) = Density(widget, width, height);
        var fitsNaturally = widget.NaturalBodyHeight * widget.Scale <= height;
        // Sparse wide panels retain their authored density and column width. Smaller batches
        // bound their working raster without stretching a handful of rows over the whole panel.
        var logicalWidth = fitsNaturally ? requestedWidth : Math.Min(BatchLogicalWidth, requestedWidth);
        var rowsPerBatch = Math.Min(RowsPerBatch, Math.Max(1,
            (int)Math.Floor((RowWorkingPixelBudget / (logicalWidth * 4d) - header - 16) / rowHeight)));
        if (rowsPerBatch > 1) rowsPerBatch -= rowsPerBatch % 2; // Keep table stripe phase between batches.
        var batches = Math.Max(1, (int)Math.Ceiling(count / (double)rowsPerBatch));
        var totalHeight = count * rowHeight + batches * 16d + header;
        var contentHeight = fitsNaturally ? Math.Min(height, totalHeight * widget.Scale) : height;
        var output = ImageComposition.CreateTransparent(width * 2, height * 2);
        var marker = list?.Marker ?? BgInfoListMarker.None;
        if (marker == BgInfoListMarker.Auto) {
            marker = list!.Items.Exists(item => item.Checked.HasValue) ? BgInfoListMarker.Check
                : list.Items.Exists(item => item.Status != BgInfoStatus.None) ? BgInfoListMarker.Status : BgInfoListMarker.Bullet;
        }
        double consumed = 0;
        for (var batchIndex = 0; batchIndex < batches; batchIndex++) {
            var offset = batchIndex * rowsPerBatch;
            var length = Math.Min(rowsPerBatch, count - offset);
            BgInfoWidget batch;
            if (table != null) {
                var rows = new BgInfoTableWidget {
                    Dense = table.Dense, ShowHeader = table.ShowHeader && offset == 0,
                    RowStriping = table.RowStriping, StatusColumn = table.StatusColumn
                };
                rows.Columns.AddRange(table.Columns);
                for (var index = offset; index < offset + length; index++) rows.Rows.Add(table.Rows[index]);
                batch = rows;
            } else {
                var items = new BgInfoListWidget { Dense = list!.Dense, Marker = marker };
                for (var index = offset; index < offset + length; index++) {
                    var item = list.Items[index];
                    // The owner numbers each block from one; pagination preserves global numbering.
                    items.Items.Add(marker == BgInfoListMarker.Number
                        ? new BgInfoListItem { Text = (index + 1) + ". " + item.Text, Value = item.Value, Checked = item.Checked, Status = item.Status }
                        : item);
                }
                if (marker == BgInfoListMarker.Number) items.Marker = BgInfoListMarker.None;
                batch = items;
            }
            var batchHeight = batch.NaturalBodyHeight;
            var image = batch is BgInfoTableWidget tableBatch
                ? RenderTable(tableBatch, style, logicalWidth, (int)Math.Ceiling(batchHeight), outputScale: 1)
                : RenderList((BgInfoListWidget)batch, style, logicalWidth, (int)Math.Ceiling(batchHeight), outputScale: 1);
            var top = contentHeight * 2d * consumed / totalHeight;
            consumed += batchHeight;
            var bottom = contentHeight * 2d * consumed / totalHeight;
            output.DrawImage(image, 0, top, width * 2d, bottom - top);
        }
        return output.ToImage();
    }
}
