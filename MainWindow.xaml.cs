using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace HyperionWPF;

public partial class MainWindow : Window
{
	private readonly Dictionary<string, Dictionary<string, object>> softwareDb = new Dictionary<string, Dictionary<string, object>>
	{
		{
			"Web Browsers",
			new Dictionary<string, object>
			{
				{ "Chrome", "Google.Chrome" },
				{ "Opera", "Opera.Opera" },
				{ "Firefox", "Mozilla.Firefox" },
				{ "Edge", "Microsoft.Edge" },
				{ "Brave", "Brave.Brave" }
			}
		},
		{
			"Messaging",
			new Dictionary<string, object>
			{
				{ "Telegram", "Telegram.TelegramDesktop" },
				{ "Discord", "Discord.Discord" },
				{ "Teams", "Microsoft.Teams" },
				{ "Zoom", "Zoom.Zoom" }
			}
		},
		{
			"Media",
			new Dictionary<string, object>
			{
				{ "VLC", "VideoLAN.VLC" },
				{ "Spotify", "Spotify.Spotify" },
				{ "Audacity", "Audacity.Audacity" },
				{ "HandBrake", "HandBrake.HandBrake" }
			}
		},
		{
			".NET",
			new Dictionary<string, object>
			{
				{
					".NET Desktop Runtime",
					new Dictionary<string, string>
					{
						{ "v8 (x64)", "Microsoft.DotNet.DesktopRuntime.8" },
						{ "v9 (x64)", "Microsoft.DotNet.DesktopRuntime.9" },
						{ "v10 (x64)", "Microsoft.DotNet.DesktopRuntime.10" }
					}
				},
				{
					"ASP.NET Core Runtime",
					new Dictionary<string, string>
					{
						{ "v8 (x64)", "Microsoft.DotNet.AspNetCore.8" },
						{ "v9 (x64)", "Microsoft.DotNet.AspNetCore.9" },
						{ "v10 (x64)", "Microsoft.DotNet.AspNetCore.10" }
					}
				},
				{ ".NET 4.8.1", "Microsoft.DotNet.Framework.DeveloperPack_4" }
			}
		},
		{
			"Java",
			new Dictionary<string, object>
			{
				{
					"Java (AdoptOpenJDK)",
					new Dictionary<string, string>
					{
						{ "8 (x64)", "EclipseAdoptium.Temurin.8.JRE" },
						{ "11 (x64)", "EclipseAdoptium.Temurin.11.JRE" },
						{ "17 (x64)", "EclipseAdoptium.Temurin.17.JRE" },
						{ "21 (x64)", "EclipseAdoptium.Temurin.21.JRE" }
					}
				},
				{
					"Amazon Corretto JDK",
					new Dictionary<string, string>
					{
						{ "8 (x64)", "Amazon.Corretto.8.JDK" },
						{ "11 (x64)", "Amazon.Corretto.11.JDK" },
						{ "21 (x64)", "Amazon.Corretto.21.JDK" }
					}
				}
			}
		},
		{
			"Imaging",
			new Dictionary<string, object>
			{
				{ "Krita", "KDE.Krita" },
				{ "Blender", "BlenderFoundation.Blender" },
				{ "GIMP", "GIMP.GIMP" },
				{ "IrfanView", "IrfanSkiljan.IrfanView" },
				{ "Inkscape", "Inkscape.Inkscape" },
				{ "Greenshot", "Greenshot.Greenshot" },
				{ "ShareX", "ShareX.ShareX" }
			}
		},
		{
			"Documents",
			new Dictionary<string, object>
			{
				{ "Foxit Reader", "Foxit.FoxitReader" },
				{ "SumatraPDF", "SumatraPDF.SumatraPDF" }
			}
		},
		{
			"Security",
			new Dictionary<string, object> { { "Malwarebytes", "Malwarebytes.Malwarebytes" } }
		},
		{
			"File Sharing & Storage",
			new Dictionary<string, object>
			{
				{ "qBittorrent", "qBittorrent.qBittorrent" },
				{ "Dropbox", "Dropbox.Dropbox" },
				{ "Google Drive", "Google.GoogleDrive" },
				{ "Quick Share", "Google.QuickShare" },
				{ "OneDrive", "Microsoft.OneDrive" }
			}
		},
		{
			"Other",
			new Dictionary<string, object>
			{
				{ "Evernote", "Evernote.Evernote" },
				{ "Steam", "Valve.Steam" },
				{ "Epic Games", "EpicGames.EpicGamesLauncher" },
				{ "EA App", "ElectronicArts.EADesktop" },
				{ "GOG Galaxy", "GOG.Galaxy" },
				{ "KeePass 2", "DominikReichl.KeePass" },
				{ "Everything", "voidtools.Everything" }
			}
		},
		{
			"Utilities",
			new Dictionary<string, object>
			{
				{ "TeamViewer", "TeamViewer.TeamViewer" },
				{
					"RealVNC",
					new Dictionary<string, string>
					{
						{ "Server", "RealVNC.VNCServer" },
						{ "Viewer", "RealVNC.VNCViewer" }
					}
				},
				{ "TightVNC", "GlavSoft.TightVNC" },
				{ "TeraCopy", "CodeSector.TeraCopy" },
				{ "Revo", "RevoUninstaller.RevoUninstaller" },
				{ "WizTree", "AntibodySoftware.WizTree" },
				{ "CCleaner", "Piriform.CCleaner" }
			}
		},
		{
			"Compression",
			new Dictionary<string, object>
			{
				{ "7-Zip", "7zip.7zip" },
				{ "WinRAR", "RARLab.WinRAR" }
			}
		},
		{
			"VC++ Redistributables",
			new Dictionary<string, object>
			{
				{
					"VC Redist 2015-2022",
					new Dictionary<string, string>
					{
						{ "x64", "Microsoft.VCRedist.2015+.x64" },
						{ "x86", "Microsoft.VCRedist.2015+.x86" }
					}
				},
				{
					"VC Redist 2013",
					new Dictionary<string, string>
					{
						{ "x64", "Microsoft.VCRedist.2013.x64" },
						{ "x86", "Microsoft.VCRedist.2013.x86" }
					}
				},
				{
					"VC Redist 2012",
					new Dictionary<string, string>
					{
						{ "x64", "Microsoft.VCRedist.2012.x64" },
						{ "x86", "Microsoft.VCRedist.2012.x86" }
					}
				},
				{
					"VC Redist 2010",
					new Dictionary<string, string>
					{
						{ "x64", "Microsoft.VCRedist.2010.x64" },
						{ "x86", "Microsoft.VCRedist.2010.x86" }
					}
				},
				{
					"VC Redist 2008",
					new Dictionary<string, string>
					{
						{ "x64", "Microsoft.VCRedist.2008.x64" },
						{ "x86", "Microsoft.VCRedist.2008.x86" }
					}
				}
			}
		},
		{
			"Developer Tools",
			new Dictionary<string, object>
			{
				{ "Python 3", "Python.Python.3.14" },
				{ "Git", "Git.Git" },
				{ "FileZilla", "TimKosse.FileZillaClient" },
				{ "Notepad++", "Notepad++.Notepad++" },
				{ "WinSCP", "WinSCP.WinSCP" },
				{ "PuTTY", "PuTTY.PuTTY" },
				{ "WinMerge", "WinMerge.WinMerge" },
				{ "Visual Studio Code", "Microsoft.VisualStudioCode" },
				{ "Cursor", "Anysphere.Cursor" }
			}
		}
	};

