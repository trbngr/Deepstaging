using System.Text.Json;
using System.Text.Json.Nodes;

namespace Deepstaging.Text.Json;

public static class JsonExtensions
{
    /// <summary>
    /// Merges the specified Json Node into the base JsonNode for which this method is called.
    /// It is null safe and can be easily used with null-check & null coalesce operators for fluent calls.
    /// NOTE: JsonNodes are context aware and track their parent relationships therefore to merge the values both JsonNode objects
    ///         specified are mutated. The Base is mutated with new data while the source is mutated to remove reverences to all
    ///         fields so that they can be added to the base.
    ///
    /// Source taken directly from the open-source Gist here:
    /// https://gist.github.com/cajuncoding/bf78bdcf790782090d231590cbc2438f
    ///
    /// </summary>
    /// <param name="jsonBase"></param>
    /// <param name="merge"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static JsonNode? Merge(this JsonNode? jsonBase, JsonNode? merge)
    {
        if (jsonBase == null || merge == null)
            return jsonBase;

        switch (jsonBase)
        {
            case JsonObject baseObj when merge is JsonObject jsonMergeObj:
            {
                //NOTE: We must materialize the set (e.g. to an Array), and then clear the merge array so the node can then be 
                //      re-assigned to the target/base Json; clearing the Object seems to be the most efficient approach...
                var mergeNodesArray = jsonMergeObj.ToArray();
                jsonMergeObj.Clear();

                foreach (var prop in mergeNodesArray)
                {
                    baseObj[prop.Key] = baseObj[prop.Key] switch
                    {
                        JsonObject childObj when prop.Value is JsonObject obj => childObj.Merge(obj),
                        JsonArray childArray when prop.Value is JsonArray arr => childArray.Merge(arr),
                        _ => prop.Value
                    };
                }

                break;
            }
            case JsonArray baseArray when merge is JsonArray mergeArray:
            {
                //NOTE: We must materialize the set (e.g. to an Array), and then clear the merge array,
                //      so they can then be re-assigned to the target/base Json...
                var mergeNodesArray = mergeArray.ToArray();
                mergeArray.Clear();
                foreach (var mergeNode in mergeNodesArray) baseArray.Add(mergeNode);
                break;
            }
            default:
                throw new ArgumentException(
                    $"The JsonNode type [{jsonBase.GetType().Name}] is incompatible for merging with the target/base " +
                    $"type [{merge.GetType().Name}]; merge requires the types to be the same.");
        }

        return jsonBase;
    }

    /// <summary>
    /// Merges the specified Dictionary of values into the base JsonNode for which this method is called.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="jsonBase"></param>
    /// <param name="dictionary"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static JsonNode? MergeDictionary<TKey, TValue>(this JsonNode jsonBase, IDictionary<TKey, TValue> dictionary,
        JsonSerializerOptions? options = null)
        => jsonBase.Merge(JsonSerializer.SerializeToNode(dictionary, options));
}