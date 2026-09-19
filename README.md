# ⚡ Hyperion

[🇬🇧 English](#-english) | [🇷🇺 Русский](#-русский)

---

<a id="-english"></a>
## 🇬🇧 English

**Hyperion** is a batch software installer for Windows with a dark, keyboard-friendly interface.
Pick what you need on a fresh machine, press one button, and Hyperion installs everything
silently through **winget**, falling back to **scoop** and **Chocolatey** when a package is
missing or a download fails.

### 🚀 Features
- **Popular first.** The opening page is the set of apps most people put on a new Windows
  install — browsers, a messenger, an archiver, the VC++ runtime — so a usable machine is two
  clicks away. Everything else lives in its category.
- **Batch installation.** Select any number of apps; they install one after another with a
  progress bar, a running count, and a **Cancel** button that actually stops the run.
- **Three install engines.** `winget` → `scoop` → `chocolatey`. Each package carries the id for
  every manager that has it, so a package winget dropped still installs.
- **Search.** Type in the sidebar to search all 119 entries by name, description or package id.
- **Version groups.** Runtimes (.NET, Java, VC++) expand into per-version switches with a master
  switch on the group.
- **Bilingual.** English and Russian, chosen from the system UI language.
- **Detailed log.** Everything the package managers print, timestamped, clearable and saveable
  to a file.
- **Already-installed detection.** Packages that are present and current are reported as such
  instead of counting as failures.
- **Tidy uninstall.** If Hyperion installed scoop or Chocolatey for you, it offers to remove
  them again when you close it — and it never touches a copy you installed yourself.

### 📚 Catalogue
**119 entries / 138 packages** across **14 categories**, plus the Popular page:

| Category | Entries | Category | Entries |
| --- | --: | --- | --: |
| Browsers | 9 | Compression | 4 |
| Messaging | 8 | Security | 5 |
| Media | 10 | Developer Tools | 16 |
| Graphics | 11 | .NET | 4 |
| Documents | 11 | Java | 3 |
| Gaming | 7 | VC++ Redistributables | 5 |
| Files & Cloud | 10 | | |
| Utilities | 16 | | |

Every winget id in the catalogue is checked against the official winget source index before it
is committed — see [Maintaining the catalogue](#maintaining-the-catalogue).

### 🛠️ Tech stack
- C# / .NET Framework 4.8
- WPF, no third-party UI packages
- winget, scoop and Chocolatey command-line interfaces

### 📦 Install & use
1. Download the latest build from the Releases page.
2. Run `Hyperion.exe`. It requests administrator rights, which the package managers need.
3. Pick apps from **Popular**, browse the categories, or search.
4. Press **Install selected**.
5. Watch the log. Hyperion downloads and installs everything without further prompts.

Requires Windows 10 1709 or newer with App Installer (winget) present.

<a id="maintaining-the-catalogue"></a>
### 🧰 Maintaining the catalogue
The catalogue is data, not hand-written C#. `Model/Catalog.Data.cs` is generated:

```bash
cd tools
python3 verify_ids.py    # checks every winget id against the live winget source index
python3 gen_catalog.py   # regenerates Model/Catalog.Data.cs from catalog.py
```

To add an app, add one line to `tools/catalog.py`, run both scripts, and rebuild. Drop a 32×32
PNG into `Icons/` named after the entry key to give it a logo; without one the app draws a
coloured monogram tile.

---

<a id="-русский"></a>
## 🇷🇺 Русский

**Hyperion** — десктопная утилита для массовой установки программ на Windows с тёмным
интерфейсом. Отметьте нужное на свежей системе, нажмите одну кнопку — и Hyperion установит всё
в тихом режиме через **winget**, а если пакета там нет или загрузка сорвалась, попробует
**scoop** и **Chocolatey**.

### 🚀 Особенности
- **Сначала популярное.** Первая страница — то, что чаще всего ставят на новую Windows:
  браузер, мессенджер, архиватор, библиотеки VC++. Рабочая система в два клика. Остальное
  разложено по категориям.
- **Пакетная установка.** Выберите сколько угодно программ: они ставятся по очереди, с
  прогресс-баром, счётчиком и кнопкой **Отмена**, которая действительно прерывает установку.
- **Три механизма установки.** `winget` → `scoop` → `chocolatey`. У каждого пакета указаны
  идентификаторы всех менеджеров, где он есть, поэтому программа установится даже если её
  убрали из winget.
- **Поиск.** Начните печатать в боковой панели — поиск идёт по названию, описанию и
  идентификатору пакета среди всех 119 позиций.
- **Группы версий.** Среды выполнения (.NET, Java, VC++) раскрываются в переключатели по
  версиям, с общим переключателем на группе.
- **Два языка.** Русский и английский, по языку системы.
- **Подробный лог.** Всё, что печатают пакетные менеджеры, с отметками времени, с очисткой и
  сохранением в файл.
- **Определение установленного.** Актуальные версии отмечаются как «уже было», а не как ошибки.
- **Аккуратное удаление.** Если Hyperion сам поставил scoop или Chocolatey, при закрытии он
  предложит их удалить — и никогда не тронет те, что вы ставили сами.

### 📚 Каталог
**119 позиций / 138 пакетов** в **14 категориях**, плюс страница «Популярное»:

| Категория | Позиций | Категория | Позиций |
| --- | --: | --- | --: |
| Браузеры | 9 | Архиваторы | 4 |
| Мессенджеры | 8 | Безопасность | 5 |
| Мультимедиа | 10 | Для разработчиков | 16 |
| Графика и дизайн | 11 | .NET | 4 |
| Документы | 11 | Java | 3 |
| Игры | 7 | Библиотеки VC++ | 5 |
| Файлы и облако | 10 | | |
| Утилиты | 16 | | |

Каждый идентификатор winget сверяется с официальным индексом источника winget перед коммитом —
см. [Поддержка каталога](#поддержка-каталога).

### 🛠️ Стек технологий
- C# / .NET Framework 4.8
- WPF, без сторонних UI-библиотек
- Командные интерфейсы winget, scoop и Chocolatey

### 📦 Установка и запуск
1. Скачайте готовую сборку со страницы Releases.
2. Запустите `Hyperion.exe` — он запросит права администратора, нужные пакетным менеджерам.
3. Выберите программы на странице «Популярное», в категориях или через поиск.
4. Нажмите **Установить выбранное**.
5. Следите за логом: Hyperion всё скачает и установит без лишних вопросов.

Требуется Windows 10 1709 или новее с установленным App Installer (winget).

<a id="поддержка-каталога"></a>
### 🧰 Поддержка каталога
Каталог — это данные, а не рукописный C#. Файл `Model/Catalog.Data.cs` генерируется:

```bash
cd tools
python3 verify_ids.py    # сверяет все идентификаторы winget с живым индексом источника
python3 gen_catalog.py   # пересобирает Model/Catalog.Data.cs из catalog.py
```

Чтобы добавить программу, допишите строку в `tools/catalog.py`, запустите оба скрипта и
пересоберите проект. Положите PNG 32×32 в `Icons/` с именем ключа записи — и у неё появится
логотип; без него рисуется цветная плитка с буквой.

---
*Made with ❤️ / Сделано с ❤️*
