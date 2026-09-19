using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using HyperionWPF.Localization;
using HyperionWPF.Model;
using HyperionWPF.Services;
using Microsoft.Win32;

namespace HyperionWPF
{
    public partial class MainWindow : Window
    {
        private const int DwmwaWindowCornerPreference = 33;
        private const int DwmwcpRound = 2;

        /// <summary>The log is trimmed once it passes this, so a long run cannot grow without bound.</summary>
        private const int LogCharacterLimit = 200_000;

        private readonly Dictionary<string, Category> categoriesByKey = new Dictionary<string, Category>(StringComparer.Ordinal);
        private readonly Dictionary<string, StackPanel> pages = new Dictionary<string, StackPanel>(StringComparer.Ordinal);

        /// <summary>Every toggle that currently exists, grouped by package. One package can appear on two pages.</summary>
        private readonly Dictionary<string, List<ToggleButton>> togglesByPackage =
            new Dictionary<string, List<ToggleButton>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>The packages a page offers, in display order, so "select all" knows its scope.</summary>
        private readonly Dictionary<string, List<string>> packagesByPage =
            new Dictionary<string, List<string>>(StringComparer.Ordinal);

        /// <summary>Toggles belonging to the throwaway search page, cleared on every new search.</summary>
        private readonly List<KeyValuePair<string, ToggleButton>> searchToggles =
            new List<KeyValuePair<string, ToggleButton>>();

        /// <summary>When set, every toggle created is also recorded here. Used while building the search page.</summary>
        private List<KeyValuePair<string, ToggleButton>> toggleSink;

        private PackageManagerService managers;
        private PackageInstaller installer;
        private CancellationTokenSource installCancellation;

        private StackPanel searchPage;
        private bool syncingToggles;
        private bool syncingSearchBox;
        private bool installing;
        private bool closeConfirmed;

        private int installedCount;
        private int alreadyInstalledCount;
        private int failedCount;

        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        private static extern void DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        public MainWindow()
        {
            InitializeComponent();

            SubtitleText.Text = Strings.Subtitle;
            SearchBox.Tag = Strings.SearchPlaceholder;
            SelectAllBtn.Content = Strings.SelectAll;
            ClearAllBtn.Content = Strings.ClearAll;
            CancelBtn.Content = Strings.Cancel;
            ClearLogBtn.ToolTip = Strings.ClearLogTip;
            SaveLogBtn.ToolTip = Strings.SaveLogTip;
            LogBox.Text = string.Empty;

            InstallBtn.Content = Strings.InstallSelected(0);
            InstallBtn.IsEnabled = false;

            SourceInitialized += (sender, e) => EnableRoundedCorners();
            StateChanged += OnStateChanged;
            Loaded += Window_Loaded;
            Closing += MainWindow_Closing;

            BuildUi();
            UpdateStatusBar();
        }

        // =====================================================================
        // Start-up and shutdown
        // =====================================================================

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AppendLog(Strings.WaitingForSelection);

            managers = new PackageManagerService(AppendLog);
            installer = new PackageInstaller(managers, AppendLog);

            SubtitleText.Text = Strings.Preparing;
            bool ready = await managers.EnsureAsync(CancellationToken.None);
            SubtitleText.Text = ready ? Strings.Ready : Strings.WingetMissingTitle;

