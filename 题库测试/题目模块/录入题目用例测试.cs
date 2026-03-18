using System.IO;
using 题库应用.题目模块;
using 题库基础设施.数据访问;
using 题库测试.测试支持;

namespace 题库测试.题目模块
{
    public class 录入题目用例测试 : 集成测试基础类
    {
        [Fact]
        public void 执行后_应该保存题目并生成预览文件()
        {
            using var 数据库 = 创建数据库上下文();
            var 题目仓储 = new 题目仓储(数据库);
            var 题目文件存储 = 创建题目文件存储();
            var 题目预览生成器 = new 假题目预览生成器();

            var 用例 = new 录入题目用例(题目仓储, 题目文件存储, 题目预览生成器);
            var 请求 = new 录入题目的请求
            {
                Description = "测试题目",
                文件扩展名 = ".docx",
                题目文件内容 = new byte[] { 1, 2, 3, 4 }
            };

            var 新题目 = 用例.执行(请求);

            Assert.True(新题目.Id > 0);
            Assert.True(File.Exists(题目文件存储.获取题目文件路径(新题目.Id, ".docx")));
            Assert.True(File.Exists(题目文件存储.获取题目预览文件路径(新题目.Id)));

            var 已保存题目 = 题目仓储.GetById(新题目.Id);
            Assert.NotNull(已保存题目);
            Assert.Equal("测试题目", 已保存题目!.Description);
        }
    }
}
