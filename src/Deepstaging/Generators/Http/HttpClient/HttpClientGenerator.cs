using Deepstaging.Generators.Http.HttpClient.Models;
using Deepstaging.Generators.Http.HttpClient.Providers;
using Microsoft.CodeAnalysis;
using SGF;
using SGF.Diagnostics;
using SGF.Diagnostics.Sinks;

namespace Deepstaging.Generators.Http.HttpClient;

using static Template;

[IncrementalGenerator]
public class HttpClientGenerator()
    : IncrementalGenerator(name: nameof(HttpClientGenerator))
{
    public override void OnInitialize(SgfInitializationContext context)
    {
        Logger.AddSink(new ConsoleSink { Level = LogLevel.Error });
        var hint = new HintNameProvider("Deepstaging.Http.HttpClient");

        string TemplateName(string name) => $"Http.HttpClient.Templates.{name}";

        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource(hint.Create("Attributes"), RenderTemplate(
                name: TemplateName("Attributes"),
                context: new { }
            ));
        });

        context.RegisterImplementationSourceOutput(
            source: context.SyntaxProvider.ForHttpClientAttributes().Collect(),
            action: (ctx, httpClients) =>
            {
                if (httpClients.IsDefaultOrEmpty)
                    return;

                foreach (var client in httpClients)
                {
                    var scopedHint = new HintNameProvider(client.Namespace);
                    
                    ctx.AddSource(scopedHint.Create(client.TypeName, "Base"), RenderTemplate(
                        name: TemplateName("Base"),
                        context: client
                    ));

                    ctx.AddSource(scopedHint.Create(client.TypeName, $"Request"), RenderTemplate(
                        name: TemplateName("Requests"),
                        context: client
                    ));

                    ctx.AddSource(scopedHint.Create(client.TypeName, $"Extensions"), RenderTemplate(
                        name: TemplateName("Extensions"),
                        context: client
                    ));

                    foreach (var request in client.Requests)
                    {
                        var templateName = request.Verb switch
                        {
                            Verb.Post => TemplateName("Post"),
                            _ => throw new NotSupportedException($"Unsupported HTTP verb: {request.Verb}")
                        };

                        ctx.AddSource(scopedHint.Create(client.TypeName, request.Name),
                            RenderTemplate(
                                name: templateName,
                                context: new { client, request }
                            ));
                    }
                }
            });
    }
}