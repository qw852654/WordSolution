namespace WordSolution.CmsV2.Domain.Documents;

public enum QuestionImportSessionStatus
{
    Created = 0,
    Opening = 1,
    Editing = 2,
    Parsing = 3,
    ReadyForReview = 4,
    Importing = 5,
    Imported = 6,
    Failed = 7,
    Cancelled = 8,
    Expired = 9
}

public sealed record QuestionImportSessionLaunchRequest(
    string SessionId,
    string SourceDocxPath);

public interface IQuestionImportSessionLauncher
{
    Task OpenAsync(
        QuestionImportSessionLaunchRequest request,
        CancellationToken cancellationToken = default);
}

public interface IQuestionImportDocumentCloseChecker
{
    Task<bool> IsClosedAsync(
        string sourceDocxPath,
        CancellationToken cancellationToken = default);
}
