using System.Linq;
using Aspose.Words;

namespace 题库应用.试卷导入模块.试卷解析
{
    public class 试卷模板识别器
    {
        private readonly 题目边界识别器 _题目边界识别器;
        private readonly 答案区识别器 _答案区识别器;

        public 试卷模板识别器(题目边界识别器 题目边界识别器, 答案区识别器 答案区识别器)
        {
            _题目边界识别器 = 题目边界识别器;
            _答案区识别器 = 答案区识别器;
        }

        public bool 是当前模板(Document 文档)
        {
            var 正文段落列表 = 文档.Sections
                .Cast<Section>()
                .SelectMany(节 => 节.Body.Paragraphs.Cast<Paragraph>())
                .ToList();

            var 有题号起点 = 正文段落列表.Any(段落 => _题目边界识别器.是题号起点文本(段落.ToString(SaveFormat.Text).Trim()));
            var 有答案区底纹 = 正文段落列表.Any(_答案区识别器.是答案区段落);
            return 有题号起点 && 有答案区底纹;
        }
    }
}
