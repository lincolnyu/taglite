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
    }
}