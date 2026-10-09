using System.Text.Json;
using ChartForgeX.Raster;

namespace PowerBGInfo.Tests;

public class BgInfoTopologyRendererTests
{
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public void AutomaticLayeredGroupContainsEveryRenderedMemberAfterJsonLoad(int memberCount)
    {
        var path = Path.Combine(Path.GetTempPath(), "bginfo-group-" + Path.GetRandomFileName() + ".json");
        try
        {
            foreach (var theme in new[] { "Light", "Dark" })
            {
                var model = new {
                    Topologies = new[] { new {
                        Width = 560, Height = 280, Layout = "Layered", Direction = "LeftToRight", Theme = theme,
                        // Omitted dimensions request automatic bounds, as the group cmdlet does.
                        Groups = new[] { new { Id = "lab", Label = "Lab", Color = "#00FFFF" } },
                        Nodes = Enumerable.Range(0, memberCount).Select(index => new {
                            Id = "service-" + index, Label = "Service " + index, GroupId = "lab", Color = "#FF00FF"
                        }).ToArray(),
                        Edges = Enumerable.Range(1, memberCount - 1).Select(index => new {
                            SourceNodeId = "service-" + (index - 1), TargetNodeId = "service-" + index, Direction = "Forward"
                        }).ToArray()
                    } }
                };
                File.WriteAllText(path, JsonSerializer.Serialize(model));
                var configuration = BgInfoConfigurationJson.Load(path);
                var topology = Assert.Single(configuration.Topologies);

                using var rendered = BgInfoTopologyRenderer.Render(topology, configuration);
                var pixels = rendered.ToRgbaImage();
                var shell = ColoredBounds(pixels, group: true);
                var members = ColoredBounds(pixels, group: false);
                Assert.True(shell.Count > 0 && members.Count > 0, "The authored group and member colors must both render.");
                Assert.True(shell.Left <= members.Left && shell.Top <= members.Top &&
                    shell.Right >= members.Right && shell.Bottom >= members.Bottom,
                    $"The {theme} group shell must contain all {memberCount} rendered members.");
                var sourceGroup = Assert.Single(topology.Groups);
                Assert.Equal(0, sourceGroup.Width);
                Assert.Equal(0, sourceGroup.Height);
            }
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static (int Left, int Top, int Right, int Bottom, int Count) ColoredBounds(RgbaImage image, bool group)
    {
        var left = image.Width;
        var top = image.Height;
        var right = -1;
        var bottom = -1;
        var count = 0;
        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                var offset = (y * image.Width + x) * 4;
                var red = image.Pixels[offset];
                var green = image.Pixels[offset + 1];
                var blue = image.Pixels[offset + 2];
                var matches = group ? red < 80 && green > 180 && blue > 180 : red > 180 && green < 80 && blue > 180;
                if (!matches || image.Pixels[offset + 3] < 64) continue;
                left = Math.Min(left, x);
                top = Math.Min(top, y);
                right = Math.Max(right, x);
                bottom = Math.Max(bottom, y);
                count++;
            }
        }
        return (left, top, right, bottom, count);
    }
}
