using System.Diagnostics;
using WordSolution.CmsV2.Domain.Documents;

namespace WordSolution.CmsV2.Infrastructure.Documents;

public sealed class LocalContentBlockEditSessionLauncher : IContentBlockEditSessionLauncher
{
    public Task<ContentBlockEditLaunchResult> LaunchAsync(
        ContentBlockEditSession session,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(session.EditableDocxPath))
        {
            throw new ArgumentException("Edit session DOCX path cannot be empty.", nameof(session));
        }

        if (!File.Exists(session.EditableDocxPath))
        {
            throw new FileNotFoundException("The edit session DOCX file was not found.", session.EditableDocxPath);
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = session.EditableDocxPath,
            UseShellExecute = true
        });

        return Task.FromResult(new ContentBlockEditLaunchResult(
            ContentBlockEditLaunchMode.LocalShell,
            OpenedByServer: true,
            Message: "Word edit session launched."));
    }
}
