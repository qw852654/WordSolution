namespace WordSolution.CmsV2.Api;

public sealed record CmsV2CurrentBank(
    string BankKey,
    string DisplayName,
    string Kind,
    string RootDirectory);