            if (!ready)
            {
                MessageBox.Show(this, Strings.WingetMissing, Strings.WingetMissingTitle,
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            UpdateInstallButton();
        }

        private async void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (closeConfirmed)
            {
                return;
            }

            e.Cancel = true;

            if (installing && installCancellation != null)
            {
                installCancellation.Cancel();
            }

            try
            {
                bool removable = managers != null && (managers.InstalledScoop || managers.InstalledChocolatey);
                if (removable)
                {
                    MessageBoxResult answer = MessageBox.Show(
                        this,
                        Strings.UninstallManagersQuestion,
                        Strings.UninstallManagersTitle,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (answer == MessageBoxResult.Yes)
                    {
                        IsEnabled = false;
                        Title = "Hyperion — " + Strings.UninstallingManagers;
                        AppendLog(Strings.UninstallingManagers);
                        await managers.UninstallInstalledManagersAsync(CancellationToken.None);
                        AppendLog(Strings.ManagersRemoved);
                        Title = "Hyperion";
                        IsEnabled = true;
                    }
                    else
                    {
                        AppendLog(Strings.KeepingManagers);
                    }
                }
            }
            catch (Exception ex)
            {
                AppendLog(ex.Message);
            }

            closeConfirmed = true;
            Close();
        }

        private void EnableRoundedCorners()
        {
            try
            {
                IntPtr handle = new WindowInteropHelper(this).Handle;
                int preference = DwmwcpRound;
                DwmSetWindowAttribute(handle, DwmwaWindowCornerPreference, ref preference, sizeof(int));
            }
            catch (Exception)
            {
                // Rounded corners are a Windows 11 nicety; older builds simply keep square ones.
            }
        }

        // =====================================================================
        // Building the catalogue UI
        // =====================================================================

        private void BuildUi()
        {
            foreach (Category category in Catalog.Categories)
            {
                categoriesByKey[category.Key] = category;

                var page = new StackPanel();
                var packages = new List<string>();

                foreach (AppEntry entry in category.Apps)
                {
                    page.Children.Add(CreateEntryCard(entry, packages));
                }

                pages[category.Key] = page;
                packagesByPage[category.Key] = packages;

                NavList.Items.Add(new ListBoxItem
                {
                    Content = CreateNavContent(category.Glyph, category.Name(Strings.Russian), category.Apps.Count),
                    Tag = category.Key,
                });
            }

            if (NavList.Items.Count > 0)
            {
                NavList.SelectedIndex = 0;
            }
        }

        private UIElement CreateNavContent(CategoryGlyph glyph, string name, int count)
        {
            var row = new Grid();
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var icon = new TextBlock
            {
                Text = GlyphFor(glyph),
                FontFamily = (FontFamily)FindResource("IconFont"),
                FontSize = 15,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0),
            };
            Grid.SetColumn(icon, 0);
            row.Children.Add(icon);

            var label = new TextBlock
            {
                Text = name,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
            };
            Grid.SetColumn(label, 1);
            row.Children.Add(label);

            var badge = new TextBlock
            {
                Text = count.ToString(),
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0),
                Foreground = (Brush)FindResource("TextMutedBrush"),
            };
            Grid.SetColumn(badge, 2);
            row.Children.Add(badge);

            return row;
        }

        private static string GlyphFor(CategoryGlyph glyph)
        {
            switch (glyph)
            {
                case CategoryGlyph.Star: return "";
                case CategoryGlyph.Browser: return "";
                case CategoryGlyph.Chat: return "";
                case CategoryGlyph.Media: return "";
                case CategoryGlyph.Image: return "";
                case CategoryGlyph.Doc: return "";
                case CategoryGlyph.Game: return "";
                case CategoryGlyph.Cloud: return "";
                case CategoryGlyph.Tools: return "";
                case CategoryGlyph.Archive: return "";
                case CategoryGlyph.Shield: return "";
                case CategoryGlyph.Code: return "";
                case CategoryGlyph.Runtime: return "";
                default: return "";
            }
        }

        /// <summary>Builds the card for one entry and records the packages it offers on that page.</summary>
        private Border CreateEntryCard(AppEntry entry, List<string> pagePackages)
        {
            return entry.IsGroup
                ? CreateGroupCard(entry, pagePackages)
                : CreateSimpleCard(entry, pagePackages);
        }

        private Border CreateSimpleCard(AppEntry entry, List<string> pagePackages)
        {
            Border card = CreateCardShell();
            Grid row = CreateCardRow(entry);

            ToggleButton toggle = CreateToggle(entry.Package, pagePackages);
            var status = CreateStatusText();
            AttachStatus(toggle, status, (Brush)FindResource("TextMutedBrush"));

            Grid.SetColumn(status, 2);
            row.Children.Add(status);
            Grid.SetColumn(toggle, 3);
            row.Children.Add(toggle);

            card.Child = row;
            MakeRowToggleable(card, toggle);
            AttachHover(card);
            return card;
        }

