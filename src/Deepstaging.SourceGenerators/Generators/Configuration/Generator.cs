using System.Collections.Immutable;
using Deepstaging.SourceGenerators.Generators.Configuration.Models;
using Deepstaging.SourceGenerators.Generators.Configuration.Providers;
using Microsoft.CodeAnalysis;
using SGF;

namespace Deepstaging.SourceGenerators.Generators.Configuration;

using static Template;
using static JsonSchemaType;

[IncrementalGenerator]
public class Generator()
    : IncrementalGenerator(name: nameof(Generator))
{
    public override void OnInitialize(SgfInitializationContext context)
    {
        var hint = new HintNameProvider("Deepstaging.Configuration");

        context.RegisterPostInitializationOutput(callback: ctx =>
        {
            ctx.AddSource(
                hintName: hint.Create("SecretAttribute"),
                source: RenderSecretAttribute()
            );
            ctx.AddSource(
                hintName: hint.Create("ConfigurationRegistrationAttribute"),
                source: RenderConfigurationRegistrationAttribute()
            );
        });

        context.RegisterImplementationSourceOutput(
            source: context.SyntaxProvider.ForConfigurationRegistrations().Collect(),
            action: (ctx, registrations) =>
            {
                if (registrations.IsDefaultOrEmpty)
                    return;

                ctx.AddSource(
                    hintName: hint.Create("ConfigurationRegistrationExtensions"),
                    source: RenderRegistrationExtensions(infos: registrations)
                );

                ctx.AddSource(
                    hintName: hint.Create("JsonSchema"),
                    source: RenderJsonSchema(infos: registrations)
                );
            });
    }

    private static string RenderSecretAttribute() =>
        RenderTemplate(name: "Configuration.Templates.SecretAttribute");

    private static string RenderConfigurationRegistrationAttribute() =>
        RenderTemplate(
            name: "Configuration.Templates.ConfigurationRegistrationAttribute",
            context: new { }
        );

    private static string RenderRegistrationExtensions(ImmutableArray<ConfigurationRegistrationInfo> infos) =>
        RenderTemplate(
            name: "Configuration.Templates.ConfigurationRegistrationExtensions",
            context: new
            {
                @namespace = infos[0].Namespace,
                registrations = infos
            }
        );

    private static string RenderJsonSchema(ImmutableArray<ConfigurationRegistrationInfo> infos) =>
        RenderTemplate(
            name: "Configuration.Templates.JsonSchema",
            context: new
            {
                @namespace = infos[0].Namespace,
                app_settings_schema = JsonSchema.RenderJsonSchema(infos, AppSettings),
                secrets_schema = JsonSchema.RenderJsonSchema(infos, Secrets),
                app_settings_example = JsonSchema.RenderExampleFile(infos, AppSettings),
                secrets_example = JsonSchema.RenderExampleFile(infos, Secrets)
            }
        );
}