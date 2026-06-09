using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Rules;

namespace WordSolution.CmsV2.Domain.Entities;

public sealed class OutputTemplate
{
    public OutputTemplate(
        string title,
        string templateDocxPath,
        string? description = null,
        OutputTemplateStatus status = OutputTemplateStatus.Active,
        DateTimeOffset? updatedTime = null)
    {
        DomainGuard.NotWhiteSpace(title, nameof(Title));
        DomainGuard.NotWhiteSpace(templateDocxPath, nameof(TemplateDocxPath));
        DomainGuard.ValidEnum(status, nameof(Status));

        Title = title.Trim();
        Description = description?.Trim();
        TemplateDocxPath = templateDocxPath.Trim();
        Status = status;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public int Id { get; private set; }

    public string Title { get; private set; }

    public string? Description { get; private set; }

    public string TemplateDocxPath { get; private set; }

    public OutputTemplateStatus Status { get; private set; }

    public DateTimeOffset UpdatedTime { get; private set; }
}

