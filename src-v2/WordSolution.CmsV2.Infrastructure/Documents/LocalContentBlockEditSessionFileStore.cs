using System.Security.Cryptography;
using WordSolution.CmsV2.Domain.Documents;

namespace WordSolution.CmsV2.Infrastructure.Documents;

public sealed class LocalContentBlockEditSessionFileStore : IContentBlockEditSessionFileStore
{
    private const string EditSessionsDirectoryName = "edit-sessions";
    private const string ContentBlocksDirectoryName = "content-blocks";
    private const string EditableDocxFileName = "edit.docx";

    public async Task<string> CreateEditableDocxAsync(
        string bankRootDirectory,
        string sessionId,
        byte[] sourceDocx,
        CancellationToken cancellationToken = default)
    {
        ValidateBankRootDirectory(bankRootDirectory);
        ValidateSessionId(sessionId);

        if (sourceDocx.Length == 0)
        {
            throw new ArgumentException("Source DOCX cannot be empty.", nameof(sourceDocx));
        }

        var sessionDirectory = GetSessionDirectory(bankRootDirectory, sessionId);
        Directory.CreateDirectory(sessionDirectory);

        var editableDocxPath = Path.Combine(sessionDirectory, EditableDocxFileName);
        await File.WriteAllBytesAsync(editableDocxPath, sourceDocx, cancellationToken);

        return editableDocxPath;
    }

    public async Task<byte[]?> ReadEditableDocxAsync(
        string editableDocxPath,
        CancellationToken cancellationToken = default)
    {
        ValidatePath(editableDocxPath, nameof(editableDocxPath));

        if (!File.Exists(editableDocxPath))
        {
            return null;
        }

        return await File.ReadAllBytesAsync(editableDocxPath, cancellationToken);
    }

    public async Task<string> ComputeHashAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ValidatePath(filePath, nameof(filePath));

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The edit session DOCX file was not found.", filePath);
        }

        await using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite,
            bufferSize: 81920,
            useAsync: true);

        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(hash);
    }

    public Task<bool> IsEditableDocxAvailableForSyncAsync(
        string editableDocxPath,
        CancellationToken cancellationToken = default)
    {
        ValidatePath(editableDocxPath, nameof(editableDocxPath));
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(editableDocxPath))
        {
            return Task.FromResult(false);
        }

        var directory = Path.GetDirectoryName(editableDocxPath);
        var fileName = Path.GetFileName(editableDocxPath);
        var wordLockFilePath = directory is null ? null : Path.Combine(directory, $"~${fileName}");
        if (wordLockFilePath is not null && File.Exists(wordLockFilePath))
        {
            return Task.FromResult(false);
        }

        try
        {
            using var stream = new FileStream(
                editableDocxPath,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None);
            return Task.FromResult(true);
        }
        catch (IOException)
        {
            return Task.FromResult(false);
        }
        catch (UnauthorizedAccessException)
        {
            return Task.FromResult(false);
        }
    }

    private static string GetSessionDirectory(string bankRootDirectory, string sessionId)
    {
        return Path.Combine(
            bankRootDirectory,
            EditSessionsDirectoryName,
            ContentBlocksDirectoryName,
            sessionId);
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

    private static void ValidatePath(string filePath, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be empty.", parameterName);
        }
    }
}
