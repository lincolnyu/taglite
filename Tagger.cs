using System.Text;

namespace Taglite
{
    class Tagger
    {
        static DateTime? StringToDateTime(string str)
        {
            try
            {
                int year, month=1, day=1;
                int hour=0, minute=0;
                if (str.Length >= 2)
                {
                    year = int.Parse(str[0..2]);
                    if (year >= 70)
                    {
                        year += 1900;
                    }
                    else
                    {
                        year += 2000;
                    }
                }
                else
                {
                    throw new ArgumentException("Date string have less than 2 characters.");
                }
                if (str.Length >= 4)
                {
                    month = int.Parse(str[2..4]);
                }
                if (str.Length >= 6)
                {
                    day = int.Parse(str[4..6]);
                }
                if (str.Length >= 8)
                {
                    hour = int.Parse(str[6..8]);
                }
                if (str.Length >= 10)
                {
                    minute = int.Parse(str[8..10]);
                }
                return new DateTime(year, month, day, hour, minute, 0);
            }
            catch (Exception)
            {
                return null;
            }
        }

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

            string? dateStr = null;
            tags = tags.Where(x=>{
                if (x.StartsWith('[') && x.EndsWith(']'))
                {
                    dateStr = x[1..(x.Length-1)];
                    return false;
                }
                return true;
            }).ToList();

            var dateTime = dateStr!=null?StringToDateTime(dateStr):null;
            if (dateTime == null)
            {
                dateTime = DateTime.Now;
            }
            var subdirName = $"{dateTime.Value.Year%100:00}{dateTime.Value.Month:00}{dateTime.Value.Day:00}{dateTime.Value.Hour:00}{dateTime.Value.Minute:00}";

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

            System.Diagnostics.Process.Start("explorer.exe", subdirFullName);
        }

        static void PrintUsage()
        {
            Console.WriteLine();
            Console.WriteLine($"{UsageString()}");
        }

        internal static string UsageString()
        {
            var sb = new StringBuilder("=== Tagger ===\n");
            sb.AppendLine("tag <store-dir> <tag-list-string> (<list-of-files-or-dirs-to-tag>|<folder-to-tag>)");
            sb.AppendLine(" <list-of-files-to-tag>: A list of files and directories to be moved to a timestamp named tagged folder in <store-dir>.");
            sb.Append(" <folder-to-tag>: A folder of which the content is moved to a timestamp named tagged folder in <store-dir>");
            return sb.ToString();
        }
    }
}