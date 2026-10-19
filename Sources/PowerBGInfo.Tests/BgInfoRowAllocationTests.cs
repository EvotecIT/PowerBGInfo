using System.Linq;

namespace PowerBGInfo.Tests;

public sealed class BgInfoRowAllocationTests {
    [Theory]
    [InlineData(false, 1, 3)]
    [InlineData(true, 1, 3)]
    [InlineData(false, 2, 3)]
    [InlineData(true, 2, 3)]
    [InlineData(false, 1, 24)]
    [InlineData(true, 1, 24)]
    public void LargeSparsePanelsKeepTheirNaturalRowHeight(bool table, int scale, int rowCount) {
        BgInfoWidget widget;
        if (table) {
            var grid = new BgInfoTableWidget();
            grid.Columns.Add("Service");
            for (var index = 0; index < rowCount; index++) grid.Rows.Add(new[] { "service " + index });
            widget = grid;
        } else {
            var list = new BgInfoListWidget { Marker = BgInfoListMarker.Number };
            for (var index = 0; index < rowCount; index++) list.Items.Add(new BgInfoListItem { Text = "service " + index });
            widget = list;
        }
        widget.Scale = scale;
        using var image = BgInfoWidgetRenderer.Render(widget, new BgInfoConfiguration(), 1920, 1080);
        var rgba = image.ToRgbaImage();
        var lastPaintedRow = -1;
        for (var y = 0; y < rgba.Height; y++) {
            for (var x = 0; x < rgba.Width; x++) {
                if (rgba.Pixels[(y * rgba.Width + x) * 4 + 3] != 0) lastPaintedRow = y;
            }
        }
        Assert.InRange(lastPaintedRow, 40, widget.Padding + (int)(widget.NaturalBodyHeight * scale) + 2);
        Assert.Equal(1920, image.Width);
        Assert.Equal(1080, image.Height);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LargeCompressedContentKeepsPanelBoundsAndPaintsItsLastRow(bool table) {
        BgInfoWidget widget;
        if (table) {
            var value = new BgInfoTableWidget();
            value.Columns.Add("Service");
            for (var index = 0; index < 120; index++) value.Rows.Add(new[] { "service " + index });
            widget = value;
        } else {
            var value = new BgInfoListWidget { Marker = BgInfoListMarker.Number };
            for (var index = 0; index < 120; index++) value.Items.Add(new BgInfoListItem { Text = "service " + index });
            widget = value;
        }
        using var before = BgInfoWidgetRenderer.Render(widget, new BgInfoConfiguration(), 400, 240);
        if (widget is BgInfoTableWidget grid) grid.Rows[119][0] = "XXXXXXXXXXXXXXXXXXXX";
        else ((BgInfoListWidget)widget).Items[119].Text = "XXXXXXXXXXXXXXXXXXXX";
        using var after = BgInfoWidgetRenderer.Render(widget, new BgInfoConfiguration(), 400, 240);
        Assert.Equal(400, after.Width);
        Assert.Equal(240, after.Height);
        Assert.False(before.ToRgbaImage().Pixels.SequenceEqual(after.ToRgbaImage().Pixels));
    }
}
