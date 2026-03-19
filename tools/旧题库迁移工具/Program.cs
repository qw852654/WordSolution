using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using 题库基础设施.Aspose;
using 题库基础设施.初始化;
using 题库基础设施.数据访问;
using 题库基础设施.题库实例;
using 题库基础设施.预览生成;
using 题库核心.标签模块.领域;
using 题库核心.题目模块.领域;

var 参数 = 命令行参数.Parse(args);
if (!参数.合法)
{
    Console.WriteLine("用法：dotnet run --project tools/旧题库迁移工具 -- --source <旧题库目录> --key <新题库键>");
    return 1;
}

var 仓库根目录 = 获取仓库根目录();
var 授权文件路径 = Path.Combine(仓库根目录, "lib", "Aspose.Total.NET.lic");
var 题库中心根目录 = @"E:\Desktop\题库中心";

var 题库路径提供器 = new 题库路径提供器(new HttpContextAccessor(), 题库中心根目录);
var 题库DbContext工厂 = new 题库DbContext工厂(题库路径提供器);
var 题库初始化器 = new 题库实例初始化器(题库路径提供器, 题库DbContext工厂);
var 预览生成器 = new 题目预览生成器();
new Aspose授权初始化器(授权文件路径).初始化授权();

if (!Directory.Exists(参数.旧题库目录))
{
    Console.WriteLine($"旧题库目录不存在：{参数.旧题库目录}");
    return 1;
}

var 目标题库目录 = 题库路径提供器.获取题库根目录(参数.新题库键);
if (Directory.Exists(目标题库目录) && Directory.EnumerateFileSystemEntries(目标题库目录).Any())
{
    Console.WriteLine($"目标题库目录已存在且非空：{目标题库目录}");
    Console.WriteLine("为了避免覆盖已有数据，迁移工具已停止。");
    return 1;
}

var 报告 = 执行迁移(
    参数.旧题库目录,
    参数.新题库键,
    题库路径提供器,
    题库DbContext工厂,
    题库初始化器,
    预览生成器);

var 报告路径 = Path.Combine(目标题库目录, "migration-report.json");
Directory.CreateDirectory(目标题库目录);
File.WriteAllText(报告路径, JsonSerializer.Serialize(报告, new JsonSerializerOptions { WriteIndented = true }));

Console.WriteLine("迁移完成。");
Console.WriteLine($"新题库键：{报告.新题库键}");
Console.WriteLine($"导入标签数：{报告.导入标签数}");
Console.WriteLine($"导入题目数：{报告.导入题目数}");
Console.WriteLine($"跳过题目数：{报告.跳过题目数}");
Console.WriteLine($"报告路径：{报告路径}");
return 0;

