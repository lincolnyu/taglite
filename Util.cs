namespace Taglite
{
    static class Util
    {
        public static string GetRelative(string source, string sourceBase)
            => source.Substring(sourceBase.Length).TrimStart(Path.PathSeparator);

        public static string? TryGetArg(string[] args, int index, string? defaultStr=null)=>args.Length > index? args[index] : defaultStr;

    }
}