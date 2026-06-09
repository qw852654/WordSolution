using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Exceptions;

namespace WordSolution.CmsV2.Domain.Rules;

internal static class DomainGuard
{
    public static void NotWhiteSpace(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{fieldName} cannot be empty.");
        }
    }

    public static void Positive(int value, string fieldName)
    {
        if (value <= 0)
        {
            throw new DomainException($"{fieldName} must be greater than zero.");
        }
    }

    public static void PositiveOrNull(int? value, string fieldName)
    {
        if (value.HasValue)
        {
            Positive(value.Value, fieldName);
        }
    }

    public static void NonNegative(int value, string fieldName)
    {
        if (value < 0)
        {
            throw new DomainException($"{fieldName} cannot be negative.");
        }
    }

    public static void ValidEnum<TEnum>(TEnum value, string fieldName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new DomainException($"{fieldName} has an invalid value.");
        }
    }

    public static void LockedVersionRequiresId(
        ReferenceMode referenceMode,
        int? lockedContentBlockVersionId)
    {
        ValidEnum(referenceMode, nameof(ReferenceMode));

        if (referenceMode == ReferenceMode.LockedVersion && !lockedContentBlockVersionId.HasValue)
        {
            throw new DomainException("LockedContentBlockVersionId is required when ReferenceMode is LockedVersion.");
        }
    }

    public static DateTimeOffset UpdatedNow(DateTimeOffset? updatedTime)
    {
        return updatedTime ?? DateTimeOffset.UtcNow;
    }
}

