namespace WordSolution.CmsV2.Domain.Documents;

public sealed class QuestionImportStyleOptions
{
    public static QuestionImportStyleOptions Default { get; } = new(
        QuestionStartStyleNames: ["例题", "典型例题", "变式", "提高", "练习题"]);

    public QuestionImportStyleOptions(IReadOnlyCollection<string> QuestionStartStyleNames)
    {
        QuestionStartStyleNamesNormalized = QuestionStartStyleNames
            .Select(QuestionPartStyleOptions.NormalizeStyleName)
            .OfType<string>()
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (QuestionStartStyleNamesNormalized.Count == 0)
        {
            throw new ArgumentException("Question start style names cannot be empty.", nameof(QuestionStartStyleNames));
        }
    }

    public IReadOnlySet<string> QuestionStartStyleNamesNormalized { get; }

    public bool IsQuestionStartStyle(string? styleName)
    {
        var normalized = QuestionPartStyleOptions.NormalizeStyleName(styleName);
        return normalized is not null && QuestionStartStyleNamesNormalized.Contains(normalized);
    }
}

public sealed record QuestionImportCandidateDocumentResult(
    string CandidateId,
    int SortOrder,
    string DocxPath,
    string HtmlPreviewPath);

public interface IQuestionImportDocumentProcessor
{
    Task<IReadOnlyList<QuestionImportCandidateDocumentResult>> SplitCandidatesAsync(
        string sourceDocxPath,
        string candidateDirectory,
        CancellationToken cancellationToken = default);

    Task CreateNeutralizedCandidateDocxAsync(
        string candidateDocxPath,
        string outputDocxPath,
        CancellationToken cancellationToken = default);
}
