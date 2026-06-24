using System.ComponentModel;

namespace topory.Services;

public enum AppLanguage
{
    English,
    Turkish,
}

/// <summary>
/// The app's tiny translation table and current-language state.
///
/// UI elements bind to the string indexer (e.g. <c>[ManagerTitle]</c>) against
/// the shared <see cref="Instance"/>. When <see cref="Language"/> changes we
/// raise the special "Item[]" property change so every bound string re-reads
/// itself, giving a live language switch without rebuilding the UI. Non-WPF
/// consumers (the tray menu) can instead listen to <see cref="LanguageChanged"/>.
/// </summary>
public sealed class Localization : INotifyPropertyChanged
{
    public static Localization Instance { get; } = new();

    private AppLanguage _language = AppLanguage.English;

    private static readonly Dictionary<string, string> English = new()
    {
        ["TrayPin"] = "Pin current window",
        ["TrayManage"] = "Pinned windows",
        ["TrayAutostart"] = "Start with Windows",
        ["TrayLanguage"] = "Language",
        ["TrayTheme"] = "Theme",
        ["ThemeSystem"] = "System",
        ["ThemeDark"] = "Dark",
        ["ThemeLight"] = "Light",
        ["TrayUpdate"] = "Update to v{0}",
        ["TrayCheckUpdate"] = "Check for updates",
        ["UpdateBalloonTitle"] = "topory update",
        ["UpdateBalloonText"] = "A new version is available. Click to install.",
        ["UpdateFailed"] = "Could not download the update. Please try again later.",
        ["UpToDate"] = "topory is up to date.",
        ["TrayAbout"] = "About",
        ["TrayQuit"] = "Quit",
        ["ManagerTitle"] = "topory — Pinned windows",
        ["Empty"] = "No pinned windows — press Ctrl + Shift + T over any window to keep it on top",
        ["Unpin"] = "Unpin",
        ["UnpinAll"] = "Unpin all",
        ["Hint"] = "Press Ctrl + Shift + T to pin or unpin the window you're using.",
        ["Pinned"] = "Pinned on top",
        ["Unpinned"] = "Unpinned",
        ["AboutDescription"] = "A lightweight \"always on top\" manager.",
        ["AboutVersion"] = "Version",
        ["AboutClose"] = "Close",
    };

    private static readonly Dictionary<string, string> Turkish = new()
    {
        ["TrayPin"] = "Geçerli pencereyi sabitle",
        ["TrayManage"] = "Sabitlenen pencereler",
        ["TrayAutostart"] = "Windows ile başlat",
        ["TrayLanguage"] = "Dil",
        ["TrayTheme"] = "Tema",
        ["ThemeSystem"] = "Sistem",
        ["ThemeDark"] = "Koyu",
        ["ThemeLight"] = "Açık",
        ["TrayUpdate"] = "v{0} sürümüne güncelle",
        ["TrayCheckUpdate"] = "Güncellemeleri denetle",
        ["UpdateBalloonTitle"] = "topory güncellemesi",
        ["UpdateBalloonText"] = "Yeni sürüm çıktı. Kurmak için tıklayın.",
        ["UpdateFailed"] = "Güncelleme indirilemedi. Lütfen daha sonra tekrar deneyin.",
        ["UpToDate"] = "topory güncel.",
        ["TrayAbout"] = "Hakkında",
        ["TrayQuit"] = "Çıkış",
        ["ManagerTitle"] = "topory — Sabitlenen pencereler",
        ["Empty"] = "Sabitlenen pencere yok — üstte tutmak için bir pencerenin üzerindeyken Ctrl + Shift + T",
        ["Unpin"] = "Sabitlemeyi kaldır",
        ["UnpinAll"] = "Tümünü kaldır",
        ["Hint"] = "Kullandığın pencereyi sabitlemek/kaldırmak için Ctrl + Shift + T.",
        ["Pinned"] = "Üstte sabitlendi",
        ["Unpinned"] = "Sabitleme kaldırıldı",
        ["AboutDescription"] = "Hafif bir \"her zaman üstte\" yöneticisi.",
        ["AboutVersion"] = "Sürüm",
        ["AboutClose"] = "Kapat",
    };

    /// <summary>The active language. Changing it refreshes all bound strings.</summary>
    public AppLanguage Language
    {
        get => _language;
        set
        {
            if (_language == value)
                return;

            _language = value;

            // "Item[]" tells WPF that every indexer binding should re-evaluate.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Language)));
            LanguageChanged?.Invoke();
        }
    }

    /// <summary>The translation for <paramref name="key"/> in the current language.</summary>
    public string this[string key]
    {
        get
        {
            var table = _language == AppLanguage.Turkish ? Turkish : English;
            return table.TryGetValue(key, out var value) ? value : key;
        }
    }

    /// <summary>Raised after the language changes (for non-binding consumers).</summary>
    public event Action? LanguageChanged;

    public event PropertyChangedEventHandler? PropertyChanged;
}
