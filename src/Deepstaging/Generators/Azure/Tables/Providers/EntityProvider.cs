using Deepstaging.Generators.Azure.Tables.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Deepstaging.Generators.Azure.Tables.Providers;

using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

internal static class EntityProvider
{
    private static string AttributeFqn => "Deepstaging.Azure.Tables.AzureTableEntityAttribute";

    public static IncrementalValuesProvider<EntityInfo> ForAzureTableEntityAttributes(this SyntaxValueProvider provider) =>
        provider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: AttributeFqn,
                predicate: (node, _) => IsMatch(node),
                transform: (ctx, _) => GetEntity(ctx))
            .Where(static info => info.HasValue)
            .Select(static (info, _) => info!.Value)
            .WithTrackingName(TrackingNames.DeepstagingEntities);

    private static bool IsMatch(SyntaxNode node)
    {
        if (node is not TypeDeclarationSyntax type)
            return false;

        if (type.Kind() is not (ClassDeclaration or StructDeclaration or RecordDeclaration))
            return false;

        return type.Modifiers.Any(m => m.IsKind(PartialKeyword));
    }

    private static EntityInfo? GetEntity(GeneratorAttributeSyntaxContext context)
    {
        var syntax = (TypeDeclarationSyntax)context.TargetNode;
        var symbol = context.SemanticModel.GetDeclaredSymbol(syntax);

        var attribute = context.Attributes.Single(attr => attr.AttributeClass?.ToDisplayString() == AttributeFqn);

        if (symbol is not INamedTypeSymbol typeSymbol)
            return null;

        return EntityInfo.From(typeSymbol, attribute);
    }
}