// See https://aka.ms/new-console-template for more information


using System.Text;
using Taglite;

if (args.Length < 1)
{
    PrintUsage();
    return;
}

if (args.Length == 1 && args[0] == "-h")
{
    PrintUsage();
    return;
}

var inputRootDir = args[0];

if (args.Length == 2 && args[1].ToLower() == "all")
{
    var tagRepo = GetTagRepo(inputRootDir);
    Console.WriteLine("All tags:");
    foreach (var tag in tagRepo.TagMapping.Keys.Order())
    {
        Console.Write(" ");
        Console.Write(tag);
    }
    return;
}

if (args.Length != 4)
{
    PrintUsage();
    return;
}


var tagListString = args[1];
var anyOrAll = args[2].ToLower();
var outputDir = args[3];

if (!Directory.Exists(inputRootDir))
{
    Console.WriteLine($"<input-root-dir> '{inputRootDir}' does not exist");
    PrintUsage();
    return;
}
if (!Directory.Exists(outputDir))
{
    Directory.CreateDirectory(outputDir);
    if (!Directory.Exists(outputDir))
    {
        Console.WriteLine($"<output-dir> '{outputDir}' does not exist and cannot be created");
        PrintUsage();
        return;
    }
}

bool isAll = false;
switch (anyOrAll)
{
    case "any": isAll = false; break;
    case "all": isAll = true; break;
    default:
        PrintUsage();
        return;
}

var tags = tagListString.Split(',', StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries).Select(x=>x.ToLower());

HashSet<TagNode> nodes;
{
    var tagRepo = GetTagRepo(inputRootDir);
    if (isAll)
    {
        nodes = tagRepo.FindAllNodesContainingAll(tags);
    }
    else
    {
        nodes = tagRepo.FindAllNodesContainingAny(tags);
    }
}

foreach (var node in nodes)
{
    var dir = new DirectoryInfo(node.Directory);
    // TODO protect against long file name
    var sb = new StringBuilder(dir.Name);
    foreach (var tag in node.Tags.Order())
    {
        sb.Append(".");
        sb.Append(tag);
    }
    System.Diagnostics.Process.Start("cmd.exe", "/c mklink /d \"" + Path.Combine(outputDir, sb.ToString()) + "\" \"" + node.Directory + "\"");
}

TagRepo GetTagRepo(string dir)
{
    var tagScanner = new TagScanner(dir);
    var tagRepo = tagScanner.Scan();
    return tagRepo;
}

void PrintUsage()
{
    Console.WriteLine("Usage 1: <input-root-dir> <tag-list-string> <any|all> <output-dir>");
    Console.WriteLine(" input-root-dir: The directory contains all the subdirectories to search for the tags from.");
    Console.WriteLine(" tag-list-string: Tags separate by commas.");
    Console.WriteLine(" any|all: Whether to find the directories that contain any or all of the tags in the list.");
    Console.WriteLine(" output-dir: The directory where the shortcuts to all the found directories are put.");
    Console.WriteLine("Usage 2: <input-root-dir> all");
    Console.WriteLine(" To list all tags in alphabetic order.");
    Console.WriteLine("Show this help: -h");
}