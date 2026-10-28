using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using System.Collections.Concurrent;
using System.Numerics;

namespace BladeProclamations;

public sealed unsafe class Plugin : IDalamudPlugin
{
    public string Name => "Blade Proclamations";

    private const uint SamuraiClassJobId = 34;
    private static readonly TimeSpan DuplicateWindow = TimeSpan.FromMilliseconds(650);

    private readonly IDalamudPluginInterface pluginInterface;
    private readonly ICommandManager commandManager;
    private readonly IClientState clientState;
    private readonly IChatGui chatGui;
    private readonly IFramework framework;
    private readonly IPluginLog log;
    private readonly Configuration config;
    private readonly VerseDisplay display = new();
    private readonly ConcurrentQueue<uint> pendingActions = new();
    private readonly Dictionary<uint, DateTime> lastSeen = new();
    private readonly WindowSystem windowSystem = new("BladeProclamations");
    private readonly OverlayWindow overlayWindow;
    private readonly ConfigWindow configWindow;
    private readonly Hook<ActionEffectHandler.Delegates.Receive> actionEffectHook;

    public Plugin(
        IDalamudPluginInterface pluginInterface,
        ICommandManager commandManager,
        IClientState clientState,
        IChatGui chatGui,
        IFramework framework,
        IGameInteropProvider interopProvider,
        IPluginLog log)
    {
        this.pluginInterface = pluginInterface;
        this.commandManager = commandManager;
        this.clientState = clientState;
        this.chatGui = chatGui;
        this.framework = framework;
        this.log = log;

        config = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        config.Initialize(pluginInterface);

        overlayWindow = new OverlayWindow(config, display);
        configWindow = new ConfigWindow(config, Preview, SaveConfig);
        windowSystem.AddWindow(overlayWindow);
        windowSystem.AddWindow(configWindow);

        commandManager.AddHandler("/bladeproclaim", new CommandInfo(OnCommand)
        {
            HelpMessage = "Open settings. Subcommands: test, lock, unlock, on, off",
        });
        commandManager.AddHandler("/bp", new CommandInfo(OnCommand)
        {
            HelpMessage = "Alias for /bladeproclaim",
            ShowInHelp = false,
        });

        pluginInterface.UiBuilder.Draw += DrawUi;
        pluginInterface.UiBuilder.OpenConfigUi += OpenConfig;
        framework.Update += OnFrameworkUpdate;

        actionEffectHook = interopProvider.HookFromAddress<ActionEffectHandler.Delegates.Receive>(
            ActionEffectHandler.MemberFunctionPointers.Receive,
            OnActionEffect);
        actionEffectHook.Enable();
    }

    public void Dispose()
    {
        actionEffectHook.Disable();
        actionEffectHook.Dispose();
        framework.Update -= OnFrameworkUpdate;
        pluginInterface.UiBuilder.Draw -= DrawUi;
        pluginInterface.UiBuilder.OpenConfigUi -= OpenConfig;
        commandManager.RemoveHandler("/bladeproclaim");
        commandManager.RemoveHandler("/bp");
        windowSystem.RemoveAllWindows();
    }

    private void DrawUi() => windowSystem.Draw();
    private void OpenConfig() => configWindow.IsOpen = true;

    private void OnCommand(string command, string arguments)
    {
        switch (arguments.Trim().ToLowerInvariant())
        {
            case "test":
                Preview(QuoteCatalog.OgiNamikiri);
                break;
            case "lock":
                config.OverlayLocked = true;
                SaveConfig();
                break;
            case "unlock":
                config.OverlayLocked = false;
                config.OverlayEnabled = true;
                SaveConfig();
                break;
            case "on":
                config.Enabled = true;
                SaveConfig();
                break;
            case "off":
                config.Enabled = false;
                SaveConfig();
                break;
            default:
                configWindow.IsOpen = !configWindow.IsOpen;
                break;
        }
    }

    private void SaveConfig() => config.Save();

    private void Preview(uint actionId)
    {
        if (config.Quotes.TryGetValue(actionId, out var quote))
            Present(quote, ignoreEnabled: true);
    }

    private void OnFrameworkUpdate(IFramework _)
    {
        while (pendingActions.TryDequeue(out var actionId))
        {
            if (!config.Enabled || !config.Quotes.TryGetValue(actionId, out var quote) || !quote.Enabled)
                continue;

            if (config.OnlyWhileSamurai && clientState.LocalPlayer?.ClassJob.RowId != SamuraiClassJobId)
                continue;

            Present(quote);
        }
    }

    private void Present(QuoteEntry quote, bool ignoreEnabled = false)
    {
        if (!ignoreEnabled && (!config.Enabled || !quote.Enabled))
            return;

        if (config.OverlayEnabled)
            display.Show(quote, config.Theme, config.DisplaySeconds);

        if (config.LocalChatEnabled)
        {
            var palette = ThemePalette.Get(quote.ThemeOverride ?? config.Theme);
            var builder = new SeStringBuilder()
                .AddUiForeground($"《 {quote.Title} 》", ChatColorFor(quote.ThemeOverride ?? config.Theme))
                .AddText($"  {quote.Verse}")
                .Build();

            chatGui.Print(new XivChatEntry
            {
                Type = XivChatType.Echo,
                Message = builder,
            });
        }
    }

    private static ushort ChatColorFor(VerseTheme theme) => theme switch
    {
        VerseTheme.MoonlitElegance => 35,
        VerseTheme.VoidTestament => 541,
        _ => 17,
    };

    private void OnActionEffect(
        uint casterEntityId,
        Character* caster,
        Vector3* targetPosition,
        ActionEffectHandler.Header* header,
        ActionEffectHandler.TargetEffects* effects,
        GameObjectId* targetEntityIds)
    {
        var actionId = header == null ? 0u : header->ActionId;

        // Preserve the game's processing first. This plugin only observes.
        actionEffectHook.Original(casterEntityId, caster, targetPosition, header, effects, targetEntityIds);

        try
        {
            if (actionId == 0 || clientState.LocalPlayer?.EntityId != casterEntityId)
                return;

            if (!config.Quotes.ContainsKey(actionId))
                return;

            var now = DateTime.UtcNow;
            if (lastSeen.TryGetValue(actionId, out var previous) && now - previous < DuplicateWindow)
                return;

            lastSeen[actionId] = now;
            pendingActions.Enqueue(actionId);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Failed to process a Samurai action effect.");
        }
    }
}
