# ⚡ Hyperion

<div align="center">

**Пакетный установщик Windows-приложений с тёмным интерфейсом**

[**Social**](https://e-z.bio/l_limon_l) &nbsp;|&nbsp; [**Telegram**](https://t.me/l_limon_l)

</div>

Десктопная утилита для массовой установки популярного ПО на свежую систему Windows. Выберите нужные приложения из каталога, нажмите одну кнопку — Hyperion установит всё автоматически через **winget** в тихом режиме.

<img width="1200" height="880" alt="image" src="https://github.com/user-attachments/assets/3bd58bf7-8b11-48c2-9bda-3546ec61fd76" />

## 🌟 Возможности

- **⚡ Пакетная установка**: выберите любое количество приложений и установите их в один клик.
- **🔄 Тройной механизм установки**: `winget` → `scoop` → `chocolatey` — если один менеджер не справился, Hyperion автоматически пробует следующий.
- **🌍 Двуязычный интерфейс**: автоматически определяет язык системы — русский или английский.
- **🎨 Тёмная тема**: интерфейс в стиле VS Code / Visual Studio с кастомным title bar и закруглёнными углами (DWM).
- **✨ Плавные анимации**: переходы между категориями, toggle-переключатели, hover-эффекты карточек — всё анимировано.
- **📋 Подробный лог**: весь процесс установки отображается в реальном времени в консольном окне с возможностью сохранения в файл.
- **🔍 Умное определение**: Hyperion распознаёт уже установленные приложения и не устанавливает их повторно.
- **📦 76 приложений**: обширный каталог ПО, распределённый по 14 категориям.

## 📂 Каталог приложений

| Категория | Приложения |
|---|---|
| **Веб-браузеры** | Chrome, Opera, Firefox, Edge, Brave |
| **Мессенджеры** | Telegram, Discord, Teams, Zoom |
| **Мультимедиа** | VLC, Spotify, Audacity, HandBrake |
| **.NET** | Desktop Runtime (v8/v9/v10), ASP.NET Core (v8/v9/v10), .NET 4.8.1 |
| **Java** | Eclipse Temurin JRE (8/11/17/21), Amazon Corretto JDK (8/11/21) |
| **Изображения и дизайн** | Krita, Blender, GIMP, IrfanView, Inkscape, Greenshot, ShareX |
| **Документы** | Foxit Reader, SumatraPDF |
| **Безопасность** | Malwarebytes |
| **Файлы и Облако** | qBittorrent, Dropbox, Google Drive, Quick Share, OneDrive |
| **Другое** | Evernote, Steam, Epic Games, EA App, GOG Galaxy, KeePass 2, Everything |
| **Утилиты** | TeamViewer, RealVNC (Server/Viewer), TightVNC, TeraCopy, Revo, WizTree, CCleaner |
| **Архивирование** | 7-Zip, WinRAR |
| **Библиотеки VC++** | VC Redist 2008 – 2022 (x64 & x86) |
| **Для разработчиков** | Python 3, Git, FileZilla, Notepad++, WinSCP, PuTTY, WinMerge, VS Code, Cursor |

## ⚙️ Как это работает

```
Пользователь выбирает приложения
        │
        ▼
┌───────────────────────┐
│ 1. winget install      │──── Успех ──── ✔ Установлено
│    --exact --silent    │
└───────────────────────┘
        │ Неудача
        ▼
┌───────────────────────┐
│ 2. scoop install       │──── Успех ──── ✔ Установлено
│    (если доступен)     │
└───────────────────────┘
        │ Неудача
        ▼
┌───────────────────────┐
│ 3. choco install -y    │──── Успех ──── ✔ Установлено
│    (если доступен)     │
└───────────────────────┘
        │ Неудача
        ▼
      ✘ Ошибка записана в лог
```

1. **Winget (приоритет)**: `winget install --id <ID> --exact --silent` — основной способ.
2. **Scoop (fallback #1)**: если winget недоступен или завершился с ошибкой, Hyperion пробует `scoop install <pkg>`.
3. **Chocolatey (fallback #2)**: если и scoop не помог — `choco install <pkg> -y`.
4. **Определение статуса**: Hyperion анализирует exit code winget — если приложение уже установлено, это отображается отдельно.

### Требования
- **Windows 10 1709+** (winget предустановлен в Windows 11; для Windows 10 может потребоваться установка [App Installer](https://apps.microsoft.com/detail/9NBLGGH4NNS1) из Microsoft Store)

## 🛠 Сборка из исходного кода

### Требования
- **.NET Framework 4.8** SDK
- **Visual Studio 2022** или **.NET CLI**

### Сборка

1. **Клонирование репозитория:**
   ```bash
   git clone https://github.com/l-limon-l/Hyperion.git
   cd Hyperion
   ```

2. **Сборка через .NET CLI:**
   ```bash
   dotnet build -c Release
   ```

3. **Или откройте `HyperionWPF.csproj` в Visual Studio** и соберите проект (Ctrl + Shift + B).

### Публикация (single-file)
```bash
dotnet publish -c Release -r win-x64 --self-contained -o publish_out
```

## 🏗 Структура проекта

```
HyperionWPF/
├── App.xaml / App.xaml.cs      — точка входа приложения
├── MainWindow.xaml              — разметка главного окна (XAML)
├── MainWindow.xaml.cs           — логика UI, каталог ПО, механизм установки
├── Styles/
│   └── DarkTheme.xaml           — тёмная тема: цвета, toggle switch, кнопки,
│                                  scrollbar, навигация
├── Icons/                       — иконки приложений (PNG, 32×32)
├── hyperion.ico                 — иконка приложения
├── app.manifest                 — манифест (запрос прав администратора)
└── HyperionWPF.csproj           — файл проекта (.NET Framework 4.8, WPF)
```

## 🖥 Интерфейс

- **Левая панель** — навигация по категориям с иконками Segoe MDL2 Assets и акцентным индикатором выбранной категории.
- **Основная область** — карточки приложений с toggle-переключателями. Группы (Java, .NET, VC++) раскрываются по клику с анимацией.
- **Кнопка «Установить»** — отображает количество выбранных приложений и запускает пакетную установку.
- **Лог** — консольная область с кнопками очистки и сохранения лога в файл.

## 📜 Лицензия

Проект распространяется под лицензией MIT. Подробности в файле [`LICENSE`](LICENSE).
