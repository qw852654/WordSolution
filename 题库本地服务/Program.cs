using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using 题库应用.标签模块;
using 题库应用.筛选模块;
using 题库应用.题目模块;
using 题库基础设施.数据访问;
using 题库基础设施.文件存储;
using 题库基础设施.初始化;
using 题库基础设施.预览生成;
using 题库核心.标签模块.契约;
using 题库核心.题目模块.契约;

var builder = WebApplication.CreateBuilder(args);
var 题库数据根目录 = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "WordSolution",
    "题库");
var 数据库文件路径 = Path.Combine(题库数据根目录, "question-bank.db");
var 文件存储根目录 = Path.Combine(题库数据根目录, "files");

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<题库DbContext>(options =>
{
    options.UseSqlite($"Data Source={数据库文件路径}");
});

builder.Services.AddScoped(服务提供器 => new 题库初始化器(
    服务提供器.GetRequiredService<题库DbContext>(),
    题库数据根目录,
    文件存储根目录));
builder.Services.AddScoped<I题目仓储, 题目仓储>();
builder.Services.AddScoped<I标签仓储, 标签仓储>();
builder.Services.AddScoped<I题目文件存储>(服务提供器 => new 题目文件存储(文件存储根目录));
builder.Services.AddScoped<I题目预览生成器, 题目预览生成器>();

builder.Services.AddScoped<录入题目用例>();
builder.Services.AddScoped<根据ID获取题目详情用例>();
builder.Services.AddScoped<获取题目预览HTML用例>();
builder.Services.AddScoped<根据标签筛选题目用例>();
builder.Services.AddScoped<获取标签树用例>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var 题库初始化器 = scope.ServiceProvider.GetRequiredService<题库初始化器>();
    题库初始化器.初始化题库();
}

app.UseCors();
app.MapControllers();

app.Run();
