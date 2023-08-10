using System.Text;
using Taglite.Core;
using static Taglite.Util;

namespace Taglite
{
    class TagReplacer
    {
        public static void Replace(DirectoryInfo dir, IEnumerable<(string,string)> replacements)
        {
            if (TagNode.IsTaggedDirectory(dir.FullName))
            {
                var tagNode = new TagNode(dir.FullName);
                foreach (var repl in replacements)
                {
                    if (tagNode.Tags.Remove(repl.Item1) && !string.IsNullOrWhiteSpace(repl.Item2))
                    {
                        tagNode.Tags.Add(repl.Item2);
                    }
                }
                tagNode.SaveToFile();
            }
            foreach (var subdir in dir.GetDirectories())
            {
                Replace(subdir, replacements);
            }
        }

        public static void ProcessArgs(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Invalid arguments");
                PrintUsage();
                return;
            }
            var storeDir = CompleteDir(args[1], Constants.EnvVar.StoreDir);
            if (storeDir == null || !Directory.Exists(storeDir))
            {
                Console.WriteLine($"<store-dir> '{storeDir}' is not provided or does not exist.");
                return;
            }
            var repl = new List<(string, string)>();
            var split = args[2].Split(',', StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries).Select(x=>x.ToLower());
            foreach (var pair in split)
            {
                var pairSplit = pair.Split(":");
                if (pairSplit.Length != 2)
                {
                    Console.WriteLine($"Invalid replacement pattern: {pair}");
                    PrintUsage();
                    return;
                }
                repl.Add((pairSplit[0], pairSplit[1]));
            }
            Replace(new DirectoryInfo(storeDir!), repl);
        }

        public static void PrintUsage()
        {
            Console.WriteLine($"{UsageString()}");
        }

        internal static string UsageString()
        {
            var sb = new StringBuilder("=== TagReplacer ===\n");
            sb.AppendLine("replace <store-dir> <orig-tag1>:<new-tag1>,<orig-tag2>:<new-tag2>,...");
            sb.AppendLine($" <store-dir>: The directory contains all the subdirectories to search for the tags from. When absent, {Constants.EnvVar.StoreDir} env variable is used.");
            sb.Append(" orig-tag?:new-tag?: orig-tag? is to be replaced by new-tag?");
            return sb.ToString();
        }
    }
}