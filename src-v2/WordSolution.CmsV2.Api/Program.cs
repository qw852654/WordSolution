using System.Text.Json;
using System.Text.Json.Serialization;
using WordSolution.CmsV2.Api;
using WordSolution.CmsV2.Infrastructure.Documents;

var builder = WebApplication.CreateBuilder(args);
var Aspose授权文件路径 = Path.Combine(AppContext.BaseDirectory, "Aspose.Total.NET.lic");

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddSingleton(new Aspose授权初始化器(Aspose授权文件路径));
builder.Services.AddCmsV2Api(builder.Configuration);

var app = builder.Build();

app.UseCmsV2ExceptionHandling();
using (var scope = app.Services.CreateScope())
{
    var Aspose授权初始化器 = scope.ServiceProvider.GetRequiredService<Aspose授权初始化器>();
    Aspose授权初始化器.初始化授权();
}

await app.InitializeCmsV2DatabaseAsync();

app.MapGet("/", () => Results.Redirect("/api/cms-v2/health"));
app.MapCmsV2Api();

await app.RunAsync();

public partial class Program;
