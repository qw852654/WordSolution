using WordSolution.CmsV2.Domain.Enums;

namespace WordSolution.CmsV2.Domain.Documents;

public sealed record QuestionPartParseResult(
    ContentBlockPartParseStatus Status,
    string? Message,
    IReadOnlyList<QuestionPartParseResultItem> Parts);

public sealed record QuestionPartParseResultItem(
    ContentBlockPartType PartType,
    int SortOrder,
    string? PlainText,
    IReadOnlyList<string> SourceStyleNames,
    string? WarningMessage);
