using Deepstaging.Generators.Azure.Tables.Providers;
using Microsoft.CodeAnalysis;
using SGF;

namespace Deepstaging.Generators.Azure.Tables;

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
            ctx.AddSource(hint.Create("IAzureTableEntity"),
                RenderTemplate("Azure.Tables.Templates.IAzureTableEntity"));

            ctx.AddSource(hint.Create("AzureTableEntityAttribute"),
                RenderTemplate("Azure.Tables.Templates.AzureTableEntityAttribute"));

            ctx.AddSource(hint.Create("AzureTableEntityExtensions"),
                RenderTemplate("Azure.Tables.Templates.AzureTableEntityExtensions"));
        });

        context.RegisterImplementationSourceOutput(
            source: context.SyntaxProvider.ForAzureTableEntityAttributes().Collect(),
            action: (ctx, entities) =>
            {
                if (entities.IsDefaultOrEmpty)
                    return;

                foreach (var entity in entities)
                {
                    var scoped = new HintNameProvider(entity.Namespace);
                    ctx.AddSource(scoped.Create(entity.Name),
                        RenderTemplate("Azure.Tables.Templates.DeepstagingEntityExtensions", entity));
                }
            }
        );
    }
}