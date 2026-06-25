namespace WordSolution.CmsV2.Api;

public sealed class CmsV2ApiOptions
{
    public const string SectionName = "CmsV2";
    public const string DefaultBankRootDirectory = @"E:\Desktop\题库中心-v2";

    public string BankRootDirectory { get; set; } = DefaultBankRootDirectory;

    public string ActiveBankKey { get; set; } = string.Empty;

    public List<CmsV2BankOptions> Banks { get; set; } = [];
}
