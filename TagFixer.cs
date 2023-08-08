using System.Text;
using Taglite.Core;
using static Taglite.Util;

namespace Taglite
{
    class TagFixer
    {

        static void Fix(string storeDir)
        {
            foreach (var node in TagScanner.EnumerateAllTagNodes(new DirectoryInfo(storeDir)))
            {
                if (!node.Validate())
                {
                    node.SaveToFile();
                    Console.WriteLine($"Fixed {node.Directory}.");
                }
            }
        }

        public static void ProcessArgs(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Invalid arguments");
                PrintUsage();
                return;
            }
            var storeDir = CompleteDir(args[1], "taglite_store");
            if (storeDir == null || !Directory.Exists(storeDir))
            {
                Console.WriteLine($"<store-dir> '{storeDir}' is not provided or does not exist.");
                return;
            }
            Fix(storeDir);
        }

        public static void PrintUsage()
        {
            Console.WriteLine($"{UsageString()}");
        }

        internal static string UsageString()
        {
            var sb = new StringBuilder("=== TagFixer ===\n");
            sb.AppendLine("fix <store-dir>");
            sb.AppendLine(" <store-dir>: The directory where the tags are to be fixed. When absent taglite_store env variable is used.");
            return sb.ToString();
        }
    }
}