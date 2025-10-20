using Deepstaging.SourceGenerators.Generators.Azure.Tables.Models;
using Deepstaging.SourceGenerators.Generators.Azure.Tables.Providers;
using Microsoft.CodeAnalysis;
using SGF;

namespace Deepstaging.SourceGenerators.Generators.Azure.Tables;

using static Template;

[IncrementalGenerator]
public class EntityGenerator() : IncrementalGenerator(name: nameof(EntityGenerator))
{
    public override void OnInitialize(SgfInitializationContext context)
    {
        var hint = new HintNameProvider("Deepstaging.Azure.Tables");

        context.RegisterPostInitializationOutput(ctx =>
        {
            // ctx.AddEmbeddedAttributeDefinition();
            ctx.AddSource(hint.Create("AzureTableEntityAttribute"), RenderEntityAttribute());
            ctx.AddSource(hint.Create("AzureTableEntityExtensions"), RenderAzureTableEntityExtensions());
        });

        context.RegisterImplementationSourceOutput(
            source: context.SyntaxProvider.ForDeepstagingEntities().Collect(),
            action:  (ctx, entities) =>
            {
                if (entities.IsDefaultOrEmpty)
                    return;

                ctx.AddSource(hintName: hint.Create("IAzureTableEntity"), source: RenderEntityInterface());

                foreach (var entity in entities)
                {
                    ctx.AddSource(hintName: hint.Create("Entities", entity.Name), source: RenderAzureTableEntity(entity));
                }
            }
        );
    }

    private static string RenderAzureTableEntityExtensions() => RenderTemplate(
        name: "Azure.Tables.Templates.AzureTableEntityExtensions",
        context: new { }
    );

    private static string RenderEntityAttribute() => RenderTemplate(
        name: "Azure.Tables.Templates.AzureTableEntityAttribute",
        context: new { }
    );

    private static string RenderAzureTableEntity(EntityInfo info) => RenderTemplate(
        name: "Azure.Tables.Templates.DeepstagingEntityExtensions",
        context: info
    );

    private static string RenderEntityInterface() => RenderTemplate(
        name: "Azure.Tables.Templates.IAzureTableEntity",
        context: new { }
    );
}