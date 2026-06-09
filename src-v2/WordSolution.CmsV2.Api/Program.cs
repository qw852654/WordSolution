using System.Text.Json;
using System.Text.Json.Serialization;
using WordSolution.CmsV2.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddCmsV2Api(builder.Configuration);

var app = builder.Build();

app.UseCmsV2ExceptionHandling();
await app.InitializeCmsV2DatabaseAsync();

app.MapGet("/", () => Results.Redirect("/api/cms-v2/health"));
app.MapCmsV2Api();

await app.RunAsync();

public partial class Program;
