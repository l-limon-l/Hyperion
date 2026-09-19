using System.Globalization;

namespace HyperionWPF.Localization
{
    /// <summary>
    /// Every user-visible string, in the two languages Hyperion ships.
    /// The language follows the system UI culture and is fixed for the lifetime of the process.
    /// </summary>
    internal static class Strings
    {
        /// <summary>True when the interface should be Russian.</summary>
        public static bool Russian { get; } =
            CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru";

        private static string S(string en, string ru) => Russian ? ru : en;

        public static string Subtitle => S("Batch installer for Windows", "Пакетная установка для Windows");

        public static string SearchPlaceholder => S("Search apps…", "Поиск программ…");

        public static string SearchResults => S("Search results", "Результаты поиска");

        public static string NothingFound => S("Nothing matches that search.", "По запросу ничего не найдено.");

        public static string SelectAll => S("Select all", "Выбрать всё");

        public static string ClearAll => S("Clear", "Снять выбор");

        public static string On => S("On", "Вкл.");

        /// <summary>"3 versions" / "3 версии" — Russian needs the count to pick the ending.</summary>
        public static string VariantCount(int count)
        {
            if (!Russian)
            {
                return count + (count == 1 ? " version" : " versions");
            }

            int lastTwo = count % 100;
            int last = count % 10;
            if (lastTwo >= 11 && lastTwo <= 14)
            {
                return count + " версий";
            }

            if (last == 1)
            {
                return count + " версия";
            }

            return last >= 2 && last <= 4 ? count + " версии" : count + " версий";
        }

        public static string Off => S("Off", "Откл.");

        public static string WaitingForSelection => S("Waiting for selection…", "Ожидание выбора…");

        public static string LogCleared => S("Log cleared.", "Лог очищен.");

        public static string Cancel => S("Cancel", "Отмена");

        public static string Preparing => S("Preparing…", "Подготовка…");

        public static string Ready => S("Ready", "Готово к работе");

        public static string ClearLogTip => S("Clear log", "Очистить лог");

        public static string SaveLogTip => S("Save log to a file", "Сохранить лог в файл");

        public static string InstallSelected(int count) =>
            S("Install selected (" + count + ")", "Установить выбранное (" + count + ")");

        public static string Installing(int done, int total) =>
            S("Installing " + done + " of " + total + "…", "Установка " + done + " из " + total + "…");

        public static string PopularHint => S(
            "The apps most people put on a fresh Windows install. Turn on the ones you want, or select them all.",
            "То, что чаще всего ставят на свежую Windows. Включите нужное или выберите всё сразу.");

        public static string NoSelectionTitle => S("No selection", "Ничего не выбрано");

        public static string NoSelectionBody =>
            S("Please select at least one application.", "Пожалуйста, выберите хотя бы одно приложение.");

        public static string LogSaved(string path) =>
            S("Log saved: " + path, "Лог сохранён: " + path);

        public static string LogSaveFailed(string reason) =>
            S("Failed to save the log: " + reason, "Не удалось сохранить лог: " + reason);

        public static string SaveLogFilter =>
            S("Text files (*.txt)|*.txt|All files (*.*)|*.*", "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*");

        public static string StatusInstalled(int count) =>
            S("Installed: " + count, "Установлено: " + count);

        public static string StatusAlready(int count) =>
            S("Already installed: " + count, "Уже было: " + count);

        public static string StatusErrors(int count) =>
            S("Errors: " + count, "Ошибки: " + count);

        // --- Package managers -------------------------------------------------

        public static string CheckingManagers =>
            S("Checking package managers…", "Проверяю пакетные менеджеры…");

        public static string WingetMissing => S(
            "winget was not found. Install \"App Installer\" from the Microsoft Store, then restart Hyperion.",
            "winget не найден. Установите «Установщик приложений» из Microsoft Store и перезапустите Hyperion.");

        public static string WingetMissingTitle => S("winget is required", "Требуется winget");

        public static string Detected(string tool) => S(tool + " detected.", tool + " найден.");

        public static string InstallingTool(string tool) =>
            S("Installing " + tool + "…", "Устанавливаю " + tool + "…");

        public static string ToolInstallFailed(string tool) => S(
            tool + " could not be installed; it will be skipped as a fallback.",
            tool + " установить не удалось; он не будет использоваться как запасной вариант.");

        public static string ManagersReady => S("Package managers ready.", "Пакетные менеджеры готовы.");

        public static string UninstallManagersQuestion => S(
            "Remove the package managers Hyperion installed (Scoop, Chocolatey)?",
            "Удалить пакетные менеджеры, установленные Hyperion (Scoop, Chocolatey)?");

        public static string UninstallManagersTitle => S("Before closing", "Перед закрытием");

        public static string UninstallingManagers =>
            S("Removing package managers…", "Удаляю пакетные менеджеры…");

        public static string KeepingManagers =>
            S("Keeping the package managers.", "Пакетные менеджеры оставлены.");

        public static string ManagersRemoved =>
            S("Package managers removed.", "Пакетные менеджеры удалены.");

        // --- Installation -----------------------------------------------------

        public static string InstallStarted(int count) =>
            S("═══ Installing " + count + " package(s) ═══", "═══ Устанавливаю пакетов: " + count + " ═══");

        public static string TryingWinget(string id) =>
            S("trying winget " + id + "…", "пробую winget " + id + "…");

        public static string TryingScoop(string id) =>
            S("trying scoop " + id + "…", "пробую scoop " + id + "…");

        public static string TryingChoco(string id) =>
            S("trying choco " + id + "…", "пробую choco " + id + "…");

        public static string InstalledVia(string manager) =>
            S("✔ installed via " + manager, "✔ установлено через " + manager);

        public static string AlreadyInstalled =>
            S("● already installed and up to date", "● уже установлено, версия актуальна");

        public static string RebootRequired =>
            S("✔ installed — a reboot is required to finish", "✔ установлено — для завершения нужна перезагрузка");

        public static string InstallFailed =>
            S("✘ could not be installed by any package manager", "✘ не удалось установить ни одним менеджером");

        public static string NoManagerFor(string id) =>
            S("✘ no package manager carries " + id, "✘ ни один менеджер не знает пакет " + id);

        public static string Cancelled => S("■ cancelled", "■ отменено");

        public static string InstallCancelled =>
            S("═══ Cancelled ═══", "═══ Отменено ═══");

        public static string Summary(int installed, int already, int failed) => S(
            "═══ Done. Installed: " + installed + ", already present: " + already + ", failed: " + failed + " ═══",
            "═══ Готово. Установлено: " + installed + ", уже было: " + already + ", ошибок: " + failed + " ═══");
    }
}
