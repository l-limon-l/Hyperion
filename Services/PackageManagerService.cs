using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HyperionWPF.Localization;

namespace HyperionWPF.Services
{
    /// <summary>Which of the three package managers Hyperion can currently use.</summary>
    internal sealed class ManagerAvailability
    {
        public bool Winget { get; set; }

        public bool Scoop { get; set; }

        public bool Choco { get; set; }
    }

    /// <summary>
    /// Detects the package managers, installs the two optional ones on first run and
    /// removes them again if the user asks for that when closing.
    /// </summary>
    internal sealed class PackageManagerService
    {
        private readonly Action<string> log;

        public PackageManagerService(Action<string> log)
        {
            this.log = log;
        }

        public ManagerAvailability Availability { get; } = new ManagerAvailability();

        /// <summary>True for a manager Hyperion installed itself, and may therefore remove again.</summary>
        public bool InstalledScoop { get; private set; }

        public bool InstalledChocolatey { get; private set; }

        /// <summary>
        /// Makes sure winget is present and brings up scoop and Chocolatey as fallbacks.
        /// Returns false when winget is missing, which is the one hard requirement.
        /// </summary>
        public async Task<bool> EnsureAsync(CancellationToken cancellationToken)
        {
            log(Strings.CheckingManagers);

            Availability.Winget = ProcessRunner.IsOnPath("winget");
            if (!Availability.Winget)
            {
                log(Strings.WingetMissing);
                return false;
            }

            log(Strings.Detected("winget"));

            Availability.Scoop = ProcessRunner.IsOnPath("scoop");
            if (Availability.Scoop)
            {
                log(Strings.Detected("scoop"));
            }
            else
            {
                await InstallScoopAsync(cancellationToken).ConfigureAwait(true);
            }

            Availability.Choco = ProcessRunner.IsOnPath("choco");
            if (Availability.Choco)
            {
                log(Strings.Detected("Chocolatey"));
            }
            else
            {
                await InstallChocolateyAsync(cancellationToken).ConfigureAwait(true);
            }

            log(Strings.ManagersReady);
            return true;
        }

        private async Task InstallScoopAsync(CancellationToken cancellationToken)
        {
            log(Strings.InstallingTool("scoop"));

            // Scoop refuses to install into an administrator session unless asked to.
            const string Script =
                "$ErrorActionPreference='Stop'; " +
                "Invoke-RestMethod -Uri https://get.scoop.sh -OutFile \"$env:TEMP\\install-scoop.ps1\"; " +
                "& \"$env:TEMP\\install-scoop.ps1\" -RunAsAdmin";

            await ProcessRunner.RunAsync(
                "powershell",
                "-NoProfile -ExecutionPolicy Bypass -Command \"" + Script + "\"",
                log,
                cancellationToken).ConfigureAwait(true);

            ProcessRunner.RefreshPath();
            Availability.Scoop = ProcessRunner.IsOnPath("scoop");
            InstalledScoop = Availability.Scoop;
            if (!Availability.Scoop)
            {
                log(Strings.ToolInstallFailed("scoop"));
            }
        }

        private async Task InstallChocolateyAsync(CancellationToken cancellationToken)
        {
            log(Strings.InstallingTool("Chocolatey"));

            await ProcessRunner.RunAsync(
                "winget",
                "install --id Chocolatey.Chocolatey --exact --silent " +
                "--accept-package-agreements --accept-source-agreements --disable-interactivity",
                log,
                cancellationToken).ConfigureAwait(true);

            ProcessRunner.RefreshPath();
            Availability.Choco = ProcessRunner.IsOnPath("choco");
            InstalledChocolatey = Availability.Choco;
            if (!Availability.Choco)
            {
                log(Strings.ToolInstallFailed("Chocolatey"));
            }
        }

        /// <summary>Removes only the managers Hyperion installed during this session.</summary>
        public async Task UninstallInstalledManagersAsync(CancellationToken cancellationToken)
        {
            if (InstalledChocolatey)
            {
                await UninstallChocolateyAsync(cancellationToken).ConfigureAwait(true);
            }

            if (InstalledScoop)
            {
                await UninstallScoopAsync(cancellationToken).ConfigureAwait(true);
            }
        }

        private async Task UninstallChocolateyAsync(CancellationToken cancellationToken)
        {
            await ProcessRunner.RunAsync(
                "winget",
                "uninstall --id Chocolatey.Chocolatey --exact --silent --disable-interactivity",
                log,
                cancellationToken).ConfigureAwait(true);

            string folder = Environment.ExpandEnvironmentVariables(@"%ProgramData%\chocolatey");
            await RemoveFolderAsync(folder, cancellationToken).ConfigureAwait(true);
            RemovePathEntry(Path.Combine(folder, "bin"));
            Availability.Choco = false;
            InstalledChocolatey = false;
        }

        private async Task UninstallScoopAsync(CancellationToken cancellationToken)
        {
            await ProcessRunner.RunAsync(
                "powershell",
                "-NoProfile -ExecutionPolicy Bypass -Command \"scoop uninstall scoop -p\"",
                log,
                cancellationToken).ConfigureAwait(true);

            string folder = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\scoop");
            await RemoveFolderAsync(folder, cancellationToken).ConfigureAwait(true);
            RemovePathEntry(Path.Combine(folder, "shims"));
            Availability.Scoop = false;
            InstalledScoop = false;
        }

        private async Task RemoveFolderAsync(string folder, CancellationToken cancellationToken)
        {
            if (!Directory.Exists(folder))
            {
                return;
            }

            await ProcessRunner.RunAsync(
                "cmd",
                "/C rmdir /S /Q \"" + folder + "\"",
                log,
                cancellationToken).ConfigureAwait(true);
        }

        private void RemovePathEntry(string entry)
        {
            try
            {
                foreach (EnvironmentVariableTarget target in
                    new[] { EnvironmentVariableTarget.User, EnvironmentVariableTarget.Machine })
                {
                    string current = Environment.GetEnvironmentVariable("PATH", target);
                    if (string.IsNullOrEmpty(current))
                    {
                        continue;
                    }

                    string[] kept = current
                        .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(part => !part.Trim().TrimEnd('\\')
                            .Equals(entry.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
                        .ToArray();

                    if (kept.Length != current.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Length)
                    {
                        Environment.SetEnvironmentVariable("PATH", string.Join(";", kept), target);
                    }
                }

                ProcessRunner.RefreshPath();
            }
            catch (Exception ex)
            {
                log("Could not clean PATH entry '" + entry + "': " + ex.Message);
            }
        }
    }
}
