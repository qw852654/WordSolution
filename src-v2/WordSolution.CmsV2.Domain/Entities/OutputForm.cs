using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Rules;

namespace WordSolution.CmsV2.Domain.Entities;

public sealed class OutputForm
{
    private OutputForm()
    {
        Title = string.Empty;
    }

    public OutputForm(
        int handoutVersionId,
        int outputTemplateId,
        string title,
        OutputAudience audience,
        OutputFormat outputFormat,
        VisibilityMode visibilityMode,
        OutputFormStatus status = OutputFormStatus.Active,
        int sortOrder = 0,
        DateTimeOffset? updatedTime = null)
    {
        DomainGuard.Positive(handoutVersionId, nameof(HandoutVersionId));
        DomainGuard.Positive(outputTemplateId, nameof(OutputTemplateId));
        DomainGuard.NotWhiteSpace(title, nameof(Title));
        DomainGuard.ValidEnum(audience, nameof(Audience));
        DomainGuard.ValidEnum(outputFormat, nameof(OutputFormat));
        DomainGuard.ValidEnum(visibilityMode, nameof(VisibilityMode));
        DomainGuard.ValidEnum(status, nameof(Status));
        DomainGuard.NonNegative(sortOrder, nameof(SortOrder));

        HandoutVersionId = handoutVersionId;
        OutputTemplateId = outputTemplateId;
        Title = title.Trim();
        Audience = audience;
        OutputFormat = outputFormat;
        VisibilityMode = visibilityMode;
        Status = status;
        SortOrder = sortOrder;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public int Id { get; private set; }

    public int HandoutVersionId { get; private set; }

    public int OutputTemplateId { get; private set; }

    public string Title { get; private set; }

    public OutputAudience Audience { get; private set; }

    public OutputFormat OutputFormat { get; private set; }

    public VisibilityMode VisibilityMode { get; private set; }

    public OutputFormStatus Status { get; private set; }

    public int SortOrder { get; private set; }

    public DateTimeOffset UpdatedTime { get; private set; }
}
