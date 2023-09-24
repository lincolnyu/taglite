using System.Text;
using Taglite.Core;
using static Taglite.Util;

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

        public static IEnumerable<FileInfo> GetAllFilesIncludingInSubfolders(IEnumerable<string> fileNames)
        {
            foreach (var fn in fileNames)
            {
                if (File.Exists(fn))
                {
                    yield return new FileInfo(fn);
                }
                else if (Directory.Exists(fn))
                {
                    foreach (var fi in Core.Util.WalkDirectory(new DirectoryInfo(fn)).OfType<FileInfo>())
                    {
                        yield return fi;
                    }
                }
            }
        }

        public static DateTime GetLatestDateTimeFromFiles(IList<string> fileNames)
        {
            return GetAllFilesIncludingInSubfolders(fileNames).Select(fn => fn.CreationTime).Max();
        }

        public static DateTime GetEarliestDateTimeFromFiles(IList<string> fileNames)
        {
            return GetAllFilesIncludingInSubfolders(fileNames).Select(fn => fn.CreationTime).Min();
        }

        public static void ProcessArgs(string[] args)
        {
            if (args.Length < 3)
            {
                PrintUsage();
                return;
            }

            var tagStoreDirStr = CompleteDir(args[1], "taglite_store");
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

            var sourceItems = args[3..];

            if (dateStr != null)
            {
                dateStr = dateStr.Trim();
            }

            DateTime? dateTime = null;
            if (dateStr != null && sourceItems.Length > 0)
            {
                if (dateStr == "-" || dateStr == "")
                {
                    dateTime = GetEarliestDateTimeFromFiles(sourceItems);
                }
                else if (dateStr == "+")
                {
                    dateTime = GetLatestDateTimeFromFiles(sourceItems);
                }
            }

            if (dateTime == null)
            {
                dateTime = dateStr != null ? StringToDateTime(dateStr) : null;
                if (dateTime == null)
                {
                    var dateOverride = Environment.GetEnvironmentVariable(Constants.EnvVar.DateOverride);
                    if (dateOverride != null)
                    {
                        dateTime = StringToDateTime(dateOverride);
                    }
                    if (dateTime == null)
                    {
                        dateTime = DateTime.Now;
                    }
                }
            }
          
            string subdirName;
            string subdirFullName;  
            for (var dtval = dateTime.Value; ; dtval = dtval.AddMinutes(1))
            {
                subdirName = $"{dtval.Year%100:00}{dtval.Month:00}{dtval.Day:00}{dtval.Hour:00}{dtval.Minute:00}";
                subdirFullName = Path.Combine(tagStoreDirStr, subdirName);  
                if (!Directory.Exists(subdirFullName))
                {
                    break;
                }
            }

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
                        dir.MoveTo(Path.Combine(subdirFullName, dir.Name));
                    }
                }
            }

            var tagNode = new TagNode(subdirFullName);
            foreach (var tag in tags)
            {
                tagNode.Tags.Add(tag);
            }
            tagNode.SaveToFile();

            System.Diagnostics.Process.Start("explorer.exe", subdirFullName);
        }

        public static void PrintUsage()
        {
            Console.WriteLine($"{UsageString()}");
        }

        internal static string UsageString()
        {
            var sb = new StringBuilder("=== Tagger ===\n");
            sb.AppendLine("tag <store-dir> <tag-list-string> (<list-of-files-or-dirs-to-tag>|<folder-to-tag>)");
            sb.AppendLine(" <store-dir>: The directory contains all the subdirectories to search for the tags from. When absent taglite_store env variable is used.");
            sb.AppendLine(" <tag-list-string>: Tags separate by commas. Optionally use [<date>|+|-|] to specify tag folder date, [-] or [] for earliest creation date, [+] for latest when absent. Env variable taglite_date_override if existent overrides.");
            sb.AppendLine(" <list-of-files-or-dirs-to-tag>: A list of files and directories to be moved to a timestamp named tagged folder in <store-dir>.");
            sb.Append(" <folder-to-tag>: A folder of which the content is moved to a timestamp named tagged folder in <store-dir>.");
            return sb.ToString();
        }
    }
}