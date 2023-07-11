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

var cmd = args[0].Trim().ToLower();
if (cmd == "alltags")
{
    var storeDirToShowAllTags = CompleteDir(TryGetArg(1), "taglite_store");
    if (storeDirToShowAllTags == null)
    {
        Console.WriteLine($"<store-dir> '{storeDirToShowAllTags}' is not provided or does not exist.");
        return;
    }
    var tagRepo = GetTagRepo(storeDirToShowAllTags);
    Console.WriteLine("All tags:");
    foreach (var tag in tagRepo.TagMapping.Keys.Order())
    {
        Console.Write(" ");
        Console.Write(tag);
    }
    return;
}

if (args.Length < 1 || args.Length > 4)
{
    PrintUsage();
    return;
}

var viewDir = CompleteDir(TryGetArg(2), "taglite_view");
var storeDir = CompleteDir(TryGetArg(3), "taglite_store");

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
if (!Directory.Exists(viewDir))
{
    Directory.CreateDirectory(viewDir);
    if (!Directory.Exists(viewDir))
    {
        Console.WriteLine($"<view-dir> '{viewDir}' does not exist and cannot be created.");
        return;
    }
}

HashSet<TagNode> nodes;
bool expandFiles = false;
{
    var tagRepo = GetTagRepo(storeDir);
    var tagListString = TryGetArg(1);
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
            var rel = Util.GetRelative(item.FullName, dir.FullName);
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
        System.Diagnostics.Process.Start("cmd.exe", "/c mklink /d \"" + Path.Combine(viewDir, name) + "\" \"" + node.Directory + "\"");
    }
}

TagRepo GetTagRepo(string dir)
{
    var tagScanner = new TagScanner(dir);
    var tagRepo = tagScanner.Scan();
    return tagRepo;
}

string? CompleteDir(string? inputDir, string backupEnvVariable)
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

string? TryGetArg(int index, string? defaultStr=null)=>args.Length > index? args[index] : defaultStr;

void PrintUsage()
{
    Console.WriteLine("Usage 1: (any|all)(f|) [<tag-list-string>] [<view-dir>] [<store-dir>]");
    Console.WriteLine(" any|all: Whether to find the directories that contain any or all of the tags in the list.");
    Console.WriteLine(" f: Create file symlinks instead.");
    Console.WriteLine(" <tag-list-string>: Tags separate by commas. Optional only if 'any' is chosen.");
    Console.WriteLine(" <view-dir>: The directory where the shortcuts to all the found directories are put.");
    Console.WriteLine(" <store-dir>: The directory contains all the subdirectories to search for the tags from.");
    Console.WriteLine("Usage 2: alltags [<store-dir>]");
    Console.WriteLine(" To list all the tags from <store-dir> in alphabetic order.");
    Console.WriteLine("To show this help: -h");
}

class NameClashResolver
{
    private HashSet<string> _usedNames = new HashSet<string>();
    internal string New(string input)
    {
        int dupSuffix = 0;
        var attempt = input;
        // TODO can be optimized
        while (true)
        {
            if (!_usedNames.Contains(attempt))
            {
                _usedNames.Add(attempt);
                return attempt;
            }
            attempt = input + $"({++dupSuffix})";
        }
    }
}
