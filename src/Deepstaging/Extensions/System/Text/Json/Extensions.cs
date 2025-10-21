using System.Text.Json.Nodes;

namespace Deepstaging.Text.Json;

public static class Extensions
{
    public static JsonObject AddChild(this JsonObject parent, string name, JsonObject? child) 
    {
        if (child is null)
            return parent;
        
        parent.Add(name, child);
        return parent;
    }
        

    public static JsonArray AddItem(this JsonArray parent, JsonNode child)
    {
        parent.Add(child);
        return parent;
    }
}