using ChartForgeX.Composition;

namespace PowerBGInfo;

internal static class BgInfoImageFitMapper {
    public static VisualCanvasImageFit ToVisualCanvasFit(BgInfoImageFit fit) {
        switch (fit) {
            case BgInfoImageFit.Contain: return VisualCanvasImageFit.Contain;
            case BgInfoImageFit.Cover: return VisualCanvasImageFit.Cover;
            case BgInfoImageFit.Center: return VisualCanvasImageFit.Center;
            case BgInfoImageFit.Tile: return VisualCanvasImageFit.Tile;
            default: return VisualCanvasImageFit.Stretch;
        }
    }
}