迁移报告 执行迁移(
    string 旧题库目录,
    string 新题库键,
    题库路径提供器 题库路径提供器,
    题库DbContext工厂 题库DbContext工厂,
    题库实例初始化器 题库实例初始化器,
    题目预览生成器 预览生成器)
{
    var 旧数据库路径 = Path.Combine(旧题库目录, "tagrunner.db");
    if (!File.Exists(旧数据库路径))
    {
        throw new FileNotFoundException("旧题库数据库不存在。", 旧数据库路径);
    }

    题库初始化器.确保题库已初始化(新题库键);

    var 报告 = new 迁移报告
    {
        源题库目录 = 旧题库目录,
        新题库键 = 新题库键,
    };

    var 旧标签列表 = 读取旧标签列表(旧数据库路径);
    var 旧题目列表 = 读取旧题目列表(旧数据库路径);
    var 旧题目标签映射 = 读取旧题目标签映射(旧数据库路径);
    var 旧标签到新标签映射 = new Dictionary<int, int>();

    using (var 新题库DbContext = 题库DbContext工厂.创建(新题库键))
    {
        var 按父级分组的旧标签 = 旧标签列表
            .GroupBy(标签 => 获取父级分组键(标签.ParentId))
            .ToDictionary(
                分组 => 分组.Key,
                分组 => 按同级顺序排序(分组.ToList(), 报告));

        导入标签子树(
            旧父标签ID: null,
            新父标签ID: null,
            按父级分组的旧标签,
            旧标签到新标签映射,
            新题库DbContext,
            报告);

        报告.导入标签数 = 旧标签到新标签映射.Count;

        var 题目仓储 = new 题目仓储(新题库DbContext);
        var 新Source目录 = 题库路径提供器.获取Source目录(新题库键);
        var 新Html目录 = 题库路径提供器.获取Html目录(新题库键);
        Directory.CreateDirectory(新Source目录);
        Directory.CreateDirectory(新Html目录);

        foreach (var 旧题目 in 旧题目列表.OrderBy(题目 => 题目.Id))
        {
            var 旧Docx路径 = Path.Combine(旧题库目录, "source", $"{旧题目.Id}.docx");
            if (!File.Exists(旧Docx路径))
            {
                报告.跳过题目数++;
                报告.缺失Docx题目列表.Add(new 跳过题目记录
                {
                    旧题目ID = 旧题目.Id,
                    描述 = 旧题目.Description,
                    原因 = "缺少 docx",
                });
                continue;
            }

            var 新标签ID列表 = (旧题目标签映射.TryGetValue(旧题目.Id, out var 旧标签ID列表) ? 旧标签ID列表 : [])
            .Where(旧标签到新标签映射.ContainsKey)
            .Select(旧标签ID => 旧标签到新标签映射[旧标签ID])
                .Distinct()
                .ToList();

            var 新题目 = 题目.从持久化恢复题目(
                0,
                旧题目.Description,
                旧题目.CreatedTime,
                旧题目.UpdateTime,
                新标签ID列表);

            题目仓储.增加题目(新题目);

            var 新Docx路径 = Path.Combine(新Source目录, $"{新题目.Id}.docx");
            var 新Html路径 = Path.Combine(新Html目录, $"{新题目.Id}.html");

            File.Copy(旧Docx路径, 新Docx路径, overwrite: true);
            var 迁移文档 = new Aspose.Words.Document(新Docx路径);
            文档清理帮助类.清理页眉页脚(迁移文档);
            迁移文档.Save(新Docx路径);
            预览生成器.生成HTML预览(新Docx路径, 新Html路径);
            报告.导入题目数++;
        }
    }

    return 报告;
}

void 导入标签子树(
    int? 旧父标签ID,
    int? 新父标签ID,
    IReadOnlyDictionary<string, List<旧标签记录>> 按父级分组的旧标签,
    IDictionary<int, int> 旧标签到新标签映射,
    题库DbContext 新题库DbContext,
    迁移报告 报告)
{
    if (!按父级分组的旧标签.TryGetValue(获取父级分组键(旧父标签ID), out var 子标签列表))
    {
        return;
    }

    var 当前排序值 = 0;
    foreach (var 旧标签 in 子标签列表)
    {
        var 名称 = 生成可用标签名称(
            新题库DbContext,
            旧标签.Name,
            系统标签种类.待整理,
            新父标签ID,
            旧标签.Id,
            报告);

        var 新标签 = 标签.创建标签(
            系统标签种类.待整理,
            名称,
            旧标签.Description,
            新父标签ID,
            当前排序值++,
            null,
            true);

        新题库DbContext.标签表.Add(新标签);
        新题库DbContext.SaveChanges();

        旧标签到新标签映射[旧标签.Id] = 新标签.Id;
        导入标签子树(旧标签.Id, 新标签.Id, 按父级分组的旧标签, 旧标签到新标签映射, 新题库DbContext, 报告);
    }
}

string 生成可用标签名称(
    题库DbContext 新题库DbContext,
    string 原名称,
    int 标签种类ID,
    int? 父标签ID,
    int 旧标签ID,
    迁移报告 报告)
{
    var 基础名称 = string.IsNullOrWhiteSpace(原名称) ? $"未命名标签_{旧标签ID}" : 原名称.Trim();
    var 当前名称 = 基础名称;
    var 序号 = 1;

    while (新题库DbContext.标签表.Any(标签 =>
               标签.标签种类ID == 标签种类ID
               && 标签.ParentId == 父标签ID
               && 标签.名称 == 当前名称))
    {
        当前名称 = $"{基础名称}（迁移{序号}）";
        序号++;
    }

    if (当前名称 != 基础名称)
    {
        报告.重名修正标签列表.Add(new 重名修正记录
        {
            旧标签ID = 旧标签ID,
            原名称 = 基础名称,
            新名称 = 当前名称,
        });
    }

    return 当前名称;
}

