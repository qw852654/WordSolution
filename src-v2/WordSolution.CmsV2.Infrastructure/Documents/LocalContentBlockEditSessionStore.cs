using System.Text.Json;
using WordSolution.CmsV2.Domain.Documents;

namespace WordSolution.CmsV2.Infrastructure.Documents;

public sealed class LocalContentBlockEditSessionStore : IContentBlockEditSessionStore
{
    private const string EditSessionsDirectoryName = "edit-sessions";
    private const string ContentBlocksDirectoryName = "content-blocks";
    private const string SessionFileName = "session.json";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public async Task SaveAsync(
        string bankRootDirectory,
        ContentBlockEditSession session,
        CancellationToken cancellationToken = default)
    {
        ValidateBankRootDirectory(bankRootDirectory);
        ArgumentNullException.ThrowIfNull(session);
        ValidateSessionId(session.SessionId);

        var sessionDirectory = GetSessionDirectory(bankRootDirectory, session.SessionId);
        Directory.CreateDirectory(sessionDirectory);

        var sessionFilePath = Path.Combine(sessionDirectory, SessionFileName);
        await using var stream = new FileStream(
            sessionFilePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        await JsonSerializer.SerializeAsync(stream, session, JsonOptions, cancellationToken);
    }

    public async Task<ContentBlockEditSession?> GetAsync(
        string bankRootDirectory,
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        ValidateBankRootDirectory(bankRootDirectory);
        ValidateSessionId(sessionId);

        var sessionFilePath = Path.Combine(GetSessionDirectory(bankRootDirectory, sessionId), SessionFileName);
        if (!File.Exists(sessionFilePath))
        {
            return null;
        }

        await using var stream = new FileStream(
            sessionFilePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite,
            bufferSize: 81920,
            useAsync: true);

        return await JsonSerializer.DeserializeAsync<ContentBlockEditSession>(stream, JsonOptions, cancellationToken);
    }

    public async Task<IReadOnlyList<ContentBlockEditSession>> ListActiveAsync(
        string bankRootDirectory,
        CancellationToken cancellationToken = default)
    {
        ValidateBankRootDirectory(bankRootDirectory);

        var contentBlocksDirectory = Path.Combine(
            bankRootDirectory,
            EditSessionsDirectoryName,
            ContentBlocksDirectoryName);

        if (!Directory.Exists(contentBlocksDirectory))
        {
            return Array.Empty<ContentBlockEditSession>();
        }

        var sessions = new List<ContentBlockEditSession>();
        foreach (var sessionFilePath in Directory.EnumerateFiles(
                     contentBlocksDirectory,
                     SessionFileName,
                     SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var session = await ReadSessionFileAsync(sessionFilePath, cancellationToken);
            if (session is not null && IsActive(session.Status))
            {
                sessions.Add(session);
            }
        }

        return sessions
            .OrderBy(session => session.CreatedTime)
            .ThenBy(session => session.SessionId, StringComparer.Ordinal)
            .ToArray();
    }

    private static string GetSessionDirectory(string bankRootDirectory, string sessionId)
    {
        return Path.Combine(
            bankRootDirectory,
            EditSessionsDirectoryName,
            ContentBlocksDirectoryName,
            sessionId);
    }

    private static bool IsActive(ContentBlockEditSessionStatus status)
    {
        return status is
            ContentBlockEditSessionStatus.Created or
            ContentBlockEditSessionStatus.Opening or
            ContentBlockEditSessionStatus.Editing;
    }

    private static async Task<ContentBlockEditSession?> ReadSessionFileAsync(
        string sessionFilePath,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = new FileStream(
                sessionFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite,
                bufferSize: 81920,
                useAsync: true);

            return await JsonSerializer.DeserializeAsync<ContentBlockEditSession>(stream, JsonOptions, cancellationToken);
        }
        catch (JsonException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
    }

    private static void ValidateBankRootDirectory(string bankRootDirectory)
    {
        if (string.IsNullOrWhiteSpace(bankRootDirectory))
        {
            throw new ArgumentException("Bank root directory cannot be empty.", nameof(bankRootDirectory));
        }
    }

    private static void ValidateSessionId(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException("Session id cannot be empty.", nameof(sessionId));
        }
    }
}
