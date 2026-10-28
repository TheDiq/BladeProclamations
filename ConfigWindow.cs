using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System.Numerics;

namespace BladeProclamations;

public sealed class ConfigWindow : Window
{
    private readonly Configuration config;
    private readonly Action<uint> preview;
    private readonly Action save;

    public ConfigWindow(Configuration config, Action<uint> preview, Action save)
        : base("Blade Proclamations — Configuration###BladeProclamationsConfig")
    {
        this.config = config;
        this.preview = preview;
        this.save = save;
        Size = new Vector2(760, 720);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        var changed = false;

        var enabled = config.Enabled;
        if (ImGui.Checkbox("Enable Blade Proclamations", ref enabled))
        {
            config.Enabled = enabled;
            changed = true;
        }

        var localChat = config.LocalChatEnabled;
        if (ImGui.Checkbox("Private local-chat verses", ref localChat))
        {
            config.LocalChatEnabled = localChat;
            changed = true;
        }

        var overlayEnabled = config.OverlayEnabled;
        if (ImGui.Checkbox("Movable on-screen verse", ref overlayEnabled))
        {
            config.OverlayEnabled = overlayEnabled;
            changed = true;
        }

        var samuraiOnly = config.OnlyWhileSamurai;
        if (ImGui.Checkbox("Only activate while playing Samurai", ref samuraiOnly))
        {
            config.OnlyWhileSamurai = samuraiOnly;
            changed = true;
        }

        ImGui.Separator();
        ImGui.TextUnformatted("Overlay placement");
        ImGui.SameLine();
        if (ImGui.Button(config.OverlayLocked ? "Unlock overlay" : "Lock overlay"))
        {
            config.OverlayLocked = !config.OverlayLocked;
            changed = true;
        }

        ImGui.SameLine();
        ImGui.TextDisabled(config.OverlayLocked
            ? "Locked: click-through mode"
            : "Unlocked: drag or resize the visible panel");

        var duration = config.DisplaySeconds;
        if (ImGui.SliderFloat("Display duration", ref duration, 1.5f, 10f, "%.1f seconds"))
        {
            config.DisplaySeconds = duration;
            changed = true;
        }

        var scale = config.OverlayScale;
        if (ImGui.SliderFloat("Text scale", ref scale, 0.8f, 1.8f, "%.2fx"))
        {
            config.OverlayScale = scale;
            changed = true;
        }

        var opacity = config.BackgroundOpacity;
        if (ImGui.SliderFloat("Background opacity", ref opacity, 0.2f, 1f, "%.2f"))
        {
            config.BackgroundOpacity = opacity;
            changed = true;
        }

        var theme = (int)config.Theme;
        var themeNames = new[] { "Crimson Eclipse", "Moonlit Elegance", "Void Testament" };
        if (ImGui.Combo("Default visual theme", ref theme, themeNames, themeNames.Length))
        {
            config.Theme = (VerseTheme)theme;
            changed = true;
        }

        ImGui.Separator();
        ImGui.TextWrapped("Every trigger is client-local. It does not press abilities, alter your hotbar, or send anything to party chat. The verse appears only after the game processes your action.");
        ImGui.Spacing();

        if (ImGui.BeginTabBar("QuoteTabs"))
        {
            foreach (var pair in config.Quotes.OrderBy(q => q.Value.ActionName))
            {
                var quote = pair.Value;
                if (!ImGui.BeginTabItem(quote.ActionName))
                    continue;

                var quoteEnabled = quote.Enabled;
                if (ImGui.Checkbox($"Enabled##{pair.Key}", ref quoteEnabled))
                {
                    quote.Enabled = quoteEnabled;
                    changed = true;
                }

                var title = quote.Title;
                if (ImGui.InputText($"Display title##{pair.Key}", ref title, 100))
                {
                    quote.Title = title;
                    changed = true;
                }

                var verse = quote.Verse;
                if (ImGui.InputTextMultiline($"Verse##{pair.Key}", ref verse, 500, new Vector2(-1, 115)))
                {
                    quote.Verse = verse;
                    changed = true;
                }

                var overrideIndex = quote.ThemeOverride is null ? 0 : (int)quote.ThemeOverride.Value + 1;
                var overrides = new[] { "Use default", "Crimson Eclipse", "Moonlit Elegance", "Void Testament" };
                if (ImGui.Combo($"Theme##{pair.Key}", ref overrideIndex, overrides, overrides.Length))
                {
                    quote.ThemeOverride = overrideIndex == 0 ? null : (VerseTheme)(overrideIndex - 1);
                    changed = true;
                }

                if (ImGui.Button($"Preview {quote.ActionName}##{pair.Key}"))
                    preview(pair.Key);

                ImGui.SameLine();
                if (ImGui.Button($"Restore default##{pair.Key}"))
                {
                    quote.CopyFrom(QuoteCatalog.CreateDefaults()[pair.Key]);
                    changed = true;
                }

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        if (changed)
            save();
    }
}

internal static class QuoteEntryExtensions
{
    public static void CopyFrom(this QuoteEntry target, QuoteEntry source)
    {
        target.ActionName = source.ActionName;
        target.Title = source.Title;
        target.Verse = source.Verse;
        target.Enabled = source.Enabled;
        target.ThemeOverride = source.ThemeOverride;
    }
}
