namespace PowerBGInfo;

/// <summary>Commits a complete image without modifying the inode behind an existing hard-link alias.</summary>
internal static class BgInfoImageOutput {
    internal static void Write(string path, byte[] bytes) {
        var fullPath = Path.GetFullPath(path);
        var temporaryPath = Path.Combine(Path.GetDirectoryName(fullPath)!, "." + Path.GetFileName(fullPath) + "." + Guid.NewGuid().ToString("N") + ".tmp");
        try {
            using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None)) {
                stream.Write(bytes, 0, bytes.Length);
            }
            if (File.Exists(fullPath)) File.Replace(temporaryPath, fullPath, null);
            else File.Move(temporaryPath, fullPath);
        } finally {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }
}
