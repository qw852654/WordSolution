using System.Text;
using WordSolution.CmsV2.Domain.Documents;

namespace WordSolution.CmsV2.Infrastructure.Documents;

public sealed class LocalContentBlockFileStore : IContentBlockFileStore
{
    public async Task SaveContentBlockDocxAsync(
        string docxPath,
        Stream docxStream,
        CancellationToken cancellationToken = default)
    {
        ValidatePath(docxPath, nameof(docxPath));
        ArgumentNullException.ThrowIfNull(docxStream);

        if (!docxStream.CanRead)
        {
            throw new ArgumentException("The DOCX stream must be readable.", nameof(docxStream));
        }

        if (docxStream.CanSeek)
        {
            if (docxStream.Length == 0)
            {
                throw new ArgumentException("The DOCX stream cannot be empty.", nameof(docxStream));
            }

            docxStream.Position = 0;
        }

        EnsureParentDirectory(docxPath);

        await using var outputStream = new FileStream(
            docxPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        await docxStream.CopyToAsync(outputStream, cancellationToken);
    }

    public async Task<byte[]?> ReadContentBlockDocxAsync(
        string docxPath,
        CancellationToken cancellationToken = default)
    {
        ValidatePath(docxPath, nameof(docxPath));

        if (!File.Exists(docxPath))
        {
            return null;
        }

        return await File.ReadAllBytesAsync(docxPath, cancellationToken);
    }

    public async Task<string?> ReadHtmlPreviewAsync(
        string htmlPreviewPath,
        CancellationToken cancellationToken = default)
    {
        return await ReadTextFileAsync(htmlPreviewPath, cancellationToken);
    }

    public async Task SavePlainTextAsync(
        string plainTextPath,
        string plainText,
        CancellationToken cancellationToken = default)
    {
        ValidatePath(plainTextPath, nameof(plainTextPath));
        EnsureParentDirectory(plainTextPath);

        await File.WriteAllTextAsync(plainTextPath, plainText ?? string.Empty, Encoding.UTF8, cancellationToken);
    }

    public async Task<string?> ReadPlainTextAsync(
        string plainTextPath,
        CancellationToken cancellationToken = default)
    {
        return await ReadTextFileAsync(plainTextPath, cancellationToken);
    }

    public Task<bool> ExistsAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ValidatePath(filePath, nameof(filePath));
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(File.Exists(filePath));
    }

    public Task DeleteIfExistsAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ValidatePath(filePath, nameof(filePath));
        cancellationToken.ThrowIfCancellationRequested();

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    private static async Task<string?> ReadTextFileAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        ValidatePath(filePath, nameof(filePath));

        if (!File.Exists(filePath))
        {
            return null;
        }

        return await File.ReadAllTextAsync(filePath, Encoding.UTF8, cancellationToken);
    }

    private static void EnsureParentDirectory(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
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
