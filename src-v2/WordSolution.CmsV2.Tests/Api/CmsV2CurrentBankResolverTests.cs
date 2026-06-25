using WordSolution.CmsV2.Api;

namespace WordSolution.CmsV2.Tests.Api;

public sealed class CmsV2CurrentBankResolverTests
{
    [Fact]
    public void Resolve_uses_active_test_bank()
    {
        var options = CreateThreeBankOptions("TEST");

        var currentBank = CmsV2CurrentBankResolver.Resolve(options);

        Assert.Equal("TEST", currentBank.BankKey);
        Assert.Equal("测试题库", currentBank.DisplayName);
        Assert.Equal("Test", currentBank.Kind);
        Assert.Equal(Path.GetFullPath(@"E:\Desktop\题库中心\TEST\cms-v2"), currentBank.RootDirectory);
    }

    [Fact]
    public void Resolve_matches_active_key_case_insensitively_and_returns_configured_key()
    {
        var options = CreateThreeBankOptions("gz");

        var currentBank = CmsV2CurrentBankResolver.Resolve(options);

        Assert.Equal("GZ", currentBank.BankKey);
        Assert.Equal("高中题库", currentBank.DisplayName);
        Assert.Equal("Production", currentBank.Kind);
        Assert.Equal(Path.GetFullPath(@"E:\Desktop\题库中心\GZ\cms-v2"), currentBank.RootDirectory);
    }

    [Fact]
    public void Resolve_rejects_duplicate_keys_case_insensitively()
    {
        var options = new CmsV2ApiOptions
        {
            ActiveBankKey = "TEST",
            Banks =
            [
                new CmsV2BankOptions
                {
                    Key = "TEST",
                    DisplayName = "测试题库",
                    Kind = "Test",
                    RootDirectory = @"E:\Desktop\题库中心\TEST\cms-v2"
                },
                new CmsV2BankOptions
                {
                    Key = " test ",
                    DisplayName = "重复测试题库",
                    Kind = "Test",
                    RootDirectory = @"E:\Desktop\题库中心\TEST2\cms-v2"
                }
            ]
        };

        var exception = Assert.Throws<InvalidOperationException>(() => CmsV2CurrentBankResolver.Resolve(options));

        Assert.Contains("duplicate", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("TEST", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Resolve_rejects_unknown_active_bank_key()
    {
        var options = CreateThreeBankOptions("UNKNOWN");

        var exception = Assert.Throws<InvalidOperationException>(() => CmsV2CurrentBankResolver.Resolve(options));

        Assert.Contains("ActiveBankKey", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("UNKNOWN", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Resolve_rejects_invalid_kind()
    {
        var options = CreateThreeBankOptions("TEST");
        options.Banks[0].Kind = "Sandbox";

        var exception = Assert.Throws<InvalidOperationException>(() => CmsV2CurrentBankResolver.Resolve(options));

        Assert.Contains("Kind", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Test", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Production", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Resolve_uses_legacy_bank_root_directory_when_banks_are_empty()
    {
        var legacyRootDirectory = Path.Combine(Path.GetTempPath(), "cms-v2-legacy-bank");
        var options = new CmsV2ApiOptions
        {
            BankRootDirectory = legacyRootDirectory
        };

        var currentBank = CmsV2CurrentBankResolver.Resolve(options);

        Assert.Equal("LEGACY", currentBank.BankKey);
        Assert.Equal("当前题库", currentBank.DisplayName);
        Assert.Equal("Test", currentBank.Kind);
        Assert.Equal(Path.GetFullPath(legacyRootDirectory), currentBank.RootDirectory);
    }

    [Fact]
    public void Resolve_uses_legacy_default_when_banks_and_bank_root_directory_are_empty()
    {
        var options = new CmsV2ApiOptions
        {
            BankRootDirectory = string.Empty
        };

        var currentBank = CmsV2CurrentBankResolver.Resolve(options);

        Assert.Equal("LEGACY", currentBank.BankKey);
        Assert.Equal(Path.GetFullPath(CmsV2ApiOptions.DefaultBankRootDirectory), currentBank.RootDirectory);
    }

    private static CmsV2ApiOptions CreateThreeBankOptions(string activeBankKey)
    {
        return new CmsV2ApiOptions
        {
            ActiveBankKey = activeBankKey,
            Banks =
            [
                new CmsV2BankOptions
                {
                    Key = "TEST",
                    DisplayName = "测试题库",
                    Kind = "Test",
                    RootDirectory = @"E:\Desktop\题库中心\TEST\cms-v2"
                },
                new CmsV2BankOptions
                {
                    Key = "GZ",
                    DisplayName = "高中题库",
                    Kind = "Production",
                    RootDirectory = @"E:\Desktop\题库中心\GZ\cms-v2"
                },
                new CmsV2BankOptions
                {
                    Key = "CZ",
                    DisplayName = "初中题库",
                    Kind = "Production",
                    RootDirectory = @"E:\Desktop\题库中心\CZ\cms-v2"
                }
            ]
        };
    }
}
