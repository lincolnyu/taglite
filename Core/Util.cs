namespace Taglite.Core
{
    static class Util
    {
        public static IEnumerable<FileSystemInfo> WalkDirectory(DirectoryInfo startingDir, Func<DirectoryInfo, bool>? avoidDirectoryPredicate = null)
        {
            if (avoidDirectoryPredicate != null && avoidDirectoryPredicate(startingDir))
            {
                yield break;
            }
            foreach (var file in startingDir.GetFiles())
            {
                yield return file;
            }
            foreach (var dir in startingDir.GetDirectories())
            {
                yield return dir;
                foreach (var item in WalkDirectory(dir, avoidDirectoryPredicate))
                {
                    yield return item;
                }
            }
        }
    }
}
