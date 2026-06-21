using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Text;
using Topory.Models;

namespace Topory.Services;

/// <summary>
/// Pins and unpins other applications' windows so they stay above everything
/// else, by toggling the <c>WS_EX_TOPMOST</c> style via <c>SetWindowPos</c>.
/// Keeps an observable list of what is currently pinned for the manager window.
/// </summary>
public sealed class WindowPinner
{
    private const int GWL_EXSTYLE = -20;
    private const long WS_EX_TOPMOST = 0x00000008;

    private static readonly IntPtr HWND_TOPMOST = new(-1);
    private static readonly IntPtr HWND_NOTOPMOST = new(-2);

    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOACTIVATE = 0x0010;

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int x, int y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("user32.dll")]
    private static extern IntPtr GetShellWindow();

    private readonly ObservableCollection<PinnedWindow> _pinned = new();

    public WindowPinner()
    {
        Pinned = new ReadOnlyObservableCollection<PinnedWindow>(_pinned);
    }

    /// <summary>The currently pinned windows, exposed read-only for binding.</summary>
    public ReadOnlyObservableCollection<PinnedWindow> Pinned { get; }

    /// <summary>Raised after the pinned set changes.</summary>
    public event Action? Changed;

    /// <summary>The outcome of toggling a window.</summary>
    public readonly record struct ToggleResult(bool Acted, bool Pinned, string Title);

    /// <summary>
    /// Pins the foreground window if it isn't already on top, or unpins it if it
    /// is. Ignores our own windows and the desktop/shell.
    /// </summary>
    public ToggleResult ToggleForeground()
    {
        var hwnd = GetForegroundWindow();
        if (hwnd == IntPtr.Zero || !IsWindow(hwnd) || IsOwnOrShell(hwnd))
            return new ToggleResult(false, false, string.Empty);

        var title = GetTitle(hwnd);

        if (IsTopmost(hwnd))
        {
            SetTopmost(hwnd, false);
            RemoveHandle(hwnd);
            Changed?.Invoke();
            return new ToggleResult(true, false, title);
        }

        SetTopmost(hwnd, true);
        if (!_pinned.Any(p => p.Handle == hwnd))
            _pinned.Add(new PinnedWindow(hwnd, title));
        Changed?.Invoke();
        return new ToggleResult(true, true, title);
    }

    /// <summary>Unpins a specific window (from the manager list).</summary>
    public void Unpin(PinnedWindow window)
    {
        if (IsWindow(window.Handle))
            SetTopmost(window.Handle, false);
        RemoveHandle(window.Handle);
        Changed?.Invoke();
    }

    /// <summary>Unpins every window Topory has pinned.</summary>
    public void UnpinAll()
    {
        foreach (var window in _pinned.ToArray())
        {
            if (IsWindow(window.Handle))
                SetTopmost(window.Handle, false);
        }

        _pinned.Clear();
        Changed?.Invoke();
    }

    /// <summary>
    /// Drops entries whose window has closed or is no longer topmost (e.g. the
    /// app un-pinned itself), so the list reflects reality.
    /// </summary>
    public void Prune()
    {
        var stale = _pinned.Where(p => !IsWindow(p.Handle) || !IsTopmost(p.Handle)).ToArray();
        if (stale.Length == 0)
            return;

        foreach (var window in stale)
            _pinned.Remove(window);
        Changed?.Invoke();
    }

    private void RemoveHandle(IntPtr hwnd)
    {
        var existing = _pinned.FirstOrDefault(p => p.Handle == hwnd);
        if (existing is not null)
            _pinned.Remove(existing);
    }

    private static bool IsTopmost(IntPtr hwnd)
        => (GetWindowLongPtr(hwnd, GWL_EXSTYLE).ToInt64() & WS_EX_TOPMOST) != 0;

    private static void SetTopmost(IntPtr hwnd, bool on)
        => SetWindowPos(hwnd, on ? HWND_TOPMOST : HWND_NOTOPMOST, 0, 0, 0, 0,
            SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);

    private static bool IsOwnOrShell(IntPtr hwnd)
    {
        if (hwnd == GetShellWindow())
            return true;

        GetWindowThreadProcessId(hwnd, out var pid);
        return pid == (uint)Environment.ProcessId;
    }

    private static string GetTitle(IntPtr hwnd)
    {
        var buffer = new StringBuilder(256);
        GetWindowText(hwnd, buffer, buffer.Capacity);
        var title = buffer.ToString();
        return string.IsNullOrWhiteSpace(title) ? "(untitled window)" : title;
    }
}
