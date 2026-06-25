namespace WordSolution.CmsV2.Api;

public static class CmsV2CurrentBankResolver
{
    private const string LegacyBankKey = "LEGACY";
    private const string LegacyDisplayName = "当前题库";
    private const string TestKind = "Test";
    private const string ProductionKind = "Production";

    public static CmsV2CurrentBank Resolve(CmsV2ApiOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.Banks.Count == 0)
        {
            return ResolveLegacy(options);
        }

        var activeBankKey = NormalizeRequired(options.ActiveBankKey, "CmsV2:ActiveBankKey");
        var banksByKey = new Dictionary<string, CmsV2CurrentBank>(StringComparer.OrdinalIgnoreCase);

        foreach (var bank in options.Banks)
        {
            var key = NormalizeRequired(bank.Key, "CmsV2:Banks[].Key");
            if (banksByKey.ContainsKey(key))
            {
                throw new InvalidOperationException($"CmsV2 bank key '{key}' is duplicate. Bank keys must be unique ignoring case.");
            }

            var displayName = NormalizeRequired(bank.DisplayName, $"CmsV2:Banks[{key}].DisplayName");
            var kind = NormalizeKind(bank.Kind, key);
            var rootDirectory = NormalizeRequired(bank.RootDirectory, $"CmsV2:Banks[{key}].RootDirectory");

            banksByKey.Add(
                key,
                new CmsV2CurrentBank(
                    key,
                    displayName,
                    kind,
                    Path.GetFullPath(rootDirectory)));
        }

        if (!banksByKey.TryGetValue(activeBankKey, out var currentBank))
        {
            throw new InvalidOperationException($"CmsV2 ActiveBankKey '{activeBankKey}' does not match any configured bank key.");
        }

        return currentBank;
    }

    private static CmsV2CurrentBank ResolveLegacy(CmsV2ApiOptions options)
    {
        var rootDirectory = string.IsNullOrWhiteSpace(options.BankRootDirectory)
            ? CmsV2ApiOptions.DefaultBankRootDirectory
            : options.BankRootDirectory.Trim();

        return new CmsV2CurrentBank(
            LegacyBankKey,
            LegacyDisplayName,
            TestKind,
            Path.GetFullPath(rootDirectory));
    }

    private static string NormalizeRequired(string? value, string fieldName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrEmpty(normalized))
        {
            throw new InvalidOperationException($"{fieldName} must not be empty.");
        }

        return normalized;
    }

    private static string NormalizeKind(string? kind, string key)
    {
        var normalizedKind = NormalizeRequired(kind, $"CmsV2:Banks[{key}].Kind");

        if (string.Equals(normalizedKind, TestKind, StringComparison.OrdinalIgnoreCase))
        {
            return TestKind;
        }

        if (string.Equals(normalizedKind, ProductionKind, StringComparison.OrdinalIgnoreCase))
        {
            return ProductionKind;
        }

        throw new InvalidOperationException($"CmsV2 bank '{key}' Kind must be either '{TestKind}' or '{ProductionKind}'.");
    }
}
