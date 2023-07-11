namespace Taglite
{
    class TagRepo
    {
        // Tag to all containing directories
        public readonly Dictionary<string, HashSet<TagNode>> TagMapping = new Dictionary<string, HashSet<TagNode>>();

        public void AddNode(TagNode node)
        {
            foreach(var tag in node.Tags)
            {
                if (!TagMapping.TryGetValue(tag, out var taggedNodes))
                {
                    taggedNodes = new HashSet<TagNode>();
                    TagMapping[tag] = taggedNodes;
                }
                taggedNodes.Add(node);
            }
        }

        public void AddDirectoryIfTagged(string directory)
        {
            var tagFile = Path.Combine(directory, ".taglite");
            if (File.Exists(tagFile))
            {
                AddNode(new TagNode(directory));
            }
        }

        public HashSet<TagNode> FindAllNodesContainingAtLeastOne(IEnumerable<string> tags)
            => tags.Aggregate(new HashSet<TagNode>(), (theSet, tag)=>
            {
                if (TagMapping.TryGetValue(tag, out var dirs))
                {
                    theSet.UnionWith(dirs);
                }
                return theSet;
            });

        public HashSet<TagNode> FindAllNodesContainingAll(IEnumerable<string> tags)
        {
            HashSet<TagNode>? theSet = null;
            foreach (var tag in tags)
            {
                if (!TagMapping.TryGetValue(tag, out var dirs))
                {
                    theSet = null;
                    break;
                }
                if (theSet == null)
                {
                    theSet = new HashSet<TagNode>(dirs);
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
            return theSet?? new HashSet<TagNode>();
        }
    }
}