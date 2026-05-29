using Microsoft.EntityFrameworkCore;
using 题库应用.内容块模块;
using 题库应用.讲义模块;
using 题库应用.引用关系模块;
using 题库应用.标签模块;
using 题库应用.筛选模块;
using 题库应用.试卷导入模块;
using 题库应用.试卷导入模块.试卷解析;
using 题库应用.题目模块;
using 题库应用.题目模块.题型识别;
using 题库应用.题目模块.题型识别.题型特征;
using 题库基础设施.内容块模块;
using 题库基础设施.Aspose;
using 题库基础设施.初始化;
using 题库基础设施.数据访问;
using 题库基础设施.文件存储;
using 题库基础设施.讲义模块;
using 题库基础设施.小节模块;
using 题库基础设施.题库实例;
using 题库基础设施.预览生成;
using 题库核心.内容块模块.契约;
using 题库核心.讲义模块.契约;
using 题库核心.小节模块.契约;
using 题库核心.标签模块.契约;
using 题库核心.试卷导入模块.契约;
using 题库核心.题目模块.契约;
using 题库本地服务.内容块模块;
using 题库应用.小节模块;

namespace 题库本地服务.依赖注入
{
    public static class 题库服务注册扩展
    {
        public static IServiceCollection Add题库实例服务(this IServiceCollection services, string 题库中心根目录)
        {
            services.AddScoped(_ => 题库中心根目录);
            services.AddScoped<当前题库上下文>();
            services.AddScoped<题库路径提供器>();
            services.AddScoped<题库DbContext工厂>();
            services.AddScoped<题库实例初始化器>();
            services.AddScoped<题库实例服务>();

            services.AddDbContext<题库DbContext>((服务提供器, options) =>
            {
                var 题库路径提供器 = 服务提供器.GetRequiredService<题库路径提供器>();
                var 当前题库键 = 题库路径提供器.获取当前请求题库键();
                options.UseSqlite($"Data Source={题库路径提供器.获取数据库文件路径(当前题库键)}");
            });

            return services;
        }

        public static IServiceCollection Add题库基础设施服务(this IServiceCollection services, string Aspose授权文件路径)
        {
            services.AddScoped(_ => new Aspose授权初始化器(Aspose授权文件路径));
            services.AddSingleton<I内容块编辑会话存储, 内存内容块编辑会话存储>();
            services.AddScoped<I内容块仓储, 内容块仓储>();
            services.AddScoped<I内容块标签仓储, 内容块标签仓储>();
            services.AddScoped<I元数据选项仓储, 元数据选项仓储>();
            services.AddScoped<I内容块文件存储, 内容块文件存储>();
            services.AddScoped<I内容块文档转换器, Aspose内容块文档转换器>();
            services.AddScoped<I内容块预览生成器, 内容块预览生成器>();
            services.AddScoped<I内容块编辑会话文件存储, 内容块编辑会话文件存储>();
            services.AddScoped<I本地Word启动器, 默认本地Word启动器>();
            services.AddScoped<I文件哈希服务, SHA256文件哈希服务>();
            services.AddScoped<I编辑文件稳定性检测器, 编辑文件稳定性检测器>();
            services.AddScoped<I小节仓储, 小节仓储>();
            services.AddScoped<I讲义仓储, 讲义仓储>();
            services.AddScoped<I讲义文件存储, 讲义文件存储>();
            services.AddScoped<I讲义Word生成器, Aspose讲义Word生成器>();
            services.AddScoped<I题目仓储, 题目仓储>();
            services.AddScoped<I标签仓储, 标签仓储>();
            services.AddScoped<I标签种类仓储, 标签种类仓储>();
            services.AddScoped<I题型定义仓储, 题型定义仓储>();
            services.AddScoped<I试卷记录仓储, 试卷记录仓储>();
            services.AddScoped<I试卷源文件仓储, 试卷源文件仓储>();
            services.AddScoped<I试卷题目项仓储, 试卷题目项仓储>();
            services.AddScoped<I知识点映射仓储, 知识点映射仓储>();
            services.AddScoped<I题目文件存储, 题目文件存储>();
            services.AddScoped<I试卷源文件存储, 试卷源文件存储>();
            services.AddScoped<I导入会话存储, 导入会话文件存储>();
            services.AddScoped<I题目文档转换器, Aspose题目文档转换器>();
            services.AddScoped<I题目预览生成器, 题目预览生成器>();
            services.AddScoped<I题型识别器, 题型识别器>();

            return services;
        }

