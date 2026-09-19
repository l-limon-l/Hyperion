using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HyperionWPF.Services
{
    /// <summary>Outcome of a child process, including the case where it could not be started at all.</summary>
    internal sealed class ProcessResult
    {
        private ProcessResult(bool started, int exitCode, string failure)
        {
            Started = started;
            ExitCode = exitCode;
            Failure = failure;
        }

        /// <summary>False when the executable was not found or could not be launched.</summary>
        public bool Started { get; }

        public int ExitCode { get; }

        /// <summary>Why the process could not be started, or null.</summary>
        public string Failure { get; }

        public bool Succeeded => Started && ExitCode == 0;

        public static ProcessResult Completed(int exitCode) => new ProcessResult(true, exitCode, null);

        public static ProcessResult NotStarted(string failure) => new ProcessResult(false, int.MinValue, failure);
    }

    /// <summary>Runs console tools without a visible window and streams their output back line by line.</summary>
    internal static class ProcessRunner
    {
        /// <summary>
        /// Runs <paramref name="fileName"/> and reports each output line through <paramref name="onLine"/>.
        /// Never throws for an ordinary failure: a missing executable comes back as
        /// <see cref="ProcessResult.NotStarted"/>. Cancelling kills the child process.
        /// </summary>
        public static async Task<ProcessResult> RunAsync(
            string fileName,
            string arguments,
            Action<string> onLine = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };

            using (var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true })
            {
                var exited = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                process.Exited += (sender, e) => exited.TrySetResult(true);

                DataReceivedEventHandler forward = (sender, e) =>
                {
                    if (onLine != null && !string.IsNullOrWhiteSpace(e.Data))
                    {
                        onLine(e.Data);
                    }
                };
                process.OutputDataReceived += forward;
                process.ErrorDataReceived += forward;

                try
                {
                    process.Start();
                }
                catch (Win32Exception ex)
                {
                    return ProcessResult.NotStarted(ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    return ProcessResult.NotStarted(ex.Message);
                }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                using (cancellationToken.Register(() => TryKill(process)))
                {
                    await exited.Task.ConfigureAwait(false);
                }

                // Blocks only until the asynchronous readers have drained; the process has already exited.
                process.WaitForExit();
                return ProcessResult.Completed(process.ExitCode);
            }
        }

        /// <summary>True when <paramref name="command"/> resolves to a file on the current PATH.</summary>
        public static bool IsOnPath(string command)
        {
            string path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            string[] extensions = (Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT")
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string directory in path.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string trimmed = directory.Trim('"');
                if (trimmed.Length == 0)
                {
                    continue;
                }

                try
                {
                    if (extensions.Any(extension => File.Exists(Path.Combine(trimmed, command + extension)))
                        || File.Exists(Path.Combine(trimmed, command)))
                    {
                        return true;
                    }
                }
                catch (ArgumentException)
                {
                    // An unusable PATH entry (invalid characters) is simply skipped.
                }
            }

            return false;
        }

        /// <summary>
        /// Re-reads the machine and user PATH into this process. A package manager installed while
        /// Hyperion is running is otherwise invisible to it, because the environment block was
        /// captured at startup.
        /// </summary>
        public static void RefreshPath()
        {
            try
            {
                string machine = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) ?? string.Empty;
                string user = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? string.Empty;
                string combined = string.Join(";", new[] { machine, user }.Where(p => p.Length > 0));
                if (combined.Length > 0)
                {
                    Environment.SetEnvironmentVariable("PATH", combined);
                }
            }
            catch (Exception)
            {
                // Losing the refresh only costs us a fallback manager; it must never break installing.
            }
        }

        private static void TryKill(Process process)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill();
                }
            }
            catch (Exception)
            {
                // The process ended between the check and the kill, or we are not allowed to kill it.
            }
        }
    }
}
