using Deepstaging.Sample.HttpClients;
using Oakton;

var builder = WebApplication.CreateBuilder(args);

builder
    .RegisterConfigurations()
    .AddServiceClient((client, config) =>
    {
        client.BaseAddress = new(config.BaseUrl);
        client.DefaultRequestHeaders.Add("x-functions-key", config.ApiKey);
    });

builder.Host.ApplyOaktonExtensions();

var app = builder.Build();

var serviceClient = app.Services.GetRequiredService<IServiceClient>();
var clients = await serviceClient.GetClients("6023265200");

app.MapGet("/", () => "Hello World!");

await app.RunOaktonCommands(args);