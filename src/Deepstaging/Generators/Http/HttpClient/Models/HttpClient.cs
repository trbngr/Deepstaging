using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

// ReSharper disable MemberCanBePrivate.Global

namespace Deepstaging.Generators.Http.HttpClient.Models;

public record struct HttpClient
{
    public string Namespace { get; init; }
    public string Name { get; init; }
    public string TypeName { get; init; }

    public bool HasConfigureRequestMethod { get; init; }

    public ImmutableArray<Request> Requests { get; init; }

    public string? ConfigurationType { get; init; }

    public static HttpClient From(INamedTypeSymbol typeSymbol, AttributeData attribute)
    {
        var typeName = typeSymbol.Name;
        var methodSymbols = typeSymbol.GetMembers().OfType<IMethodSymbol>().ToArray();
        var configurationType = attribute.ConstructorArguments[1].Value! as INamedTypeSymbol;

        var hasConfigureRequestMethod = methodSymbols
            .Any(m => m is { Name: "ConfigureRequest", Parameters.Length: 1, IsStatic: true, IsGenericMethod: false } &&
                      m.Parameters[0].Type.ToDisplayString() == $"{typeName}Req");

        HttpClient info = new()
        {
            Namespace = typeSymbol.ContainingNamespace.ToDisplayString(),
            TypeName = typeName,
            Requests =
            [
                ..GetRequests(Verb.Post, "Deepstaging.HttpClient.HttpPostAttribute", typeSymbol)
            ],
            Name = attribute.ConstructorArguments[0].Value?.ToString()!,
            ConfigurationType = configurationType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            HasConfigureRequestMethod = hasConfigureRequestMethod
        };


        return info;

        ImmutableArray<Request> GetRequests(Verb verb, string aname, INamedTypeSymbol symbol)
        {
            var methods = symbol.GetMembers().OfType<IMethodSymbol>().ToArray();
            var withAttrs = methods
                .Where(m =>
                {
                    var attributes = m.GetAttributes();
                    return attributes.Any(attr => attr.AttributeClass?.ToDisplayString() == aname);
                })
                .ToArray();
            return
            [
                ..withAttrs
                    .Select(m => Request.From(verb, m,
                        m.GetAttributes().First(attr => attr.AttributeClass?.ToDisplayString() == aname)))
            ];
        }
    }
}