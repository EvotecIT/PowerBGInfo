namespace PowerBGInfo;

/// <summary>Commits a complete image without modifying the inode behind an existing hard-link alias.</summary>
internal static class BgInfoImageOutput {
    internal static void Write(string path, byte[] bytes) {
        var fullPath = Path.GetFullPath(path);
        string? temporaryPath = null;
        try {
            using (var stream = CreateStagingFile(fullPath, out temporaryPath)) {
                stream.Write(bytes, 0, bytes.Length);
            }
            if (File.Exists(fullPath)) File.Replace(temporaryPath, fullPath, null);
            else File.Move(temporaryPath, fullPath);
        } finally {
            if (temporaryPath != null && File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }

    private static FileStream CreateStagingFile(string fullPath, out string temporaryPath) {
        var maximumLength = Path.GetFileName(fullPath).Length;
        if (maximumLength == 0) throw new ArgumentException("An output file name is required.", nameof(fullPath));
        for (var attempt = 0; attempt < 10; attempt++) {
            // A sibling no longer than the destination fits the same component and
            // full-path limits. The encoder already supplied bytes, so no extension is needed.
            var name = (maximumLength > 1 ? "." : string.Empty) + Guid.NewGuid().ToString("N");
            var candidate = Path.Combine(Path.GetDirectoryName(fullPath)!, name.Substring(0, Math.Min(name.Length, maximumLength)));
            if (string.Equals(candidate, fullPath, BgInfoPaths.Comparison)) continue;
            try {
                var stream = new FileStream(candidate, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                temporaryPath = candidate; // Cleanup owns it only after CreateNew succeeds.
                return stream;
            } catch (IOException) when (attempt < 9 && File.Exists(candidate)) {
                // Short output names have a smaller random namespace; retry a collision.
            }
        }
        throw new IOException("Could not reserve a distinct sibling file for the image output.");
    }
}
