namespace Taglite.Core
{
    class TagNode
    {
        public const string TagliteFileName = ".taglite";
        public TagNode(string directory)
        {
            // To normalize the directory path
            if (!directory.EndsWith(Path.DirectorySeparatorChar))
            {
                directory += Path.DirectorySeparatorChar;
            }
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
                    if (File.Exists(tagFile))
                    {
                        _tags = new HashSet<string>(LoadTagsFromTagFile(tagFile).Select(x=>x.Item1));
                    }
                    else
                    {
                        _tags = new HashSet<string>();
                    }
                }
                return _tags;
            }
        }
        private HashSet<string>? _tags;

        public void SaveToFile()
        {
            var tagFileName = Path.Combine(Directory, TagliteFileName);
            var tags = Tags;
            using (var sw = new StreamWriter(tagFileName))
            {
                foreach (var tag in tags)
                {
                    sw.WriteLine(tag);
                }
            }
        }

        public static bool IsTaggedDirectory(string directory)
        {
            var tagFile = Path.Combine(directory, TagNode.TagliteFileName);
            return File.Exists(tagFile);
        }

        public bool Validate()
        {
            var tagFile = Path.Combine(Directory, TagliteFileName);
            return LoadTagsFromTagFile(tagFile).All(x=>x.Item2);
        }

        public static IEnumerable<(string, bool)> LoadTagsFromTagFile(string tagFile)
        {
            using var sr = new StreamReader(tagFile);
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                var tag = line?.Trim()?.ToLower();
                if (string.IsNullOrEmpty(tag)) continue;
                var validatedTag = ValidateTag(tag);
                yield return (validatedTag, validatedTag == tag);
            }
        }

        private static string ValidateTag(string tag)
        {
            return tag.Replace(",", "").Replace(" ", "").Replace("\t", "");
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
                yield return dir;
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
                yield return dir;
                foreach (var item in EnumerateContentsRecursiveUntilTagged(dir))
                {
                    yield return item;
                }
            }
        }
    }
}