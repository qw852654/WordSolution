using System.Collections.Generic;
using System.IO;
using System.Linq;
using 题库基础设施.初始化;

namespace 题库基础设施.题库实例
{
    public class 题库实例服务
    {
        private readonly 题库路径提供器 _题库路径提供器;
        private readonly 题库DbContext工厂 _题库DbContext工厂;
        private readonly 题库实例初始化器 _题库实例初始化器;

        public 题库实例服务(
            题库路径提供器 题库路径提供器,
            题库DbContext工厂 题库DbContext工厂,
            题库实例初始化器 题库实例初始化器)
        {
            _题库路径提供器 = 题库路径提供器;
            _题库DbContext工厂 = 题库DbContext工厂;
            _题库实例初始化器 = 题库实例初始化器;
        }

        public void 确保测试题库已初始化()
        {
            _题库实例初始化器.确保题库已初始化(题库路径提供器.默认测试题库键);
        }

        public void 确保现有题库已补齐初始化()
        {
            Directory.CreateDirectory(_题库路径提供器.获取题库中心根目录());

            foreach (var 题库键 in _题库路径提供器.获取所有题库键().Where(_题库路径提供器.题库目录结构完整))
            {
                _题库实例初始化器.确保题库已初始化(题库键);
            }
        }

        public IReadOnlyList<题库实例信息> 获取题库实例列表()
        {
            Directory.CreateDirectory(_题库路径提供器.获取题库中心根目录());

            return _题库路径提供器.获取所有题库键()
                .Select(构建题库实例信息)
                .ToList();
        }

        private 题库实例信息 构建题库实例信息(string 题库键)
        {
            var 是否已初始化 = _题库路径提供器.题库目录结构完整(题库键);
            var 题库实例信息 = new 题库实例信息
            {
                题库键 = 题库键,
                显示名称 = 题库键,
                是否已初始化 = 是否已初始化
            };

            if (!是否已初始化)
            {
                return 题库实例信息;
            }

            using var dbContext = _题库DbContext工厂.创建(题库键);
            题库实例信息.题目数量 = dbContext.题目表.Count();
            题库实例信息.标签数量 = dbContext.标签表.Count();
            return 题库实例信息;
        }
    }
}