        private Border CreateGroupCard(AppEntry entry, List<string> pagePackages)
        {
            Border card = CreateCardShell();
            var stack = new StackPanel();
            Grid header = CreateCardRow(entry);
            header.Background = Brushes.Transparent;
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });

            var summary = CreateStatusText();
            Grid.SetColumn(summary, 2);
            header.Children.Add(summary);

            var master = new ToggleButton
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0),
                Style = (Style)FindResource("ToggleSwitchStyle"),
            };
            Grid.SetColumn(master, 3);
            header.Children.Add(master);

            var chevron = new TextBlock
            {
                Text = "",
                FontFamily = (FontFamily)FindResource("IconFont"),
                Foreground = (Brush)FindResource("TextMutedBrush"),
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform(0),
            };
            Grid.SetColumn(chevron, 4);
            header.Children.Add(chevron);

            stack.Children.Add(header);

            var variantStack = new StackPanel { Margin = new Thickness(68, 2, 16, 12) };
            var variantsBorder = new Border
            {
                BorderBrush = (Brush)FindResource("BorderSubtleBrush"),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Child = variantStack,
                Visibility = Visibility.Collapsed,
                RenderTransform = new TranslateTransform(0, 0),
            };

            var variantToggles = new List<ToggleButton>();
            bool syncingGroup = false;

            RoutedEventHandler masterChanged = (sender, e) =>
            {
                if (syncingGroup)
                {
                    return;
                }

                syncingGroup = true;
                try
                {
                    foreach (ToggleButton variantToggle in variantToggles)
                    {
                        variantToggle.IsChecked = master.IsChecked;
                    }
                }
                finally
                {
                    syncingGroup = false;
                }
            };
            master.Checked += masterChanged;
            master.Unchecked += masterChanged;

            foreach (AppVariant variant in entry.Variants)
            {
                var variantRow = new Grid
                {
                    Margin = new Thickness(0, 7, 0, 7),
                    Background = Brushes.Transparent,
                };
                variantRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                variantRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                variantRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var label = new TextBlock
                {
                    Text = variant.Label,
                    Foreground = (Brush)FindResource("TextSecondaryBrush"),
                    FontSize = 13,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetColumn(label, 0);
                variantRow.Children.Add(label);

                var variantStatus = CreateStatusText();
                Grid.SetColumn(variantStatus, 1);
                variantRow.Children.Add(variantStatus);

                ToggleButton variantToggle = CreateToggle(variant.Package, pagePackages);
                AttachStatus(variantToggle, variantStatus, (Brush)FindResource("TextMutedBrush"));
                RoutedEventHandler variantChanged = (sender, e) =>
                {
                    UpdateGroupSummary(entry, variantToggles, summary);
                    if (syncingGroup)
                    {
                        return;
                    }

                    syncingGroup = true;
                    try
                    {
                        master.IsChecked = variantToggles.All(t => t.IsChecked == true)
                            ? true
                            : variantToggles.Any(t => t.IsChecked == true) ? (bool?)null : false;
                    }
                    finally
                    {
                        syncingGroup = false;
                    }
                };
                variantToggle.Checked += variantChanged;
                variantToggle.Unchecked += variantChanged;
                Grid.SetColumn(variantToggle, 2);
                variantRow.Children.Add(variantToggle);

                variantToggles.Add(variantToggle);
                MakeRowToggleable(variantRow, variantToggle);
                variantStack.Children.Add(variantRow);
            }

            UpdateGroupSummary(entry, variantToggles, summary);

            stack.Children.Add(variantsBorder);
            card.Child = stack;

            bool expanded = false;
            header.PreviewMouseLeftButtonUp += (sender, e) =>
            {
                if (e.OriginalSource is DependencyObject source && IsDescendantOf(source, master))
                {
                    return;
                }

                expanded = !expanded;
                e.Handled = true;
                AnimateExpansion(variantsBorder, chevron, expanded);
            };

            AttachHover(card);
            return card;
        }

        private Border CreateCardShell()
        {
            return new Border
            {
                CornerRadius = new CornerRadius(8),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 6),
                Cursor = Cursors.Hand,
                BorderThickness = new Thickness(1),
                BorderBrush = (Brush)FindResource("BorderSubtleBrush"),
                Background = new SolidColorBrush(((SolidColorBrush)FindResource("SurfaceBrush")).Color),
            };
        }

        /// <summary>Icon, title and description: the part every card shares.</summary>
        private Grid CreateCardRow(AppEntry entry)
        {
            var row = new Grid { Height = 68 };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(68) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            FrameworkElement icon = CreateIcon(entry);
            Grid.SetColumn(icon, 0);
            row.Children.Add(icon);

            var textStack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0),
            };
            textStack.Children.Add(new TextBlock
            {
                Text = entry.Title,
                Foreground = (Brush)FindResource("TextPrimaryBrush"),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                TextTrimming = TextTrimming.CharacterEllipsis,
            });
            textStack.Children.Add(new TextBlock
            {
                Text = entry.Description(Strings.Russian),
                Foreground = (Brush)FindResource("TextMutedBrush"),
                FontSize = 12,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(0, 2, 0, 0),
            });
            Grid.SetColumn(textStack, 1);
            row.Children.Add(textStack);

            return row;
        }

        /// <summary>The app's PNG when we ship one, otherwise a monogram tile derived from its name.</summary>
        private FrameworkElement CreateIcon(AppEntry entry)
        {
            if (entry.IconKey != null)
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri("pack://application:,,,/Icons/" + entry.IconKey + ".png");
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    return new Image
                    {
                        Source = bitmap,
                        Width = 32,
                        Height = 32,
                        Margin = new Thickness(18, 0, 18, 0),
                    };
                }
                catch (Exception)
                {
                    // A missing or unreadable PNG falls through to the monogram below.
                }
            }

            return CreateMonogram(entry.Title);
        }

        private FrameworkElement CreateMonogram(string title)
        {
            char letter = title.FirstOrDefault(char.IsLetterOrDigit);
            if (letter == '\0')
            {
                letter = '?';
            }

            Color tint = TintFor(title);

            return new Border
            {
                Width = 32,
                Height = 32,
                CornerRadius = new CornerRadius(7),
                Margin = new Thickness(18, 0, 18, 0),
                Background = new SolidColorBrush(Color.FromArgb(0x38, tint.R, tint.G, tint.B)),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromArgb(0x55, tint.R, tint.G, tint.B)),
                Child = new TextBlock
                {
                    Text = char.ToUpperInvariant(letter).ToString(),
                    FontSize = 15,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(tint),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            };
        }

        /// <summary>A stable hue per title, so a monogram keeps the same colour between runs.</summary>
        private static Color TintFor(string title)
        {
            unchecked
            {
                int hash = 17;
                foreach (char c in title)
                {
                    hash = (hash * 31) + char.ToUpperInvariant(c);
                }

                double hue = Math.Abs(hash % 360);
                return FromHsl(hue, 0.55, 0.68);
            }
        }

        private static Color FromHsl(double hue, double saturation, double lightness)
        {
            double c = (1 - Math.Abs((2 * lightness) - 1)) * saturation;
            double x = c * (1 - Math.Abs(((hue / 60.0) % 2) - 1));
            double m = lightness - (c / 2);

            double r, g, b;
            if (hue < 60) { r = c; g = x; b = 0; }
            else if (hue < 120) { r = x; g = c; b = 0; }
            else if (hue < 180) { r = 0; g = c; b = x; }
            else if (hue < 240) { r = 0; g = x; b = c; }
            else if (hue < 300) { r = x; g = 0; b = c; }
            else { r = c; g = 0; b = x; }

            return Color.FromRgb(
                (byte)Math.Round((r + m) * 255),
                (byte)Math.Round((g + m) * 255),
                (byte)Math.Round((b + m) * 255));
        }

        private TextBlock CreateStatusText()
        {
            return new TextBlock
            {
                Text = Strings.Off,
                Foreground = (Brush)FindResource("TextMutedBrush"),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0),
                FontSize = 12,
            };
        }

        private void AttachStatus(ToggleButton toggle, TextBlock status, Brush offBrush)
        {
            Brush onBrush = (Brush)FindResource("TextPrimaryBrush");

            if (toggle.IsChecked == true)
            {
                status.Text = Strings.On;
                status.Foreground = onBrush;
            }

            toggle.Checked += (sender, e) =>
            {
                status.Text = Strings.On;
                status.Foreground = onBrush;
            };
            toggle.Unchecked += (sender, e) =>
            {
                status.Text = Strings.Off;
                status.Foreground = offBrush;
            };
        }

        private void UpdateGroupSummary(AppEntry entry, List<ToggleButton> variantToggles, TextBlock summary)
        {
            int selected = variantToggles.Count(t => t.IsChecked == true);
            if (selected == 0)
            {
                summary.Text = Strings.VariantCount(entry.Variants.Count);
                summary.Foreground = (Brush)FindResource("TextMutedBrush");
            }
            else
            {
                summary.Text = selected + " / " + entry.Variants.Count;
                summary.Foreground = (Brush)FindResource("AccentBrush");
            }
        }

        /// <summary>Clicking anywhere on the row flips its switch, except on the switch itself.</summary>
        private void MakeRowToggleable(UIElement row, ToggleButton toggle)
        {
            row.PreviewMouseLeftButtonUp += (sender, e) =>
            {
                if (e.OriginalSource is DependencyObject source && IsDescendantOf(source, toggle))
                {
                    return;
                }

                toggle.IsChecked = !(toggle.IsChecked == true);
                e.Handled = true;
            };
        }

        private void AttachHover(Border card)
        {
            var background = (SolidColorBrush)card.Background;
            Color resting = ((SolidColorBrush)FindResource("SurfaceBrush")).Color;
            Color hovered = ((SolidColorBrush)FindResource("SurfaceHoverBrush")).Color;

            card.MouseEnter += (sender, e) => background.BeginAnimation(
                SolidColorBrush.ColorProperty,
                new ColorAnimation(hovered, TimeSpan.FromMilliseconds(120)));
            card.MouseLeave += (sender, e) => background.BeginAnimation(
                SolidColorBrush.ColorProperty,
                new ColorAnimation(resting, TimeSpan.FromMilliseconds(150)));
        }

        private static void AnimateExpansion(Border panel, TextBlock chevron, bool expanded)
        {
            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

            ((RotateTransform)chevron.RenderTransform).BeginAnimation(
                RotateTransform.AngleProperty,
                new DoubleAnimation(expanded ? 0 : 180, expanded ? 180 : 0, TimeSpan.FromMilliseconds(220)) { EasingFunction = ease });

            if (expanded)
            {
                panel.Visibility = Visibility.Visible;
                panel.Opacity = 0;
                panel.RenderTransform = new TranslateTransform(0, -10);
                panel.BeginAnimation(OpacityProperty,
                    new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220)) { EasingFunction = ease });
                panel.RenderTransform.BeginAnimation(TranslateTransform.YProperty,
                    new DoubleAnimation(-10, 0, TimeSpan.FromMilliseconds(260)) { EasingFunction = ease });
                return;
            }

            var fade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(160)) { EasingFunction = ease };
            fade.Completed += (sender, e) =>
            {
                panel.Visibility = Visibility.Collapsed;
                panel.BeginAnimation(OpacityProperty, null);
                panel.Opacity = 1;
            };
            panel.BeginAnimation(OpacityProperty, fade);
        }

        private static bool IsDescendantOf(DependencyObject candidate, DependencyObject ancestor)
        {
            for (DependencyObject node = candidate; node != null; node = VisualTreeHelper.GetParent(node))
            {
                if (ReferenceEquals(node, ancestor))
                {
                    return true;
                }
            }

            return false;
        }

        // =====================================================================
        // Selection
        // =====================================================================

        private ToggleButton CreateToggle(PackageRef package, List<string> pagePackages)
        {
            var toggle = new ToggleButton
            {
                Tag = package.Id,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 16, 0),
                Style = (Style)FindResource("ToggleSwitchStyle"),
                ToolTip = package.DisplayId,
            };

            if (pagePackages != null && !pagePackages.Contains(package.Id))
            {
                pagePackages.Add(package.Id);
            }

            RegisterToggle(package.Id, toggle);

            if (toggleSink != null)
            {
                toggleSink.Add(new KeyValuePair<string, ToggleButton>(package.Id, toggle));
            }

            toggle.Checked += OnToggleChanged;
            toggle.Unchecked += OnToggleChanged;
            return toggle;
        }

        private void RegisterToggle(string packageId, ToggleButton toggle)
        {
            if (!togglesByPackage.TryGetValue(packageId, out List<ToggleButton> list))
            {
                list = new List<ToggleButton>();
                togglesByPackage[packageId] = list;
            }
            else if (list.Count > 0)
            {
                // A package shown on two pages starts out matching what is already selected.
                toggle.IsChecked = list[0].IsChecked;
            }

            list.Add(toggle);
        }

        private void UnregisterToggle(string packageId, ToggleButton toggle)
        {
            if (togglesByPackage.TryGetValue(packageId, out List<ToggleButton> list))
            {
                list.Remove(toggle);
            }
        }

        /// <summary>Keeps the copies of one package's switch (Popular page, category page, search) in step.</summary>
        private void OnToggleChanged(object sender, RoutedEventArgs e)
        {
            if (!(sender is ToggleButton toggle) || !(toggle.Tag is string packageId))
            {
                return;
            }

            if (!syncingToggles && togglesByPackage.TryGetValue(packageId, out List<ToggleButton> siblings))
            {
                syncingToggles = true;
                try
                {
                    foreach (ToggleButton sibling in siblings)
                    {
                        if (!ReferenceEquals(sibling, toggle))
                        {
                            sibling.IsChecked = toggle.IsChecked;
                        }
                    }
                }
                finally
                {
                    syncingToggles = false;
                }
            }

            UpdateInstallButton();
        }

        private IEnumerable<string> SelectedPackageIds()
        {
            return togglesByPackage
                .Where(pair => pair.Value.Any(toggle => toggle.IsChecked == true))
                .Select(pair => pair.Key);
        }

        private int SelectedCount() => SelectedPackageIds().Count();

        private void UpdateInstallButton()
        {
            int count = SelectedCount();
            if (!installing)
            {
                InstallBtn.Content = Strings.InstallSelected(count);
                InstallBtn.IsEnabled = count > 0 && managers != null && managers.Availability.Winget;
            }
        }

        private void SetPageSelection(bool selected)
        {
            if (!(NavList.SelectedItem is ListBoxItem item) || !(item.Tag is string key))
            {
                return;
            }

            List<string> packages = key == SearchPageKey
                ? searchToggles.Select(pair => pair.Key).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
                : packagesByPage.TryGetValue(key, out List<string> known) ? known : new List<string>();

            foreach (string packageId in packages)
            {
                if (togglesByPackage.TryGetValue(packageId, out List<ToggleButton> toggles) && toggles.Count > 0)
                {
                    toggles[0].IsChecked = selected;
                }
            }

            UpdateInstallButton();
        }

        private void SelectAllBtn_Click(object sender, RoutedEventArgs e) => SetPageSelection(true);

        private void ClearAllBtn_Click(object sender, RoutedEventArgs e) => SetPageSelection(false);

        // =====================================================================
        // Navigation and search
        // =====================================================================

        private const string SearchPageKey = "\u0000search";

        private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(NavList.SelectedItem is ListBoxItem item) || !(item.Tag is string key))
            {
                return;
            }

            if (key != SearchPageKey && SearchBox.Text.Length > 0 && !syncingSearchBox)
            {
                syncingSearchBox = true;
                try
                {
                    SearchBox.Clear();
                }
                finally
                {
                    syncingSearchBox = false;
                }
            }

            StackPanel page = key == SearchPageKey ? searchPage : pages[key];
            if (page == null)
            {
                return;
            }

            if (key == SearchPageKey)
            {
                PageTitle.Text = Strings.SearchResults;
                PageHint.Text = string.Empty;
            }
            else
            {
                Category category = categoriesByKey[key];
                PageTitle.Text = category.Name(Strings.Russian);
                PageHint.Text = category.Key == "Popular" ? Strings.PopularHint : string.Empty;
            }

            ShowPage(page);
        }

        private void ShowPage(StackPanel page)
        {
            ContentPanel.Children.Clear();

            if (page.Parent is Panel previousParent)
            {
                previousParent.Children.Remove(page);
            }

            page.Opacity = 0;
            page.RenderTransform = new TranslateTransform(0, 16);
            ContentPanel.Children.Add(page);

            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
            page.BeginAnimation(OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220)) { EasingFunction = ease });
            page.RenderTransform.BeginAnimation(TranslateTransform.YProperty,
                new DoubleAnimation(16, 0, TimeSpan.FromMilliseconds(260)) { EasingFunction = ease });

            MainScroll.ScrollToTop();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = SearchBox.Text.Trim();

            foreach (KeyValuePair<string, ToggleButton> pair in searchToggles)
            {
                UnregisterToggle(pair.Key, pair.Value);
            }

            searchToggles.Clear();

            if (query.Length == 0)
            {
                RemoveSearchNavItem();
                if (NavList.SelectedIndex < 0 && NavList.Items.Count > 0)
                {
                    NavList.SelectedIndex = 0;
                }

                return;
            }

            List<AppEntry> matches = Catalog.Categories
                .Skip(1)
                .SelectMany(category => category.Apps)
                .Where(entry => Matches(entry, query))
                .ToList();

            searchPage = new StackPanel();
            var pagePackages = new List<string>();
            toggleSink = searchToggles;

            if (matches.Count == 0)
            {
                searchPage.Children.Add(new TextBlock
                {
                    Text = Strings.NothingFound,
                    Foreground = (Brush)FindResource("TextMutedBrush"),
                    FontSize = 13,
                    Margin = new Thickness(2, 8, 0, 0),
                });
            }
            else
            {
                foreach (AppEntry entry in matches)
                {
                    searchPage.Children.Add(CreateEntryCard(entry, pagePackages));
                }
            }

            toggleSink = null;

            ShowSearchNavItem(matches.Count);
        }

        private static bool Matches(AppEntry entry, string query)
        {
            if (entry.Title.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0
                || entry.Key.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0
                || entry.Description(Strings.Russian).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return entry.Packages.Any(package =>
                package.DisplayId.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void ShowSearchNavItem(int matchCount)
        {
            ListBoxItem item = FindSearchNavItem();
            if (item == null)
            {
                item = new ListBoxItem { Tag = SearchPageKey };
                NavList.Items.Insert(0, item);
            }

            item.Content = CreateNavContent(CategoryGlyph.Tools, Strings.SearchResults, matchCount);

            if (ReferenceEquals(NavList.SelectedItem, item))
            {
                NavList_SelectionChanged(NavList, null);
            }
            else
            {
                NavList.SelectedItem = item;
            }
        }

        private void RemoveSearchNavItem()
        {
            ListBoxItem item = FindSearchNavItem();
            if (item != null)
            {
                NavList.Items.Remove(item);
            }

            searchPage = null;
        }

        private ListBoxItem FindSearchNavItem()
        {
            return NavList.Items
                .OfType<ListBoxItem>()
                .FirstOrDefault(candidate => (candidate.Tag as string) == SearchPageKey);
        }

        // =====================================================================
        // Installing
        // =====================================================================

        private async void InstallBtn_Click(object sender, RoutedEventArgs e)
        {
            List<PackageRef> selected = OrderedSelection();
            if (selected.Count == 0)
            {
                MessageBox.Show(this, Strings.NoSelectionBody, Strings.NoSelectionTitle,
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            installing = true;
            installedCount = 0;
            alreadyInstalledCount = 0;
            failedCount = 0;
            UpdateStatusBar();

            InstallBtn.IsEnabled = false;
            CancelBtn.Visibility = Visibility.Visible;
            InstallProgress.Visibility = Visibility.Visible;
            InstallProgress.Maximum = selected.Count;
            InstallProgress.Value = 0;
            SetTogglesEnabled(false);

            installCancellation = new CancellationTokenSource();
            CancellationToken token = installCancellation.Token;

            AppendLog(string.Empty);
            AppendLog(Strings.InstallStarted(selected.Count));

            int done = 0;
            bool cancelled = false;

            foreach (PackageRef package in selected)
            {
                if (token.IsCancellationRequested)
                {
                    cancelled = true;
                    break;
                }

                InstallBtn.Content = Strings.Installing(done + 1, selected.Count);
                AppendLog("▶ " + package.DisplayId);

                InstallResult result;
                try
                {
                    result = await installer.InstallAsync(package, token);
                }
                catch (Exception ex)
                {
                    AppendLog("  " + ex.Message);
                    result = InstallResult.From(InstallStatus.Failed, null);
                }

                switch (result.Status)
                {
                    case InstallStatus.Installed:
                        installedCount++;
                        AppendLog("  " + Strings.InstalledVia(result.Manager));
                        break;
                    case InstallStatus.RebootRequired:
                        installedCount++;
                        AppendLog("  " + Strings.RebootRequired);
                        break;
                    case InstallStatus.AlreadyInstalled:
                        alreadyInstalledCount++;
                        AppendLog("  " + Strings.AlreadyInstalled);
                        break;
                    case InstallStatus.Cancelled:
                        cancelled = true;
                        AppendLog("  " + Strings.Cancelled);
                        break;
                    default:
                        failedCount++;
                        AppendLog("  " + Strings.InstallFailed);
                        break;
                }

                done++;
                InstallProgress.Value = done;
                UpdateStatusBar();

                if (cancelled)
                {
                    break;
                }
            }

            AppendLog(cancelled
                ? Strings.InstallCancelled
                : Strings.Summary(installedCount, alreadyInstalledCount, failedCount));

            installCancellation.Dispose();
            installCancellation = null;
            installing = false;

            CancelBtn.Visibility = Visibility.Collapsed;
            InstallProgress.Visibility = Visibility.Collapsed;
            SetTogglesEnabled(true);
            UpdateInstallButton();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (installCancellation != null)
            {
                CancelBtn.IsEnabled = false;
                installCancellation.Cancel();
            }
        }

        /// <summary>The selected packages in catalogue order, each one only once.</summary>
        private List<PackageRef> OrderedSelection()
        {
            var selected = new HashSet<string>(SelectedPackageIds(), StringComparer.OrdinalIgnoreCase);

            return Catalog.Categories
                .Skip(1)
                .SelectMany(category => category.Apps)
                .SelectMany(entry => entry.Packages)
                .Where(package => selected.Contains(package.Id))
                .GroupBy(package => package.Id, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();
        }

        /// <summary>
        /// Freezes the selection for the duration of a run. Disabling the panel the pages live in
        /// covers every switch, including ones on pages that have not been shown yet.
        /// </summary>
        private void SetTogglesEnabled(bool enabled)
        {
            ContentPanel.IsEnabled = enabled;
            SelectAllBtn.IsEnabled = enabled;
            ClearAllBtn.IsEnabled = enabled;
            SearchBox.IsEnabled = enabled;
            CancelBtn.IsEnabled = true;
        }

        // =====================================================================
        // Log and status bar
        // =====================================================================

        private void AppendLog(string message)
        {
            if (!Dispatcher.CheckAccess())
            {
                try
                {
                    Dispatcher.BeginInvoke((Action)(() => AppendLog(message)));
                }
                catch (Exception)
                {
                    // The window is closing and its dispatcher has shut down; the line is simply lost.
                }

                return;
            }

            if (LogBox.Text.Length > LogCharacterLimit)
            {
                LogBox.Text = LogBox.Text.Substring(LogBox.Text.Length - (LogCharacterLimit / 2));
            }

            string line = message.Length == 0
                ? Environment.NewLine
                : "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message + Environment.NewLine;

            LogBox.AppendText(line);
            LogBox.ScrollToEnd();
        }

        private void UpdateStatusBar()
        {
            StatusInstalled.Text = installedCount > 0 ? Strings.StatusInstalled(installedCount) : string.Empty;
            StatusAlready.Text = alreadyInstalledCount > 0 ? Strings.StatusAlready(alreadyInstalledCount) : string.Empty;
            StatusErrors.Text = failedCount > 0 ? Strings.StatusErrors(failedCount) : string.Empty;
        }

        private void ClearLogBtn_Click(object sender, RoutedEventArgs e)
        {
            LogBox.Clear();
            installedCount = 0;
            alreadyInstalledCount = 0;
            failedCount = 0;
            UpdateStatusBar();
            AppendLog(Strings.LogCleared);
        }

        private void SaveLogBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                FileName = "Hyperion_log_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"),
                DefaultExt = ".txt",
                Filter = Strings.SaveLogFilter,
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            try
            {
                var contents = new StringBuilder();
                contents.AppendLine("--- Hyperion log ---");
                contents.AppendLine("Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                contents.AppendLine("---");
                contents.Append(LogBox.Text);

                File.WriteAllText(dialog.FileName, contents.ToString(), Encoding.UTF8);
                AppendLog(Strings.LogSaved(dialog.FileName));
            }
            catch (Exception ex)
            {
                AppendLog(Strings.LogSaveFailed(ex.Message));
            }
        }

        // =====================================================================
        // Title bar
        // =====================================================================

        private void MinBtn_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void MaxBtn_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

        private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();

        private void OnStateChanged(object sender, EventArgs e) =>
            MaxBtn.Content = WindowState == WindowState.Maximized ? "" : "";
    }
}
