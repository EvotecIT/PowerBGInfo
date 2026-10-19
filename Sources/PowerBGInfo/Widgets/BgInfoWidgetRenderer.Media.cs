using System;
using System.Linq;
using ChartForgeX;
using ChartForgeX.Core;
using ChartForgeX.Mermaid;
using ChartForgeX.Primitives;
using ChartForgeX.Raster;
using ChartForgeX.Topology;
using ChartForgeX.Typography;
using ChartForgeX.VisualArtifacts;
using CodeGlyphX;

namespace PowerBGInfo;

internal static partial class BgInfoWidgetRenderer {
    private static RgbaImage RenderMermaid(BgInfoMermaidWidget widget, BgInfoWidgetStyle style, int width, int height) {
        var result = MermaidRenderer.Render(widget.Definition);
        if (result.HasErrors || result.Artifact == null) {
            var error = result.Diagnostics.FirstOrDefault(diagnostic => diagnostic.Severity == ChartForgeX.Diagnostics.VisualDiagnosticSeverity.Error) ?? result.Diagnostics.FirstOrDefault();
            var detail = error == null ? "the diagram family is not supported" : $"line {error.Span.Line}: {error.Message}";
            throw new InvalidOperationException("The Mermaid definition could not be rendered (" + detail + ").");
        }

        var (w, h, scale) = Density(widget, width, height);
        var theme = style.CreateTheme(ChartColor.Transparent);
        switch (result.Artifact.Model) {
            case TopologyChart topology:
                topology.WithViewport(w, h, 12).WithTheme(theme, style.Mode);
                return topology.ToRgbaImage(new TopologyRenderOptions {
                    IncludeTitle = false,
                    IncludeLegend = false,
                    FitContentToViewport = true,
                    CanvasSurfaceStyle = TopologyCanvasSurfaceStyle.Plain,
                    EdgeCornerStyle = TopologyEdgeCornerStyle.Rounded,
                    EdgeCornerRadius = 8,
                    PngOutputScale = scale
                });
            case Chart chart:
                chart.WithSize(w, h).WithTheme(theme, style.Mode).WithTransparentBackground().WithHeader(false)
                    .WithCard(false).WithPlotBackground(false).WithPngOutputScale(scale);
                return chart.ToRgbaImage();
            default:
                // Other families keep their own appearance and natural size; fit them without distortion.
                return Contain(result.Artifact.ToRgbaImage(), width, height);
        }
    }

    private static void DrawQrCode(BgInfoQrCodeWidget widget, BgInfoWidgetStyle style, BgInfoRasterImage panel, double x, double y, int width, int height) {
        var code = QrCodeEncoder.EncodeText(widget.Text, QrErrorCorrectionLevel.M, 1, 40, null);
        var modules = code.Modules;
        var count = modules.Width;
        const int quietZone = 4;

        var captionStyle = BgInfoRasterImage.CreateTextStyle(Math.Max(11, Math.Round(15 * widget.Scale)), style.FontFamily, style.Ink, 600, false,
            TextDecorationStyle.None, TextDecorationStyle.None, TextBaseline.Normal, TextCaseTransform.None);
        var captionHeight = string.IsNullOrWhiteSpace(widget.Caption) ? 0 : panel.GetTextSize("Ag", captionStyle).Height + 10;
        var side = Math.Max(1, Math.Min(width, height - captionHeight));
        var minimumSide = count + quietZone * 2;
        if (side < minimumSide) {
            throw new InvalidOperationException($"QR widget '{widget.Id}' needs at least {minimumSide} pixels of square space, but its slot is {side} pixels.");
        }
        // Whole-pixel modules keep every edge sharp, which scanners need far more than an exact size.
        var moduleSize = (int)Math.Floor(side / (double)minimumSide);
        var fieldSide = moduleSize * (count + quietZone * 2);
        var left = Math.Round(x + (width - fieldSide) / 2d);
        var top = Math.Round(y + Math.Max(0, (height - captionHeight - fieldSide) / 2d));
        var field = widget.FieldColor ?? ChartColor.White;
        var ink = widget.ModuleColor ?? ChartColor.FromHex("#0F172A");

        var composition = panel.Composition;
        composition.FillRoundedRectangle(left, top, fieldSide, fieldSide, Math.Min(12, moduleSize * 2), field);
        var origin = quietZone * moduleSize;
        for (var row = 0; row < count; row++) {
            var column = 0;
            while (column < count) {
                if (!modules[column, row]) { column++; continue; }
                var start = column;
                while (column < count && modules[column, row]) column++;
                composition.FillRectangle(left + origin + start * moduleSize, top + origin + row * moduleSize, (column - start) * moduleSize, moduleSize, ink);
            }
        }

        if (captionHeight > 0) {
            var layout = TextLayoutEngine.Layout(widget.Caption, width, captionStyle, TextWrapMode.NoWrap, 1, TextTrimming.Ellipsis);
            foreach (var line in layout.Lines) {
                panel.AddText(x + Math.Max(0, (width - line.Width) / 2), top + fieldSide + 10, line.Text, captionStyle);
            }
        }
    }
}
