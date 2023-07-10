namespace Taglite
{
    class TagRepo
    {
        // Tag to all containing directories
        public readonly Dictionary<string, HashSet<string>> TagMapping = new Dictionary<string, HashSet<string>>();

        public HashSet<string> FindAllDirectoreisContainingAny(IEnumerable<string> tags)
        {
            var theSet = new HashSet<string>();
            foreach (var tag in tags)
            {
                if (!TagMapping.TryGetValue(tag, out var dirs))
                {
                    continue;
                }
                theSet.UnionWith(dirs);
            }
            return theSet;
        }

        public HashSet<string> FindAllDirectoriesContainingAll(IEnumerable<string> tags)
        {
            HashSet<string>? theSet = null;
            foreach (var tag in tags)
            {
                if (!TagMapping.TryGetValue(tag, out var dirs))
                {
                    theSet = null;
                    break;
                }
                if (theSet == null)
                {
                    theSet = new HashSet<string>(dirs);
                }
                else
                {
                    theSet.IntersectWith(dirs);
                    if (theSet.Count == 0)
                    {
                        break;
                    }
                }
            }
            return theSet?? new HashSet<string>{};
        }
    }
}