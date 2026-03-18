using 题库应用.题目模块;
using System.Collections.Generic;
using 题库基础设施.数据访问;
using 题库测试.测试支持;

namespace 题库测试.题目模块
{
    public class 根据ID获取题目详情用例测试 : 集成测试基础类
    {
        [Fact]
        public void 执行后_应该返回对应题目()
        {
            using var 数据库 = 创建数据库上下文();
            var 题目仓储 = new 题目仓储(数据库);
            var 题目文件存储 = 创建题目文件存储();
            var 录入用例 = new 录入题目用例(题目仓储, 题目文件存储, new 假题目预览生成器());

            var 新题目 = 录入用例.执行(new 录入题目的请求
            {
                Description = "按ID查询测试题",
                标签ID列表 = new List<int> { 1, 2 },
                文件扩展名 = ".docx",
                题目文件内容 = new byte[] { 1, 2, 3 }
            });

            var 用例 = new 根据ID获取题目详情用例(题目仓储);

            var 查询结果 = 用例.执行(新题目.Id);

            Assert.NotNull(查询结果);
            Assert.Equal(新题目.Id, 查询结果!.Id);
            Assert.Equal("按ID查询测试题", 查询结果.Description);
            Assert.Equal(new[] { 1, 2 }, 查询结果.标签ID列表.OrderBy(id => id));
        }

        [Fact]
        public void 题目不存在时_应该返回空()
        {
            using var 数据库 = 创建数据库上下文();
            var 用例 = new 根据ID获取题目详情用例(new 题目仓储(数据库));

            var 查询结果 = 用例.执行(999);

            Assert.Null(查询结果);
        }
    }
}
