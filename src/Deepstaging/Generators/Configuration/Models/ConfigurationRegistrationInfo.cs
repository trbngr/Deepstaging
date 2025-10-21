using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Deepstaging.Generators.Configuration.Models;

using static Accessibility;
using static SymbolDisplayFormat;

public readonly record struct ConfigurationRegistrationInfo
{
    public static ConfigurationRegistrationInfo From(INamedTypeSymbol symbol, AttributeData? data = null)
    {
        var configuredSection = data?.ConstructorArguments.Length > 0
            ? data.ConstructorArguments[0].Value as string
            : null;

        var fullTypeName = symbol.ToDisplayString(CSharpErrorMessageFormat);

        return new ConfigurationRegistrationInfo
        {
            TypeName = symbol.Name,
            FullTypeName = fullTypeName,
            ConfigurationSection = configuredSection ?? fullTypeName.Replace(".", ":"),
            ConfiguredSection = configuredSection,
            Description = symbol.GetAttributes()
                              .FirstOrDefault(attr =>
                                  attr.AttributeClass?.ToDisplayString() ==
                                  "System.ComponentModel.DescriptionAttribute")
                              ?.ConstructorArguments.FirstOrDefault().Value as string
                          ?? $"{symbol.Name} Configuration",
            Namespace = symbol.ContainingNamespace.IsGlobalNamespace
                ? string.Empty
                : symbol.ContainingNamespace.ToDisplayString(),
            Properties =
            [
                ..symbol.GetMembers()
                    .OfType<IPropertySymbol>()
                    .Where(p => !p.IsStatic && p.DeclaredAccessibility == Public)
            ]
        };
    }

    public string Description { get; init; }
    public string ConfigurationSection { get; init; }
    private string? ConfiguredSection { get; init; }
    public string FullTypeName { get; init; }
    public string TypeName { get; init; }
    public string Namespace { get; init; }
    public ImmutableArray<IPropertySymbol> Properties { get; init; }
}