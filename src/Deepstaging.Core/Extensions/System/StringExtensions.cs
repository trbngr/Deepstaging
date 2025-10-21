using YamlDotNet.Serialization;

namespace Deepstaging;

public static class StringExtensions
{
    public static string YamlToJson(this string value)
    {
        var deserializer = new DeserializerBuilder()
            .Build();

        var yamlObject = deserializer.Deserialize(new StringReader(value));

        var serializer = new SerializerBuilder()
            .JsonCompatible()
            .Build();

        return serializer.Serialize(yamlObject);
    }
}