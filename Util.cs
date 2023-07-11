namespace Taglite
{
    static class Util
    {
        public static string GetRelative(string source, string sourceBase)
            => source.Substring(sourceBase.Length).TrimStart(Path.PathSeparator);
    }
}