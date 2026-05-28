using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using 题库应用.讲义模块;
using 题库核心.讲义模块.契约;
using 题库核心.小节模块.契约;

namespace 题库应用.小节模块
{
    public class 导出小节Word用例
    {
        private readonly I小节仓储 _小节仓储;
        private readonly I讲义文件存储 _讲义文件存储;
        private readonly I讲义Word生成器 _讲义Word生成器;
        private readonly 内容块Word展开服务 _内容块Word展开服务;

        public 导出小节Word用例(
            I小节仓储 小节仓储,
            I讲义文件存储 讲义文件存储,
            I讲义Word生成器 讲义Word生成器,
            内容块Word展开服务 内容块Word展开服务)
        {
            _小节仓储 = 小节仓储;
            _讲义文件存储 = 讲义文件存储;
            _讲义Word生成器 = 讲义Word生成器;
            _内容块Word展开服务 = 内容块Word展开服务;
        }

        public 小节Word导出结果? 执行(int 小节ID)
        {
            var 小节 = _小节仓储.GetById(小节ID);
            if (小节 == null)
            {
                return null;
            }

            var 源文件列表 = new List<讲义生成源文件>();
            var 版本清单 = new List<Word生成版本清单项>();
            var 小节项列表 = _小节仓储.获取小节项列表(小节ID);
            foreach (var 小节项 in 小节项列表)
            {
                _内容块Word展开服务.展开内容块(
                    小节项.内容块ID,
                    小节项.引用版本模式,
                    小节项.内容块版本ID,
                    小节项.角色,
                    源文件列表,
                    版本清单);
            }

            var 文件名 = $"{清理文件名(小节.标题)}-小节-{DateTime.Now:yyyyMMddHHmmss}.docx";
            var 输出路径 = _讲义文件存储.获取小节导出文件路径(小节.Id, 文件名);
            _讲义Word生成器.生成(小节.标题, 源文件列表, 输出路径);

            var 文件内容 = _讲义文件存储.读取生成文件(输出路径);
            if (文件内容 == null)
            {
                throw new InvalidOperationException("小节 Word 已生成，但读取导出文件失败。");
            }

            return new 小节Word导出结果
            {
                文件名 = 文件名,
                文件内容 = 文件内容
            };
        }

        private static string 清理文件名(string 文件名)
        {
            var 清理后 = Path.GetInvalidFileNameChars()
                .Aggregate(文件名, (当前, 非法字符) => 当前.Replace(非法字符, '-'))
                .Trim();

            return string.IsNullOrWhiteSpace(清理后) ? "小节" : 清理后;
        }
    }

    public class 小节Word导出结果
    {
        public string 文件名 { get; set; } = string.Empty;

        public byte[] 文件内容 { get; set; } = Array.Empty<byte>();
    }
}
