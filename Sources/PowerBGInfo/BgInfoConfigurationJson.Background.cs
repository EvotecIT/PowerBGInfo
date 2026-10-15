using System;
using System.Collections.Generic;

namespace PowerBGInfo;

public static partial class BgInfoConfigurationJson {
    private static BgInfoBackground? MapBackground(BgInfoBackgroundFile? model, string baseDirectory) {
        if (model == null) return null;

        var background = new BgInfoBackground();
        if (model.Width.HasValue) background.Width = Math.Max(0, model.Width.Value);
        if (model.Height.HasValue) background.Height = Math.Max(0, model.Height.Value);
        ApplyColor(model.Color, value => background.Color = value);
        if (!string.IsNullOrWhiteSpace(model.ImagePath)) background.ImagePath = ResolvePath(model.ImagePath, baseDirectory);
        ApplyEnum<BgInfoImageFit>(model.ImageFit, value => background.ImageFit = value);
        if (model.ImageOpacity.HasValue) background.ImageOpacity = ValidateImageOpacity(model.ImageOpacity.Value);
        ApplyColor(model.GradientStart, value => background.GradientStart = value);
        ApplyColor(model.GradientEnd, value => background.GradientEnd = value);
        if (model.GradientAngle.HasValue) background.GradientAngle = model.GradientAngle.Value;
        ApplyEnum<BgInfoBackgroundPattern>(model.Pattern, value => background.Pattern = value);
        ApplyColor(model.PatternColor, value => background.PatternColor = value);
        if (model.PatternSpacing.HasValue) background.PatternSpacing = Math.Max(8, model.PatternSpacing.Value);

        if (model.Glows != null) {
            foreach (var glowModel in model.Glows) {
                if (glowModel == null) continue;
                var glow = new BgInfoBackgroundGlow();
                if (glowModel.X.HasValue) glow.X = glowModel.X.Value;
                if (glowModel.Y.HasValue) glow.Y = glowModel.Y.Value;
                if (glowModel.Radius.HasValue) glow.Radius = Math.Max(0, glowModel.Radius.Value);
                ApplyColor(glowModel.Color, value => glow.Color = value);
                background.Glows.Add(glow);
            }
        }

        return background;
    }

    private static BgInfoBackgroundFile? MapBackgroundFile(BgInfoBackground? background) {
        if (background == null) return null;

        var defaults = new BgInfoBackground();
        var model = new BgInfoBackgroundFile {
            Width = background.Width > 0 ? background.Width : null,
            Height = background.Height > 0 ? background.Height : null,
            Color = BgInfoColorParser.ToHex(background.Color),
            ImagePath = string.IsNullOrWhiteSpace(background.ImagePath) ? null : background.ImagePath,
            ImageFit = background.ImageFit == defaults.ImageFit ? null : background.ImageFit.ToString(),
            ImageOpacity = background.ImageOpacity == defaults.ImageOpacity ? null : ValidateImageOpacity(background.ImageOpacity),
            GradientStart = background.GradientStart.HasValue ? BgInfoColorParser.ToHex(background.GradientStart.Value) : null,
            GradientEnd = background.GradientEnd.HasValue ? BgInfoColorParser.ToHex(background.GradientEnd.Value) : null,
            GradientAngle = background.GradientAngle == defaults.GradientAngle ? null : background.GradientAngle,
            Pattern = background.Pattern == BgInfoBackgroundPattern.None ? null : background.Pattern.ToString(),
            PatternColor = background.Pattern == BgInfoBackgroundPattern.None ? null : BgInfoColorParser.ToHex(background.PatternColor),
            PatternSpacing = background.PatternSpacing == defaults.PatternSpacing ? null : background.PatternSpacing
        };

        if (background.Glows.Count > 0) {
            model.Glows = new List<BgInfoBackgroundGlowFile>();
            foreach (var glow in background.Glows) {
                if (glow == null) continue;
                model.Glows.Add(new BgInfoBackgroundGlowFile {
                    X = glow.X,
                    Y = glow.Y,
                    Radius = glow.Radius,
                    Color = BgInfoColorParser.ToHex(glow.Color)
                });
            }
        }

        return model;
    }

    internal sealed class BgInfoBackgroundFile {
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string? Color { get; set; }
        public string? ImagePath { get; set; }
        public string? ImageFit { get; set; }
        public double? ImageOpacity { get; set; }
        public string? GradientStart { get; set; }
        public string? GradientEnd { get; set; }
        public double? GradientAngle { get; set; }
        public List<BgInfoBackgroundGlowFile>? Glows { get; set; }
        public string? Pattern { get; set; }
        public string? PatternColor { get; set; }
        public int? PatternSpacing { get; set; }
    }

    internal sealed class BgInfoBackgroundGlowFile {
        public double? X { get; set; }
        public double? Y { get; set; }
        public double? Radius { get; set; }
        public string? Color { get; set; }
    }
}
