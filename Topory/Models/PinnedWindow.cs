namespace Topory.Models;

/// <summary>
/// A window Topory has pinned on top: its handle plus a title to show in the
/// manager list.
/// </summary>
public sealed class PinnedWindow
{
    public PinnedWindow(IntPtr handle, string title)
    {
        Handle = handle;
        Title = title;
    }

    public IntPtr Handle { get; }

    /// <summary>The window title captured when it was pinned.</summary>
    public string Title { get; }
}
