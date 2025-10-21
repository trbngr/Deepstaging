using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Nodes;
using Deepstaging.CodeAnalysis;
using Deepstaging.Text.Json;
using Microsoft.CodeAnalysis;

namespace Deepstaging.Generators.Configuration.Models;

using static JsonSchemaType;
using static NullableAnnotation;

public static class JsonSchema
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerOptions.Default)
        { WriteIndented = true };

    public static string RenderExampleFile(ImmutableArray<ConfigurationRegistrationInfo> registrations,
        JsonSchemaType schemaType)
    {
        var schemata = registrations
            .GroupBy(info => info.ConfigurationSection)
            .OrderBy(group => group.Key)
            .Select(group => (Section: group.Key, Schema: CreateExampleJson(group.First(), schemaType)));

        return RenderExampleFile(schemata);
    }

    private static string RenderExampleFile(IEnumerable<(string Section, JsonObject? Schema)> registrations)
    {
        var root = new JsonObject();
        foreach (var (section, schema) in registrations)
        {
            if (schema is null)
                continue;

            var sectionParts = section.Split(':');
            var current = root;
            foreach (var part in sectionParts)
            {
                if (!current.TryGetPropertyValue(part, out var childNode) || childNode is not JsonObject childObj)
                {
                    childObj = new JsonObject();
                    current[part] = childObj;
                }

                current = childObj;
            }

            foreach (var prop in schema)
            {
                current[prop.Key] = prop.Value?.DeepClone();
            }
        }

        return root.ToJsonString(SerializerOptions);
    }

    public static string RenderJsonSchema(ImmutableArray<ConfigurationRegistrationInfo> registrations,
        JsonSchemaType schemaType)
    {
        var schemata = registrations
            .GroupBy(info => info.ConfigurationSection)
            .OrderBy(group => group.Key)
            .Select(group => (Section: group.Key, Schema: CreateSchema(group.First(), schemaType)));

        var required = registrations
            .Select(x => x.ConfigurationSection.Split(':').First())
            .Distinct()
            .Aggregate(new JsonArray(), (array, s) => array.AddItem(s));

        return MergeSchemata(schemata, new JsonObject
        {
            ["$id"] = "root",
            ["$schema"] = "http://json-schema.org/draft-07/schema#",
            ["type"] = "object",
            ["additionalProperties"] = true,
            ["properties"] = new JsonObject(),
            ["required"] = required
        }).ToJsonString(SerializerOptions);
    }

    private static JsonObject? CreateSchema(ConfigurationRegistrationInfo info, JsonSchemaType schemaType)
    {
        var props = Filter(info.Properties, schemaType);

        if (props.IsDefaultOrEmpty)
            return null;

        var required = props.Aggregate(new JsonArray(), (acc, prop) => acc.AddItem(prop.Name));
        var properties = props.Aggregate(new JsonObject(),
            (acc, prop) => acc.AddChild(prop.Name, CreateProp(prop, schemaType)));

        return new JsonObject
        {
            ["type"] = "object",
            ["description"] = info.Description,
            ["additionalProperties"] = false,
            ["properties"] = properties,
            ["required"] = required
        };
    }

    private static JsonObject? CreateExampleJson(ConfigurationRegistrationInfo info, JsonSchemaType schemaType)
    {
        var root = new JsonObject();
        var props = Filter(info.Properties, schemaType);

        if (props.IsDefaultOrEmpty)
            return null;

        foreach (var property in props)
            root.Add(property.Name,
                IsSupportedType(property)
                    ? GetDefaultValue(property)
                    : CreateProp(property, schemaType)
            );

        return root;
    }

    private static JsonNode? GetDefaultValue(IPropertySymbol property) => property.Type.Name switch
    {
        "String" => "",
        "Byte[]" => "base64-encoded-string",
        "Boolean" => false,
        "DateTime" => "2024-01-01T00:00:00Z",
        "DateTimeOffset" => "2024-01-01T00:00:00+00:00",
        "Double" => 0.0,
        "Guid" => "00000000-0000-0000-0000-000000000000",
        "Int32" or "Int64" or "Int16" or "Byte" or "SByte" or "UInt16" or "UInt32" or "UInt64" => 0,
        _ => null
    };

    private static ImmutableArray<IPropertySymbol> Filter(IEnumerable<IPropertySymbol> props, JsonSchemaType type) =>
        type switch
        {
            Secrets => [..props.Where(p => p.IsSecret())],
            AppSettings => [..props.Where(p => p.IsNotSecret())],
            _ => throw new InvalidOperationException($"Unsupported schema type {type}")
        };

    private static JsonObject? CreateProp(IPropertySymbol property, JsonSchemaType schemaType)
    {
        if (IsSupportedType(property))
            return CreatSimpleProp(property);
        return CreateSchema(ConfigurationRegistrationInfo.From((INamedTypeSymbol)property.Type), schemaType);
    }

    private static bool IsSupportedType(IPropertySymbol propertyType) => propertyType.Type.Name switch
    {
        "String" or
            "Byte[]" or
            "Boolean" or
            "DateTime" or
            "DateTimeOffset" or
            "Double" or
            "Guid" or
            "Int32" or
            "Int64" or
            "Int16" or
            "Byte" or
            "SByte" or
            "UInt16" or
            "UInt32" or
            "UInt64" => true,
        _ => false
    };

    private static JsonObject CreatSimpleProp(IPropertySymbol property)
    {
        return new JsonObject
        {
            ["type"] = GetPropertyType(property),
            ["description"] = GetDescription(property)
        };
    }

    private static string GetDescription(IPropertySymbol property)
    {
        var description = property.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "DescriptionAttribute");

        if (description is null || description.ConstructorArguments.Length == 0)
            return string.Empty;

        return description.ConstructorArguments[0].Value?.ToString() ?? string.Empty;
    }

    private static JsonNode GetPropertyType(IPropertySymbol property)
    {
        return property.Type switch
        {
            { IsValueType: true } type => type.NullableAnnotation switch
            {
                Annotated => new JsonArray("null", MapType(property.Type.Name)),
                _ => MapType(property.Type.Name)
            },
            { IsValueType: false } type => type.NullableAnnotation switch
            {
                Annotated => new JsonArray("null", "string"),
                _ => "string"
            },
            _ => throw new InvalidOperationException(
                $"Unsupported property type {property.Type} for property {property.Name}")
        };

        string MapType(string typeName)
        {
            return typeName switch
            {
                "String" => "string",
                "Byte[]" => "string", // Base64-encoded
                "Boolean" => "boolean",
                "DateTime" => "string", // ISO 8601
                "DateTimeOffset" => "string", // ISO 8601
                "Double" => "number",
                "Guid" => "string", // GUID format
                "Int32" or "Int64" or "Int16" or "Byte" or "SByte" or "UInt16" or "UInt32" or "UInt64" => "integer",
                _ => throw new InvalidOperationException(
                    $"Unsupported property type {typeName} for property {property.Name}")
            };
        }
    }

    private static JsonObject MergeSchemata(IEnumerable<(string Section, JsonObject? Schema)> schemata, JsonObject root)
    {
        JsonObject properties = (JsonObject)root["properties"]!;

        foreach (var (section, schema) in schemata)
        {
            if (schema is null)
                continue;

            var sectionParts = section.Split(':');
            var current = properties;
            foreach (var (part, index) in sectionParts.Select((p, i) => (p, i)))
            {
                if (!current.TryGetPropertyValue(part, out var childNode) || childNode is not JsonObject childObj)
                {
                    childObj = new JsonObject
                    {
                        ["type"] = "object",
                        ["additionalProperties"] = false,
                        ["properties"] = new JsonObject(),
                        ["required"] = new JsonArray()
                    };
                    current[part] = childObj;
                }

                // Add the next part to required if there is one
                if (index + 1 < sectionParts.Length)
                {
                    var requiredArray = (JsonArray)childObj["required"]!;
                    var nextPart = sectionParts[index + 1];
                    if (!requiredArray.Any(x => x?.ToString() == nextPart))
                        requiredArray.Add(nextPart);
                }

                current = (JsonObject)childObj["properties"]!;
            }

            current.Parent!["required"] = schema["required"]!.AsArray().DeepClone();
            current.Parent!["description"] = schema["description"]?.GetValue<string>() ?? string.Empty;

            foreach (var prop in schema["properties"]!.AsObject())
            {
                current[prop.Key] = prop.Value!.DeepClone();
            }
        }

        return root;
    }
}

public enum JsonSchemaType
{
    AppSettings,
    Secrets
}

