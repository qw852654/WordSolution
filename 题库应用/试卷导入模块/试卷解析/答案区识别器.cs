using System.Drawing;
using Aspose.Words;

namespace 题库应用.试卷导入模块.试卷解析
{
    public class 答案区识别器
    {
        public bool 是答案区段落(Paragraph 段落)
        {
            var 底纹 = 段落.ParagraphFormat?.Shading;
            if (底纹 == null)
            {
                return false;
            }

            return 是有效底纹颜色(底纹.BackgroundPatternColor)
                || 是有效底纹颜色(底纹.ForegroundPatternColor)
                || 底纹.Texture != TextureIndex.TextureNone;
        }

        private static bool 是有效底纹颜色(Color 颜色)
        {
            if (颜色 == Color.Empty)
            {
                return false;
            }

            return 颜色.ToArgb() != Color.White.ToArgb();
        }
    }
}
