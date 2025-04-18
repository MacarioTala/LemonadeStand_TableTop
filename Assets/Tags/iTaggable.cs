using System.Collections.Generic;
using System.Linq;

public interface iTaggable
{
    List<string> Tags { get; }
    public List<string> GetTags();
    public void AddTag(string tag);
    public void RemoveTag(string tag);
    public bool HasTag(string tag);
}

public static class iTaggableExtensions
{
     public static bool IsCompatibleWith(this iTaggable self, iTaggable other)
    {
        foreach (var tag in self.Tags)
        {
            if (TagIncompatibilities.Incompatibilities.TryGetValue(tag, out var incompatibleTags))
            {
                if(other.Tags.Any(incompatibleTags.Contains))
                {
                    return false;
                }
            }
        }
        return true;
    }
}