namespace PowerBGInfo;

public partial class BgInfoGenerator
{
    /// <summary>Receives warnings from a run, for example when a base image is PowerBGInfo's own earlier output.</summary>
    public Action<string>? Warning { get; set; }

    /// <summary>Replaces the current-wallpaper lookup; tests use it to stand in for Windows.</summary>
    internal Func<int, string?>? CurrentWallpaper { get; set; }

    /// <summary>
    /// Returns the image to draw on instead of <paramref name="candidate"/> when drawing on the candidate would stack
    /// PowerBGInfo's text: the original recorded for it, or null (a plain background) when the original is not known.
    /// </summary>
    /// <param name="candidate">Base image the run would use.</param>
    /// <param name="plannedOutputPath">Path this run writes, when it is known before the base image is chosen.</param>
    /// <param name="role">How the candidate is described in a warning.</param>
    /// <param name="chosenByUser">
    /// True for -FilePath and New-BGInfoBackground -ImagePath. A chosen image is replaced only when it is the file this run
    /// overwrites; any other image, even one PowerBGInfo rendered, is used as given. The current wallpaper and slideshow
    /// images are replaced whenever PowerBGInfo wrote them.
    /// </param>
    /// <param name="knownOutputPaths">Additional generated paths recognized by a caller, such as the current slideshow entries.</param>
    internal string? ResolveOriginalImage(string? candidate, string? plannedOutputPath, string role, bool chosenByUser,
        ISet<string>? knownOutputPaths = null)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return candidate;
        }

        var isPlannedOutput = IsSamePath(candidate!, plannedOutputPath);
        if (chosenByUser && !isPlannedOutput)
        {
            return candidate;
        }

        // The record holds the base the output was drawn on, already resolved when it was written,
        // so one step back is the original. Null means a plain or generated background.
        if (BgInfoOutputRegistry.TryGetSource(candidate!, out var source))
        {
            if (source != null && !File.Exists(source))
            {
                WriteWarning($"The original image '{source}' recorded for '{candidate}' no longer exists. A plain background is used instead.");
                return null;
            }
            return IsSamePath(source ?? string.Empty, candidate) ? null : source;
        }

        if (chosenByUser && isPlannedOutput && File.Exists(candidate))
        {
            return candidate;
        }

        var isKnownOutputPath = knownOutputPaths?.Contains(NormalizePath(candidate!)) == true;
        if (!isPlannedOutput && !isKnownOutputPath && !BgInfoOutputRegistry.HasOwnOutputName(candidate!))
        {
            return candidate;
        }

        WriteWarning($"{role} '{candidate}' is an image PowerBGInfo wrote earlier, and the image it was drawn on is not recorded. "
            + "It is not used, so the text is not drawn twice. Pass -FilePath or New-BGInfoBackground to choose the base image.");
        return null;
    }

    private static string PreserveInPlaceBaseImage(string sourcePath, string outputPath)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(outputPath))!;
        var name = Path.GetFileNameWithoutExtension(outputPath);
        var extension = Path.GetExtension(outputPath);
        var backupPath = Path.Combine(directory, $"{name}.PowerBGInfo-original-{Guid.NewGuid():N}{extension}");
        File.Copy(sourcePath, backupPath);
        return backupPath;
    }

    private void WriteWarning(string message) => Warning?.Invoke(message);

    private static bool IsSamePath(string path, string? other)
    {
        return !string.IsNullOrWhiteSpace(path) && !string.IsNullOrWhiteSpace(other)
            && BgInfoPaths.AreSame(path, other!);
    }

    private static string NormalizePath(string path)
    {
        return Path.GetFullPath(path);
    }
}
