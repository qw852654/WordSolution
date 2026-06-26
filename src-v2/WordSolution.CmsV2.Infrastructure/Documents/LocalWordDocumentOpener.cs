using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WordSolution.CmsV2.Infrastructure.Documents;

internal static class LocalWordDocumentOpener
{
    private const int ShowWindowRestore = 9;
    private const int ActivationAttemptCount = 12;
    private const int ActivationAttemptDelayMilliseconds = 150;

    public static void Open(string docxPath)
    {
        var existingWordProcessIds = CaptureWordProcessIds();
        var startedProcess = Process.Start(new ProcessStartInfo
        {
            FileName = docxPath,
            UseShellExecute = true
        });

        TryBringWordWindowToFront(startedProcess, docxPath, existingWordProcessIds);
        startedProcess?.Dispose();
    }

    private static void TryBringWordWindowToFront(
        Process? startedProcess,
        string docxPath,
        HashSet<int> existingWordProcessIds)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var documentTitle = Path.GetFileNameWithoutExtension(docxPath);
        for (var attempt = 0; attempt < ActivationAttemptCount; attempt++)
        {
            foreach (var process in GetCandidateProcesses(startedProcess, documentTitle, existingWordProcessIds))
            {
                if (TryActivateProcessWindow(process))
                {
                    return;
                }
            }

            Thread.Sleep(ActivationAttemptDelayMilliseconds);
        }
    }

    private static HashSet<int> CaptureWordProcessIds()
    {
        return Process.GetProcessesByName("WINWORD")
            .Select(process => process.Id)
            .ToHashSet();
    }

    private static IReadOnlyList<Process> GetCandidateProcesses(
        Process? startedProcess,
        string documentTitle,
        HashSet<int> existingWordProcessIds)
    {
        var candidates = new List<Process>();
        if (startedProcess is not null)
        {
            candidates.Add(startedProcess);
        }

        candidates.AddRange(Process.GetProcessesByName("WINWORD"));

        return candidates
            .GroupBy(process => process.Id)
            .Select(group => group.First())
            .OrderByDescending(process => ScoreProcess(process, documentTitle, existingWordProcessIds))
            .ThenByDescending(GetSafeStartTime)
            .ToArray();
    }

    private static int ScoreProcess(
        Process process,
        string documentTitle,
        HashSet<int> existingWordProcessIds)
    {
        var score = 0;
        if (!existingWordProcessIds.Contains(process.Id))
        {
            score += 10;
        }

        if (WindowTitleContains(process, documentTitle))
        {
            score += 100;
        }

        return score;
    }

    private static bool WindowTitleContains(Process process, string documentTitle)
    {
        try
        {
            process.Refresh();
            return !string.IsNullOrWhiteSpace(documentTitle)
                && process.MainWindowTitle.Contains(documentTitle, StringComparison.OrdinalIgnoreCase);
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private static DateTime GetSafeStartTime(Process process)
    {
        try
        {
            return process.StartTime;
        }
        catch (InvalidOperationException)
        {
            return DateTime.MinValue;
        }
    }

    private static bool TryActivateProcessWindow(Process process)
    {
        try
        {
            process.Refresh();
            var mainWindowHandle = process.MainWindowHandle;
            if (mainWindowHandle == IntPtr.Zero)
            {
                return false;
            }

            ShowWindow(mainWindowHandle, ShowWindowRestore);
            return SetForegroundWindow(mainWindowHandle);
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
}
