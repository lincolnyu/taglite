using System.Text;

using static Taglite.Util;

namespace Taglite
{
    class Tagger
    {
        public static void ProcessArgs(string[] args)
        {
            if (args.Length < 3)
            {
                PrintUsage();
            }
            var tagListString = args[1];
            var tags = tagListString!.Split(',', StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries).Select(x=>x.ToLower());

            var tagStoreDirStr = args[2];
            Console.WriteLine($"tag: {tagStoreDirStr}");
            if (tagStoreDirStr == "env")
            {
                tagStoreDirStr = Environment.GetEnvironmentVariable("taglite_store");
            }
            if (!Directory.Exists(tagStoreDirStr))
            {
                Console.WriteLine($"Store directory '{tagStoreDirStr}' does not exist.");
                return;
            }
            var tagStoreDir = new DirectoryInfo(tagStoreDirStr);

            var now = DateTime.Now;
            var subdirName = $"{now.Year%100:00}{now.Month:00}{now.Day:00}{now.Hour:00}{now.Minute:00}";
            
            DirectoryInfo subdir;
            try
            {
                subdir = tagStoreDir.CreateSubdirectory(subdirName);
            }
            catch (IOException)
            {
                Console.WriteLine("Error creating subdirectory.");
                return;
            }
            var tagFileName = Path.Combine(subdir.FullName, ".taglite");
            using (var sw = new StreamWriter(tagFileName))
            {
                foreach (var tag in tags)
                {
                    sw.WriteLine(tag);
                }
            }

            var sourceItems = args[3..];
            if (sourceItems.Length == 1 &&  Directory.Exists(sourceItems[0]))
            {
                Directory.Move(sourceItems[0], subdir.FullName);
            }
            else
            {
                foreach (var item in sourceItems)
                {
                    if (File.Exists(item))
                    {
                        var file = new FileInfo(item);
                        file.MoveTo(Path.Combine(subdir.FullName, file.Name));
                    }
                    else if (Directory.Exists(item))
                    {
                        var dir  = new DirectoryInfo(item);
                        dir.MoveTo(Path.Combine(subdir.FullName, dir.Name));
                    }
                }
            }

        }

        static void PrintUsage()
        {
            Console.WriteLine($"Tagger usage: {UsageString()}");
        }

        internal static string UsageString()
        {
            return "tag <tag-list-string> <destination-folder> (<list-of-files-to-tag>|<folder-to-tag>)";
        }
    }
}