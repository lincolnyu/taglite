namespace Taglite
{
    class TagNode
    {
        public const string TagliteFileName = ".taglite";
        public TagNode(string directory)
        {
            Directory = directory;
        }
        public string Directory {get;}
        public HashSet<string> Tags
        {
            get
            {
                if (_tags == null)
                {
                    var tagFile = Path.Combine(Directory, TagliteFileName);
                    _tags = new HashSet<string>(LoadTagsFromTagFile(tagFile));
                }
                return _tags;
            }
        }
        private HashSet<string>? _tags;

        public static bool IsTaggedDirectory(string directory)
        {
            var tagFile = Path.Combine(directory, TagNode.TagliteFileName);
            return File.Exists(tagFile);
        }

        public static IEnumerable<string> LoadTagsFromTagFile(string tagFile)
        {
            using var sr = new StreamReader(tagFile);
            
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                var tag = line?.Trim()?.ToLower();
                if (string.IsNullOrEmpty(tag)) continue;
                yield return tag;
            }
        }
        public void Invalidate()
        {
            _tags = null;
        }

        public override bool Equals(object? obj)
        {
            if (obj is TagNode otherNode)
            {
                return Directory == otherNode.Directory;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Directory.GetHashCode();
        }

        public IEnumerable<FileSystemInfo> EnumerateContents()
        {
            var startingDir = new DirectoryInfo(Directory);
            foreach (var file in startingDir.GetFiles().Where(x=>!x.Name.Equals(TagliteFileName)))
            {
                yield return file;
            }
            foreach (var dir in startingDir.GetDirectories())
            {
                foreach (var item in EnumerateContentsRecursiveUntilTagged(dir))
                {
                    yield return item;
                }
            }
        }

        private static IEnumerable<FileSystemInfo> EnumerateContentsRecursiveUntilTagged(DirectoryInfo startingDir)
        {
            if (IsTaggedDirectory(startingDir.FullName))
            {
                yield break;
            }
            foreach (var file in startingDir.GetFiles())
            {
                yield return file;
            }
            foreach (var dir in startingDir.GetDirectories())
            {
                foreach (var item in EnumerateContentsRecursiveUntilTagged(dir))
                {
                    yield return item;
                }
            }
        }
    }
}