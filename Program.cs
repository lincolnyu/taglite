// See https://aka.ms/new-console-template for more information

using System.Text;
using Taglite;
using static Taglite.Util;


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

if (cmd == "tag")
{
    Tagger.ProcessArgs(args);
}
if (cmd == "replace")
{
    TagReplacer.ProcessArgs(args);
}
else
{
    TagView.ProcessArgs(args);
}

void PrintUsage()
{
    Console.WriteLine(Tagger.UsageString());
    Console.WriteLine(TagView.UsageString());
    Console.WriteLine(TagReplacer.UsageString());
    Console.WriteLine("To show this help: -h");
}
