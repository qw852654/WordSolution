using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using 题库核心.讲义模块.契约;
using 题库核心.讲义模块.领域;
using 题库核心.小节模块.契约;

namespace 题库应用.讲义模块
{
    public class 生成讲义用例
    {
        private readonly I讲义仓储 _讲义仓储;
        private readonly I小节仓储 _小节仓储;
        private readonly I讲义文件存储 _讲义文件存储;
        private readonly I讲义Word生成器 _讲义Word生成器;
        private readonly 内容块Word展开服务 _内容块Word展开服务;

        public 生成讲义用例(
            I讲义仓储 讲义仓储,
            I小节仓储 小节仓储,
            I讲义文件存储 讲义文件存储,
            I讲义Word生成器 讲义Word生成器,
            内容块Word展开服务 内容块Word展开服务)
        {
            _讲义仓储 = 讲义仓储;
            _小节仓储 = 小节仓储;
            _讲义文件存储 = 讲义文件存储;
            _讲义Word生成器 = 讲义Word生成器;
            _内容块Word展开服务 = 内容块Word展开服务;
        }

        public 讲义生成记录结果? 执行(int 讲义ID)
        {
            var 讲义 = _讲义仓储.GetById(讲义ID);
            if (讲义 == null)
            {
                return null;
            }

            var 源文件列表 = new List<讲义生成源文件>();
            var 版本清单 = new List<Word生成版本清单项>();
            var 讲义项列表 = _讲义仓储.获取讲义项列表(讲义ID);
            foreach (var 讲义项 in 讲义项列表)
            {
                if (讲义项.目标类型 == 讲义项目标类型.小节)
                {
                    展开小节(讲义项, 源文件列表, 版本清单);
                }
                else
                {
                    _内容块Word展开服务.展开内容块(
                        讲义项.目标ID,
                        讲义项.引用版本模式,
                        讲义项.锁定内容块版本ID,
                        讲义项.角色,
                        源文件列表,
                        版本清单);
                }
            }

            var 文件名 = $"{清理文件名(讲义.标题)}-{DateTime.Now:yyyyMMddHHmmss}.docx";
            var 输出路径 = _讲义文件存储.获取讲义生成文件路径(讲义ID, 文件名);
            _讲义Word生成器.生成(讲义.标题, 源文件列表, 输出路径);

            var 版本清单Json = JsonSerializer.Serialize(版本清单, new JsonSerializerOptions { WriteIndented = true });
            var 生成记录 = 讲义生成记录.创建(讲义ID, 输出路径, 版本清单Json);
            _讲义仓储.增加生成记录(生成记录);
            return 讲义生成记录结果.从生成记录(生成记录);
        }

        private void 展开小节(
            讲义项 讲义项,
            List<讲义生成源文件> 源文件列表,
            List<Word生成版本清单项> 版本清单)
        {
            var 小节 = _小节仓储.GetById(讲义项.目标ID);
            if (小节 == null)
            {
                throw new InvalidOperationException($"讲义中的小节 {讲义项.目标ID} 不存在。");
            }

            var 小节项列表 = _小节仓储.获取小节项列表(小节.Id);
            foreach (var 小节项 in 小节项列表)
            {
                _内容块Word展开服务.展开内容块(
                    小节项.内容块ID,
                    小节项.引用版本模式,
                    小节项.内容块版本ID,
                    $"{讲义项.角色 ?? "小节"} / {小节项.角色 ?? "内容块"}",
                    源文件列表,
                    版本清单);
            }
        }

        private static string 清理文件名(string 文件名)
        {
            var 清理后 = Path.GetInvalidFileNameChars()
                .Aggregate(文件名, (当前, 非法字符) => 当前.Replace(非法字符, '-'))
                .Trim();

            return string.IsNullOrWhiteSpace(清理后) ? "讲义" : 清理后;
        }
    }
}
