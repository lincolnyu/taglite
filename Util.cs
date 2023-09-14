namespace Taglite
{
    static class Util
    {
        public static string GetRelative(string source, string sourceBase)
            => source.Substring(sourceBase.Length).TrimStart(Path.PathSeparator);

        public static string? TryGetArg(string[] args, int index, string? defaultStr=null)=>args.Length > index? args[index] : defaultStr;

        public static string? CompleteDir(string? inputDir, string backupEnvVariable)
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
    }
}