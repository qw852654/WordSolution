using System.Linq;
using 题库应用.标签模块;
using 题库基础设施.数据访问;
using 题库核心.标签模块.领域;
using 题库测试.测试支持;

namespace 题库测试.标签模块
{
    public class 获取标签树用例测试 : 集成测试基础类
    {
        [Fact]
        public void 执行后_应该按父子关系构建标签树并按同级排序值排序()
        {
            保存标签(
                标签.创建标签(1, "力学", "章节", null, 2, null, true),
                标签.创建标签(1, "运动学", "章节", null, 1, null, true),
                标签.创建标签(1, "匀速直线运动", "知识点", 2, 2, null, true),
                标签.创建标签(1, "位移", "知识点", 2, 1, null, true));

            using var 数据库 = 创建数据库上下文();
            var 用例 = new 获取标签树用例(new 标签仓储(数据库));

            var 结果 = 用例.执行();

            Assert.Equal(2, 结果.Count);
            Assert.Equal("运动学", 结果[0].名称);
            Assert.Equal("力学", 结果[1].名称);
            Assert.Equal(2, 结果[0].子标签列表.Count);
            Assert.Equal("位移", 结果[0].子标签列表[0].名称);
            Assert.Equal("匀速直线运动", 结果[0].子标签列表[1].名称);
        }

        [Fact]
        public void 执行后_没有父标签的标签应该作为根标签返回()
        {
            保存标签(
                标签.创建标签(1, "难度", null, null, 1, 1, true),
                标签.创建标签(2, "基础", null, 999, 1, 1, true));

            using var 数据库 = 创建数据库上下文();
            var 用例 = new 获取标签树用例(new 标签仓储(数据库));

            var 结果 = 用例.执行();

            Assert.Equal(2, 结果.Count);
            Assert.Contains(结果, 标签 => 标签.名称 == "难度");
            Assert.Contains(结果, 标签 => 标签.名称 == "基础");
        }
    }
}
