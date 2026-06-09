namespace WordSolution.CmsV2.Domain.Exceptions;

public sealed class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }
}