	private static readonly Dictionary<string, string[]> fallbackMap = new Dictionary<string, string[]>
	{
		{
			"Google.Chrome",
			new string[2] { "googlechrome", "googlechrome" }
		},
		{
			"Opera.Opera",
			new string[2] { "opera", "opera" }
		},
		{
			"Mozilla.Firefox",
			new string[2] { "firefox", "firefox" }
		},
		{
			"Microsoft.Edge",
			new string[2] { null, "microsoft-edge" }
		},
		{
			"Brave.Brave",
			new string[2] { "brave", "brave" }
		},
		{
			"Telegram.TelegramDesktop",
			new string[2] { "telegram", "telegram" }
		},
		{
			"Discord.Discord",
			new string[2] { "discord", "discord" }
		},
		{
			"Microsoft.Teams",
			new string[2] { "microsoft-teams", "microsoft-teams" }
		},
		{
			"Zoom.Zoom",
			new string[2] { "zoom", "zoom" }
		},
		{
			"VideoLAN.VLC",
			new string[2] { "vlc", "vlc" }
		},
		{
			"Spotify.Spotify",
			new string[2] { "spotify", "spotify" }
		},
		{
			"Audacity.Audacity",
			new string[2] { "audacity", "audacity" }
		},
		{
			"HandBrake.HandBrake",
			new string[2] { "handbrake", "handbrake" }
		},
		{
			"Microsoft.DotNet.DesktopRuntime.8",
			new string[2] { null, "dotnet-8.0-desktopruntime" }
		},
		{
			"Microsoft.DotNet.DesktopRuntime.9",
			new string[2] { null, "dotnet-9.0-desktopruntime" }
		},
		{
			"Microsoft.DotNet.DesktopRuntime.10",
			new string[2] { null, "dotnet-10.0-desktopruntime" }
		},
		{
			"Microsoft.DotNet.AspNetCore.8",
			new string[2] { null, "dotnet-8.0-aspnetruntime" }
		},
		{
			"Microsoft.DotNet.AspNetCore.9",
			new string[2] { null, "dotnet-9.0-aspnetruntime" }
		},
		{
			"Microsoft.DotNet.AspNetCore.10",
			new string[2] { null, "dotnet-10.0-aspnetruntime" }
		},
		{
			"Microsoft.DotNet.Framework.DeveloperPack_4",
			new string[2] { null, "netfx-4.8.1-devpack" }
		},
		{
			"EclipseAdoptium.Temurin.8.JRE",
			new string[2] { "temurin8-jre", "temurin8jre" }
		},
		{
			"EclipseAdoptium.Temurin.11.JRE",
			new string[2] { "temurin11-jre", "temurin11jre" }
		},
		{
			"EclipseAdoptium.Temurin.17.JRE",
			new string[2] { "temurin17-jre", "temurin17jre" }
		},
		{
			"EclipseAdoptium.Temurin.21.JRE",
			new string[2] { "temurin21-jre", "temurin21jre" }
		},
		{
			"Amazon.Corretto.8.JDK",
			new string[2] { "corretto8-jdk", "corretto8jdk" }
		},
		{
			"Amazon.Corretto.11.JDK",
			new string[2] { "corretto11-jdk", "corretto11jdk" }
		},
		{
			"Amazon.Corretto.21.JDK",
			new string[2] { "corretto21-jdk", "corretto21jdk" }
		},
		{
			"KDE.Krita",
			new string[2] { "krita", "krita" }
		},
		{
			"BlenderFoundation.Blender",
			new string[2] { "blender", "blender" }
		},
		{
			"GIMP.GIMP",
			new string[2] { "gimp", "gimp" }
		},
		{
			"IrfanSkiljan.IrfanView",
			new string[2] { "irfanview", "irfanview" }
		},
		{
			"Inkscape.Inkscape",
			new string[2] { "inkscape", "inkscape" }
		},
		{
			"Greenshot.Greenshot",
			new string[2] { "greenshot", "greenshot" }
		},
		{
			"ShareX.ShareX",
			new string[2] { "sharex", "sharex" }
		},
		{
			"Foxit.FoxitReader",
			new string[2] { "foxit-reader", "foxitreader" }
		},
		{
			"SumatraPDF.SumatraPDF",
			new string[2] { "sumatrapdf", "sumatrapdf" }
		},
		{
			"Malwarebytes.Malwarebytes",
			new string[2] { null, "malwarebytes" }
		},
		{
			"qBittorrent.qBittorrent",
			new string[2] { "qbittorrent", "qbittorrent" }
		},
		{
			"Dropbox.Dropbox",
			new string[2] { "dropbox", "dropbox" }
		},
		{
			"Google.GoogleDrive",
			new string[2] { "googledrive", "googledrive" }
		},
		{
			"Google.QuickShare",
			new string[2]
		},
		{
			"Microsoft.OneDrive",
			new string[2]
		},
		{
			"Evernote.Evernote",
			new string[2] { "evernote", "evernote" }
		},
		{
			"Valve.Steam",
			new string[2] { "steam", "steam" }
		},
		{
			"EpicGames.EpicGamesLauncher",
			new string[2] { "epic-games-launcher", "epicgameslauncher" }
		},
		{
			"ElectronicArts.EADesktop",
			new string[2] { null, "ea-app" }
		},
		{
			"GOG.Galaxy",
			new string[2] { null, "goggalaxy" }
		},
		{
			"DominikReichl.KeePass",
			new string[2] { "keepass", "keepass" }
		},
		{
			"voidtools.Everything",
			new string[2] { "everything", "everything" }
		},
		{
			"TeamViewer.TeamViewer",
			new string[2] { "teamviewer", "teamviewer" }
		},
		{
			"RealVNC.VNCServer",
			new string[2] { null, "vnc-server" }
		},
		{
			"RealVNC.VNCViewer",
			new string[2] { "vnc-viewer", "vnc-viewer" }
		},
		{
			"GlavSoft.TightVNC",
			new string[2] { "tightvnc", "tightvnc" }
		},
		{
			"CodeSector.TeraCopy",
			new string[2] { "teracopy", "teracopy" }
		},
		{
			"RevoUninstaller.RevoUninstaller",
			new string[2] { "revo-uninstaller", "revo-uninstaller" }
		},
		{
			"AntibodySoftware.WizTree",
			new string[2] { "wiztree", "wiztree" }
		},
		{
			"Piriform.CCleaner",
			new string[2] { "ccleaner", "ccleaner" }
		},
		{
			"7zip.7zip",
			new string[2] { "7zip", "7zip" }
		},
		{
			"RARLab.WinRAR",
			new string[2] { "winrar", "winrar" }
		},
		{
			"Microsoft.VCRedist.2015+.x64",
			new string[2] { null, "vcredist140" }
		},
		{
			"Microsoft.VCRedist.2015+.x86",
			new string[2] { null, "vcredist140" }
		},
		{
			"Microsoft.VCRedist.2013.x64",
			new string[2] { null, "vcredist2013" }
		},
		{
			"Microsoft.VCRedist.2013.x86",
			new string[2] { null, "vcredist2013" }
		},
		{
			"Microsoft.VCRedist.2012.x64",
			new string[2] { null, "vcredist2012" }
		},
		{
			"Microsoft.VCRedist.2012.x86",
			new string[2] { null, "vcredist2012" }
		},
		{
			"Microsoft.VCRedist.2010.x64",
			new string[2] { null, "vcredist2010" }
		},
		{
			"Microsoft.VCRedist.2010.x86",
			new string[2] { null, "vcredist2010" }
		},
		{
			"Microsoft.VCRedist.2008.x64",
			new string[2] { null, "vcredist2008" }
		},
		{
			"Microsoft.VCRedist.2008.x86",
			new string[2] { null, "vcredist2008" }
		},
		{
			"Python.Python.3.14",
			new string[2] { "python", "python3" }
		},
		{
			"Git.Git",
			new string[2] { "git", "git" }
		},
		{
			"TimKosse.FileZillaClient",
			new string[2] { "filezilla", "filezilla" }
		},
		{
			"Notepad++.Notepad++",
			new string[2] { "notepadplusplus", "notepadplusplus" }
		},
		{
			"WinSCP.WinSCP",
			new string[2] { "winscp", "winscp" }
		},
		{
			"PuTTY.PuTTY",
			new string[2] { "putty", "putty" }
		},
		{
			"WinMerge.WinMerge",
			new string[2] { "winmerge", "winmerge" }
		},
		{
			"Microsoft.VisualStudioCode",
			new string[2] { "vscode", "vscode" }
		},
		{
			"Anysphere.Cursor",
			new string[2] { "cursor", null }
		}
	};

	private List<ToggleButton> allToggleSwitches = new List<ToggleButton>();

	private Dictionary<string, StackPanel> pagePanels = new Dictionary<string, StackPanel>();

	private int installSuccessCount = 0;

	private int installErrorCount = 0;

	private int alreadyInstalledCount = 0;

	private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;

	private const int DWMWCP_ROUND = 2;

	private bool _packageManagersReady = false;


	private bool IsRussian => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru";

	public bool PackageManagersReady => _packageManagersReady;

