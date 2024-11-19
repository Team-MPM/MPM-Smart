namespace TestBase.Helpers;

public static class DirectoryHelpers
{
    public static void Copy(string sourceDirName, string destDirName, bool copySubDirs)
    {
        var src = new DirectoryInfo(sourceDirName);

        if (!src.Exists)
            throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);

        var dirs = src.GetDirectories();
        
        if (!Directory.Exists(destDirName))
            Directory.CreateDirectory(destDirName);

        var files = src.GetFiles();
        foreach (var file in files)
        {
            var tempPath = Path.Combine(destDirName, file.Name);
            file.CopyTo(tempPath, false);
        }

        if (!copySubDirs) return;
        
        foreach (var subDir in dirs)
        {
            var tempPath = Path.Combine(destDirName, subDir.Name);
            Copy(subDir.FullName, tempPath, copySubDirs);
        }
    }
    
    public static void EnsureDeleted(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, true);
    }
    
    public static void EnsureEmpty(string path)
    {
        EnsureDeleted(path);
        Directory.CreateDirectory(path);
    }
}