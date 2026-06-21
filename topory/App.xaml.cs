using System.Windows;
using topory.Services;

// Enabling WinForms (for the tray icon) pulls the System.Windows.Forms version
// of Application into scope too, so spell out the WPF one; also disambiguate from
// System.Windows.Localization.
using Application = System.Windows.Application;
using Localization = topory.Services.Localization;

namespace topory;

/// <summary>
/// Application entry point. Runs topory as a tray application: no window on
/// startup, lives in the system tray, exits only on "Quit".
///
/// The core flow: press Ctrl+Shift+T → the window you're using is pinned on top
/// of everything (press again to unpin). The manager window lists what's pinned.
/// </summary>
public partial class App : Application
{
    private Mutex? _singleInstanceMutex;
    private SettingsStore _settings = null!;
    private WindowPinner _pinner = null!;
    private HotkeyService _hotkey = null!;
    private TrayIcon _tray = null!;
    private ManagerWindow? _managerWindow;
    private AboutWindow? _aboutWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        // Only one topory should own the global hotkey at a time.
        _singleInstanceMutex = new Mutex(initiallyOwned: true,
            @"Local\topory.SingleInstance", out var isFirstInstance);
        if (!isFirstInstance)
        {
            Shutdown();
            return;
        }

        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        // Apply saved language + theme before any UI is built, then persist changes.
        _settings = new SettingsStore();
        Localization.Instance.Language = _settings.LoadLanguage();
        ThemeService.Apply(_settings.LoadTheme());
        Localization.Instance.LanguageChanged += SavePreferences;
        ThemeService.Changed += SavePreferences;

        _pinner = new WindowPinner();

        // Ctrl+Shift+T pins/unpins the focused window.
        _hotkey = new HotkeyService();
        _hotkey.Pressed += TogglePin;

        _tray = new TrayIcon();
        _tray.PinRequested += TogglePin;
        _tray.ManageRequested += ShowManager;
        _tray.AboutRequested += ShowAbout;
        _tray.QuitRequested += Shutdown;

        if (e.Args.Contains("--manage") || e.Args.Contains("--open"))
            ShowManager();
    }

    private void TogglePin()
    {
        var result = _pinner.ToggleForeground();
        if (!result.Acted)
            return;

        var label = result.Pinned ? Localization.Instance["Pinned"] : Localization.Instance["Unpinned"];
        _tray.ShowBalloon(label, result.Title);
    }

    private void SavePreferences()
        => _settings.Save(Localization.Instance.Language, ThemeService.Theme);

    /// <summary>Shows the pinned-windows manager, reusing the single window.</summary>
    private void ShowManager()
    {
        if (_managerWindow is null)
        {
            _managerWindow = new ManagerWindow(_pinner);
            _managerWindow.PinRequested += TogglePin;
            _managerWindow.AboutRequested += ShowAbout;
            _managerWindow.Closed += (_, _) => _managerWindow = null;
        }

        Surface(_managerWindow);
    }

    /// <summary>Shows the About window, reusing it if already open.</summary>
    private void ShowAbout()
    {
        if (_aboutWindow is not null)
        {
            _aboutWindow.Activate();
            return;
        }

        _aboutWindow = new AboutWindow();
        _aboutWindow.Closed += (_, _) => _aboutWindow = null;
        _aboutWindow.Show();
    }

    private static void Surface(Window window)
    {
        if (!window.IsVisible)
            window.Show();
        if (window.WindowState == WindowState.Minimized)
            window.WindowState = WindowState.Normal;
        window.Activate();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Restore every pinned window so nothing is left stuck on top after we go.
        _pinner?.UnpinAll();
        _tray?.Dispose();
        _hotkey?.Dispose();
        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }
}
