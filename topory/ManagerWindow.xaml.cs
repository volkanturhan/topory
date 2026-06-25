using System.Windows;
using topory.Models;
using topory.Services;

// Disambiguate from System.Windows.Localization (pulled in via System.Windows).
using Localization = topory.Services.Localization;

namespace topory;

/// <summary>
/// Lists the windows topory is keeping on top, with a button to unpin each (or
/// all). Settings (language, theme, start with Windows, about) live in the tray
/// menu. Pinning happens via the global hotkey; this window manages what's
/// already pinned.
/// </summary>
public partial class ManagerWindow : Window
{
    private readonly WindowPinner _pinner;

    /// <summary>Raised when the user asks to pin/unpin the current window.</summary>
    public event Action? PinRequested;

    public ManagerWindow(WindowPinner pinner)
    {
        InitializeComponent();

        _pinner = pinner;
        PinnedList.ItemsSource = pinner.Pinned;
        pinner.Changed += UpdateEmptyState;

        UpdateEmptyState();

        // Drop windows that have since closed whenever the user returns here.
        Activated += (_, _) => _pinner.Prune();
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

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // Closing (X) hides to the tray; the app keeps running and is shut down
        // from the tray's Quit command.
        e.Cancel = true;
        Hide();

        base.OnClosing(e);
    }
}
