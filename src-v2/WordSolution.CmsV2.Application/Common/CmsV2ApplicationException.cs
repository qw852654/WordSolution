namespace WordSolution.CmsV2.Application.Common;

public sealed class CmsV2ApplicationException : Exception
{
    public CmsV2ApplicationException(string message)
        : base(message)
    {
    }

    public CmsV2ApplicationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