	[DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
	private static extern void DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

	public MainWindow()
	{
		InitializeComponent();
		LogBox.Text = (IsRussian ? "Ожидание выбора..." : "Waiting for selection...");
		InstallBtn.Content = (IsRussian ? "Установить выбранное (0)" : "Install Selected (0)");
		InstallBtn.IsEnabled = false;
		base.StateChanged += OnStateChanged;
		base.SourceInitialized += delegate
		{
			EnableRoundedCorners();
		};
		BuildUI();
		base.Loaded += Window_Loaded;
		base.Closing += MainWindow_Closing;
	}

	private async void Window_Loaded(object sender, RoutedEventArgs e)
	{
		await EnsurePackageManagersAsync();
		_packageManagersReady = true;
		InstallBtn.IsEnabled = true;
		AppendLog("Package managers ready.");
	}

	private async Task EnsurePackageManagersAsync()
	{
		AppendLog("Ensuring package managers are available...");
		if (!IsCommandAvailable("winget"))
		{
			AppendLog("Winget not found. Please install App Installer from Microsoft Store.");
			await EnsureWingetAsync();
			return;
		}
		AppendLog("Winget detected.");
		if (!IsCommandAvailable("scoop"))
		{
			AppendLog("Scoop not found. Installing scoop...");
			await InstallScoopAsync();
		}
		else
		{
			AppendLog("Scoop detected.");
		}
		if (!IsCommandAvailable("choco"))
		{
			AppendLog("Chocolatey not found. Installing chocolatey via winget...");
			await InstallChocolateyAsync();
		}
		else
		{
			AppendLog("Chocolatey detected.");
		}
	}

	private void EnableRoundedCorners()
	{
		try
		{
			IntPtr handle = new WindowInteropHelper(this).Handle;
			int attrValue = 2;
			DwmSetWindowAttribute(handle, 33, ref attrValue, 4);
		}
		catch
		{
		}
	}

	private async void MainWindow_Closing(object sender, CancelEventArgs e)
	{
		e.Cancel = true;
		try
		{
			MessageBoxResult result = MessageBox.Show(this, IsRussian ? "Удалить установленные менеджеры пакетов (Scoop, Chocolatey)?" : "Delete installed package managers (Scoop, Chocolatey)?", IsRussian ? "Подтверждение удаления" : "Uninstall Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
			if (result == MessageBoxResult.Yes)
			{
				base.IsEnabled = false;
				base.Title = IsRussian ? "Hyperion — Удаление пакетных менеджеров..." : "Hyperion — Uninstalling package managers...";
				AppendLog("User chose to uninstall package managers.");
				await UninstallPackageManagersAsync();
				_packageManagersReady = false;
				InstallBtn.IsEnabled = false;
				AppendLog("Package managers uninstall finished.");
				base.Title = "Hyperion";
				base.IsEnabled = true;
				MessageBox.Show(this,
					IsRussian ? "Удаление пакетных менеджеров завершено." : "Package managers uninstall completed.",
					"Hyperion", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			else
			{
				AppendLog("User chose to keep package managers.");
			}
		}
		catch (Exception ex)
		{
			AppendLog("Error during closing: " + ex.Message);
		}
		base.Closing -= MainWindow_Closing;
		Application.Current.Shutdown();
	}

	private async Task UninstallPackageManagersAsync()
	{
		AppendLog("Uninstalling Chocolatey...");
		await UninstallChocolateyAsync();

		AppendLog("Uninstalling Scoop...");
		await UninstallScoopAsync();
	}

	private async Task UninstallChocolateyAsync()
	{
		// Chocolatey was installed via winget, so uninstall via winget to remove the Windows installer entry
		try
		{
			AppendLog("Running: winget uninstall Chocolatey.Chocolatey ...");
			ProcessStartInfo psi = new ProcessStartInfo
			{
				FileName = "winget",
				Arguments = "uninstall Chocolatey.Chocolatey --silent",
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			using Process proc = Process.Start(psi);
			if (proc != null)
			{
				Task<string> outTask = proc.StandardOutput.ReadToEndAsync();
				Task<string> errTask = proc.StandardError.ReadToEndAsync();
				await Task.WhenAll(outTask, errTask);
				await Task.Run(() => proc.WaitForExit());
				if (!string.IsNullOrWhiteSpace(outTask.Result)) AppendLog(outTask.Result);
				if (!string.IsNullOrWhiteSpace(errTask.Result)) AppendLog(errTask.Result);
				AppendLog($"winget uninstall exited with code {proc.ExitCode}");
			}
		}
		catch (Exception ex)
		{
			AppendLog("Error uninstalling Chocolatey via winget: " + ex.Message);
		}
		await RemoveChocolateyFolderAsync();
	}

	private async Task UninstallScoopAsync()
	{
		string scoopPath = Environment.ExpandEnvironmentVariables("%USERPROFILE%\\scoop");
		// Step 1: try clean uninstall via scoop itself
		try
		{
			AppendLog("Running: scoop uninstall scoop -p ...");
			ProcessStartInfo psiScoop = new ProcessStartInfo
			{
				FileName = "powershell",
				Arguments = "-NoProfile -Command \"scoop uninstall scoop -p\"",
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			using Process procScoop = Process.Start(psiScoop);
			if (procScoop != null)
			{
				Task<string> outTask = procScoop.StandardOutput.ReadToEndAsync();
				Task<string> errTask = procScoop.StandardError.ReadToEndAsync();
				await Task.WhenAll(outTask, errTask);
				await Task.Run(() => procScoop.WaitForExit());
				if (!string.IsNullOrWhiteSpace(outTask.Result)) AppendLog(outTask.Result);
				if (!string.IsNullOrWhiteSpace(errTask.Result)) AppendLog(errTask.Result);
				AppendLog($"scoop uninstall exited with code {procScoop.ExitCode}");
			}
		}
		catch (Exception ex)
		{
			AppendLog("scoop self-uninstall failed: " + ex.Message);
		}
		// Step 2: force-remove the scoop folder if it still exists
		try
		{
			if (Directory.Exists(scoopPath))
			{
				AppendLog("Scoop folder still present, removing via rmdir...");
				ProcessStartInfo psi = new ProcessStartInfo
				{
					FileName = "cmd",
					Arguments = "/C rmdir /S /Q \"" + scoopPath + "\"",
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true
				};
				using Process proc = Process.Start(psi);
				if (proc != null)
				{
					Task<string> outTask = proc.StandardOutput.ReadToEndAsync();
					Task<string> errTask = proc.StandardError.ReadToEndAsync();
					await Task.WhenAll(outTask, errTask);
					await Task.Run(() => proc.WaitForExit());
					if (!string.IsNullOrWhiteSpace(outTask.Result)) AppendLog(outTask.Result);
					if (!string.IsNullOrWhiteSpace(errTask.Result)) AppendLog(errTask.Result);
				}
				AppendLog("Scoop folder removed.");
			}
			else
			{
				AppendLog("Scoop folder already gone.");
			}
		}
		catch (Exception ex2)
		{
			AppendLog("Error removing Scoop folder: " + ex2.Message);
		}
		// Step 3: clean PATH
		try
		{
			RemovePathEntry(Environment.ExpandEnvironmentVariables("%USERPROFILE%\\scoop\\shims"));
			AppendLog("Scoop path entry removed from PATH.");
		}
		catch (Exception ex)
		{
			AppendLog("Error cleaning Scoop PATH entry: " + ex.Message);
		}
	}

	private async Task RemoveChocolateyFolderAsync()
	{
		string chocPath = Environment.ExpandEnvironmentVariables("%ProgramData%\\chocolatey");
		try
		{
			if (Directory.Exists(chocPath))
			{
				ProcessStartInfo psi = new ProcessStartInfo
				{
					FileName = "cmd",
					Arguments = "/C rmdir /S /Q \"" + chocPath + "\"",
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true
				};
				using Process proc = Process.Start(psi);
				if (proc != null)
				{
					Task<string> outTask = proc.StandardOutput.ReadToEndAsync();
					Task<string> errTask = proc.StandardError.ReadToEndAsync();
					await Task.WhenAll(outTask, errTask);
					await Task.Run(() => proc.WaitForExit());
					if (!string.IsNullOrWhiteSpace(outTask.Result))
					{
						AppendLog(outTask.Result);
					}
					if (!string.IsNullOrWhiteSpace(errTask.Result))
					{
						AppendLog(errTask.Result);
					}
				}
				AppendLog("Chocolatey folder removed via cmd.");
				try
				{
					RemovePathEntry(Environment.ExpandEnvironmentVariables("%ProgramData%\\chocolatey\\bin"));
					AppendLog("Chocolatey path entry removed from PATH.");
				}
				catch (Exception ex)
				{
					AppendLog("Error cleaning Chocolatey PATH entry: " + ex.Message);
				}
			}
			else
			{
				AppendLog("Chocolatey folder not found, nothing to delete.");
			}
		}
		catch (Exception ex2)
		{
			AppendLog("Error removing Chocolatey folder: " + ex2.Message);
		}
	}

	private bool IsCommandAvailable(string command)
	{
		try
		{
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = "where",
				Arguments = command,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			using Process process = Process.Start(startInfo);
			string text = process.StandardOutput.ReadToEnd();
			process.WaitForExit(2000);
			if (process.ExitCode != 0)
			{
				return false;
			}
			string[] array = text.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				if (File.Exists(text2.Trim()))
				{
					return true;
				}
			}
			return false;
		}
		catch
		{
			return false;
		}
	}

	private void RemovePathEntry(string entryToRemove)
	{
		try
		{
			string text = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User) ?? string.Empty;
			string[] source = text.Split(';');
			string value = string.Join(";", source.Where((string p) => !p.Equals(entryToRemove, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(p)));
			Environment.SetEnvironmentVariable("Path", value, EnvironmentVariableTarget.User);
			Environment.SetEnvironmentVariable("Path", value);
		}
		catch (Exception ex)
		{
			AppendLog("Failed to remove PATH entry '" + entryToRemove + "': " + ex.Message);
		}
	}

	private async Task InstallScoopAsync()
	{
		ProcessStartInfo psi = new ProcessStartInfo
		{
			FileName = "powershell",
			Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"Invoke-RestMethod -Uri https://get.scoop.sh | Invoke-Expression\"",
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};
		using Process proc = Process.Start(psi);
		if (proc != null)
		{
			Task<string> outTask = proc.StandardOutput.ReadToEndAsync();
			Task<string> errTask = proc.StandardError.ReadToEndAsync();
			await Task.WhenAll(outTask, errTask);
			await Task.Run(() => proc.WaitForExit());
			AppendLog(outTask.Result);
			if (!string.IsNullOrWhiteSpace(errTask.Result))
			{
				AppendLog(errTask.Result);
			}
			AppendLog("Scoop installation completed.");
		}
	}

	private async Task InstallChocolateyAsync()
	{
		ProcessStartInfo psi = new ProcessStartInfo
		{
			FileName = "winget",
			Arguments = "install Chocolatey.Chocolatey --silent --accept-source-agreements --accept-package-agreements",
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};
		using Process proc = Process.Start(psi);
		if (proc != null)
		{
			Task<string> outTask = proc.StandardOutput.ReadToEndAsync();
			Task<string> errTask = proc.StandardError.ReadToEndAsync();
			await Task.WhenAll(outTask, errTask);
			await Task.Run(() => proc.WaitForExit());
			AppendLog(outTask.Result);
			if (!string.IsNullOrWhiteSpace(errTask.Result))
			{
				AppendLog(errTask.Result);
			}
			AppendLog("Chocolatey installation completed.");
		}
	}

	private async Task EnsureWingetAsync()
	{
		await base.Dispatcher.InvokeAsync(delegate
		{
			MessageBox.Show(this, IsRussian ? "Winget не найден. Пожалуйста, установите App Installer из Microsoft Store." : "Winget not found. Please install App Installer from the Microsoft Store.", "Dependency missing", MessageBoxButton.OK, MessageBoxImage.Exclamation);
		});
	}

	private void AppendLog(string message)
	{
		string text = DateTime.Now.ToString("HH:mm:ss");
		TextBox logBox = LogBox;
		logBox.Text = logBox.Text + "[" + text + "] " + message + "\n";
	}

	private void MinBtn_Click(object sender, RoutedEventArgs e)
	{
		base.WindowState = WindowState.Minimized;
	}

	private void MaxBtn_Click(object sender, RoutedEventArgs e)
	{
		base.WindowState = ((base.WindowState != WindowState.Maximized) ? WindowState.Maximized : WindowState.Normal);
	}

	private void CloseBtn_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void OnStateChanged(object sender, EventArgs e)
	{
		MaxBtn.Content = ((base.WindowState == WindowState.Maximized) ? "\ue923" : "\ue922");
	}

	private void ClearLogBtn_Click(object sender, RoutedEventArgs e)
	{
		LogBox.Text = (IsRussian ? "Лог очищен.\n" : "Log cleared.\n");
		installSuccessCount = 0;
		installErrorCount = 0;
		UpdateStatusBar();
	}

	private void SaveLogBtn_Click(object sender, RoutedEventArgs e)
	{
		SaveFileDialog saveFileDialog = new SaveFileDialog
		{
			FileName = $"Hyperion_log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}",
			DefaultExt = ".txt",
			Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
		};
		if (saveFileDialog.ShowDialog() == true)
		{
			try
			{
				string text = $"--- Hyperion Log ---\r\nGenerated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\n---\r\n";
				string contents = text + LogBox.Text.Replace("\n", "\r\n");
				File.WriteAllText(saveFileDialog.FileName, contents, Encoding.UTF8);
				LogBox.Text += (IsRussian ? ("\r\n[ИНФО] Лог сохранён: " + saveFileDialog.FileName + "\r\n") : ("\r\n[INFO] Log saved: " + saveFileDialog.FileName + "\r\n"));
			}
			catch (Exception ex)
			{
				LogBox.Text += (IsRussian ? ("\r\n[ОШИБКА] Не удалось сохранить лог: " + ex.Message + "\r\n") : ("\r\n[ERROR] Failed to save log: " + ex.Message + "\r\n"));
			}
		}
	}

	private void UpdateStatusBar()
	{
		string text = "";
		if (installSuccessCount > 0)
		{
			text += (IsRussian ? $"Установлено: {installSuccessCount}" : $"Installed: {installSuccessCount}");
		}
		if (alreadyInstalledCount > 0)
		{
			if (text.Length > 0)
			{
				text += "  │  ";
			}
			text += (IsRussian ? $"Уже было: {alreadyInstalledCount}" : $"Already installed: {alreadyInstalledCount}");
		}
		StatusInstalled.Text = text;
		StatusErrors.Text = ((installErrorCount <= 0) ? "" : (IsRussian ? $"Ошибки: {installErrorCount}" : $"Errors: {installErrorCount}"));
	}

	private UIElement GetCategoryIcon(string category)
	{
		switch (category)
		{
		case "Web Browsers":
		case "Messaging":
		case "Media":
		case "Imaging":
		case "Documents":
		case "Security":
		case "File Sharing & Storage":
		case "Other":
		case "Utilities":
		case "Compression":
		case "Developer Tools":
		{
			TextBlock textBlock = new TextBlock();
			TextBlock textBlock2 = textBlock;
			if (1 == 0)
			{
			}
			string text = category switch
			{
				"Web Browsers" => "\ue774", 
				"Messaging" => "\ue8f2", 
				"Media" => "\ue189", 
				"Imaging" => "\ue790", 
				"Documents" => "\ue8a5", 
				"Security" => "\ue727", 
				"File Sharing & Storage" => "\ue8b7", 
				"Other" => "\ue712", 
				"Utilities" => "\uec7a", 
				"Compression" => "\ue8fa", 
				"Developer Tools" => "\ue70f", 
				_ => "\ue712", 
			};
			if (1 == 0)
			{
			}
			textBlock2.Text = text;
			textBlock.FontFamily = new FontFamily("Segoe MDL2 Assets");
			textBlock.FontSize = 15.0;
			textBlock.VerticalAlignment = VerticalAlignment.Center;
			textBlock.Foreground = (Brush)FindResource("TextMutedBrush");
			textBlock.Margin = new Thickness(0.0, 0.0, 12.0, 0.0);
			return textBlock;
		}
		case ".NET":
		case "Java":
			return new TextBlock
			{
				Text = "\ue943",
				FontFamily = new FontFamily("Segoe MDL2 Assets"),
				FontSize = 15.0,
				VerticalAlignment = VerticalAlignment.Center,
				Foreground = (Brush)FindResource("TextMutedBrush"),
				Margin = new Thickness(0.0, 0.0, 12.0, 0.0)
			};
		case "VC++ Redistributables":
			return new TextBlock
			{
				Text = "\ue943",
				FontFamily = new FontFamily("Segoe MDL2 Assets"),
				FontSize = 15.0,
				VerticalAlignment = VerticalAlignment.Center,
				Foreground = (Brush)FindResource("TextMutedBrush"),
				Margin = new Thickness(0.0, 0.0, 12.0, 0.0)
			};
		default:
			return new TextBlock
			{
				Text = "\ue712",
				FontFamily = new FontFamily("Segoe MDL2 Assets"),
				FontSize = 15.0,
				VerticalAlignment = VerticalAlignment.Center,
				Foreground = (Brush)FindResource("TextMutedBrush"),
				Margin = new Thickness(0.0, 0.0, 12.0, 0.0)
			};
		}
	}

	private string GetLocalizedCategory(string k)
	{
		if (!IsRussian)
		{
			return k;
		}
		return k switch
		{
			"Web Browsers" => "Веб-браузеры", 
			"Messaging" => "Мессенджеры", 
			"Media" => "Мультимедиа", 
			".NET" => ".NET", 
			"Java" => "Java", 
			"Imaging" => "Изображения и дизайн", 
			"Documents" => "Документы", 
			"Security" => "Безопасность", 
			"File Sharing & Storage" => "Файлы и Облако", 
			"Other" => "Другое", 
			"Utilities" => "Утилиты", 
			"Compression" => "Архивирование", 
			"VC++ Redistributables" => "Библиотеки VC++", 
			"Developer Tools" => "Для разработчиков", 
			_ => k, 
		};
	}

	private string GetAppDescription(string n)
	{
		if (IsRussian)
		{
			return n switch
			{
				"Chrome" => "Быстрый и безопасный браузер от Google", 
				"Opera" => "Браузер с VPN и блокировщиком рекламы", 
				"Firefox" => "Свободный браузер с акцентом на приватность", 
				"Edge" => "Современный браузер от Microsoft", 
				"Brave" => "Приватный браузер со встроенной блокировкой рекламы", 
				"Telegram" => "Быстрый и безопасный мессенджер", 
				"Discord" => "Голосовой и текстовый чат для сообществ", 
				"Teams" => "Платформа для совместной работы от Microsoft", 
				"Zoom" => "Программа для видеоконференций и онлайн-встреч", 
				"VLC" => "Универсальный медиаплеер для всех форматов", 
				"Spotify" => "Стриминговый сервис для музыки и подкастов", 
				"Audacity" => "Бесплатный аудиоредактор с открытым кодом", 
				"HandBrake" => "Мощный видеоконвертер с открытым кодом", 
				".NET Desktop Runtime" => "Среда выполнения для десктопных приложений .NET", 
				"ASP.NET Core Runtime" => "Среда выполнения для веб-приложений ASP.NET Core", 
				".NET 4.8.1" => "Классическая среда выполнения .NET Framework", 
				"Java (AdoptOpenJDK)" => "Свободная среда выполнения Java (Eclipse Temurin)", 
				"Amazon Corretto JDK" => "Дистрибутив OpenJDK от Amazon", 
				"Krita" => "Профессиональный редактор для цифровой живописи", 
				"Blender" => "Программа для 3D-моделирования и анимации", 
				"GIMP" => "Бесплатный редактор растровой графики", 
				"IrfanView" => "Быстрый просмотрщик изображений", 
				"Inkscape" => "Бесплатный редактор векторной графики", 
				"Greenshot" => "Утилита для создания скриншотов", 
				"ShareX" => "Захват экрана с загрузкой и редактированием", 
				"Foxit Reader" => "Быстрый и лёгкий просмотрщик PDF", 
				"SumatraPDF" => "Минималистичный просмотрщик PDF и книг", 
				"Malwarebytes" => "Защита от вредоносного ПО и угроз", 
				"qBittorrent" => "Бесплатный торрент-клиент с открытым кодом", 
				"Dropbox" => "Облачное хранилище файлов и синхронизация", 
				"Google Drive" => "Облачное хранилище от Google", 
				"Quick Share" => "Быстрая передача файлов между устройствами", 
				"OneDrive" => "Облачное хранилище от Microsoft", 
				"Evernote" => "Приложение для заметок и организации дел", 
				"Steam" => "Платформа для покупки и запуска игр", 
				"Epic Games" => "Магазин игр от Epic Games", 
				"EA App" => "Платформа для игр Electronic Arts", 
				"GOG Galaxy" => "Магазин DRM-free игр и библиотека", 
				"KeePass 2" => "Менеджер паролей с открытым кодом", 
				"Everything" => "Мгновенный поиск файлов по имени", 
				"TeamViewer" => "Удалённый доступ и управление компьютером", 
				"RealVNC" => "Удалённый рабочий стол по протоколу VNC", 
				"TightVNC" => "Бесплатный VNC-клиент для удалённого доступа", 
				"TeraCopy" => "Быстрое и надёжное копирование файлов", 
				"Revo" => "Полное удаление программ с очисткой следов", 
				"WizTree" => "Быстрый анализатор дискового пространства", 
				"CCleaner" => "Очистка системы и оптимизация ПК", 
				"7-Zip" => "Бесплатный архиватор с открытым кодом", 
				"WinRAR" => "Популярный архиватор с поддержкой RAR и ZIP", 
				"VC Redist 2015-2022" => "Библиотеки Visual C++ 2015–2022", 
				"VC Redist 2013" => "Библиотеки Visual C++ 2013", 
				"VC Redist 2012" => "Библиотеки Visual C++ 2012", 
				"VC Redist 2010" => "Библиотеки Visual C++ 2010", 
				"VC Redist 2008" => "Библиотеки Visual C++ 2008", 
				"Python 3" => "Язык программирования Python 3", 
				"Git" => "Система контроля версий", 
				"FileZilla" => "Бесплатный FTP/SFTP клиент", 
				"Notepad++" => "Продвинутый текстовый редактор для кода", 
				"WinSCP" => "Клиент для передачи файлов по SCP/SFTP", 
				"PuTTY" => "SSH-клиент и эмулятор терминала", 
				"WinMerge" => "Сравнение и слияние файлов и папок", 
				"Visual Studio Code" => "Лёгкий и мощный редактор кода от Microsoft", 
				"Cursor" => "AI-редактор кода на базе VS Code", 
				_ => "Настройка и установка компонента", 
			};
		}
		return n switch
		{
			"Chrome" => "Fast and secure browser by Google", 
			"Opera" => "Browser with built-in VPN and ad blocker", 
			"Firefox" => "Free browser focused on privacy", 
			"Edge" => "Modern browser by Microsoft", 
			"Brave" => "Private browser with built-in ad blocking", 
			"Telegram" => "Fast and secure messenger", 
			"Discord" => "Voice and text chat for communities", 
			"Teams" => "Collaboration platform by Microsoft", 
			"Zoom" => "Video conferencing and online meetings", 
			"VLC" => "Universal media player for all formats", 
			"Spotify" => "Streaming service for music and podcasts", 
			"Audacity" => "Free open-source audio editor", 
			"HandBrake" => "Powerful open-source video converter", 
			".NET Desktop Runtime" => "Runtime for .NET desktop applications", 
			"ASP.NET Core Runtime" => "Runtime for ASP.NET Core web applications", 
			".NET 4.8.1" => "Classic .NET Framework runtime", 
			"Java (AdoptOpenJDK)" => "Free Java runtime (Eclipse Temurin)", 
			"Amazon Corretto JDK" => "OpenJDK distribution by Amazon", 
			"Krita" => "Professional digital painting application", 
			"Blender" => "3D modeling and animation software", 
			"GIMP" => "Free raster graphics editor", 
			"IrfanView" => "Fast and lightweight image viewer", 
			"Inkscape" => "Free vector graphics editor", 
			"Greenshot" => "Screenshot capture utility", 
			"ShareX" => "Screen capture with upload and editing", 
			"Foxit Reader" => "Fast and lightweight PDF viewer", 
			"SumatraPDF" => "Minimalist PDF and e-book reader", 
			"Malwarebytes" => "Malware protection and threat removal", 
			"qBittorrent" => "Free open-source torrent client", 
			"Dropbox" => "Cloud file storage and synchronization", 
			"Google Drive" => "Cloud storage by Google", 
			"Quick Share" => "Fast file sharing between devices", 
			"OneDrive" => "Cloud storage by Microsoft", 
			"Evernote" => "Note-taking and organization app", 
			"Steam" => "Platform for purchasing and playing games", 
			"Epic Games" => "Game store by Epic Games", 
			"EA App" => "Platform for Electronic Arts games", 
			"GOG Galaxy" => "DRM-free game store and library", 
			"KeePass 2" => "Open-source password manager", 
			"Everything" => "Instant file search by name", 
			"TeamViewer" => "Remote desktop access and control", 
			"RealVNC" => "Remote desktop via VNC protocol", 
			"TightVNC" => "Free VNC client for remote access", 
			"TeraCopy" => "Fast and reliable file copying", 
			"Revo" => "Complete program uninstaller with cleanup", 
			"WizTree" => "Fast disk space analyzer", 
			"CCleaner" => "System cleanup and PC optimization", 
			"7-Zip" => "Free open-source file archiver", 
			"WinRAR" => "Popular archiver supporting RAR and ZIP", 
			"VC Redist 2015-2022" => "Visual C++ 2015–2022 libraries", 
			"VC Redist 2013" => "Visual C++ 2013 libraries", 
			"VC Redist 2012" => "Visual C++ 2012 libraries", 
			"VC Redist 2010" => "Visual C++ 2010 libraries", 
			"VC Redist 2008" => "Visual C++ 2008 libraries", 
			"Python 3" => "Python 3 programming language", 
			"Git" => "Version control system", 
			"FileZilla" => "Free FTP/SFTP client", 
			"Notepad++" => "Advanced code text editor", 
			"WinSCP" => "SCP/SFTP file transfer client", 
			"PuTTY" => "SSH client and terminal emulator", 
			"WinMerge" => "File and folder comparison tool", 
			"Visual Studio Code" => "Lightweight code editor by Microsoft", 
			"Cursor" => "AI-powered code editor based on VS Code", 
			_ => "System component setup", 
		};
	}

	private void BuildUI()
	{
		foreach (KeyValuePair<string, Dictionary<string, object>> item in softwareDb)
		{
			StackPanel stackPanel = new StackPanel();
			stackPanel.Children.Add(new TextBlock
			{
				Text = GetLocalizedCategory(item.Key),
				FontSize = 26.0,
				FontWeight = FontWeights.Bold,
				Foreground = Brushes.White,
				Margin = new Thickness(0.0, 16.0, 0.0, 12.0)
			});
			foreach (KeyValuePair<string, object> item2 in item.Value)
			{
				if (item2.Value is string tag)
				{
					stackPanel.Children.Add(CreateCard(item2.Key, tag, item2.Key));
				}
				else if (item2.Value is Dictionary<string, string> subApps)
				{
					stackPanel.Children.Add(CreateGroupCard(item2.Key, subApps));
				}
			}
			pagePanels[item.Key] = stackPanel;
		}
		foreach (KeyValuePair<string, Dictionary<string, object>> item3 in softwareDb)
		{
			StackPanel stackPanel2 = new StackPanel
			{
				Orientation = Orientation.Horizontal
			};
			UIElement categoryIcon = GetCategoryIcon(item3.Key);
			stackPanel2.Children.Add(categoryIcon);
			stackPanel2.Children.Add(new TextBlock
			{
				Text = GetLocalizedCategory(item3.Key),
				VerticalAlignment = VerticalAlignment.Center
			});
			NavList.Items.Add(new ListBoxItem
			{
				Content = stackPanel2,
				Tag = item3.Key
			});
		}
		if (NavList.Items.Count > 0)
		{
			NavList.SelectedIndex = 0;
		}
	}

	private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!(NavList.SelectedItem is ListBoxItem { Tag: string tag }))
		{
			return;
		}
		ContentPanel.Children.Clear();
		if (pagePanels.TryGetValue(tag, out var value))
		{
			if (value.Parent is Panel panel)
			{
				panel.Children.Remove(value);
			}
			value.Opacity = 0.0;
			value.RenderTransform = new TranslateTransform(0.0, 20.0);
			ContentPanel.Children.Add(value);
			DoubleAnimation animation = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(250.0))
			{
				EasingFunction = new CubicEase
				{
					EasingMode = EasingMode.EaseOut
				}
			};
			DoubleAnimation animation2 = new DoubleAnimation(20.0, 0.0, TimeSpan.FromMilliseconds(300.0))
			{
				EasingFunction = new CubicEase
				{
					EasingMode = EasingMode.EaseOut
				}
			};
			value.BeginAnimation(UIElement.OpacityProperty, animation);
			value.RenderTransform.BeginAnimation(TranslateTransform.YProperty, animation2);
		}
		MainScroll.ScrollToTop();
	}

	private Image GetIconImage(string iconName)
	{
		string text = Regex.Replace(iconName, "[^a-zA-Z0-9_\\-\\.]", "_");
		Image image = new Image
		{
			Width = 32.0,
			Height = 32.0,
			Margin = new Thickness(15.0, 0.0, 15.0, 0.0)
		};
		try
		{
			image.Source = new BitmapImage(new Uri("pack://application:,,,/Icons/" + text + ".png"));
		}
		catch
		{
		}
		return image;
	}

	private Border CreateCard(string title, string tag, string iconKey)
	{
		if (iconKey == ".NET 4.8.1")
		{
			iconKey = ".net";
		}
		Border border = new Border
		{
			CornerRadius = new CornerRadius(8.0),
			HorizontalAlignment = HorizontalAlignment.Stretch,
			Height = 68.0,
			Margin = new Thickness(0.0, 0.0, 0.0, 6.0),
			Cursor = Cursors.Hand,
			Background = new SolidColorBrush(Color.FromRgb(45, 45, 48))
		};
		Grid grid = new Grid();
		grid.ColumnDefinitions.Add(new ColumnDefinition
		{
			Width = new GridLength(68.0)
		});
		grid.ColumnDefinitions.Add(new ColumnDefinition
		{
			Width = new GridLength(1.0, GridUnitType.Star)
		});
		grid.ColumnDefinitions.Add(new ColumnDefinition
		{
			Width = GridLength.Auto
		});
		grid.ColumnDefinitions.Add(new ColumnDefinition
		{
			Width = GridLength.Auto
		});
		Image iconImage = GetIconImage(iconKey);
		Grid.SetColumn(iconImage, 0);
		grid.Children.Add(iconImage);
		StackPanel stackPanel = new StackPanel
		{
			VerticalAlignment = VerticalAlignment.Center
		};
		stackPanel.Children.Add(new TextBlock
		{
			Text = title,
			Foreground = Brushes.White,
			FontSize = 14.0,
			FontWeight = FontWeights.SemiBold,
			TextTrimming = TextTrimming.CharacterEllipsis
		});
		stackPanel.Children.Add(new TextBlock
		{
			Text = GetAppDescription(title),
			Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 160)),
			FontSize = 12.0,
			TextWrapping = TextWrapping.Wrap,
			Margin = new Thickness(0.0, 2.0, 0.0, 0.0)
		});
		Grid.SetColumn(stackPanel, 1);
		grid.Children.Add(stackPanel);
		TextBlock statusTxt = new TextBlock
		{
			Text = (IsRussian ? "Откл." : "Off"),
			Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 160)),
			VerticalAlignment = VerticalAlignment.Center,
			Margin = new Thickness(0.0, 0.0, 12.0, 0.0),
			FontSize = 12.0
		};
		Grid.SetColumn(statusTxt, 2);
		grid.Children.Add(statusTxt);
		ToggleButton ts = new ToggleButton
		{
			Tag = tag,
			VerticalAlignment = VerticalAlignment.Center,
			Margin = new Thickness(0.0, 0.0, 14.0, 0.0),
			Style = (Style)FindResource("ToggleSwitchStyle")
		};
		ts.Checked += delegate
		{
			statusTxt.Text = (IsRussian ? "Вкл." : "On");
			statusTxt.Foreground = Brushes.White;
			UpdateCount();
		};
		ts.Unchecked += delegate
		{
			statusTxt.Text = (IsRussian ? "Откл." : "Off");
			statusTxt.Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 160));
			UpdateCount();
		};
		Grid.SetColumn(ts, 3);
		grid.Children.Add(ts);
		allToggleSwitches.Add(ts);
		border.PreviewMouseLeftButtonUp += delegate(object s, MouseButtonEventArgs e)
		{
			if (!(e.OriginalSource is DependencyObject child) || !IsChildOf(child, ts))
			{
				ts.IsChecked = !ts.IsChecked;
				e.Handled = true;
			}
		};
		AnimateCardHover(border);
		border.Child = grid;
		return border;
	}

	private Border CreateGroupCard(string groupTitle, Dictionary<string, string> subApps)
	{
		Border border = new Border
		{
			CornerRadius = new CornerRadius(8.0),
			HorizontalAlignment = HorizontalAlignment.Stretch,
			Margin = new Thickness(0.0, 0.0, 0.0, 6.0),
			Cursor = Cursors.Hand,
			Background = new SolidColorBrush(Color.FromRgb(45, 45, 48))
		};
		StackPanel stackPanel = new StackPanel();
		Grid grid = new Grid
		{
			Height = 68.0,
			Background = Brushes.Transparent
		};
		grid.ColumnDefinitions.Add(new ColumnDefinition
		{
			Width = new GridLength(68.0)
		});
		grid.ColumnDefinitions.Add(new ColumnDefinition
		{
			Width = new GridLength(1.0, GridUnitType.Star)
		});
		grid.ColumnDefinitions.Add(new ColumnDefinition
		{
			Width = GridLength.Auto
		});
		grid.ColumnDefinitions.Add(new ColumnDefinition
		{
			Width = GridLength.Auto
		});
		grid.ColumnDefinitions.Add(new ColumnDefinition
		{
			Width = new GridLength(44.0)
		});
		string iconName;
		switch (groupTitle)
		{
		case ".NET Desktop Runtime":
		case "ASP.NET Core Runtime":
			iconName = ".net";
			break;
		case "Java (AdoptOpenJDK)":
			iconName = "Java (AdoptOpenJDK) 8";
			break;
		case "Amazon Corretto JDK":
			iconName = "JDK (Amazon Corretto) 8";
			break;
		case "RealVNC":
			iconName = "RealVNC Server";
			break;
		case "VC Redist 2015-2022":
			iconName = "VC Redist x64 2015+";
			break;
		case "VC Redist 2013":
			iconName = "VC Redist x64 2013";
			break;
		case "VC Redist 2012":
			iconName = "VC Redist x64 2012";
			break;
		case "VC Redist 2010":
			iconName = "VC Redist x64 2010";
			break;
		case "VC Redist 2008":
			iconName = "VC Redist x64 2008";
			break;
		default:
			iconName = groupTitle;
			break;
		}
		Image iconImage = GetIconImage(iconName);
		Grid.SetColumn(iconImage, 0);
		grid.Children.Add(iconImage);
		StackPanel stackPanel2 = new StackPanel
		{
			VerticalAlignment = VerticalAlignment.Center
		};
		stackPanel2.Children.Add(new TextBlock
		{
			Text = groupTitle,
			Foreground = Brushes.White,
			FontSize = 14.0,
			FontWeight = FontWeights.SemiBold,
			TextTrimming = TextTrimming.CharacterEllipsis
		});
		stackPanel2.Children.Add(new TextBlock
		{
			Text = GetAppDescription(groupTitle),
			Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 160)),
			FontSize = 12.0,
			TextWrapping = TextWrapping.Wrap,
			Margin = new Thickness(0.0, 2.0, 0.0, 0.0)
		});
		Grid.SetColumn(stackPanel2, 1);
		grid.Children.Add(stackPanel2);
		TextBlock groupStatusTxt = new TextBlock
		{
			Text = (IsRussian ? "Откл." : "Off"),
			Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 160)),
			VerticalAlignment = VerticalAlignment.Center,
			Margin = new Thickness(0.0, 0.0, 12.0, 0.0),
			FontSize = 12.0
		};
		Grid.SetColumn(groupStatusTxt, 2);
		grid.Children.Add(groupStatusTxt);
		ToggleButton groupCb = new ToggleButton
		{
			VerticalAlignment = VerticalAlignment.Center,
			Margin = new Thickness(0.0, 0.0, 6.0, 0.0),
			Style = (Style)FindResource("ToggleSwitchStyle")
		};
		Grid.SetColumn(groupCb, 3);
		grid.Children.Add(groupCb);
		TextBlock arrow = new TextBlock
		{
			Text = "\ue70d",
			FontFamily = new FontFamily("Segoe MDL2 Assets"),
			Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 160)),
			FontSize = 12.0,
			VerticalAlignment = VerticalAlignment.Center,
			HorizontalAlignment = HorizontalAlignment.Center,
			RenderTransformOrigin = new Point(0.5, 0.5),
			RenderTransform = new RotateTransform(0.0)
		};
		Grid.SetColumn(arrow, 4);
		grid.Children.Add(arrow);
		stackPanel.Children.Add(grid);
		StackPanel stackPanel3 = new StackPanel
		{
			Margin = new Thickness(68.0, 0.0, 14.0, 10.0)
		};
		Border subBorder = new Border
		{
			BorderBrush = new SolidColorBrush(Color.FromRgb(62, 62, 66)),
			BorderThickness = new Thickness(0.0, 1.0, 0.0, 0.0),
			Child = stackPanel3,
			Visibility = Visibility.Collapsed,
			RenderTransform = new TranslateTransform(0.0, 0.0)
		};
		List<ToggleButton> subCbs = new List<ToggleButton>();
		bool isUpdatingSub = false;
		bool isUpdatingGroup = false;
		groupCb.Checked += delegate
		{
			groupStatusTxt.Text = (IsRussian ? "Вкл." : "On");
			groupStatusTxt.Foreground = Brushes.White;
			if (!isUpdatingGroup)
			{
				isUpdatingSub = true;
				foreach (ToggleButton item in subCbs)
				{
					item.IsChecked = true;
				}
				isUpdatingSub = false;
			}
		};
		groupCb.Unchecked += delegate
		{
			groupStatusTxt.Text = (IsRussian ? "Откл." : "Off");
			groupStatusTxt.Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 160));
			if (!isUpdatingGroup)
			{
				isUpdatingSub = true;
				foreach (ToggleButton item2 in subCbs)
				{
					item2.IsChecked = false;
				}
				isUpdatingSub = false;
			}
		};
		foreach (KeyValuePair<string, string> subApp in subApps)
		{
			Grid grid2 = new Grid
			{
				Margin = new Thickness(0.0, 6.0, 0.0, 6.0),
				Background = Brushes.Transparent
			};
			grid2.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = new GridLength(1.0, GridUnitType.Star)
			});
			grid2.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = GridLength.Auto
			});
			grid2.ColumnDefinitions.Add(new ColumnDefinition
			{
				Width = GridLength.Auto
			});
			TextBlock element = new TextBlock
			{
				Text = subApp.Key,
				Foreground = new SolidColorBrush(Color.FromRgb(176, 176, 176)),
				FontSize = 13.0,
				VerticalAlignment = VerticalAlignment.Center
			};
			Grid.SetColumn(element, 0);
			grid2.Children.Add(element);
			TextBlock subStatusTxt = new TextBlock
			{
				Text = (IsRussian ? "Откл." : "Off"),
				Foreground = new SolidColorBrush(Color.FromRgb(140, 140, 140)),
				VerticalAlignment = VerticalAlignment.Center,
				Margin = new Thickness(0.0, 0.0, 12.0, 0.0),
				FontSize = 12.0
			};
			Grid.SetColumn(subStatusTxt, 1);
			grid2.Children.Add(subStatusTxt);
			ToggleButton subCb = new ToggleButton
			{
				Tag = subApp.Value,
				VerticalAlignment = VerticalAlignment.Center,
				Margin = new Thickness(0.0, 0.0, 0.0, 0.0),
				Style = (Style)FindResource("ToggleSwitchStyle")
			};
			subCb.Checked += delegate
			{
				subStatusTxt.Text = (IsRussian ? "Вкл." : "On");
				subStatusTxt.Foreground = Brushes.White;
				UpdateCount();
				if (!isUpdatingSub)
				{
					isUpdatingGroup = true;
					groupCb.IsChecked = subCbs.Any((ToggleButton c) => c.IsChecked == true);
					isUpdatingGroup = false;
				}
			};
			subCb.Unchecked += delegate
			{
				subStatusTxt.Text = (IsRussian ? "Откл." : "Off");
				subStatusTxt.Foreground = new SolidColorBrush(Color.FromRgb(140, 140, 140));
				UpdateCount();
				if (!isUpdatingSub)
				{
					isUpdatingGroup = true;
					groupCb.IsChecked = subCbs.Any((ToggleButton c) => c.IsChecked == true);
					isUpdatingGroup = false;
				}
			};
			Grid.SetColumn(subCb, 2);
			grid2.Children.Add(subCb);
			allToggleSwitches.Add(subCb);
			subCbs.Add(subCb);
			grid2.PreviewMouseLeftButtonUp += delegate(object s, MouseButtonEventArgs e)
			{
				if (!(e.OriginalSource is DependencyObject child) || !IsChildOf(child, subCb))
				{
					subCb.IsChecked = !subCb.IsChecked;
					e.Handled = true;
				}
			};
			stackPanel3.Children.Add(grid2);
		}
		stackPanel.Children.Add(subBorder);
		border.Child = stackPanel;
		bool isExpanded = false;
		grid.PreviewMouseLeftButtonUp += delegate(object s, MouseButtonEventArgs e)
		{
			if (!(e.OriginalSource is DependencyObject child) || !IsChildOf(child, groupCb))
			{
				e.Handled = true;
				if (!isExpanded)
				{
					isExpanded = true;
					subBorder.Visibility = Visibility.Visible;
					subBorder.Opacity = 0.0;
					subBorder.RenderTransform = new TranslateTransform(0.0, -12.0);
					((RotateTransform)arrow.RenderTransform).BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation(0.0, 180.0, TimeSpan.FromMilliseconds(250.0))
					{
						EasingFunction = new CubicEase
						{
							EasingMode = EasingMode.EaseOut
						}
					});
					subBorder.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(250.0))
					{
						EasingFunction = new CubicEase
						{
							EasingMode = EasingMode.EaseOut
						}
					});
					subBorder.RenderTransform.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(-12.0, 0.0, TimeSpan.FromMilliseconds(300.0))
					{
						EasingFunction = new CubicEase
						{
							EasingMode = EasingMode.EaseOut
						}
					});
				}
				else
				{
					isExpanded = false;
					((RotateTransform)arrow.RenderTransform).BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation(180.0, 0.0, TimeSpan.FromMilliseconds(200.0))
					{
						EasingFunction = new CubicEase
						{
							EasingMode = EasingMode.EaseOut
						}
					});
					DoubleAnimation doubleAnimation = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(200.0))
					{
						EasingFunction = new CubicEase
						{
							EasingMode = EasingMode.EaseOut
						}
					};
					doubleAnimation.Completed += delegate
					{
						subBorder.Visibility = Visibility.Collapsed;
						subBorder.Opacity = 1.0;
					};
					subBorder.BeginAnimation(UIElement.OpacityProperty, doubleAnimation);
					if (subBorder.RenderTransform is TranslateTransform translateTransform)
					{
						translateTransform.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(0.0, -12.0, TimeSpan.FromMilliseconds(200.0))
						{
							EasingFunction = new CubicEase
							{
								EasingMode = EasingMode.EaseOut
							}
						});
					}
				}
			}
		};
		AnimateCardHover(border);
		return border;
	}

	private void AnimateCardHover(Border card)
	{
		SolidColorBrush bg = (SolidColorBrush)card.Background;
		card.MouseEnter += delegate
		{
			ColorAnimation animation = new ColorAnimation(Color.FromRgb(62, 62, 66), TimeSpan.FromMilliseconds(120.0));
			bg.BeginAnimation(SolidColorBrush.ColorProperty, animation);
		};
		card.MouseLeave += delegate
		{
			ColorAnimation animation = new ColorAnimation(Color.FromRgb(45, 45, 48), TimeSpan.FromMilliseconds(150.0));
			bg.BeginAnimation(SolidColorBrush.ColorProperty, animation);
		};
	}

	private bool IsChildOf(DependencyObject child, DependencyObject parent)
	{
		for (DependencyObject dependencyObject = child; dependencyObject != null; dependencyObject = VisualTreeHelper.GetParent(dependencyObject))
		{
			if (dependencyObject == parent)
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateCount()
	{
		int num = allToggleSwitches.Count((ToggleButton c) => c.IsChecked == true);
		InstallBtn.Content = (IsRussian ? $"Установить выбранное ({num})" : $"Install Selected ({num})");
	}

	private async void InstallBtn_Click(object sender, RoutedEventArgs e)
	{
		List<ToggleButton> selected = allToggleSwitches.Where((ToggleButton c) => c.IsChecked == true).ToList();
		if (selected.Count == 0)
		{
			MessageBox.Show(IsRussian ? "Пожалуйста, выберите хотя бы одно приложение." : "Please select at least one application.", IsRussian ? "Не выбрано" : "No selection", MessageBoxButton.OK, MessageBoxImage.Asterisk);
			return;
		}
		InstallBtn.IsEnabled = false;
		installSuccessCount = 0;
		installErrorCount = 0;
		alreadyInstalledCount = 0;
		UpdateStatusBar();
		LogBox.Text += (IsRussian ? "\n═══ Начало установки ═══\n" : "\n═══ Starting installation ═══\n");
		foreach (ToggleButton appCb in selected)
		{
			string wingetId = appCb.Tag.ToString();
			LogBox.Text += (IsRussian ? ("\n▶ " + wingetId + "\n") : ("\n▶ " + wingetId + "\n"));
			LogBox.ScrollToEnd();
			await Task.Run(async delegate
			{
				try
				{
					base.Dispatcher.Invoke(delegate
					{
						LogBox.Text += (IsRussian ? "  Попытка установки через winget...\n" : "  Attempting install via winget...\n");
						LogBox.ScrollToEnd();
					});
					int exitCode = -1;
					bool detectedAlreadyInstalled = false;
					try
					{
						using Process proc = new Process();
						proc.StartInfo = new ProcessStartInfo
						{
							FileName = "winget",
							Arguments = "install --id " + wingetId + " --exact --silent --accept-package-agreements --accept-source-agreements",
							UseShellExecute = false,
							CreateNoWindow = true,
							WindowStyle = ProcessWindowStyle.Hidden,
							RedirectStandardOutput = true,
							RedirectStandardError = true,
							StandardOutputEncoding = Encoding.UTF8,
							StandardErrorEncoding = Encoding.UTF8
						};
						proc.OutputDataReceived += delegate(object _, DataReceivedEventArgs ev)
						{
							if (!string.IsNullOrWhiteSpace(ev.Data))
							{
								string text = ev.Data.Trim();
								switch (text)
								{
								default:
									if (!text.Contains("██") && !text.Contains("▒▒"))
									{
										if (text.Contains("already installed") || text.Contains("No available upgrade") || text.Contains("No newer package"))
										{
											detectedAlreadyInstalled = true;
										}
										base.Dispatcher.BeginInvoke((Action)delegate
										{
											TextBox logBox = LogBox;
											logBox.Text = logBox.Text + "  " + ev.Data + "\n";
											LogBox.ScrollToEnd();
										});
									}
									break;
								case "\\":
									break;
								case "|":
									break;
								case "/":
									break;
								}
							}
						};
						proc.ErrorDataReceived += delegate(object _, DataReceivedEventArgs ev)
						{
							MainWindow mainWindow = this;
							if (!string.IsNullOrWhiteSpace(ev.Data))
							{
								base.Dispatcher.BeginInvoke((Action)delegate
								{
									TextBox logBox = mainWindow.LogBox;
									logBox.Text = logBox.Text + "  [ERR] " + ev.Data + "\n";
									mainWindow.LogBox.ScrollToEnd();
								});
							}
						};
						proc.Start();
						proc.BeginOutputReadLine();
						proc.BeginErrorReadLine();
						proc.WaitForExit();
						exitCode = proc.ExitCode;
					}
					catch
					{
					}
					bool isAlreadyInstalled = detectedAlreadyInstalled || exitCode == -1978335189 || exitCode == -1978335135;
					if (exitCode == 0)
					{
						base.Dispatcher.Invoke(delegate
						{
							installSuccessCount++;
							UpdateStatusBar();
							LogBox.Text += (IsRussian ? "  ✔ Установлено через winget\n" : "  ✔ Installed via winget\n");
							LogBox.ScrollToEnd();
						});
					}
					else if (isAlreadyInstalled)
					{
						base.Dispatcher.Invoke(delegate
						{
							alreadyInstalledCount++;
							UpdateStatusBar();
							LogBox.Text += (IsRussian ? "  ● Уже установлено (актуальная версия)\n" : "  ● Already installed (up to date)\n");
							LogBox.ScrollToEnd();
						});
					}
					else
					{
						base.Dispatcher.Invoke(delegate
						{
							LogBox.Text += (IsRussian ? $"  winget не удался (код {exitCode}). Пробую альтернативы...\n" : $"  Winget failed (code {exitCode}). Trying alternatives...\n");
							LogBox.ScrollToEnd();
						});
						bool fallbackOk = false;
						fallbackMap.TryGetValue(wingetId, out var fbNames);
						if (!fallbackOk && fbNames != null && fbNames.Length != 0 && fbNames[0] != null)
						{
							try
							{
								base.Dispatcher.BeginInvoke((Action)delegate
								{
									LogBox.Text += (IsRussian ? ("  Попытка: scoop install " + fbNames[0] + "...\n") : ("  Trying: scoop install " + fbNames[0] + "...\n"));
									LogBox.ScrollToEnd();
								});
								using Process ps = new Process();
								ps.StartInfo = new ProcessStartInfo
								{
									FileName = "scoop",
									Arguments = "install " + fbNames[0],
									UseShellExecute = false,
									CreateNoWindow = true,
									RedirectStandardOutput = true,
									RedirectStandardError = true
								};
								ps.Start();
								ps.WaitForExit();
								if (ps.ExitCode == 0)
								{
									fallbackOk = true;
								}
							}
							catch
							{
							}
						}
						if (!fallbackOk && fbNames != null && fbNames.Length > 1 && fbNames[1] != null)
						{
							try
							{
								base.Dispatcher.BeginInvoke((Action)delegate
								{
									LogBox.Text += (IsRussian ? ("  Попытка: choco install " + fbNames[1] + "...\n") : ("  Trying: choco install " + fbNames[1] + "...\n"));
									LogBox.ScrollToEnd();
								});
								using Process pc = new Process();
								pc.StartInfo = new ProcessStartInfo
								{
									FileName = "choco",
									Arguments = "install " + fbNames[1] + " -y",
									UseShellExecute = false,
									CreateNoWindow = true,
									RedirectStandardOutput = true,
									RedirectStandardError = true
								};
								pc.Start();
								pc.WaitForExit();
								if (pc.ExitCode == 0)
								{
									fallbackOk = true;
								}
							}
							catch
							{
							}
						}
						if (fallbackOk)
						{
							base.Dispatcher.Invoke(delegate
							{
								installSuccessCount++;
								UpdateStatusBar();
								LogBox.Text += (IsRussian ? "  ✔ Установлено через альтернативный менеджер\n" : "  ✔ Installed via alternative package manager\n");
								LogBox.ScrollToEnd();
							});
						}
						else
						{
							base.Dispatcher.Invoke(delegate
							{
								installErrorCount++;
								UpdateStatusBar();
								LogBox.Text += (IsRussian ? "  ✘ Не удалось установить (winget/scoop/choco)\n" : "  ✘ Failed to install (winget/scoop/choco)\n");
								LogBox.ScrollToEnd();
							});
						}
					}
				}
				catch (Exception ex)
				{
					Exception ex2 = ex;
					Exception ex3 = ex2;
					base.Dispatcher.Invoke(delegate
					{
						installErrorCount++;
						UpdateStatusBar();
						LogBox.Text += (IsRussian ? ("  ✘ Ошибка: " + ex3.Message + "\n") : ("  ✘ Error: " + ex3.Message + "\n"));
						LogBox.ScrollToEnd();
					});
				}
			});
		}
		string summary = (IsRussian ? $"\n═══ Завершено. Установлено: {installSuccessCount}, Уже было: {alreadyInstalledCount}, Ошибки: {installErrorCount} ═══\n" : $"\n═══ Done. Installed: {installSuccessCount}, Already: {alreadyInstalledCount}, Errors: {installErrorCount} ═══\n");
		LogBox.Text += summary;
		LogBox.ScrollToEnd();
		InstallBtn.IsEnabled = true;
	}
}
