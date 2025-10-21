using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Deepstaging.Generators.Http.HttpClient.Providers;

using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

public static class HttpClientAttributeProvider
{
    private static string AttributeFqn => "Deepstaging.HttpClient.HttpClientAttribute";

    public static IncrementalValuesProvider<Models.HttpClient> ForHttpClientAttributes(this SyntaxValueProvider provider) =>
        provider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: AttributeFqn,
                predicate: (node, _) => IsMatch(node),
                transform: (ctx, _) => GetEntity(ctx))
            .Where(static info => info.HasValue)
            .Select(static (info, _) => info!.Value)
            .WithTrackingName(TrackingNames.HttpClientAttribute);

    private static bool IsMatch(SyntaxNode node)
    {
        if (node is not TypeDeclarationSyntax type)
            return false;

        if (type.Kind() is not ClassDeclaration)
            return false;

        return type.Modifiers.Any(m => m.IsKind(PartialKeyword));
    }

    private static Models.HttpClient? GetEntity(GeneratorAttributeSyntaxContext context)
    {
        var syntax = (TypeDeclarationSyntax)context.TargetNode;
        var symbol = context.SemanticModel.GetDeclaredSymbol(syntax);

        var attribute = context.Attributes.Single(attr => attr.AttributeClass?.ToDisplayString() == AttributeFqn);

        if (symbol is not INamedTypeSymbol typeSymbol)
            return null;

        return Models.HttpClient.From(typeSymbol, attribute);
    }
}