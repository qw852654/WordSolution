using WordSolution.CmsV2.Domain.Enums;

namespace WordSolution.CmsV2.Domain.Documents;

public sealed class QuestionPartStyleOptions
{
    public static QuestionPartStyleOptions Default { get; } = new(
        StemStyleNames: ["例题", "典型例题", "变式", "提高", "练习题"],
        AnswerStyleNames: ["答案"],
        AnalysisStyleNames: ["解析"],
        HintStyleNames: ["教学讲解内容"],
        NormalTextStyleNames: ["正文", "Normal"]);

    public QuestionPartStyleOptions(
        IReadOnlyCollection<string> StemStyleNames,
        IReadOnlyCollection<string> AnswerStyleNames,
        IReadOnlyCollection<string> AnalysisStyleNames,
        IReadOnlyCollection<string> HintStyleNames,
        IReadOnlyCollection<string> NormalTextStyleNames)
    {
        StemStyleNamesByPart = Normalize(StemStyleNames);
        AnswerStyleNamesByPart = Normalize(AnswerStyleNames);
        AnalysisStyleNamesByPart = Normalize(AnalysisStyleNames);
        HintStyleNamesByPart = Normalize(HintStyleNames);
        NormalTextStyleNamesByPart = Normalize(NormalTextStyleNames);
    }

    public IReadOnlySet<string> StemStyleNamesByPart { get; }

    public IReadOnlySet<string> AnswerStyleNamesByPart { get; }

    public IReadOnlySet<string> AnalysisStyleNamesByPart { get; }

    public IReadOnlySet<string> HintStyleNamesByPart { get; }

    public IReadOnlySet<string> NormalTextStyleNamesByPart { get; }

    public ContentBlockPartType ResolvePartType(string? styleName)
    {
        var normalized = NormalizeStyleName(styleName);
        if (normalized is null)
        {
            return ContentBlockPartType.Other;
        }

        if (StemStyleNamesByPart.Contains(normalized) || NormalTextStyleNamesByPart.Contains(normalized))
        {
            return ContentBlockPartType.Stem;
        }

        if (AnswerStyleNamesByPart.Contains(normalized))
        {
            return ContentBlockPartType.Answer;
        }

        if (AnalysisStyleNamesByPart.Contains(normalized))
        {
            return ContentBlockPartType.Analysis;
        }

        if (HintStyleNamesByPart.Contains(normalized))
        {
            return ContentBlockPartType.Hint;
        }

        return ContentBlockPartType.Other;
    }

    public static string? NormalizeStyleName(string? styleName)
    {
        var normalized = styleName?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static IReadOnlySet<string> Normalize(IReadOnlyCollection<string> styleNames)
    {
        return styleNames
            .Select(NormalizeStyleName)
            .OfType<string>()
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
