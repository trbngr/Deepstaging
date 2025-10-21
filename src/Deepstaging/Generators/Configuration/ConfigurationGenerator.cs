using System.Collections.Immutable;
using Deepstaging.Generators.Configuration.Models;
using Deepstaging.Generators.Configuration.Providers;
using Microsoft.CodeAnalysis;
using SGF;

namespace Deepstaging.Generators.Configuration;

using static Template;
using static JsonSchemaType;

[IncrementalGenerator]
public class ConfigurationGenerator()
    : IncrementalGenerator(name: nameof(ConfigurationGenerator))
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
                hintName: hint.Create("RegisterConfigurationAttribute"),
                source: RenderConfigurationRegistrationAttribute()
            );
        });

        context.RegisterImplementationSourceOutput(
            source: context.SyntaxProvider.ForRegisterConfigurationAttributes().Collect(),
            action: (ctx, registrations) =>
            {
                if (registrations.IsDefaultOrEmpty)
                    return;

                ctx.AddSource(
                    hintName: hint.Create("ConfigurationRegistrationExtensions"),
                    source: RenderRegistrationExtensions(registrations)
                );

                ctx.AddSource(
                    hintName: hint.Create("ConfigurationSupport"),
                    source: RenderConfigurationSupport(registrations)
                );
            });
    }

    private static string RenderSecretAttribute() =>
        RenderTemplate(name: "Configuration.Templates.SecretAttribute");

    private static string RenderConfigurationRegistrationAttribute() =>
        RenderTemplate(
            name: "Configuration.Templates.RegisterConfigurationAttribute",
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

    private static string RenderConfigurationSupport(ImmutableArray<ConfigurationRegistrationInfo> infos)
    {
        try
        {
            var updateSecretsScript =
                RenderTemplate("Configuration.Templates.UpdateLocalSecretsScript", renderHeader: false);

            var re = RenderTemplate(
                name: "Configuration.Templates.ConfigurationSupport",
                context: new
                {
                    @namespace = infos[0].Namespace,
                    update_secrets_script = updateSecretsScript,
                    app_settings_schema = JsonSchema.RenderJsonSchema(infos, AppSettings),
                    secrets_schema = JsonSchema.RenderJsonSchema(infos, Secrets),
                    app_settings_example = JsonSchema.RenderExampleFile(infos, AppSettings),
                    secrets_example = JsonSchema.RenderExampleFile(infos, Secrets)
                }
            );
        
            return re;
        }
        catch (Exception e)
        {
            throw;
        }
    }
}