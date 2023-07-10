// See https://aka.ms/new-console-template for more information


using Taglite;

if (args.Length != 4)
{
    PrintUsage();
    return;
}

var inputRootDir = args[0];
var tagListString = args[1];
var anyOrAll = args[2].ToLower();
var outputDir = args[3];

if (!Directory.Exists(inputRootDir))
{
    Console.WriteLine($"input-root-dir '{inputRootDir}' does not exist");
    PrintUsage();
    return;
}
if (!Directory.Exists(outputDir))
{
    Directory.CreateDirectory(outputDir);
    if (!Directory.Exists(outputDir))
    {
        Console.WriteLine($"output-dir '{outputDir}' does not exist and cannot be created");
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

var tagScanner = new TagScanner(inputRootDir);
var tagRepo = tagScanner.Scan();
HashSet<string> dirs;
if (isAll)
{
    dirs = tagRepo.FindAllDirectoriesContainingAll(tags);
}
else
{
    dirs = tagRepo.FindAllDirectoreisContainingAny(tags);
}

foreach (var dirStr in dirs)
{
    var dir = new DirectoryInfo(dirStr);
    var dirName = dir.Name;
    System.Diagnostics.Process.Start("cmd.exe", "/c mklink /d \"" + Path.Combine(outputDir, dirName) + "\" \"" + dirStr + "\"");
}

void PrintUsage()
{
    Console.WriteLine("<input-root-dir> <tag-list-string> <any|all> <output-dir>");
    Console.WriteLine(" input-root-dir: the directory contains all the subdirectories to search for the tags from.");
    Console.WriteLine(" tag-list-string: tags separate by commas.");
    Console.WriteLine(" any|all: whether to find the directories that contain any or all of the tags in the list");
    Console.WriteLine(" output-dir: the directory where the shortcuts to all the found directories are put");
}