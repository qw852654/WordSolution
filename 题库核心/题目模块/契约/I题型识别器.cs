using System.Collections.Generic;
using 题库核心.题目模块.领域;

namespace 题库核心.题目模块.契约
{
    public interface I题型识别器
    {
        题型识别结果 识别(string ooxml内容, IReadOnlyList<题型定义> 当前题型定义列表);
    }
}