List<旧标签记录> 按同级顺序排序(List<旧标签记录> 同级标签列表, 迁移报告 报告)
{
    if (同级标签列表.Count <= 1)
    {
        return 同级标签列表.OrderBy(标签 => 标签.Id).ToList();
    }

    var 字典 = 同级标签列表.ToDictionary(标签 => 标签.Id);
    var 起点列表 = 同级标签列表
        .Where(标签 => !标签.PrevSiblingId.HasValue || !字典.ContainsKey(标签.PrevSiblingId.Value))
        .ToList();

    if (起点列表.Count != 1)
    {
        报告.排序链异常标签ID列表.AddRange(同级标签列表.Select(标签 => 标签.Id));
        return 同级标签列表.OrderBy(标签 => 标签.Id).ToList();
    }

    var 已访问 = new HashSet<int>();
    var 结果 = new List<旧标签记录>();
    var 当前标签 = 起点列表[0];

    while (当前标签 != null && 已访问.Add(当前标签.Id))
    {
        结果.Add(当前标签);

        if (!当前标签.NextSiblingId.HasValue || !字典.TryGetValue(当前标签.NextSiblingId.Value, out var 下一个标签))
        {
            break;
        }

        当前标签 = 下一个标签;
    }

    if (结果.Count != 同级标签列表.Count)
    {
        报告.排序链异常标签ID列表.AddRange(同级标签列表.Select(标签 => 标签.Id));
        return 同级标签列表.OrderBy(标签 => 标签.Id).ToList();
    }

    return 结果;
}

List<旧标签记录> 读取旧标签列表(string 旧数据库路径)
{
    using var 连接 = 创建旧数据库连接(旧数据库路径);
    using var 命令 = 连接.CreateCommand();
    命令.CommandText = "SELECT Id, Name, ParentId, Description, PrevSiblingId, NextSiblingId FROM Tags WHERE Id > 0 ORDER BY Id;";

    using var 读取器 = 命令.ExecuteReader();
    var 结果 = new List<旧标签记录>();

    while (读取器.Read())
    {
        结果.Add(new 旧标签记录
        {
            Id = 读取器.GetInt32(0),
            Name = 读取器.IsDBNull(1) ? string.Empty : 读取器.GetString(1),
            ParentId = 归一化旧外键(读取器, 2),
            Description = 读取器.IsDBNull(3) ? null : 读取器.GetString(3),
            PrevSiblingId = 归一化旧外键(读取器, 4),
            NextSiblingId = 归一化旧外键(读取器, 5),
        });
    }

    return 结果;
}

List<旧题目记录> 读取旧题目列表(string 旧数据库路径)
{
    using var 连接 = 创建旧数据库连接(旧数据库路径);
    using var 命令 = 连接.CreateCommand();
    命令.CommandText = "SELECT Id, Description, CreatedTime, UpdateTime FROM Questions ORDER BY Id;";

    using var 读取器 = 命令.ExecuteReader();
    var 结果 = new List<旧题目记录>();

    while (读取器.Read())
    {
        结果.Add(new 旧题目记录
        {
            Id = 读取器.GetInt32(0),
            Description = 读取器.IsDBNull(1) ? null : 读取器.GetString(1),
            CreatedTime = 解析旧时间(读取器.IsDBNull(2) ? null : 读取器.GetString(2)),
            UpdateTime = 解析旧时间(读取器.IsDBNull(3) ? null : 读取器.GetString(3)),
        });
    }

    return 结果;
}

