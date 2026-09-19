using System;
using System.Threading;
using System.Threading.Tasks;
using HyperionWPF.Localization;
using HyperionWPF.Model;

namespace HyperionWPF.Services
{
    internal enum InstallStatus
    {
        Installed,
        AlreadyInstalled,
        RebootRequired,
        Failed,
        Cancelled,
    }

    /// <summary>What happened to one package, and which manager it happened through.</summary>
    internal sealed class InstallResult
    {
        private InstallResult(InstallStatus status, string manager)
        {
            Status = status;
            Manager = manager;
        }

        public InstallStatus Status { get; }

        /// <summary>The package manager that produced this result, or null when none ran.</summary>
        public string Manager { get; }

        public static InstallResult From(InstallStatus status, string manager) => new InstallResult(status, manager);
    }

    /// <summary>Installs one package, trying winget first and falling back to scoop and Chocolatey.</summary>
    internal sealed class PackageInstaller
    {
        // winget's documented result codes (see winget's `AppInstallerErrors.h`).
        private const int WingetUpdateNotApplicable = unchecked((int)0x8A15002B);
        private const int WingetPackageAlreadyInstalled = unchecked((int)0x8A150061);

        // Windows Installer: the package went in, but the machine has to restart.
        private const int ErrorSuccessRebootRequired = 3010;

        private readonly PackageManagerService managers;
        private readonly Action<string> log;

        public PackageInstaller(PackageManagerService managers, Action<string> log)
        {
            this.managers = managers;
            this.log = log;
        }

        public async Task<InstallResult> InstallAsync(PackageRef package, CancellationToken cancellationToken)
        {
            bool anyManagerTried = false;

            if (managers.Availability.Winget && package.WingetId != null)
            {
                anyManagerTried = true;
                log(Strings.TryingWinget(package.WingetId));

                bool alreadyInstalled = false;
                ProcessResult result = await ProcessRunner.RunAsync(
                    "winget",
                    "install --id " + package.WingetId + " --exact --silent " +
                    "--accept-package-agreements --accept-source-agreements --disable-interactivity",
                    line =>
                    {
                        if (LooksLikeProgress(line))
                        {
                            return;
                        }

                        if (MentionsAlreadyInstalled(line))
                        {
                            alreadyInstalled = true;
                        }

                        log(line);
                    },
                    cancellationToken).ConfigureAwait(true);

                if (cancellationToken.IsCancellationRequested)
                {
                    return InstallResult.From(InstallStatus.Cancelled, "winget");
                }

                if (result.Succeeded)
                {
                    return InstallResult.From(InstallStatus.Installed, "winget");
                }

                if (result.Started && result.ExitCode == ErrorSuccessRebootRequired)
                {
                    return InstallResult.From(InstallStatus.RebootRequired, "winget");
                }

                if (alreadyInstalled
                    || (result.Started
                        && (result.ExitCode == WingetUpdateNotApplicable
                            || result.ExitCode == WingetPackageAlreadyInstalled)))
                {
                    return InstallResult.From(InstallStatus.AlreadyInstalled, "winget");
                }
            }

            if (managers.Availability.Scoop && package.ScoopId != null)
            {
                anyManagerTried = true;
                log(Strings.TryingScoop(package.ScoopId));

                ProcessResult result = await ProcessRunner.RunAsync(
                    "scoop", "install " + package.ScoopId, log, cancellationToken).ConfigureAwait(true);

                if (cancellationToken.IsCancellationRequested)
                {
                    return InstallResult.From(InstallStatus.Cancelled, "scoop");
                }

                if (result.Succeeded)
                {
                    return InstallResult.From(InstallStatus.Installed, "scoop");
                }
            }

            if (managers.Availability.Choco && package.ChocoId != null)
            {
                anyManagerTried = true;
                log(Strings.TryingChoco(package.ChocoId));

                ProcessResult result = await ProcessRunner.RunAsync(
                    "choco",
                    "install " + package.ChocoId + " -y --no-progress --limit-output",
                    log,
                    cancellationToken).ConfigureAwait(true);

                if (cancellationToken.IsCancellationRequested)
                {
                    return InstallResult.From(InstallStatus.Cancelled, "choco");
                }

                if (result.Succeeded)
                {
                    return InstallResult.From(InstallStatus.Installed, "choco");
                }

                if (result.Started && result.ExitCode == ErrorSuccessRebootRequired)
                {
                    return InstallResult.From(InstallStatus.RebootRequired, "choco");
                }
            }

            if (!anyManagerTried)
            {
                log(Strings.NoManagerFor(package.DisplayId));
            }

            return InstallResult.From(InstallStatus.Failed, null);
        }

        /// <summary>winget draws its download bar with block characters and a spinner; neither belongs in the log.</summary>
        private static bool LooksLikeProgress(string line)
        {
            string trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed == "\\" || trimmed == "|" || trimmed == "/" || trimmed == "-")
            {
                return true;
            }

            return trimmed.IndexOf('█') >= 0 || trimmed.IndexOf('▒') >= 0;
        }

        private static bool MentionsAlreadyInstalled(string line) =>
            line.IndexOf("already installed", StringComparison.OrdinalIgnoreCase) >= 0
            || line.IndexOf("No available upgrade", StringComparison.OrdinalIgnoreCase) >= 0
            || line.IndexOf("No newer package", StringComparison.OrdinalIgnoreCase) >= 0
            || line.IndexOf("уже установлен", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
