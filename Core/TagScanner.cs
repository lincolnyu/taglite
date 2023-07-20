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

        private void ScanAndAddTo(DirectoryInfo directory, TagRepo tagRepo)
        {
            var dirName = directory.FullName;
            if (!dirName.EndsWith(Path.DirectorySeparatorChar))
            {
                dirName += Path.DirectorySeparatorChar;
            }
            tagRepo.AddDirectoryIfTagged(dirName);
            foreach (var dir in directory.GetDirectories())
            {
                ScanAndAddTo(dir, tagRepo);
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