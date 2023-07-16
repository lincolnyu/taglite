using System.Text;

namespace Taglite
{
    class Tagger
    {
        public static void ProcessArgs(string[] args)
        {
            if (args.Length < 3)
            {
                PrintUsage();
                return;
            }

            var tagStoreDirStr = args[1];
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

            var tagListString = args[2];
            var tags = tagListString!.Split(',', StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries).Order().Select(x=>x.ToLower());

            var now = DateTime.Now;
            var subdirName = $"{now.Year%100:00}{now.Month:00}{now.Day:00}{now.Hour:00}{now.Minute:00}";

            string subdirFullName;            
            var sourceItems = args[3..];
            if (sourceItems.Length == 1 && Directory.Exists(sourceItems[0]))
            {
                subdirFullName = Path.Combine(tagStoreDirStr, subdirName);
                Directory.Move(sourceItems[0], subdirFullName);
            }
            else
            {
                DirectoryInfo subdir;
                try
                {
                    subdir = tagStoreDir.CreateSubdirectory(subdirName);
                    subdirFullName = subdir.FullName;
                }
                catch (IOException)
                {
                    Console.WriteLine("Error creating subdirectory.");
                    return;
                }
                foreach (var item in sourceItems)
                {
                    if (File.Exists(item))
                    {
                        var file = new FileInfo(item);
                        file.MoveTo(Path.Combine(subdirFullName, file.Name));
                    }
                    else if (Directory.Exists(item))
                    {
                        var dir  = new DirectoryInfo(item);
                        dir.MoveTo(Path.Combine(subdirFullName));
                    }
                }
            }

            var tagFileName = Path.Combine(subdirFullName, ".taglite");
            using (var sw = new StreamWriter(tagFileName))
            {
                foreach (var tag in tags)
                {
                    sw.WriteLine(tag);
                }
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine($"Tagger usage: {UsageString()}");
        }

        internal static string UsageString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("tag <store-dir> <tag-list-string> (<list-of-files-or-dirs-to-tag>|<folder-to-tag>)");
            sb.AppendLine(" <list-of-files-to-tag>: A list of files and directories to be moved to a timestamp named tagged folder in <store-dir>.");
            sb.Append(" <folder-to-tag>: A folder of which the content is moved to a timestamp named tagged folder in <store-dir>");
            return sb.ToString();
        }
    }
}