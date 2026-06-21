using System.Windows;
using Topory.Models;
using Topory.Services;

// Disambiguate from System.Windows.Localization (pulled in via System.Windows).
using Localization = Topory.Services.Localization;

namespace Topory;

/// <summary>
/// Lists the windows Topory is keeping on top, with a button to unpin each (or
/// all), plus a menu mirroring the tray settings (language, theme, start with
/// Windows, about). Pinning happens via the global hotkey; this window manages
/// what's already pinned.
/// </summary>
public partial class ManagerWindow : Window
{
    private readonly WindowPinner _pinner;

    /// <summary>Raised when the user asks to pin/unpin the current window.</summary>
    public event Action? PinRequested;

    /// <summary>Raised when the user picks About from the menu.</summary>
    public event Action? AboutRequested;

    public ManagerWindow(WindowPinner pinner)
    {
        InitializeComponent();

        _pinner = pinner;
        PinnedList.ItemsSource = pinner.Pinned;
        pinner.Changed += UpdateEmptyState;

        UpdateEmptyState();
        RefreshMenuChecks();

        // Drop windows that have since closed whenever the user returns here.
        Activated += (_, _) =>
        {
            _pinner.Prune();
            RefreshMenuChecks();
        };
    }

    private void OnPin(object sender, RoutedEventArgs e) => PinRequested?.Invoke();

    private void OnUnpinAll(object sender, RoutedEventArgs e) => _pinner.UnpinAll();

    private void OnUnpin(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: PinnedWindow window })
            _pinner.Unpin(window);
    }

    private void UpdateEmptyState()
        => EmptyState.Visibility = _pinner.Pinned.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

    private void OnEnglish(object sender, RoutedEventArgs e)
    {
        Localization.Instance.Language = AppLanguage.English;
        RefreshMenuChecks();
    }

    private void OnTurkish(object sender, RoutedEventArgs e)
    {
        Localization.Instance.Language = AppLanguage.Turkish;
        RefreshMenuChecks();
    }

    private void OnThemeSystem(object sender, RoutedEventArgs e) => SetTheme(AppTheme.System);
    private void OnThemeDark(object sender, RoutedEventArgs e) => SetTheme(AppTheme.Dark);
    private void OnThemeLight(object sender, RoutedEventArgs e) => SetTheme(AppTheme.Light);

    private void SetTheme(AppTheme theme)
    {
        ThemeService.Apply(theme);
        RefreshMenuChecks();
    }

    private void OnToggleAutoStart(object sender, RoutedEventArgs e)
        => AutoStart.SetEnabled(AutoStartMenuItem.IsChecked);

    private void OnAbout(object sender, RoutedEventArgs e) => AboutRequested?.Invoke();

    private void RefreshMenuChecks()
    {
        EnglishMenuItem.IsChecked = Localization.Instance.Language == AppLanguage.English;
        TurkishMenuItem.IsChecked = Localization.Instance.Language == AppLanguage.Turkish;
        AutoStartMenuItem.IsChecked = AutoStart.IsEnabled();
        ThemeSystemItem.IsChecked = ThemeService.Theme == AppTheme.System;
        ThemeDarkItem.IsChecked = ThemeService.Theme == AppTheme.Dark;
        ThemeLightItem.IsChecked = ThemeService.Theme == AppTheme.Light;
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // Closing (X) hides to the tray; the app keeps running and is shut down
        // from the tray's Quit command.
        e.Cancel = true;
        Hide();

        base.OnClosing(e);
    }
}
