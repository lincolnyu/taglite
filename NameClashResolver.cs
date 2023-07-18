namespace Taglite
{
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
}
