using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Deepstaging.SourceGenerators.Generators.Azure.Tables.Models;

using static TypeKind;

internal readonly record struct EntityInfo
{
    public static EntityInfo From(INamedTypeSymbol symbol, AttributeData attribute)
    {
        var allProperties = symbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => !p.IsStatic && p.DeclaredAccessibility == Accessibility.Public)
            .Select(ToPropertyInfo)
            .ToImmutableList();

        var partitionKey = (string)attribute.ConstructorArguments[0].Value!;
        var rowKey = (string)attribute.ConstructorArguments[1].Value!;

        return new EntityInfo
        {
            PartitionKey = partitionKey,
            RowKey = rowKey,
            Name = symbol.Name,
            Namespace = symbol.ContainingNamespace.ToDisplayString(),
            AllProperties = allProperties,
            PropertiesWithoutKeys = allProperties
                .Where(p => p.Name != partitionKey && p.Name != rowKey) // Exclude PK and RK
                .ToImmutableList(),
            TypeKind = symbol switch
            {
                { IsRecord: true, TypeKind: Struct } => "record struct",
                { IsRecord: true } => "record",
                { TypeKind: Class } => "class",
                { TypeKind: Struct } => "struct",
                _ => throw new InvalidOperationException($"Unexpected symbol {symbol}")
            }
        };
    }

    public record struct PropertyInfo(string Name, string Type, string SerializedValue, string DeserializedValue);

    public string PartitionKey { get; init; }
    public string RowKey { get; init; }

    public string Name { get; init; }
    public string Namespace { get; init; }

    public ImmutableList<PropertyInfo> AllProperties { get; init; }

    public ImmutableList<PropertyInfo> PropertiesWithoutKeys { get; init; }
    public string TypeKind { get; init; }

    private static readonly HashSet<string> SupportedTableTypes =
    [
        "string",
        "byte[]",
        "bool",
        "global::System.DateTime",
        "global::System.DateTimeOffset",
        "double",
        "global::System.Guid",
        "int",
        "long",
        // Nullable variants
        "string?",
        "byte[]?",
        "bool?",
        "global::System.DateTime?",
        "global::System.DateTimeOffset?",
        "double?",
        "global::System.Guid?",
        "int?",
        "long?",
    ];

    private static string GetValueAccessor(string typeName) => typeName switch
    {
        "string" => "GetRequiredString",
        "string?" => "GetString",
        "byte[]" => "GetRequiredBinary",
        "byte[]?" => "GetBinary",
        "bool" => "GetRequiredBoolean",
        "bool?" => "GetBoolean",
        "global::System.DateTime" => "GetRequiredDateTime",
        "global::System.DateTime?" => "GetDateTime",
        "global::System.DateTimeOffset" => "GetRequiredDateTimeOffset",
        "global::System.DateTimeOffset?" => "GetDateTimeOffset",
        "double" => "GetRequiredDouble",
        "double?" => "GetDouble",
        "global::System.Guid" => "GetRequiredGuid",
        "global::System.Guid?" => "GetGuid",
        "int" => "GetRequiredInt",
        "int?" => "GetInt32",
        "long" => "GetRequiredLong",
        "long?" => "GetInt64",
        _ => throw new InvalidOperationException($"Unsupported type {typeName}"),
    };


    private static PropertyInfo ToPropertyInfo(IPropertySymbol property)
    {
        var typeName = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var isSupported = SupportedTableTypes.Contains(typeName);

        return new PropertyInfo(
            Name: property.Name,
            Type: typeName,
            SerializedValue: !isSupported ? $"Serialize({property.Name})" : property.Name,
            DeserializedValue: !isSupported
                ? $"Deserialize<{typeName}>(entity.GetRequiredString(\"{property.Name}\"))!"
                : $"entity.{GetValueAccessor(typeName)}(\"{property.Name}\")"
        );
    }
}