        public static IServiceCollection Add题库应用用例(this IServiceCollection services)
        {
            services.AddScoped<新建内容块用例>();
            services.AddScoped<内容块元数据选项帮助类>();
            services.AddScoped<获取元数据选项列表用例>();
            services.AddScoped<新增元数据选项用例>();
            services.AddScoped<更新元数据选项用例>();
            services.AddScoped<设置元数据选项启用状态用例>();
            services.AddScoped<更新内容块元数据用例>();
            services.AddScoped<录入Ooxml内容块用例>();
            services.AddScoped<更新Ooxml内容块用例>();
            services.AddScoped<获取内容块详情用例>();
            services.AddScoped<获取内容块列表用例>();
            services.AddScoped<获取内容块版本列表用例>();
            services.AddScoped<内容块标签规则校验器>();
            services.AddScoped<获取内容块标签列表用例>();
            services.AddScoped<更新内容块标签列表用例>();
            services.AddScoped<获取内容块预览HTML用例>();
            services.AddScoped<获取内容块文件Base64用例>();
            services.AddScoped<获取内容块子项列表用例>();
            services.AddScoped<添加内容块子项用例>();
            services.AddScoped<调整内容块子块排序用例>();
            services.AddScoped<移除内容块子项用例>();
            services.AddScoped<获取内容块结构树用例>();
            services.AddScoped<编辑会话创建帮助类>();
            services.AddScoped<创建新内容块编辑会话用例>();
            services.AddScoped<创建已有内容块编辑会话用例>();
            services.AddScoped<获取内容块编辑会话状态用例>();
            services.AddScoped<同步内容块编辑会话用例>();
            services.AddScoped<取消内容块编辑会话用例>();
            services.AddHostedService<内容块编辑会话后台服务>();

            services.AddScoped<小节章节规则校验器>();
            services.AddScoped<小节结果构建器>();
            services.AddScoped<获取小节列表用例>();
            services.AddScoped<获取小节详情用例>();
            services.AddScoped<新建小节用例>();
            services.AddScoped<更新小节用例>();
            services.AddScoped<获取小节项列表用例>();
            services.AddScoped<添加小节项用例>();
            services.AddScoped<调整小节项排序用例>();
            services.AddScoped<移除小节项用例>();
            services.AddScoped<获取小节预览HTML用例>();
            services.AddScoped<导出小节Word用例>();

            services.AddScoped<讲义结果构建器>();
            services.AddScoped<内容块Word展开服务>();
            services.AddScoped<获取讲义列表用例>();
            services.AddScoped<获取讲义详情用例>();
            services.AddScoped<新建讲义用例>();
            services.AddScoped<更新讲义用例>();
            services.AddScoped<获取讲义项列表用例>();
            services.AddScoped<添加讲义项用例>();
            services.AddScoped<调整讲义项排序用例>();
            services.AddScoped<移除讲义项用例>();
            services.AddScoped<获取讲义结构树用例>();
            services.AddScoped<生成讲义用例>();
            services.AddScoped<获取讲义生成记录列表用例>();
            services.AddScoped<获取讲义生成文件用例>();

            services.AddScoped<引用关系分析器>();
            services.AddScoped<获取内容块引用影响用例>();
            services.AddScoped<获取旧版本引用列表用例>();

            services.AddScoped<题目标签规则校验器>();
            services.AddScoped<题型规则校验器>();
            services.AddScoped<录入题目用例>();
            services.AddScoped<录入Ooxml题目用例>();
            services.AddScoped<预览Ooxml题目用例>();
            services.AddScoped<根据ID获取题目详情用例>();
            services.AddScoped<获取题目文件Base64用例>();
            services.AddScoped<获取题目预览HTML用例>();
            services.AddScoped<根据标签筛选题目用例>();
            services.AddScoped<更新Ooxml题目用例>();
            services.AddScoped<删除题目用例>();
            services.AddScoped<更新题目题型用例>();
            services.AddScoped<获取下一道待识别题型题目用例>();
            services.AddScoped<根据Ooxml识别题型用例>();

            services.AddScoped<当前导入题目结果构建器>();
            services.AddScoped<列出试卷记录用例>();
            services.AddScoped<开始导入试卷用例>();
            services.AddScoped<获取当前导入题目用例>();
            services.AddScoped<确认导入题目用例>();
            services.AddScoped<跳过导入题目用例>();
            services.AddScoped<退出导入试卷用例>();
            services.AddScoped<试卷模板识别器>();
            services.AddScoped<当前模板试卷解析器>();
            services.AddScoped<题目边界识别器>();
            services.AddScoped<答案区识别器>();
            services.AddScoped<单题内容划分器>();
            services.AddScoped<试卷元信息提取器>();

            services.AddScoped<Ooxml题型特征提取器>();
            services.AddScoped<选择题规则>();
            services.AddScoped<填空题规则>();
            services.AddScoped<实验题规则>();
            services.AddScoped<解答题规则>();
            services.AddScoped<作图题规则>();

            services.AddScoped<获取标签树用例>();
            services.AddScoped<获取标签种类列表用例>();
            services.AddScoped<获取标签列表用例>();
            services.AddScoped<新增标签用例>();
            services.AddScoped<更新标签用例>();
            services.AddScoped<调整标签父级用例>();
            services.AddScoped<调整标签排序用例>();
            services.AddScoped<移动标签用例>();
            services.AddScoped<删除标签用例>();

            return services;
        }
    }
}
