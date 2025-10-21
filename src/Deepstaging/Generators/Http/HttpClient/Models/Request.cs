using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.SymbolDisplayFormat;

namespace Deepstaging.Generators.Http.HttpClient.Models;

public readonly record struct RequestArgument(string Type, string Name);

public enum Verb
{
    Post
}

public readonly record struct Request(
    Verb Verb,
    string Name,
    string Path,
    string ReturnType,
    string UnwrappedReturnType,
    ImmutableArray<RequestArgument> Args)
{
    public string ArgumentTypes => string.Join(", ", Args.Select(arg => arg.Type));
    public string Arguments { get; init; } = string.Join(", ", Args.Select(arg => $"{arg.Type} {arg.Name}"));
    public string ArgumentsForCall => string.Join(", ", Args.Select(arg => arg.Name));

    public string Tags => string.Join(",\n", Args.Select(arg => "{ \"" + arg.Name + "\", " + arg.Name + " }"));

    public string BodyFactory => $"({ArgumentsForCall}) => new {{ {ArgumentsForCall} }}";
    
    public bool ReturnsValue => UnwrappedReturnType != "global::System.Threading.Tasks.Task";

    public static Request From(Verb verb, IMethodSymbol symbol, AttributeData attribute)
    {
        return new(
            Verb: verb,
            Name: symbol.Name,
            Path: attribute.ConstructorArguments[0].Value?.ToString()!,
            ReturnType: symbol.ReturnType.ToDisplayString(FullyQualifiedFormat),
            UnwrappedReturnType: UnwrapTask(symbol.ReturnType),
            Args:
            [
                ..symbol.Parameters
                    .Select(p => new RequestArgument(p.Type.ToDisplayString(FullyQualifiedFormat), p.Name))
            ]
        );

        string UnwrapTask(ITypeSymbol returnType)
        {
            return returnType switch
            {
                INamedTypeSymbol { IsGenericType: true } namedType when namedType.ConstructedFrom.ToDisplayString() == "System.Threading.Tasks.Task<TResult>" =>
                    namedType.TypeArguments[0].ToDisplayString(FullyQualifiedFormat),
                _ => returnType.ToDisplayString(FullyQualifiedFormat)
            };
        }
    }
}