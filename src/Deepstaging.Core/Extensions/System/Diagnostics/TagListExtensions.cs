using System.Diagnostics;

namespace Deepstaging.Diagnostics;

public static class TagListExtensions
{
    public static TagList AddTag(this TagList tags, string key, string value)
    {
        tags.Add(key, value);
        return tags;
    }
}