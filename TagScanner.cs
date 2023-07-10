namespace Taglite
{
    class TagScanner
    {
        public TagScanner(string rootDirectory)
        {
            RootDirectory = rootDirectory;
        }
        public string RootDirectory {get;}
        public void ScanAndAddTo(TagRepo tagRepo)
        {
            ScanAndAddTo(new DirectoryInfo(RootDirectory), tagRepo);
        }

        private void ScanAndAddTo(DirectoryInfo directory, TagRepo tagRepo)
        {
            var dirName = directory.FullName;
            if (!dirName.EndsWith(Path.DirectorySeparatorChar))
            {
                dirName += Path.DirectorySeparatorChar;
            }
            var tagFile = Path.Combine(dirName, ".taglite");
            if (File.Exists(tagFile))
            {
                AddTagFileTo(dirName, tagFile, tagRepo);
            }
            foreach (var dir in directory.GetDirectories())
            {
                ScanAndAddTo(dir, tagRepo);
            }
        }

        private void AddTagFileTo(string directory, string tagFile, TagRepo tagRepo)
        {
            using var sr = new StreamReader(tagFile);
            
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                var tag = line?.Trim()?.ToLower();
                if (string.IsNullOrEmpty(tag)) continue;
                if (!tagRepo.TagMapping.TryGetValue(tag, out var allDirsContainingTag))
                {
                    allDirsContainingTag = new HashSet<string>();
                    tagRepo.TagMapping[tag] = allDirsContainingTag;
                }
                allDirsContainingTag.Add(directory);
            }
        }

        public TagRepo Scan()
        {
            var tagRepo = new TagRepo();
            ScanAndAddTo(tagRepo);
            return tagRepo;
        }
    }
}