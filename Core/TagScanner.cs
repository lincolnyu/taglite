namespace Taglite.Core
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

        public static IEnumerable<TagNode> EnumerateAllTagNodes(DirectoryInfo directory)
        {
            var dirName = directory.FullName;
            if (TagNode.IsTaggedDirectory(dirName))
            {
                yield return new TagNode(dirName);
            }
            foreach (var dir in directory.GetDirectories())
            {
                foreach (var node in EnumerateAllTagNodes(dir))
                {
                    yield return node;
                }
            }
        }

        private void ScanAndAddTo(DirectoryInfo directory, TagRepo tagRepo)
        {
            foreach (var node in EnumerateAllTagNodes(directory))
            {
                tagRepo.AddNode(node);
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