Dictionary<int, List<int>> 读取旧题目标签映射(string 旧数据库路径)
{
    using var 连接 = 创建旧数据库连接(旧数据库路径);
    using var 命令 = 连接.CreateCommand();
    命令.CommandText = "SELECT QuestionId, TagId FROM QuestionTags ORDER BY QuestionId, TagId;";

    using var 读取器 = 命令.ExecuteReader();
    var 结果 = new Dictionary<int, List<int>>();

    while (读取器.Read())
    {
        var 题目ID = 读取器.GetInt32(0);
        var 标签ID = 读取器.GetInt32(1);

        if (!结果.TryGetValue(题目ID, out var 标签ID列表))
        {
            标签ID列表 = new List<int>();
            结果[题目ID] = 标签ID列表;
        }

        标签ID列表.Add(标签ID);
    }

    return 结果;
}

SqliteConnection 创建旧数据库连接(string 旧数据库路径)
{
    var 连接 = new SqliteConnection($"Data Source={旧数据库路径}");
    连接.Open();
    return 连接;
}

DateTime 解析旧时间(string? 时间文本)
{
    if (DateTime.TryParse(时间文本, out var 时间))
    {
        return 时间;
    }

    return DateTime.Now;
}

int? 归一化旧外键(SqliteDataReader 读取器, int 列索引)
{
    if (读取器.IsDBNull(列索引))
    {
        return null;
    }

    var 值 = 读取器.GetInt32(列索引);
    return 值 <= 0 ? null : 值;
}

string 获取仓库根目录()
{
    var 当前目录 = new DirectoryInfo(AppContext.BaseDirectory);
    while (当前目录 != null)
    {
        if (File.Exists(Path.Combine(当前目录.FullName, "WordSolution.sln")))
        {
            return 当前目录.FullName;
        }

        当前目录 = 当前目录.Parent;
    }

    throw new InvalidOperationException("无法定位仓库根目录。");
}

string 获取父级分组键(int? 父标签ID)
{
    return 父标签ID?.ToString() ?? "ROOT";
}

internal sealed class 命令行参数
{
    public string 旧题库目录 { get; private init; } = string.Empty;

    public string 新题库键 { get; private init; } = string.Empty;

    public bool 合法性错误 => string.IsNullOrWhiteSpace(旧题库目录) || string.IsNullOrWhiteSpace(新题库键);

    public bool 合法 => !合法性错误;

    public static 命令行参数 Parse(string[] args)
    {
        string? 旧题库目录 = null;
        string? 新题库键 = null;

        for (var index = 0; index < args.Length; index++)
        {
            switch (args[index])
            {
                case "--source" when index + 1 < args.Length:
                    旧题库目录 = args[++index];
                    break;
                case "--key" when index + 1 < args.Length:
                    新题库键 = args[++index];
                    break;
            }
        }

        return new 命令行参数
        {
            旧题库目录 = 旧题库目录 ?? string.Empty,
            新题库键 = 新题库键 ?? string.Empty,
        };
    }
}

internal sealed class 迁移报告
{
    public string 源题库目录 { get; set; } = string.Empty;
    public string 新题库键 { get; set; } = string.Empty;
    public int 导入标签数 { get; set; }
    public int 导入题目数 { get; set; }
    public int 跳过题目数 { get; set; }
    public List<跳过题目记录> 缺失Docx题目列表 { get; set; } = [];
    public List<int> 排序链异常标签ID列表 { get; set; } = [];
    public List<重名修正记录> 重名修正标签列表 { get; set; } = [];
}

internal sealed class 跳过题目记录
{
    public int 旧题目ID { get; set; }
    public string? 描述 { get; set; }
    public string 原因 { get; set; } = string.Empty;
}

internal sealed class 重名修正记录
{
    public int 旧标签ID { get; set; }
    public string 原名称 { get; set; } = string.Empty;
    public string 新名称 { get; set; } = string.Empty;
}

internal sealed class 旧标签记录
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public string? Description { get; set; }
    public int? PrevSiblingId { get; set; }
    public int? NextSiblingId { get; set; }
}

internal sealed class 旧题目记录
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime UpdateTime { get; set; }
}
