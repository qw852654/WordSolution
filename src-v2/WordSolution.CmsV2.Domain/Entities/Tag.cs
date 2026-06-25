using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Exceptions;
using WordSolution.CmsV2.Domain.Rules;

namespace WordSolution.CmsV2.Domain.Entities;

public sealed class Tag
{
    public static readonly string[] AllowedColorTokens =
    [
        "tag-gray",
        "tag-orange",
        "tag-yellow",
        "tag-green",
        "tag-blue",
        "tag-purple",
        "tag-pink",
        "tag-red"
    ];

    private Tag()
    {
        Name = string.Empty;
        NormalizedName = string.Empty;
        Color = string.Empty;
    }

    public Tag(
        string name,
        string color,
        TagStatus status = TagStatus.Active,
        DateTimeOffset? createdTime = null,
        DateTimeOffset? updatedTime = null)
    {
        DomainGuard.NotWhiteSpace(name, nameof(Name));
        DomainGuard.NotWhiteSpace(color, nameof(Color));
        DomainGuard.ValidEnum(status, nameof(Status));

        Name = name.Trim();
        NormalizedName = NormalizeName(Name);
        Color = NormalizeColor(color);
        Status = status;
        CreatedTime = createdTime ?? DateTimeOffset.UtcNow;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime ?? CreatedTime);
    }

    public int Id { get; private set; }

    public string Name { get; private set; }

    public string NormalizedName { get; private set; }

    public string Color { get; private set; }

    public TagStatus Status { get; private set; }

    public DateTimeOffset CreatedTime { get; private set; }

    public DateTimeOffset UpdatedTime { get; private set; }

    public static string NormalizeName(string name)
    {
        DomainGuard.NotWhiteSpace(name, nameof(Name));
        return name.Trim().ToLowerInvariant();
    }

    public void Rename(string name, DateTimeOffset? updatedTime = null)
    {
        DomainGuard.NotWhiteSpace(name, nameof(Name));

        Name = name.Trim();
        NormalizedName = NormalizeName(Name);
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public void ChangeColor(string color, DateTimeOffset? updatedTime = null)
    {
        Color = NormalizeColor(color);
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public void Archive(DateTimeOffset? updatedTime = null)
    {
        Status = TagStatus.Archived;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    public void Restore(DateTimeOffset? updatedTime = null)
    {
        Status = TagStatus.Active;
        UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
    }

    private static string NormalizeColor(string color)
    {
        DomainGuard.NotWhiteSpace(color, nameof(Color));

        var normalizedColor = color.Trim();
        if (!AllowedColorTokens.Contains(normalizedColor, StringComparer.Ordinal))
        {
            throw new DomainException($"Tag.Color must be one of: {string.Join(", ", AllowedColorTokens)}.");
        }

        return normalizedColor;
    }
}
