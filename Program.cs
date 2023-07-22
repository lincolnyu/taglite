// See https://aka.ms/new-console-template for more information

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
var needsHelp = args.Contains("-h");

if (cmd == "tag")
{
    if (needsHelp)
    {
        Tagger.PrintUsage();
    }
    else
    {
        Tagger.ProcessArgs(args);
    }
}
else if (cmd == "replace")
{
    if (needsHelp)
    {
        TagReplacer.PrintUsage();
    }
    else
    {
        TagReplacer.ProcessArgs(args);
    }
}
else
{
    if (needsHelp)
    {
        TagView.PrintUsage();
    }
    else
    {
        TagView.ProcessArgs(args);
    }
}

void PrintUsage()
{
    Console.WriteLine(Tagger.UsageString());
    Console.WriteLine(TagView.UsageString());
    Console.WriteLine(TagReplacer.UsageString());
    Console.WriteLine("To show this help: -h");
}
