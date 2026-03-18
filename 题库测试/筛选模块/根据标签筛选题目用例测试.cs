using System.Linq;
using 题库应用.筛选模块;
using 题库基础设施.数据访问;
using 题库核心.筛选模块.领域;
using 题库核心.题目模块.领域;
using 题库测试.测试支持;

namespace 题库测试.筛选模块
{
    public class 根据标签筛选题目用例测试 : 集成测试基础类
    {
        [Fact]
        public void 单步交集筛选_应该返回同时拥有全部标签的题目()
        {
            using var 数据库 = 创建数据库上下文();
            var 题目仓储 = new 题目仓储(数据库);

            var 题目一 = 题目.创建题目("题目一", new[] { 1, 2 });
            var 题目二 = 题目.创建题目("题目二", new[] { 1 });
            var 题目三 = 题目.创建题目("题目三", new[] { 2, 3 });

            题目仓储.增加题目(题目一);
            题目仓储.增加题目(题目二);
            题目仓储.增加题目(题目三);

            var 用例 = new 根据标签筛选题目用例(题目仓储);

            var 结果 = 用例.执行(new[]
            {
                new 筛选步骤(new[] { 1, 2 }, 组合方式.交集)
            });

            Assert.Single(结果);
            Assert.Equal("题目一", 结果[0].Description);
        }

        [Fact]
        public void 单步并集筛选_应该返回拥有任一标签的题目()
        {
            using var 数据库 = 创建数据库上下文();
            var 题目仓储 = new 题目仓储(数据库);

            题目仓储.增加题目(题目.创建题目("题目一", new[] { 1 }));
            题目仓储.增加题目(题目.创建题目("题目二", new[] { 2 }));
            题目仓储.增加题目(题目.创建题目("题目三", new[] { 3 }));

            var 用例 = new 根据标签筛选题目用例(题目仓储);

            var 结果 = 用例.执行(new[]
            {
                new 筛选步骤(new[] { 1, 2 }, 组合方式.并集)
            });

            Assert.Equal(2, 结果.Count);
            Assert.Contains(结果, 题目 => 题目.Description == "题目一");
            Assert.Contains(结果, 题目 => 题目.Description == "题目二");
        }

        [Fact]
        public void 多步交集筛选_应该基于上一步结果继续筛选()
        {
            using var 数据库 = 创建数据库上下文();
            var 题目仓储 = new 题目仓储(数据库);

            题目仓储.增加题目(题目.创建题目("题目一", new[] { 1, 2 }));
            题目仓储.增加题目(题目.创建题目("题目二", new[] { 1, 3 }));
            题目仓储.增加题目(题目.创建题目("题目三", new[] { 2, 3 }));

            var 用例 = new 根据标签筛选题目用例(题目仓储);

            var 结果 = 用例.执行(new[]
            {
                new 筛选步骤(new[] { 1 }, 组合方式.交集),
                new 筛选步骤(new[] { 2 }, 组合方式.交集, 组合方式.交集)
            });

            Assert.Single(结果);
            Assert.Equal("题目一", 结果[0].Description);
        }

        [Fact]
        public void 多步并集合并_应该把本步结果和当前结果合并去重()
        {
            using var 数据库 = 创建数据库上下文();
            var 题目仓储 = new 题目仓储(数据库);

            题目仓储.增加题目(题目.创建题目("题目一", new[] { 1 }));
            题目仓储.增加题目(题目.创建题目("题目二", new[] { 2 }));
            题目仓储.增加题目(题目.创建题目("题目三", new[] { 3 }));

            var 用例 = new 根据标签筛选题目用例(题目仓储);

            var 结果 = 用例.执行(new[]
            {
                new 筛选步骤(new[] { 1 }, 组合方式.交集),
                new 筛选步骤(new[] { 2, 3 }, 组合方式.并集, 组合方式.并集)
            });

            Assert.Equal(3, 结果.Count);
            Assert.Contains(结果, 题目 => 题目.Description == "题目一");
            Assert.Contains(结果, 题目 => 题目.Description == "题目二");
            Assert.Contains(结果, 题目 => 题目.Description == "题目三");
        }

        [Fact]
        public void 没有筛选步骤时_应该返回空结果()
        {
            using var 数据库 = 创建数据库上下文();
            var 用例 = new 根据标签筛选题目用例(new 题目仓储(数据库));

            var 结果 = 用例.执行(new 筛选步骤[0]);

            Assert.Empty(结果);
        }
    }
}
