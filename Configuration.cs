using Dalamud.Configuration;
using Dalamud.Plugin;
using System.Numerics;

namespace BladeProclamations;

public enum VerseTheme
{
    CrimsonEclipse,
    MoonlitElegance,
    VoidTestament,
}

[Serializable]
public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;
    public bool Enabled { get; set; } = true;
    public bool LocalChatEnabled { get; set; } = true;
    public bool OverlayEnabled { get; set; } = true;
    public bool OverlayLocked { get; set; } = false;
    public bool OnlyWhileSamurai { get; set; } = true;
    public float DisplaySeconds { get; set; } = 4.5f;
    public float OverlayScale { get; set; } = 1.15f;
    public float BackgroundOpacity { get; set; } = 0.82f;
    public VerseTheme Theme { get; set; } = VerseTheme.CrimsonEclipse;
    public Dictionary<uint, QuoteEntry> Quotes { get; set; } = QuoteCatalog.CreateDefaults();

    [NonSerialized]
    private IDalamudPluginInterface? pluginInterface;

    public void Initialize(IDalamudPluginInterface pi)
    {
        pluginInterface = pi;
        QuoteCatalog.MergeMissingDefaults(Quotes);
    }

    public void Save() => pluginInterface?.SavePluginConfig(this);
}

[Serializable]
public sealed class QuoteEntry
{
    public string ActionName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Verse { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public VerseTheme? ThemeOverride { get; set; }

    public QuoteEntry Clone() => new()
    {
        ActionName = ActionName,
        Title = Title,
        Verse = Verse,
        Enabled = Enabled,
        ThemeOverride = ThemeOverride,
    };
}
