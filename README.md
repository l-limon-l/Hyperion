# ⚡ Hyperion

[🇬🇧 English](#-english) | [🇷🇺 Русский](#-русский)

---

<a id="-english"></a>
## 🇬🇧 English

**Hyperion** is a batch Windows application installer with a sleek dark interface. It's a desktop utility for mass installation of popular software on a fresh Windows system. Select the apps you need from the catalog, click one button, and Hyperion will install everything automatically via **winget** in silent mode.

### 🚀 Features
- **Batch Installation**: Select any number of applications and install them in one click.
- **Triple Installation Engine**: `winget` → `scoop` → `chocolatey` — if one package manager fails, Hyperion automatically tries the next one.
- **Bilingual Interface**: Automatically detects your system language (English or Russian).
- **Dark Theme**: VS Code / Visual Studio style interface with a custom title bar and rounded corners (DWM).
- **Smooth Animations**: Transitions between categories, toggle switches, hover effects — everything is beautifully animated.
- **Detailed Log**: The entire installation process is displayed in real-time in a built-in console window, with an option to save to a file.
- **Smart Detection**: Hyperion recognizes already installed applications and skips them.
- **Extensive Catalog**: 76 apps divided into 14 categories (Browsers, Messengers, Dev Tools, etc.).

### 🛠️ Tech Stack
- C# (.NET Framework 4.8)
- WPF (Windows Presentation Foundation)
- winget / scoop / chocolatey APIs

### 📦 Installation & Usage
1. Download the latest compiled version from the Releases page.
2. Run `Hyperion.exe` (Administrator privileges are recommended and will be requested automatically).
3. Browse categories on the left and toggle the switches for the software you want.
4. Click the large "Install" button.
5. Watch the real-time log as Hyperion automatically downloads and silently installs everything.

*(Note: Requires Windows 10 1709+ with App Installer / winget)*

---

<a id="-русский"></a>
## 🇷🇺 Русский

**Hyperion** — это десктопная утилита с тёмным интерфейсом для массовой установки популярного ПО на свежую систему Windows. Выберите нужные приложения из каталога, нажмите одну кнопку — и Hyperion установит всё автоматически через **winget** в тихом режиме.

### 🚀 Особенности
- **Пакетная установка**: выберите любое количество приложений и установите их в один клик.
- **Тройной механизм установки**: `winget` → `scoop` → `chocolatey` — если один менеджер не справился, Hyperion автоматически пробует следующий.
- **Двуязычный интерфейс**: автоматически определяет язык системы (русский или английский).
- **Тёмная тема**: интерфейс в стиле VS Code с кастомным заголовком окна и закруглёнными углами.
- **Плавные анимации**: переходы между категориями, переключатели, hover-эффекты карточек — всё анимировано.
- **Подробный лог**: весь процесс установки отображается в реальном времени с возможностью сохранения в файл.
- **Умное определение**: программа распознаёт уже установленные приложения и не ставит их повторно.
- **Обширный каталог**: 76 приложений в 14 категориях (Браузеры, Мессенджеры, Для разработчиков и т.д.).

### 🛠️ Стек технологий
- C# (.NET Framework 4.8)
- WPF (Windows Presentation Foundation)
- winget / scoop / chocolatey

### 📦 Установка и запуск
1. Скачайте готовую версию со страницы Releases.
2. Запустите `Hyperion.exe` (программа сама запросит права Администратора).
3. Пройдитесь по категориям слева и выберите нужные программы ползунками.
4. Нажмите большую кнопку «Установить».
5. Наблюдайте за консолью логов: Hyperion сам всё скачает и установит в тихом режиме!

*(Требуется Windows 10 1709+ с установленным App Installer / winget)*

---
*Made with ❤️ / Сделано с ❤️*
