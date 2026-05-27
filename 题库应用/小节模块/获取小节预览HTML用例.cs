using System.Linq;
using System.Net;
using System.Text;
using 题库核心.内容块模块.契约;
using 题库核心.内容块模块.领域;
using 题库核心.小节模块.契约;

namespace 题库应用.小节模块
{
    public class 获取小节预览HTML用例
    {
        private readonly I小节仓储 _小节仓储;
        private readonly I内容块仓储 _内容块仓储;
        private readonly I内容块文件存储 _内容块文件存储;

        public 获取小节预览HTML用例(
            I小节仓储 小节仓储,
            I内容块仓储 内容块仓储,
            I内容块文件存储 内容块文件存储)
        {
            _小节仓储 = 小节仓储;
            _内容块仓储 = 内容块仓储;
            _内容块文件存储 = 内容块文件存储;
        }

        public string? 执行(int 小节ID)
        {
            var 小节 = _小节仓储.GetById(小节ID);
            if (小节 == null)
            {
                return null;
            }

            var builder = new StringBuilder();
            builder.AppendLine("<!doctype html>");
            builder.AppendLine("<html lang=\"zh-CN\"><head><meta charset=\"utf-8\">");
            builder.AppendLine("<style>");
            builder.AppendLine("body{margin:0;padding:24px;background:#f5f7fa;color:#142033;font-family:'Microsoft YaHei','Segoe UI',Arial,sans-serif;}");
            builder.AppendLine("h1{margin:0 0 6px;font-size:22px;} .meta{color:#5c687a;margin-bottom:18px;font-size:13px;}");
            builder.AppendLine(".item{background:#fff;border:1px solid #d8dee8;border-radius:8px;margin:0 0 16px;overflow:hidden;}");
            builder.AppendLine(".item-head{display:flex;justify-content:space-between;gap:12px;padding:10px 12px;background:#f8fafc;border-bottom:1px solid #e7ebf1;font-size:13px;}");
            builder.AppendLine(".item-head strong{overflow:hidden;text-overflow:ellipsis;white-space:nowrap;} .item-head span{color:#5c687a;white-space:nowrap;}");
            builder.AppendLine("iframe{display:block;width:100%;height:440px;border:0;background:#fff;} .empty{padding:28px;text-align:center;color:#5c687a;}");
            builder.AppendLine("</style></head><body>");
            builder.AppendLine($"<h1>{WebUtility.HtmlEncode(小节.标题)}</h1>");
            builder.AppendLine($"<div class=\"meta\">{WebUtility.HtmlEncode(小节.状态.ToString())} · {WebUtility.HtmlEncode(小节.摘要 ?? "暂无摘要")}</div>");

            var 小节项列表 = _小节仓储.获取小节项列表(小节ID).OrderBy(项 => 项.排序).ThenBy(项 => 项.Id).ToList();
            if (小节项列表.Count == 0)
            {
                builder.AppendLine("<div class=\"empty\">当前小节还没有内容块。</div>");
            }

            foreach (var 小节项 in 小节项列表)
            {
                var 内容块 = _内容块仓储.GetById(小节项.内容块ID);
                if (内容块 == null)
                {
                    continue;
                }

                var 版本 = 小节项.引用版本模式 == 内容块引用版本模式.锁定版本 && 小节项.内容块版本ID.HasValue
                    ? _内容块仓储.获取版本(小节项.内容块版本ID.Value)
                    : _内容块仓储.获取当前版本(内容块.Id);
                var html = 版本 == null ? null : _内容块文件存储.读取内容块预览HTML(版本.Html预览路径);

                builder.AppendLine("<section class=\"item\">");
                builder.AppendLine("<div class=\"item-head\">");
                builder.AppendLine($"<strong>{WebUtility.HtmlEncode(内容块.标题)}</strong>");
                builder.AppendLine($"<span>{WebUtility.HtmlEncode(小节项.角色 ?? 内容块.类型.ToString())} · {WebUtility.HtmlEncode(小节项.引用版本模式.ToString())} · v{版本?.版本号.ToString() ?? "0"}</span>");
                builder.AppendLine("</div>");
                if (string.IsNullOrWhiteSpace(html))
                {
                    builder.AppendLine("<div class=\"empty\">这个内容块暂无预览。</div>");
                }
                else
                {
                    builder.AppendLine($"<iframe loading=\"lazy\" srcdoc=\"{WebUtility.HtmlEncode(html)}\"></iframe>");
                }

                builder.AppendLine("</section>");
            }

            builder.AppendLine("</body></html>");
            return builder.ToString();
        }
    }
}
