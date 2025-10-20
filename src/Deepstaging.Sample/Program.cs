using Deepstaging.Sample;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.RegisterConfigurationOptions();
var app = builder.Build();

var my = app.Services.GetRequiredService<IOptions<MyConfiguration>>();
var other = app.Services.GetRequiredService<IOptions<OtherConfiguration>>();
var yetAnother = app.Services.GetRequiredService<IOptions<MyConfigurationExtensions.YetAnotherConfiguration>>();

app.MapGet("/", () => "Hello World!");
app.Run();



