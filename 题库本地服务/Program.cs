using System;
using System.IO;
using System.Text.Json.Serialization;
using 题库基础设施.Aspose;
using 题库基础设施.题库实例;
using 题库本地服务.依赖注入;

var builder = WebApplication.CreateBuilder(args);
var 题库中心根目录 = @"E:\Desktop\题库中心";
var Aspose授权文件路径 = Path.Combine(AppContext.BaseDirectory, "Aspose.Total.NET.lic");
var 本地Https证书路径 = Path.Combine(AppContext.BaseDirectory, "certs", "localhost-dev.pfx");
const string 本地Https证书密码 = "WordSolutionLocalHttps2026!";

if (File.Exists(本地Https证书路径))
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenLocalhost(5283, listenOptions =>
        {
            listenOptions.UseHttps(本地Https证书路径, 本地Https证书密码);
        });
    });
}
else
{
    builder.WebHost.UseUrls("http://localhost:5282");
}

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddHttpContextAccessor();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.Add题库实例服务(题库中心根目录);
builder.Services.Add题库基础设施服务(Aspose授权文件路径);
builder.Services.Add题库应用用例();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var Aspose授权初始化器 = scope.ServiceProvider.GetRequiredService<Aspose授权初始化器>();
    var 题库实例服务 = scope.ServiceProvider.GetRequiredService<题库实例服务>();

    Aspose授权初始化器.初始化授权();
    题库实例服务.确保测试题库已初始化();
    题库实例服务.确保现有题库已补齐初始化();
}

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

app.Run();
