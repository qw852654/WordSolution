using System;
using System.IO;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using 题库应用.标签模块;
using 题库应用.筛选模块;
using 题库应用.题目模块;
using 题库基础设施.Aspose;
using 题库基础设施.数据访问;
using 题库基础设施.文件存储;
using 题库基础设施.初始化;
using 题库基础设施.题库实例;
using 题库基础设施.预览生成;
using 题库核心.标签模块.契约;
using 题库核心.题目模块.契约;

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
    // 开发态如果还没有安装版证书，则继续沿用当前 HTTP 端口。
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

builder.Services.AddScoped(_ => 题库中心根目录);
builder.Services.AddScoped<题库路径提供器>();
builder.Services.AddScoped<题库DbContext工厂>();
builder.Services.AddScoped<题库实例初始化器>();
builder.Services.AddScoped<题库实例服务>();

builder.Services.AddDbContext<题库DbContext>((服务提供器, options) =>
{
    var 题库路径提供器 = 服务提供器.GetRequiredService<题库路径提供器>();
    var 当前题库键 = 题库路径提供器.获取当前请求题库键();
    options.UseSqlite($"Data Source={题库路径提供器.获取数据库文件路径(当前题库键)}");
});

builder.Services.AddScoped(服务提供器 => new Aspose授权初始化器(Aspose授权文件路径));
builder.Services.AddScoped<I题目仓储, 题目仓储>();
builder.Services.AddScoped<I标签仓储, 标签仓储>();
builder.Services.AddScoped<I标签种类仓储, 标签种类仓储>();
builder.Services.AddScoped<I题目文件存储, 题目文件存储>();
builder.Services.AddScoped<I题目文档转换器, Aspose题目文档转换器>();
builder.Services.AddScoped<I题目预览生成器, 题目预览生成器>();

builder.Services.AddScoped<题目标签规则校验器>();
builder.Services.AddScoped<录入题目用例>();
builder.Services.AddScoped<录入Ooxml题目用例>();
builder.Services.AddScoped<根据ID获取题目详情用例>();
builder.Services.AddScoped<获取题目文件Base64用例>();
builder.Services.AddScoped<获取题目预览HTML用例>();
builder.Services.AddScoped<根据标签筛选题目用例>();
builder.Services.AddScoped<更新Ooxml题目用例>();
builder.Services.AddScoped<删除题目用例>();
builder.Services.AddScoped<获取标签树用例>();
builder.Services.AddScoped<获取标签种类列表用例>();
builder.Services.AddScoped<获取标签列表用例>();
builder.Services.AddScoped<新增标签用例>();
builder.Services.AddScoped<更新标签用例>();
builder.Services.AddScoped<调整标签父级用例>();
builder.Services.AddScoped<调整标签排序用例>();
builder.Services.AddScoped<删除标签用例>();

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
