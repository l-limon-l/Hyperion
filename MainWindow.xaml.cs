using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace HyperionWPF
{
    public partial class MainWindow : Window
    {

        private readonly Dictionary<string, Dictionary<string, object>> softwareDb = new Dictionary<string, Dictionary<string, object>>
        {
            { "Web Browsers", new Dictionary<string, object> {
                { "Chrome", "Google.Chrome" },
                { "Opera", "Opera.Opera" },
                { "Firefox", "Mozilla.Firefox" },
                { "Edge", "Microsoft.Edge" },
                { "Brave", "Brave.Brave" }
            }},
            { "Messaging", new Dictionary<string, object> {
                { "Telegram", "Telegram.TelegramDesktop" },
                { "Discord", "Discord.Discord" },
                { "Teams", "Microsoft.Teams" },
                { "Zoom", "Zoom.Zoom" }
            }},
            { "Media", new Dictionary<string, object> {
                { "VLC", "VideoLAN.VLC" },
                { "Spotify", "Spotify.Spotify" },
                { "Audacity", "Audacity.Audacity" },
                { "HandBrake", "HandBrake.HandBrake" }
            }},
            { ".NET", new Dictionary<string, object> {
                { ".NET Desktop Runtime", new Dictionary<string, string> {
                    { "v8 (x64)", "Microsoft.DotNet.DesktopRuntime.8" },
                    { "v9 (x64)", "Microsoft.DotNet.DesktopRuntime.9" },
                    { "v10 (x64)", "Microsoft.DotNet.DesktopRuntime.10" }
                }},
                { "ASP.NET Core Runtime", new Dictionary<string, string> {
                    { "v8 (x64)", "Microsoft.DotNet.AspNetCore.8" },
                    { "v9 (x64)", "Microsoft.DotNet.AspNetCore.9" },
                    { "v10 (x64)", "Microsoft.DotNet.AspNetCore.10" }
                }},
                { ".NET 4.8.1", "Microsoft.DotNet.Framework.DeveloperPack_4" }
            }},
            { "Java", new Dictionary<string, object> {
                { "Java (AdoptOpenJDK)", new Dictionary<string, string> {
                    { "8 (x64)", "EclipseAdoptium.Temurin.8.JRE" },
                    { "11 (x64)", "EclipseAdoptium.Temurin.11.JRE" },
                    { "17 (x64)", "EclipseAdoptium.Temurin.17.JRE" },
                    { "21 (x64)", "EclipseAdoptium.Temurin.21.JRE" }
                }},
                { "Amazon Corretto JDK", new Dictionary<string, string> {
                    { "8 (x64)", "Amazon.Corretto.8.JDK" },
                    { "11 (x64)", "Amazon.Corretto.11.JDK" },
                    { "21 (x64)", "Amazon.Corretto.21.JDK" }
                }}
            }},
            { "Imaging", new Dictionary<string, object> {
                { "Krita", "KDE.Krita" },
                { "Blender", "BlenderFoundation.Blender" },
                { "GIMP", "GIMP.GIMP" },
                { "IrfanView", "IrfanSkiljan.IrfanView" },
                { "Inkscape", "Inkscape.Inkscape" },
                { "Greenshot", "Greenshot.Greenshot" },
                { "ShareX", "ShareX.ShareX" }
            }},
            { "Documents", new Dictionary<string, object> {
                { "Foxit Reader", "Foxit.FoxitReader" },
                { "SumatraPDF", "SumatraPDF.SumatraPDF" }
            }},
            { "Security", new Dictionary<string, object> {
                { "Malwarebytes", "Malwarebytes.Malwarebytes" }
            }},
            { "File Sharing & Storage", new Dictionary<string, object> {
                { "qBittorrent", "qBittorrent.qBittorrent" },
                { "Dropbox", "Dropbox.Dropbox" },
                { "Google Drive", "Google.GoogleDrive" },
                { "Quick Share", "Google.QuickShare" },
                { "OneDrive", "Microsoft.OneDrive" }
            }},
            { "Other", new Dictionary<string, object> {
                { "Evernote", "Evernote.Evernote" },
                { "Steam", "Valve.Steam" },
                { "Epic Games", "EpicGames.EpicGamesLauncher" },
                { "EA App", "ElectronicArts.EADesktop" },
                { "GOG Galaxy", "GOG.Galaxy" },
                { "KeePass 2", "DominikReichl.KeePass" },
                { "Everything", "voidtools.Everything" }
            }},
            { "Utilities", new Dictionary<string, object> {
                { "TeamViewer", "TeamViewer.TeamViewer" },
                { "RealVNC", new Dictionary<string, string> {
                    { "Server", "RealVNC.VNCServer" },
                    { "Viewer", "RealVNC.VNCViewer" }
                }},
                { "TightVNC", "GlavSoft.TightVNC" },
                { "TeraCopy", "CodeSector.TeraCopy" },
                { "Revo", "RevoUninstaller.RevoUninstaller" },
                { "WizTree", "AntibodySoftware.WizTree" },
                { "CCleaner", "Piriform.CCleaner" }
            }},
            { "Compression", new Dictionary<string, object> {
                { "7-Zip", "7zip.7zip" },
                { "WinRAR", "RARLab.WinRAR" }
            }},
            { "VC++ Redistributables", new Dictionary<string, object> {
                { "VC Redist 2015-2022", new Dictionary<string, string> {
                    { "x64", "Microsoft.VCRedist.2015+.x64" },
                    { "x86", "Microsoft.VCRedist.2015+.x86" }
                }},
                { "VC Redist 2013", new Dictionary<string, string> {
                    { "x64", "Microsoft.VCRedist.2013.x64" },
                    { "x86", "Microsoft.VCRedist.2013.x86" }
                }},
                { "VC Redist 2012", new Dictionary<string, string> {
                    { "x64", "Microsoft.VCRedist.2012.x64" },
                    { "x86", "Microsoft.VCRedist.2012.x86" }
                }},
                { "VC Redist 2010", new Dictionary<string, string> {
                    { "x64", "Microsoft.VCRedist.2010.x64" },
                    { "x86", "Microsoft.VCRedist.2010.x86" }
                }},
                { "VC Redist 2008", new Dictionary<string, string> {
                    { "x64", "Microsoft.VCRedist.2008.x64" },
                    { "x86", "Microsoft.VCRedist.2008.x86" }
                }}
            }},
            { "Developer Tools", new Dictionary<string, object> {
                { "Python 3", "Python.Python.3.14" },
                { "Git", "Git.Git" },
                { "FileZilla", "TimKosse.FileZillaClient" },
                { "Notepad++", "Notepad++.Notepad++" },
                { "WinSCP", "WinSCP.WinSCP" },
                { "PuTTY", "PuTTY.PuTTY" },
                { "WinMerge", "WinMerge.WinMerge" },
                { "Visual Studio Code", "Microsoft.VisualStudioCode" },
                { "Cursor", "Anysphere.Cursor" }
            }}
        };

        private static readonly Dictionary<string, string[]> fallbackMap = new Dictionary<string, string[]>
        {
            { "Google.Chrome", new[] { "googlechrome", "googlechrome" } },
            { "Opera.Opera", new[] { "opera", "opera" } },
            { "Mozilla.Firefox", new[] { "firefox", "firefox" } },
            { "Microsoft.Edge", new[] { null, "microsoft-edge" } },
            { "Brave.Brave", new[] { "brave", "brave" } },
            { "Telegram.TelegramDesktop", new[] { "telegram", "telegram" } },
            { "Discord.Discord", new[] { "discord", "discord" } },
            { "Microsoft.Teams", new[] { "microsoft-teams", "microsoft-teams" } },
            { "Zoom.Zoom", new[] { "zoom", "zoom" } },
            { "VideoLAN.VLC", new[] { "vlc", "vlc" } },
            { "Spotify.Spotify", new[] { "spotify", "spotify" } },
            { "Audacity.Audacity", new[] { "audacity", "audacity" } },
            { "HandBrake.HandBrake", new[] { "handbrake", "handbrake" } },
            { "Microsoft.DotNet.DesktopRuntime.8", new[] { null, "dotnet-8.0-desktopruntime" } },
            { "Microsoft.DotNet.DesktopRuntime.9", new[] { null, "dotnet-9.0-desktopruntime" } },
            { "Microsoft.DotNet.DesktopRuntime.10", new[] { null, "dotnet-10.0-desktopruntime" } },
            { "Microsoft.DotNet.AspNetCore.8", new[] { null, "dotnet-8.0-aspnetruntime" } },
            { "Microsoft.DotNet.AspNetCore.9", new[] { null, "dotnet-9.0-aspnetruntime" } },
            { "Microsoft.DotNet.AspNetCore.10", new[] { null, "dotnet-10.0-aspnetruntime" } },
            { "Microsoft.DotNet.Framework.DeveloperPack_4", new[] { null, "netfx-4.8.1-devpack" } },
            { "EclipseAdoptium.Temurin.8.JRE", new[] { "temurin8-jre", "temurin8jre" } },
            { "EclipseAdoptium.Temurin.11.JRE", new[] { "temurin11-jre", "temurin11jre" } },
            { "EclipseAdoptium.Temurin.17.JRE", new[] { "temurin17-jre", "temurin17jre" } },
            { "EclipseAdoptium.Temurin.21.JRE", new[] { "temurin21-jre", "temurin21jre" } },
            { "Amazon.Corretto.8.JDK", new[] { "corretto8-jdk", "corretto8jdk" } },
            { "Amazon.Corretto.11.JDK", new[] { "corretto11-jdk", "corretto11jdk" } },
            { "Amazon.Corretto.21.JDK", new[] { "corretto21-jdk", "corretto21jdk" } },
            { "KDE.Krita", new[] { "krita", "krita" } },
            { "BlenderFoundation.Blender", new[] { "blender", "blender" } },
            { "GIMP.GIMP", new[] { "gimp", "gimp" } },
            { "IrfanSkiljan.IrfanView", new[] { "irfanview", "irfanview" } },
            { "Inkscape.Inkscape", new[] { "inkscape", "inkscape" } },
            { "Greenshot.Greenshot", new[] { "greenshot", "greenshot" } },
            { "ShareX.ShareX", new[] { "sharex", "sharex" } },
            { "Foxit.FoxitReader", new[] { "foxit-reader", "foxitreader" } },
            { "SumatraPDF.SumatraPDF", new[] { "sumatrapdf", "sumatrapdf" } },
            { "Malwarebytes.Malwarebytes", new[] { null, "malwarebytes" } },
            { "qBittorrent.qBittorrent", new[] { "qbittorrent", "qbittorrent" } },
            { "Dropbox.Dropbox", new[] { "dropbox", "dropbox" } },
            { "Google.GoogleDrive", new[] { "googledrive", "googledrive" } },
            { "Google.QuickShare", new string[] { null, null } },
            { "Microsoft.OneDrive", new string[] { null, null } },
            { "Evernote.Evernote", new[] { "evernote", "evernote" } },
            { "Valve.Steam", new[] { "steam", "steam" } },
            { "EpicGames.EpicGamesLauncher", new[] { "epic-games-launcher", "epicgameslauncher" } },
            { "ElectronicArts.EADesktop", new[] { null, "ea-app" } },
            { "GOG.Galaxy", new[] { null, "goggalaxy" } },
            { "DominikReichl.KeePass", new[] { "keepass", "keepass" } },
            { "voidtools.Everything", new[] { "everything", "everything" } },
            { "TeamViewer.TeamViewer", new[] { "teamviewer", "teamviewer" } },
            { "RealVNC.VNCServer", new[] { null, "vnc-server" } },
            { "RealVNC.VNCViewer", new[] { "vnc-viewer", "vnc-viewer" } },
            { "GlavSoft.TightVNC", new[] { "tightvnc", "tightvnc" } },
            { "CodeSector.TeraCopy", new[] { "teracopy", "teracopy" } },
            { "RevoUninstaller.RevoUninstaller", new[] { "revo-uninstaller", "revo-uninstaller" } },
            { "AntibodySoftware.WizTree", new[] { "wiztree", "wiztree" } },
            { "Piriform.CCleaner", new[] { "ccleaner", "ccleaner" } },
            { "7zip.7zip", new[] { "7zip", "7zip" } },
            { "RARLab.WinRAR", new[] { "winrar", "winrar" } },
            { "Microsoft.VCRedist.2015+.x64", new[] { null, "vcredist140" } },
            { "Microsoft.VCRedist.2015+.x86", new[] { null, "vcredist140" } },
            { "Microsoft.VCRedist.2013.x64", new[] { null, "vcredist2013" } },
            { "Microsoft.VCRedist.2013.x86", new[] { null, "vcredist2013" } },
            { "Microsoft.VCRedist.2012.x64", new[] { null, "vcredist2012" } },
            { "Microsoft.VCRedist.2012.x86", new[] { null, "vcredist2012" } },
            { "Microsoft.VCRedist.2010.x64", new[] { null, "vcredist2010" } },
            { "Microsoft.VCRedist.2010.x86", new[] { null, "vcredist2010" } },
            { "Microsoft.VCRedist.2008.x64", new[] { null, "vcredist2008" } },
            { "Microsoft.VCRedist.2008.x86", new[] { null, "vcredist2008" } },
            { "Python.Python.3.14", new[] { "python", "python3" } },
            { "Git.Git", new[] { "git", "git" } },
            { "TimKosse.FileZillaClient", new[] { "filezilla", "filezilla" } },
            { "Notepad++.Notepad++", new[] { "notepadplusplus", "notepadplusplus" } },
            { "WinSCP.WinSCP", new[] { "winscp", "winscp" } },
            { "PuTTY.PuTTY", new[] { "putty", "putty" } },
            { "WinMerge.WinMerge", new[] { "winmerge", "winmerge" } },
            { "Microsoft.VisualStudioCode", new[] { "vscode", "vscode" } },
            { "Anysphere.Cursor", new[] { "cursor", null } }
        };

        private List<ToggleButton> allToggleSwitches = new List<ToggleButton>();
        private Dictionary<string, StackPanel> pagePanels = new Dictionary<string, StackPanel>();
        private int installSuccessCount = 0;
        private int installErrorCount = 0;
        private int alreadyInstalledCount = 0;

        private bool IsRussian => System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru";


        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        private static extern void DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWCP_ROUND = 2;

        public MainWindow()
        {
            InitializeComponent();
            LogBox.Text = IsRussian ? "Ожидание выбора..." : "Waiting for selection...";
            InstallBtn.Content = IsRussian ? "Установить выбранное (0)" : "Install Selected (0)";
            StateChanged += OnStateChanged;
            SourceInitialized += (s, e) => EnableRoundedCorners();
            BuildUI();
            Loaded += Window_Loaded;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _ = Task.Run(() => EnsurePackageManagersAsync());
        }

        private void EnsurePackageManagersAsync()
        {
            try
            {
                var pc = new Process { StartInfo = new ProcessStartInfo { FileName = "choco", Arguments = "-v", UseShellExecute = false, CreateNoWindow = true } };
                try { pc.Start(); pc.WaitForExit(); }
                catch
                {
                    var chocoInstall = Process.Start(new ProcessStartInfo { FileName = "winget", Arguments = "install Chocolatey.Chocolatey --silent --accept-source-agreements --accept-package-agreements", UseShellExecute = false, CreateNoWindow = true });
                    chocoInstall?.WaitForExit();
                }

                var ps = new Process { StartInfo = new ProcessStartInfo { FileName = "scoop", Arguments = "-v", UseShellExecute = false, CreateNoWindow = true } };
                try { ps.Start(); ps.WaitForExit(); }
                catch
                {
                    var scoopInstall = Process.Start(new ProcessStartInfo { FileName = "powershell", Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"Invoke-RestMethod -Uri https://get.scoop.sh | Invoke-Expression\"", UseShellExecute = false, CreateNoWindow = true });
                    scoopInstall?.WaitForExit();
                }
            }
            catch { }
        }

        private void EnableRoundedCorners()
        {
            try
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                int preference = DWMWCP_ROUND;
                DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
            }
            catch {  }
        }


        private void MinBtn_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void MaxBtn_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();
        private void OnStateChanged(object sender, EventArgs e) =>
            MaxBtn.Content = WindowState == WindowState.Maximized ? "\uE923" : "\uE922";


        private void ClearLogBtn_Click(object sender, RoutedEventArgs e)
        {
            LogBox.Text = IsRussian ? "Лог очищен.\n" : "Log cleared.\n";
            installSuccessCount = 0;
            installErrorCount = 0;
            UpdateStatusBar();
        }

        private void SaveLogBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                FileName = $"Hyperion_log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}",
                DefaultExt = ".txt",
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };
            if (dlg.ShowDialog() == true)
            {
                System.IO.File.WriteAllText(dlg.FileName, LogBox.Text);
                LogBox.Text += IsRussian
                    ? $"\n[ИНФО] Лог сохранён: {dlg.FileName}\n"
                    : $"\n[INFO] Log saved: {dlg.FileName}\n";
            }
        }

        private void UpdateStatusBar()
        {
            string parts = "";
            if (installSuccessCount > 0)
                parts += IsRussian ? $"Установлено: {installSuccessCount}" : $"Installed: {installSuccessCount}";
            if (alreadyInstalledCount > 0)
            {
                if (parts.Length > 0) parts += "  │  ";
                parts += IsRussian ? $"Уже было: {alreadyInstalledCount}" : $"Already installed: {alreadyInstalledCount}";
            }
            StatusInstalled.Text = parts;
            StatusErrors.Text = installErrorCount > 0
                ? (IsRussian ? $"Ошибки: {installErrorCount}" : $"Errors: {installErrorCount}")
                : "";
        }


        private string GetCategoryIcon(string category)
        {
            switch (category)
            {
                case "Web Browsers": return "\uE774";
                case "Messaging": return "\uE8F2";
                case "Media": return "\uE189";
                case ".NET": return "\uE943";
                case "Java": return "\uE943";
                case "Imaging": return "\uE790";
                case "Documents": return "\uE8A5";
                case "Security": return "\uE727";
                case "File Sharing & Storage": return "\uE8B7";
                case "Other": return "\uE712";
                case "Utilities": return "\uEC7A";
                case "Compression": return "\uE8FA";
                case "VC++ Redistributables": return "\uE943";
                case "Developer Tools": return "\uE70F";
                default: return "\uE712";
            }
        }

        private string GetLocalizedCategory(string k)
        {
            if (!IsRussian) return k;
            switch (k)
            {
                case "Web Browsers": return "Веб-браузеры";
                case "Messaging": return "Мессенджеры";
                case "Media": return "Мультимедиа";
                case ".NET": return ".NET";
                case "Java": return "Java";
                case "Imaging": return "Изображения и дизайн";
                case "Documents": return "Документы";
                case "Security": return "Безопасность";
                case "File Sharing & Storage": return "Файлы и Облако";
                case "Other": return "Другое";
                case "Utilities": return "Утилиты";
                case "Compression": return "Архивирование";
                case "VC++ Redistributables": return "Библиотеки VC++";
                case "Developer Tools": return "Для разработчиков";
                default: return k;
            }
        }

        private string GetAppDescription(string n)
        {
            if (IsRussian)
            {
                switch (n)
                {
                    case "Chrome": return "Быстрый и безопасный браузер от Google";
                    case "Opera": return "Браузер с VPN и блокировщиком рекламы";
                    case "Firefox": return "Свободный браузер с акцентом на приватность";
                    case "Edge": return "Современный браузер от Microsoft";
                    case "Brave": return "Приватный браузер со встроенной блокировкой рекламы";
                    case "Telegram": return "Быстрый и безопасный мессенджер";
                    case "Discord": return "Голосовой и текстовый чат для сообществ";
                    case "Teams": return "Платформа для совместной работы от Microsoft";
                    case "Zoom": return "Программа для видеоконференций и онлайн-встреч";
                    case "VLC": return "Универсальный медиаплеер для всех форматов";
                    case "Spotify": return "Стриминговый сервис для музыки и подкастов";
                    case "Audacity": return "Бесплатный аудиоредактор с открытым кодом";
                    case "HandBrake": return "Мощный видеоконвертер с открытым кодом";
                    case ".NET Desktop Runtime": return "Среда выполнения для десктопных приложений .NET";
                    case "ASP.NET Core Runtime": return "Среда выполнения для веб-приложений ASP.NET Core";
                    case ".NET 4.8.1": return "Классическая среда выполнения .NET Framework";
                    case "Java (AdoptOpenJDK)": return "Свободная среда выполнения Java (Eclipse Temurin)";
                    case "Amazon Corretto JDK": return "Дистрибутив OpenJDK от Amazon";
                    case "Krita": return "Профессиональный редактор для цифровой живописи";
                    case "Blender": return "Программа для 3D-моделирования и анимации";
                    case "GIMP": return "Бесплатный редактор растровой графики";
                    case "IrfanView": return "Быстрый просмотрщик изображений";
                    case "Inkscape": return "Бесплатный редактор векторной графики";
                    case "Greenshot": return "Утилита для создания скриншотов";
                    case "ShareX": return "Захват экрана с загрузкой и редактированием";
                    case "Foxit Reader": return "Быстрый и лёгкий просмотрщик PDF";
                    case "SumatraPDF": return "Минималистичный просмотрщик PDF и книг";
                    case "Malwarebytes": return "Защита от вредоносного ПО и угроз";
                    case "qBittorrent": return "Бесплатный торрент-клиент с открытым кодом";
                    case "Dropbox": return "Облачное хранилище файлов и синхронизация";
                    case "Google Drive": return "Облачное хранилище от Google";
                    case "Quick Share": return "Быстрая передача файлов между устройствами";
                    case "OneDrive": return "Облачное хранилище от Microsoft";
                    case "Evernote": return "Приложение для заметок и организации дел";
                    case "Steam": return "Платформа для покупки и запуска игр";
                    case "Epic Games": return "Магазин игр от Epic Games";
                    case "EA App": return "Платформа для игр Electronic Arts";
                    case "GOG Galaxy": return "Магазин DRM-free игр и библиотека";
                    case "KeePass 2": return "Менеджер паролей с открытым кодом";
                    case "Everything": return "Мгновенный поиск файлов по имени";
                    case "TeamViewer": return "Удалённый доступ и управление компьютером";
                    case "RealVNC": return "Удалённый рабочий стол по протоколу VNC";
                    case "TightVNC": return "Бесплатный VNC-клиент для удалённого доступа";
                    case "TeraCopy": return "Быстрое и надёжное копирование файлов";
                    case "Revo": return "Полное удаление программ с очисткой следов";
                    case "WizTree": return "Быстрый анализатор дискового пространства";
                    case "CCleaner": return "Очистка системы и оптимизация ПК";
                    case "7-Zip": return "Бесплатный архиватор с открытым кодом";
                    case "WinRAR": return "Популярный архиватор с поддержкой RAR и ZIP";
                    case "VC Redist 2015-2022": return "Библиотеки Visual C++ 2015–2022";
                    case "VC Redist 2013": return "Библиотеки Visual C++ 2013";
                    case "VC Redist 2012": return "Библиотеки Visual C++ 2012";
                    case "VC Redist 2010": return "Библиотеки Visual C++ 2010";
                    case "VC Redist 2008": return "Библиотеки Visual C++ 2008";
                    case "Python 3": return "Язык программирования Python 3";
                    case "Git": return "Система контроля версий";
                    case "FileZilla": return "Бесплатный FTP/SFTP клиент";
                    case "Notepad++": return "Продвинутый текстовый редактор для кода";
                    case "WinSCP": return "Клиент для передачи файлов по SCP/SFTP";
                    case "PuTTY": return "SSH-клиент и эмулятор терминала";
                    case "WinMerge": return "Сравнение и слияние файлов и папок";
                    case "Visual Studio Code": return "Лёгкий и мощный редактор кода от Microsoft";
                    case "Cursor": return "AI-редактор кода на базе VS Code";
                    default: return "Настройка и установка компонента";
                }
            }
            else
            {
                switch (n)
                {
                    case "Chrome": return "Fast and secure browser by Google";
                    case "Opera": return "Browser with built-in VPN and ad blocker";
                    case "Firefox": return "Free browser focused on privacy";
                    case "Edge": return "Modern browser by Microsoft";
                    case "Brave": return "Private browser with built-in ad blocking";
                    case "Telegram": return "Fast and secure messenger";
                    case "Discord": return "Voice and text chat for communities";
                    case "Teams": return "Collaboration platform by Microsoft";
                    case "Zoom": return "Video conferencing and online meetings";
                    case "VLC": return "Universal media player for all formats";
                    case "Spotify": return "Streaming service for music and podcasts";
                    case "Audacity": return "Free open-source audio editor";
                    case "HandBrake": return "Powerful open-source video converter";
                    case ".NET Desktop Runtime": return "Runtime for .NET desktop applications";
                    case "ASP.NET Core Runtime": return "Runtime for ASP.NET Core web applications";
                    case ".NET 4.8.1": return "Classic .NET Framework runtime";
                    case "Java (AdoptOpenJDK)": return "Free Java runtime (Eclipse Temurin)";
                    case "Amazon Corretto JDK": return "OpenJDK distribution by Amazon";
                    case "Krita": return "Professional digital painting application";
                    case "Blender": return "3D modeling and animation software";
                    case "GIMP": return "Free raster graphics editor";
                    case "IrfanView": return "Fast and lightweight image viewer";
                    case "Inkscape": return "Free vector graphics editor";
                    case "Greenshot": return "Screenshot capture utility";
                    case "ShareX": return "Screen capture with upload and editing";
                    case "Foxit Reader": return "Fast and lightweight PDF viewer";
                    case "SumatraPDF": return "Minimalist PDF and e-book reader";
                    case "Malwarebytes": return "Malware protection and threat removal";
                    case "qBittorrent": return "Free open-source torrent client";
                    case "Dropbox": return "Cloud file storage and synchronization";
                    case "Google Drive": return "Cloud storage by Google";
                    case "Quick Share": return "Fast file sharing between devices";
                    case "OneDrive": return "Cloud storage by Microsoft";
                    case "Evernote": return "Note-taking and organization app";
                    case "Steam": return "Platform for purchasing and playing games";
                    case "Epic Games": return "Game store by Epic Games";
                    case "EA App": return "Platform for Electronic Arts games";
                    case "GOG Galaxy": return "DRM-free game store and library";
                    case "KeePass 2": return "Open-source password manager";
                    case "Everything": return "Instant file search by name";
                    case "TeamViewer": return "Remote desktop access and control";
                    case "RealVNC": return "Remote desktop via VNC protocol";
                    case "TightVNC": return "Free VNC client for remote access";
                    case "TeraCopy": return "Fast and reliable file copying";
                    case "Revo": return "Complete program uninstaller with cleanup";
                    case "WizTree": return "Fast disk space analyzer";
                    case "CCleaner": return "System cleanup and PC optimization";
                    case "7-Zip": return "Free open-source file archiver";
                    case "WinRAR": return "Popular archiver supporting RAR and ZIP";
                    case "VC Redist 2015-2022": return "Visual C++ 2015–2022 libraries";
                    case "VC Redist 2013": return "Visual C++ 2013 libraries";
                    case "VC Redist 2012": return "Visual C++ 2012 libraries";
                    case "VC Redist 2010": return "Visual C++ 2010 libraries";
                    case "VC Redist 2008": return "Visual C++ 2008 libraries";
                    case "Python 3": return "Python 3 programming language";
                    case "Git": return "Version control system";
                    case "FileZilla": return "Free FTP/SFTP client";
                    case "Notepad++": return "Advanced code text editor";
                    case "WinSCP": return "SCP/SFTP file transfer client";
                    case "PuTTY": return "SSH client and terminal emulator";
                    case "WinMerge": return "File and folder comparison tool";
                    case "Visual Studio Code": return "Lightweight code editor by Microsoft";
                    case "Cursor": return "AI-powered code editor based on VS Code";
                    default: return "System component setup";
                }
            }
        }


        private void BuildUI()
        {
            foreach (var category in softwareDb)
            {
                var pagePanel = new StackPanel();
                pagePanel.Children.Add(new TextBlock
                {
                    Text = GetLocalizedCategory(category.Key),
                    FontSize = 26, FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    Margin = new Thickness(0, 16, 0, 12)
                });

                foreach (var app in category.Value)
                {
                    if (app.Value is string wid)
                        pagePanel.Children.Add(CreateCard(app.Key, wid, app.Key));
                    else if (app.Value is Dictionary<string, string> group)
                        pagePanel.Children.Add(CreateGroupCard(app.Key, group));
                }
                pagePanels[category.Key] = pagePanel;
            }

            foreach (var category in softwareDb)
            {
                var sp = new StackPanel { Orientation = Orientation.Horizontal };
                sp.Children.Add(new TextBlock
                {
                    Text = GetCategoryIcon(category.Key),
                    FontFamily = new FontFamily("Segoe MDL2 Assets"), FontSize = 15,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = (Brush)FindResource("TextMutedBrush"),
                    Margin = new Thickness(0, 0, 12, 0)
                });
                sp.Children.Add(new TextBlock
                {
                    Text = GetLocalizedCategory(category.Key),
                    VerticalAlignment = VerticalAlignment.Center
                });
                NavList.Items.Add(new ListBoxItem { Content = sp, Tag = category.Key });
            }
            if (NavList.Items.Count > 0) NavList.SelectedIndex = 0;
        }


        private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NavList.SelectedItem is ListBoxItem item && item.Tag is string categoryKey)
            {
                ContentPanel.Children.Clear();
                if (pagePanels.TryGetValue(categoryKey, out var panel))
                {
                    if (panel.Parent is Panel old) old.Children.Remove(panel);

                    panel.Opacity = 0;
                    panel.RenderTransform = new TranslateTransform(0, 20);
                    ContentPanel.Children.Add(panel);

                    var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250))
                    { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
                    var slide = new DoubleAnimation(20, 0, TimeSpan.FromMilliseconds(300))
                    { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };

                    panel.BeginAnimation(UIElement.OpacityProperty, fade);
                    panel.RenderTransform.BeginAnimation(TranslateTransform.YProperty, slide);
                }
                MainScroll.ScrollToTop();
            }
        }


        private Image GetIconImage(string iconName)
        {
            string clean = System.Text.RegularExpressions.Regex.Replace(iconName, @"[^a-zA-Z0-9_\-\.]", "_");
            var img = new Image { Width = 32, Height = 32, Margin = new Thickness(15, 0, 15, 0) };
            try { img.Source = new BitmapImage(new Uri($"pack://application:,,,/Icons/{clean}.png")); } catch { }
            return img;
        }


        private Border CreateCard(string title, string tag, string iconKey)
        {
            var card = new Border
            {
                CornerRadius = new CornerRadius(8),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 68, Margin = new Thickness(0, 0, 0, 6), Cursor = Cursors.Hand,
                Background = new SolidColorBrush(Color.FromRgb(45, 45, 48))
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(68) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var img = GetIconImage(iconKey);
            Grid.SetColumn(img, 0); grid.Children.Add(img);

            var tp = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            tp.Children.Add(new TextBlock { Text = title, Foreground = Brushes.White, FontSize = 14, FontWeight = FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis });
            tp.Children.Add(new TextBlock { Text = GetAppDescription(title), Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 160)), FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 2, 0, 0) });
            Grid.SetColumn(tp, 1); grid.Children.Add(tp);

            var statusTxt = new TextBlock { Text = IsRussian ? "Откл." : "Off", Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 160)), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 12, 0), FontSize = 12 };
            Grid.SetColumn(statusTxt, 2); grid.Children.Add(statusTxt);

            var ts = new ToggleButton { Tag = tag, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 14, 0), Style = (Style)FindResource("ToggleSwitchStyle") };
            ts.Checked += (s, e) => { statusTxt.Text = IsRussian ? "Вкл." : "On"; statusTxt.Foreground = Brushes.White; UpdateCount(); };
            ts.Unchecked += (s, e) => { statusTxt.Text = IsRussian ? "Откл." : "Off"; statusTxt.Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 160)); UpdateCount(); };
            Grid.SetColumn(ts, 3); grid.Children.Add(ts);
            allToggleSwitches.Add(ts);

            card.PreviewMouseLeftButtonUp += (s, e) =>
            {
                if (e.OriginalSource is DependencyObject src && IsChildOf(src, ts)) return;
                ts.IsChecked = !ts.IsChecked;
                e.Handled = true;
            };

            AnimateCardHover(card);
            card.Child = grid;
            return card;
        }


        private Border CreateGroupCard(string groupTitle, Dictionary<string, string> subApps)
        {
            var card = new Border
            {
                CornerRadius = new CornerRadius(8),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 6), Cursor = Cursors.Hand,
                Background = new SolidColorBrush(Color.FromRgb(45, 45, 48))
            };

            var stack = new StackPanel();

            var headerGrid = new Grid { Height = 68, Background = Brushes.Transparent };
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(68) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(44) });

            string iconKey;
            switch (groupTitle)
            {
                case ".NET Desktop Runtime":
                case "ASP.NET Core Runtime": iconKey = ".NET 4.8.1"; break;
                case "Java (AdoptOpenJDK)": iconKey = "Java (AdoptOpenJDK) 8"; break;
                case "Amazon Corretto JDK": iconKey = "JDK (Amazon Corretto) 8"; break;
                case "RealVNC": iconKey = "RealVNC Server"; break;
                case "VC Redist 2015-2022": iconKey = "VC Redist x64 2015+"; break;
                case "VC Redist 2013": iconKey = "VC Redist x64 2013"; break;
                case "VC Redist 2012": iconKey = "VC Redist x64 2012"; break;
                case "VC Redist 2010": iconKey = "VC Redist x64 2010"; break;
                case "VC Redist 2008": iconKey = "VC Redist x64 2008"; break;
                default: iconKey = groupTitle; break;
            }

            var img = GetIconImage(iconKey);
            Grid.SetColumn(img, 0); headerGrid.Children.Add(img);

            var tp = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            tp.Children.Add(new TextBlock { Text = groupTitle, Foreground = Brushes.White, FontSize = 14, FontWeight = FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis });
            tp.Children.Add(new TextBlock { Text = GetAppDescription(groupTitle), Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 160)), FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 2, 0, 0) });
            Grid.SetColumn(tp, 1); headerGrid.Children.Add(tp);

            var groupStatusTxt = new TextBlock { Text = IsRussian ? "Откл." : "Off", Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 160)), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 12, 0), FontSize = 12 };
            Grid.SetColumn(groupStatusTxt, 2); headerGrid.Children.Add(groupStatusTxt);

            var groupCb = new ToggleButton { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0), Style = (Style)FindResource("ToggleSwitchStyle") };
            Grid.SetColumn(groupCb, 3); headerGrid.Children.Add(groupCb);

            var arrow = new TextBlock
            {
                Text = "\uE70D", FontFamily = new FontFamily("Segoe MDL2 Assets"),
                Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 160)), FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform(0)
            };
            Grid.SetColumn(arrow, 4); headerGrid.Children.Add(arrow);

            stack.Children.Add(headerGrid);

            var subPanel = new StackPanel { Margin = new Thickness(68, 0, 14, 10) };
            var subBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(62, 62, 66)),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Child = subPanel,
                Visibility = Visibility.Collapsed,
                RenderTransform = new TranslateTransform(0, 0)
            };

            var subCbs = new List<ToggleButton>();
            bool isUpdatingSub = false, isUpdatingGroup = false;

            groupCb.Checked += (s, e) =>
            {
                groupStatusTxt.Text = IsRussian ? "Вкл." : "On";
                groupStatusTxt.Foreground = Brushes.White;
                if (!isUpdatingGroup) { isUpdatingSub = true; foreach (var c in subCbs) c.IsChecked = true; isUpdatingSub = false; }
            };
            groupCb.Unchecked += (s, e) =>
            {
                groupStatusTxt.Text = IsRussian ? "Откл." : "Off";
                groupStatusTxt.Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 160));
                if (!isUpdatingGroup) { isUpdatingSub = true; foreach (var c in subCbs) c.IsChecked = false; isUpdatingSub = false; }
            };

            foreach (var subApp in subApps)
            {
                var subGrid = new Grid { Margin = new Thickness(0, 6, 0, 6), Background = Brushes.Transparent };
                subGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                subGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                subGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var subName = new TextBlock { Text = subApp.Key, Foreground = new SolidColorBrush(Color.FromRgb(176, 176, 176)), FontSize = 13, VerticalAlignment = VerticalAlignment.Center };
                Grid.SetColumn(subName, 0); subGrid.Children.Add(subName);

                var subStatusTxt = new TextBlock { Text = IsRussian ? "Откл." : "Off", Foreground = new SolidColorBrush(Color.FromRgb(140, 140, 140)), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 12, 0), FontSize = 12 };
                Grid.SetColumn(subStatusTxt, 1); subGrid.Children.Add(subStatusTxt);

                var subCb = new ToggleButton { Tag = subApp.Value, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 0, 0), Style = (Style)FindResource("ToggleSwitchStyle") };
                subCb.Checked += (s, e) =>
                {
                    subStatusTxt.Text = IsRussian ? "Вкл." : "On"; subStatusTxt.Foreground = Brushes.White; UpdateCount();
                    if (!isUpdatingSub) { isUpdatingGroup = true; groupCb.IsChecked = subCbs.Any(c => c.IsChecked == true); isUpdatingGroup = false; }
                };
                subCb.Unchecked += (s, e) =>
                {
                    subStatusTxt.Text = IsRussian ? "Откл." : "Off"; subStatusTxt.Foreground = new SolidColorBrush(Color.FromRgb(140, 140, 140)); UpdateCount();
                    if (!isUpdatingSub) { isUpdatingGroup = true; groupCb.IsChecked = subCbs.Any(c => c.IsChecked == true); isUpdatingGroup = false; }
                };
                Grid.SetColumn(subCb, 2); subGrid.Children.Add(subCb);
                allToggleSwitches.Add(subCb); subCbs.Add(subCb);

                subGrid.PreviewMouseLeftButtonUp += (s, e) =>
                {
                    if (e.OriginalSource is DependencyObject src && IsChildOf(src, subCb)) return;
                    subCb.IsChecked = !subCb.IsChecked; e.Handled = true;
                };
                subPanel.Children.Add(subGrid);
            }

            stack.Children.Add(subBorder);
            card.Child = stack;

            bool isExpanded = false;
            headerGrid.PreviewMouseLeftButtonUp += (s, e) =>
            {
                if (e.OriginalSource is DependencyObject src && IsChildOf(src, groupCb)) return;
                e.Handled = true;

                if (!isExpanded)
                {
                    isExpanded = true;
                    subBorder.Visibility = Visibility.Visible;
                    subBorder.Opacity = 0;
                    subBorder.RenderTransform = new TranslateTransform(0, -12);

                    ((RotateTransform)arrow.RenderTransform).BeginAnimation(RotateTransform.AngleProperty,
                        new DoubleAnimation(0, 180, TimeSpan.FromMilliseconds(250)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });

                    subBorder.BeginAnimation(UIElement.OpacityProperty,
                        new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
                    subBorder.RenderTransform.BeginAnimation(TranslateTransform.YProperty,
                        new DoubleAnimation(-12, 0, TimeSpan.FromMilliseconds(300)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
                }
                else
                {
                    isExpanded = false;
                    ((RotateTransform)arrow.RenderTransform).BeginAnimation(RotateTransform.AngleProperty,
                        new DoubleAnimation(180, 0, TimeSpan.FromMilliseconds(200)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });

                    var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
                    fadeOut.Completed += (_, __) => { subBorder.Visibility = Visibility.Collapsed; subBorder.Opacity = 1; };
                    subBorder.BeginAnimation(UIElement.OpacityProperty, fadeOut);

                    if (subBorder.RenderTransform is TranslateTransform tt)
                        tt.BeginAnimation(TranslateTransform.YProperty,
                            new DoubleAnimation(0, -12, TimeSpan.FromMilliseconds(200)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
                }
            };

            AnimateCardHover(card);
            return card;
        }


        private void AnimateCardHover(Border card)
        {
            var bg = (SolidColorBrush)card.Background;
            card.MouseEnter += (s, e) =>
            {
                var anim = new ColorAnimation(Color.FromRgb(62, 62, 66), TimeSpan.FromMilliseconds(120));
                bg.BeginAnimation(SolidColorBrush.ColorProperty, anim);
            };
            card.MouseLeave += (s, e) =>
            {
                var anim = new ColorAnimation(Color.FromRgb(45, 45, 48), TimeSpan.FromMilliseconds(150));
                bg.BeginAnimation(SolidColorBrush.ColorProperty, anim);
            };
        }


        private bool IsChildOf(DependencyObject child, DependencyObject parent)
        {
            var c = child;
            while (c != null) { if (c == parent) return true; c = VisualTreeHelper.GetParent(c); }
            return false;
        }

        private void UpdateCount()
        {
            int count = allToggleSwitches.Count(c => c.IsChecked == true);
            InstallBtn.Content = IsRussian ? $"Установить выбранное ({count})" : $"Install Selected ({count})";
        }


        private async void InstallBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = allToggleSwitches.Where(c => c.IsChecked == true).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show(
                    IsRussian ? "Пожалуйста, выберите хотя бы одно приложение." : "Please select at least one application.",
                    IsRussian ? "Не выбрано" : "No selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            InstallBtn.IsEnabled = false;
            installSuccessCount = 0;
            installErrorCount = 0;
            alreadyInstalledCount = 0;
            UpdateStatusBar();
            LogBox.Text += IsRussian ? "\n═══ Начало установки ═══\n" : "\n═══ Starting installation ═══\n";

            foreach (var appCb in selected)
            {
                string wingetId = appCb.Tag.ToString();
                LogBox.Text += IsRussian ? $"\n▶ {wingetId}\n" : $"\n▶ {wingetId}\n";
                LogBox.ScrollToEnd();

                await Task.Run(async () =>
                {
                    try
                    {
                        Dispatcher.Invoke(() => { LogBox.Text += IsRussian ? "  Попытка установки через winget...\n" : "  Attempting install via winget...\n"; LogBox.ScrollToEnd(); });

                        int exitCode = -1;
                        bool detectedAlreadyInstalled = false;
                        try
                        {
                            using (var proc = new Process())
                            {
                                proc.StartInfo = new ProcessStartInfo
                                {
                                    FileName = "winget",
                                    Arguments = $"install --id {wingetId} --exact --silent --accept-package-agreements --accept-source-agreements",
                                    UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden,
                                    RedirectStandardOutput = true, RedirectStandardError = true,
                                    StandardOutputEncoding = System.Text.Encoding.UTF8, StandardErrorEncoding = System.Text.Encoding.UTF8
                                };
                                proc.OutputDataReceived += (_, ev) =>
                                {
                                    if (!string.IsNullOrWhiteSpace(ev.Data))
                                    {
                                        string t = ev.Data.Trim();
                                        if (t == "-" || t == "\\" || t == "|" || t == "/" || t.Contains("██") || t.Contains("▒▒")) return;
                                        if (t.Contains("already installed") || t.Contains("No available upgrade") || t.Contains("No newer package"))
                                            detectedAlreadyInstalled = true;
                                        Dispatcher.BeginInvoke(new Action(() => { LogBox.Text += $"  {ev.Data}\n"; LogBox.ScrollToEnd(); }));
                                    }
                                };
                                proc.ErrorDataReceived += (_, ev) =>
                                {
                                    if (!string.IsNullOrWhiteSpace(ev.Data))
                                        Dispatcher.BeginInvoke(new Action(() => { LogBox.Text += $"  [ERR] {ev.Data}\n"; LogBox.ScrollToEnd(); }));
                                };
                                proc.Start(); proc.BeginOutputReadLine(); proc.BeginErrorReadLine();
                                proc.WaitForExit(); exitCode = proc.ExitCode;
                            }
                        }
                        catch { }

                        bool isAlreadyInstalled = detectedAlreadyInstalled
                            || exitCode == -1978335189   
                            || exitCode == -1978335135;  

                        if (exitCode == 0)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                installSuccessCount++;
                                UpdateStatusBar();
                                LogBox.Text += IsRussian ? "  ✔ Установлено через winget\n" : "  ✔ Installed via winget\n";
                                LogBox.ScrollToEnd();
                            });
                        }
                        else if (isAlreadyInstalled)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                alreadyInstalledCount++;
                                UpdateStatusBar();
                                LogBox.Text += IsRussian ? "  ● Уже установлено (актуальная версия)\n" : "  ● Already installed (up to date)\n";
                                LogBox.ScrollToEnd();
                            });
                        }
                        else
                        {
                            Dispatcher.Invoke(() => { LogBox.Text += IsRussian ? $"  winget не удался (код {exitCode}). Пробую альтернативы...\n" : $"  Winget failed (code {exitCode}). Trying alternatives...\n"; LogBox.ScrollToEnd(); });

                            bool fallbackOk = false;
                            string[] fbNames;
                            fallbackMap.TryGetValue(wingetId, out fbNames);

                            if (!fallbackOk && fbNames != null && fbNames.Length > 0 && fbNames[0] != null)
                            {
                                try
                                {
                                    Dispatcher.BeginInvoke(new Action(() => { LogBox.Text += IsRussian ? $"  Попытка: scoop install {fbNames[0]}...\n" : $"  Trying: scoop install {fbNames[0]}...\n"; LogBox.ScrollToEnd(); }));
                                    using (var ps = new Process())
                                    {
                                        ps.StartInfo = new ProcessStartInfo
                                        {
                                            FileName = "scoop",
                                            Arguments = $"install {fbNames[0]}",
                                            UseShellExecute = false, CreateNoWindow = true,
                                            RedirectStandardOutput = true, RedirectStandardError = true
                                        };
                                        ps.Start(); ps.WaitForExit();
                                        if (ps.ExitCode == 0) fallbackOk = true;
                                    }
                                }
                                catch {  }
                            }

                            if (!fallbackOk && fbNames != null && fbNames.Length > 1 && fbNames[1] != null)
                            {
                                try
                                {
                                    Dispatcher.BeginInvoke(new Action(() => { LogBox.Text += IsRussian ? $"  Попытка: choco install {fbNames[1]}...\n" : $"  Trying: choco install {fbNames[1]}...\n"; LogBox.ScrollToEnd(); }));
                                    using (var pc = new Process())
                                    {
                                        pc.StartInfo = new ProcessStartInfo
                                        {
                                            FileName = "choco",
                                            Arguments = $"install {fbNames[1]} -y",
                                            UseShellExecute = false, CreateNoWindow = true,
                                            RedirectStandardOutput = true, RedirectStandardError = true
                                        };
                                        pc.Start(); pc.WaitForExit();
                                        if (pc.ExitCode == 0) fallbackOk = true;
                                    }
                                }
                                catch {  }
                            }

                            if (fallbackOk)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    installSuccessCount++;
                                    UpdateStatusBar();
                                    LogBox.Text += IsRussian ? "  ✔ Установлено через альтернативный менеджер\n" : "  ✔ Installed via alternative package manager\n";
                                    LogBox.ScrollToEnd();
                                });
                            }
                            else
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    installErrorCount++;
                                    UpdateStatusBar();
                                    LogBox.Text += IsRussian
                                        ? $"  ✘ Не удалось установить (winget/scoop/choco)\n"
                                        : $"  ✘ Failed to install (winget/scoop/choco)\n";
                                    LogBox.ScrollToEnd();
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            installErrorCount++;
                            UpdateStatusBar();
                            LogBox.Text += IsRussian ? $"  ✘ Ошибка: {ex.Message}\n" : $"  ✘ Error: {ex.Message}\n";
                            LogBox.ScrollToEnd();
                        });
                    }
                });
            }

            string summary = IsRussian
                ? $"\n═══ Завершено. Установлено: {installSuccessCount}, Уже было: {alreadyInstalledCount}, Ошибки: {installErrorCount} ═══\n"
                : $"\n═══ Done. Installed: {installSuccessCount}, Already: {alreadyInstalledCount}, Errors: {installErrorCount} ═══\n";
            LogBox.Text += summary;
            LogBox.ScrollToEnd();
            InstallBtn.IsEnabled = true;
        }
    }
}
