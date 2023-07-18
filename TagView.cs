using System.Text;
using static Taglite.Util;

namespace Taglite
{
    class TagView
    {
        public static string UsageString()
        {
            var sb = new StringBuilder("=== TagView ===\n");
            sb.AppendLine("Usage 1: (any|all)(f|) [<view-dir>] [<tag-list-string>] [<store-dir>]");
            sb.AppendLine(" any|all: Whether to find the directories that contain any or all of the tags in the list.");
            sb.AppendLine(" f: Create file symlinks instead.");
            sb.AppendLine(" <view-dir>: The directory where the shortcuts to all the found directories are put. When absent taglite_view env variable is used.");
            sb.AppendLine(" <tag-list-string>: Tags separate by commas. Optional only if 'any' is chosen.");
            sb.AppendLine(" <store-dir>: The directory contains all the subdirectories to search for the tags from. When absent taglite_store env variable is used.");
            sb.AppendLine("Usage 2: alltags [<store-dir>]");
            sb.Append(" To list all the tags from <store-dir> in alphabetic order.");
            return sb.ToString();
        }

        static void PrintUsage()
        {
            Console.WriteLine($"== Tagger ===");
            Console.WriteLine($"{UsageString()}");
        }

        public static void ProcessArgs(string[] args)
        {
            var cmd = args[0].Trim().ToLower();
            if (cmd == "alltags")
            {
                var storeDirToShowAllTags = CompleteDir(TryGetArg(args, 1), "taglite_store");
                if (storeDirToShowAllTags == null)
                {
                    Console.WriteLine($"<store-dir> '{storeDirToShowAllTags}' is not provided or does not exist.");
                    return;
                }
                var tagRepo = GetTagRepo(storeDirToShowAllTags);
                Console.WriteLine("All tags:");
                foreach (var kvp in tagRepo.TagMapping.OrderBy(kvp=>kvp.Value.Count).Reverse())
                {
                    Console.Write(" ");
                    Console.Write(kvp.Key);
                    if (kvp.Value.Count > 1)
                    {
                        Console.Write($"({kvp.Value.Count})");
                    }
                }
                return;
            }

            if (args.Length < 1 || args.Length > 4)
            {
                PrintUsage();
                return;
            }

            var viewDir = CompleteDir(TryGetArg(args, 1), "taglite_view");
            var storeDir = CompleteDir(TryGetArg(args, 3), "taglite_store");

            if (storeDir == null || !Directory.Exists(storeDir))
            {
                Console.WriteLine($"<store-dir> '{storeDir}' is not provided or does not exist.");
                return;
            }
            if (viewDir == null)
            {
                Console.WriteLine($"<view-dir> '{viewDir}' is not provided or does not exist.");
                return;
            }

            HashSet<TagNode> nodes;
            bool expandFiles = false;
            {
                var tagRepo = GetTagRepo(storeDir);
                var tagListString = TryGetArg(args, 2);
                var tags = tagListString?.Split(',', StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries).Select(x=>x.ToLower());
                if (cmd.Length < 3)
                {
                    Console.WriteLine("Unexpected commnnand");
                    PrintUsage();
                    return;
                }
                var anyOrAll = cmd.Substring(0,3);
                switch (anyOrAll)
                {
                    case "any":
                        if (tags != null)
                        {
                            nodes = tagRepo.FindAllNodesContainingAtLeastOne(tags);
                        }
                        else
                        {
                            nodes = tagRepo.TagMapping.Values.Aggregate(new HashSet<TagNode>(), (x,y)=>{x.UnionWith(y); return x;});
                        }
                        break;
                    case "all":
                        if (tags != null)
                        {
                            nodes = tagRepo.FindAllNodesContainingAll(tags);
                        }
                        else
                        {
                            Console.WriteLine("Must provide tags.");
                            return;
                        }
                        break;
                    default:
                        Console.WriteLine("Unexpected commnnand");
                        PrintUsage();
                        return;
                }
                expandFiles = (cmd.Length == 4 && cmd[3] == 'f');
            }

            if (!Directory.Exists(viewDir))
            {
                Directory.CreateDirectory(viewDir);
                if (!Directory.Exists(viewDir))
                {
                    Console.WriteLine($"<view-dir> '{viewDir}' does not exist and cannot be created.");
                    return;
                }
            }

            bool useSymlink = false;
            var clashResolver = new NameClashResolver();
            foreach (var node in nodes)
            {
                var dir = new DirectoryInfo(node.Directory);
                if (expandFiles)
                {
                    var dirName = dir.Name;
                    var prefix = clashResolver.New(dirName);
                    foreach (var item in node.EnumerateContents())
                    {
                        var rel = GetRelative(item.FullName, dir.FullName);
                        var target = Path.Combine(viewDir, prefix + "-" + rel);
                        if (item is FileInfo fileInfo)
                        {
                            System.Diagnostics.Process.Start("cmd.exe", "/c mklink \"" + target + "\" \"" + fileInfo.FullName + "\"");
                        }
                        else if (item is DirectoryInfo dirInfo)
                        {
                            Directory.CreateDirectory(target);
                        }
                    }
                }
                else
                {
                    // TODO protect against long file name
                    var sb = new StringBuilder(dir.Name);
                    foreach (var tag in node.Tags.Order())
                    {
                        sb.Append(".");
                        sb.Append(tag);
                    }
                    var name = clashResolver.New(sb.ToString());
                    if (useSymlink)
                    {
                        try
                        {
                        }
                        catch(Exception)
                        {
                            useSymlink = false;
                        }
                    }
                    
                    CreateShortcut(Path.Combine(viewDir, name + ".lnk"), node.Directory);
                    //CreateDirSymLink(Path.Combine(viewDir, name), node.Directory);
                    //CreateCmdLink(Path.Combine(viewDir, name + ".cmd"), node.Directory);
                }
            }

            System.Diagnostics.Process.Start("explorer.exe", viewDir);
        }

        static void CreateShortcut(string source, string target)
        {
            var powershellCmd = $"$s=(New-Object -COM WScript.Shell).CreateShortcut('{source}');$s.TargetPath='{target}';$s.Save()";
            System.Diagnostics.Process.Start("powershell.exe", powershellCmd);
        }
        static void CreateDirSymLink(string source, string target)
        {
            System.Diagnostics.Process.Start("cmd.exe", "/c mklink /d \"" + source + "\" \"" + target + "\"");
        }
        static void CreateCmdLink(string source, string target)
        {
            var sbScript = new StringBuilder();
            sbScript.Append($"explorer.exe {target}");
            using var sw = new StreamWriter(source);
            sw.Write(sbScript.ToString());       
        }

        static TagRepo GetTagRepo(string dir)
        {
            var tagScanner = new TagScanner(dir);
            var tagRepo = tagScanner.Scan();
            return tagRepo;
        }

        static string? CompleteDir(string? inputDir, string backupEnvVariable)
        {
            if (Path.IsPathRooted(inputDir))
            {
                return inputDir;
            }
            else 
            {
                var defaultTagStore = Environment.GetEnvironmentVariable(backupEnvVariable);
                if (defaultTagStore == null)
                {
                    return null;
                }
                if (inputDir != null)
                {
                    return Path.Combine(defaultTagStore!, inputDir);
                }
                return defaultTagStore!;
            }
        }
    }
}