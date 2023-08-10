using System.Text;
using Taglite.Core;
using static Taglite.Util;

namespace Taglite
{
    class TagView
    {
        public static string UsageString()
        {
            var sb = new StringBuilder("=== TagView ===\n");
            sb.AppendLine("Usage 1: (any|all)(l|s|c|f|) [<view-dir>] [<tag-list-string>] [<store-dir>]");
            sb.AppendLine(" any|all: Whether to find the directories that contain any or all of the tags in the list.");
            sb.AppendLine(" l: Create command shortcuts to target directories (default).");
            sb.AppendLine(" s: Create symlinks to target directories.");
            sb.AppendLine(" c: Create command based links to target directories.");
            sb.AppendLine(" f: Create symlinks to contents of the all the target directories.");
            sb.AppendLine($" <view-dir>: The directory where the shortcuts to all the found directories are put. When absent, {Constants.EnvVar.ViewDir} env variable is used.");
            sb.AppendLine(" <tag-list-string>: Tags separate by commas. Optional only if 'any' is chosen.");
            sb.AppendLine($" <store-dir>: The directory contains all the subdirectories to search for the tags from. When absent, {Constants.EnvVar.StoreDir} env variable is used.");
            sb.AppendLine("Usage 2: alltags [<store-dir>] [<tag-list-string>]");
            sb.Append(" To list all the tags or the specified tags from <store-dir> in alphabetic order.");
            return sb.ToString();
        }

        public static void PrintUsage()
        {
            Console.WriteLine($"{UsageString()}");
        }

        class TagComparer : IComparer<KeyValuePair<string, HashSet<TagNode>>>
        {
            public int Compare(KeyValuePair<string, HashSet<TagNode>> x, KeyValuePair<string, HashSet<TagNode>> y)
            {
                var c = x.Value.Count.CompareTo(y.Value.Count);
                if (c != 0) return -c;
                return x.Key.CompareTo(y.Key);
            }
            public static TagComparer Instance {get;} = new TagComparer();
        }

        public static void ProcessArgs(string[] args)
        {   
            var cmd = args[0].Trim().ToLower();
            if (cmd == "alltags")
            {
                var storeDirToShowAllTags = CompleteDir(TryGetArg(args, 1), Constants.EnvVar.StoreDir);
                
                var tagsString = TryGetArg(args, 2);
                var tags = tagsString?.Split(',', StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries).Select(x=>x.ToLower());
                HashSet<string>? setTagsToShow = null;
                if (tags != null)
                {
                    setTagsToShow = new HashSet<string>(tags);
                }

                if (storeDirToShowAllTags == null || !Directory.Exists(storeDirToShowAllTags))
                {
                    Console.WriteLine($"<store-dir> '{storeDirToShowAllTags}' is not provided or does not exist.");
                    return;
                }
                var tagRepo = GetTagRepo(storeDirToShowAllTags);
                Console.Write("Found:");
                foreach (var kvp in tagRepo.TagMapping.Order(TagComparer.Instance))
                {
                    if (setTagsToShow?.Contains(kvp.Key) == false)
                    {
                        continue;
                    }
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
                Console.WriteLine("Invalid arguments.");
                PrintUsage();
                return;
            }

            var viewDir = CompleteDir(TryGetArg(args, 1), Constants.EnvVar.ViewDir);
            var tagListString = TryGetArg(args, 2);
            var storeDir = CompleteDir(TryGetArg(args, 3), Constants.EnvVar.StoreDir);

            Run(cmd, viewDir, tagListString, storeDir);
        }

        static void Run(string cmd, string? viewDir, string? tagListString, string? storeDir)
        {
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
            char mode = 'l';
            bool expandFiles = false;
            {
                var tagRepo = GetTagRepo(storeDir);
                
                var tags = tagListString?.Split(',', StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries).Select(x=>x.ToLower());
                if (cmd.Length < 3 || cmd.Length > 4)
                {
                    Console.WriteLine($"Unexpected commnnand {cmd}");
                    PrintUsage();
                    return;
                }
                var anyOrAll = cmd.Substring(0,3);
                if (anyOrAll != "any" && anyOrAll != "all")
                {
                    Console.WriteLine($"Unexpected command: '{cmd}'");
                    PrintUsage();
                    return;
                }
                var isAny = anyOrAll == "any";
                if (cmd.Length == 4)
                {
                    var c = cmd[3];
                    switch (c)
                    {
                        case 'f':
                            expandFiles = true;
                            break;
                        case 'l':
                        case 's':
                        case 'c':
                            mode = c;
                            break;
                        default:
                            Console.WriteLine($"Unexpected output mode: '{c}'");
                            PrintUsage();
                            return;
                    }
                }
                if (isAny)
                {
                    if (tags != null)
                    {
                        nodes = tagRepo.FindAllNodesContainingAtLeastOne(tags);
                    }
                    else
                    {
                        nodes = tagRepo.TagMapping.Values.Aggregate(new HashSet<TagNode>(), (x,y)=>{x.UnionWith(y); return x;});
                    }
                }
                else    // is All
                {
                    if (tags != null)
                    {
                        nodes = tagRepo.FindAllNodesContainingAll(tags);
                    }
                    else
                    {
                        Console.WriteLine("Must provide tags.");
                        return;
                    }
                }
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

            // Shows the folder immediately as the hardlink creation involving downloading may take time
            System.Diagnostics.Process.Start("explorer.exe", viewDir);

            var clashResolver = new NameClashResolver();
            foreach (var node in nodes)
            {
                var dir = new DirectoryInfo(node.Directory);
                if (expandFiles)
                {
                    var dirName = dir.Name;
                    var prefix = clashResolver.New(dirName) + "-";
                    // TODO test this with subfolders
                    foreach (var item in node.EnumerateContents())
                    {
                        var rel = GetRelative(item.FullName, dir.FullName);
                        var viewLink = Path.Combine(viewDir, prefix + rel);
                        if (item is FileInfo fileInfo)
                        {
                            CreateFileLink(viewLink, fileInfo.FullName, hardLink:true);
                        }
                        else if (item is DirectoryInfo dirInfo)
                        {
                            Directory.CreateDirectory(viewLink);
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
                    switch (mode)
                    {
                        case 'l':
                            CreateShortcut(Path.Combine(viewDir, name + ".lnk"), node.Directory);
                            break;
                        case 's':
                            CreateDirSymLink(Path.Combine(viewDir, name), node.Directory);
                            break;
                        case 'c':
                            CreateCmdLink(Path.Combine(viewDir, name + ".cmd"), node.Directory);
                            break;
                    }
                }
            }
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
        static void CreateFileLink(string source, string target, bool hardLink)
        {
            var hardLinkArg = hardLink? " /h" : "";
            System.Diagnostics.Process.Start("cmd.exe", "/c mklink" + hardLinkArg + " \"" + source + "\" \"" + target + "\"");
        }

        static TagRepo GetTagRepo(string dir)
        {
            var tagScanner = new TagScanner(dir);
            var tagRepo = tagScanner.Scan();
            return tagRepo;
        }
    }
}