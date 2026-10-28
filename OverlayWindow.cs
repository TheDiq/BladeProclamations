using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System.Numerics;

namespace BladeProclamations;

public sealed class OverlayWindow : Window
{
    private readonly Configuration config;
    private readonly VerseDisplay display;

    public OverlayWindow(Configuration config, VerseDisplay display)
        : base("Blade Proclamations Overlay###BladeProclamationsOverlay",
            ImGuiWindowFlags.NoScrollbar |
            ImGuiWindowFlags.NoScrollWithMouse |
            ImGuiWindowFlags.NoCollapse |
            ImGuiWindowFlags.NoTitleBar)
    {
        this.config = config;
        this.display = display;
        IsOpen = true;
        RespectCloseHotkey = false;
        Size = new Vector2(760, 170);
        SizeCondition = ImGuiCond.FirstUseEver;
        Position = new Vector2(580, 160);
        PositionCondition = ImGuiCond.FirstUseEver;
        BgAlpha = 0f;
    }

    public override bool DrawConditions() =>
        IsOpen && config.Enabled && config.OverlayEnabled && (display.Active || !config.OverlayLocked);

    public override void PreDraw()
    {
        Flags = ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse |
                ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoTitleBar;

        if (config.OverlayLocked)
            Flags |= ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoInputs;
    }

    public override void Draw()
    {
        var pos = ImGui.GetWindowPos();
        var size = ImGui.GetWindowSize();
        var alpha = display.Active ? display.Alpha : 0.48f;
        var elapsed = display.Active ? display.ElapsedSeconds : 1f;
        var exitProgress = display.Active
            ? EaseInCubic(Math.Clamp((0.65f - display.RemainingSeconds) / 0.65f, 0f, 1f))
            : 0f;
        var palette = ThemePalette.Get(display.Active ? display.Theme : config.Theme);
        var draw = ImGui.GetWindowDrawList();

        var background = palette.Background;
        background.W = config.BackgroundOpacity * alpha;
        var inner = palette.BackgroundInner;
        inner.W = config.BackgroundOpacity * 0.72f * alpha;
        var border = palette.Border;
        border.W = alpha;
        var accent = palette.Accent;
        accent.W = alpha;

        draw.AddRectFilled(pos, pos + size, Color(background), 10f);
        draw.AddRectFilled(pos + new Vector2(7, 7), pos + size - new Vector2(7, 7), Color(inner), 8f);
        draw.AddRect(pos, pos + size, Color(border), 10f, ImDrawFlags.None, 2.2f);
        draw.AddRect(pos + new Vector2(7, 7), pos + size - new Vector2(7, 7), Color(accent), 8f, ImDrawFlags.None, 1f);

        var middleY = pos.Y + size.Y / 2f;
        var streakProgress = EaseOutCubic(Math.Clamp(elapsed / 0.32f, 0f, 1f));
        var streakLength = Math.Max(0f, (size.X / 2f - 105f) * streakProgress);
        draw.AddLine(
            new Vector2(pos.X + 18, middleY),
            new Vector2(pos.X + 18 + streakLength, middleY),
            Color(accent), 1.7f);
        draw.AddLine(
            new Vector2(pos.X + size.X - 18 - streakLength, middleY),
            new Vector2(pos.X + size.X - 18, middleY),
            Color(accent), 1.7f);

        // Fast diagonal blade-trails sell the initial "whoosh" without obscuring the verse.
        var trailAlpha = Math.Clamp(1f - elapsed / 0.46f, 0f, 1f) * alpha;
        var trail = palette.Accent;
        trail.W = trailAlpha;
        var trailTravel = size.X * EaseOutCubic(Math.Clamp(elapsed / 0.42f, 0f, 1f));
        for (var i = 0; i < 3; i++)
        {
            var x = pos.X - 160f + trailTravel - i * 58f;
            draw.AddLine(
                new Vector2(x, pos.Y + size.Y - 20f),
                new Vector2(x + 145f, pos.Y + 20f),
                Color(trail), 2.2f - i * 0.45f);
        }

        draw.AddCircleFilled(new Vector2(pos.X + 86, middleY), 3.5f, Color(border));
        draw.AddCircleFilled(new Vector2(pos.X + size.X - 86, middleY), 3.5f, Color(border));

        ImGui.SetWindowFontScale(config.OverlayScale);
        ImGui.SetCursorPosY(19);

        if (!display.Active)
        {
            CenteredColoredText("UNLOCKED — drag and resize this verse panel", palette.Title, alpha);
            ImGui.SetCursorPosY(70);
            CenteredColoredText("Use /bladeproclaim to test, edit, style, and lock it.", palette.Text, alpha);
            ImGui.SetWindowFontScale(1f);
            return;
        }

        var titleIn = EaseOutBack(Math.Clamp((elapsed - 0.03f) / 0.30f, 0f, 1f));
        var actionIn = EaseOutBack(Math.Clamp((elapsed - 0.10f) / 0.30f, 0f, 1f));
        var verseIn = EaseOutCubic(Math.Clamp((elapsed - 0.18f) / 0.38f, 0f, 1f));

        // The three layers arrive from alternating directions and leave together to the right.
        var titleOffset = -190f * (1f - titleIn) + 95f * exitProgress;
        var actionOffset = 150f * (1f - actionIn) + 115f * exitProgress;
        var verseOffset = -115f * (1f - verseIn) + 135f * exitProgress;

        CenteredColoredText(
            $"{palette.LeftOrnament}   {display.Title}   {palette.RightOrnament}",
            palette.Title,
            alpha * Math.Clamp(titleIn, 0f, 1f),
            titleOffset);
        ImGui.SetCursorPosY(59);
        CenteredColoredText(
            display.ActionName.ToUpperInvariant(),
            palette.Accent,
            alpha * Math.Clamp(actionIn, 0f, 1f),
            actionOffset);

        ImGui.SetCursorPos(new Vector2(68 + verseOffset, 91));
        ImGui.PushTextWrapPos(size.X - 68 + verseOffset);
        var text = palette.Text;
        text.W = alpha * verseIn;
        ImGui.TextColored(text, $"“{display.Verse}”");
        ImGui.PopTextWrapPos();
        ImGui.SetWindowFontScale(1f);
    }

    private static void CenteredColoredText(string text, Vector4 color, float alpha, float horizontalOffset = 0f)
    {
        color.W *= alpha;
        var width = ImGui.CalcTextSize(text).X;
        ImGui.SetCursorPosX(Math.Max(-width, (ImGui.GetWindowSize().X - width) / 2f + horizontalOffset));
        ImGui.TextColored(color, text);
    }

    private static uint Color(Vector4 color) => ImGui.ColorConvertFloat4ToU32(color);

    private static float EaseOutCubic(float t) => 1f - MathF.Pow(1f - t, 3f);
    private static float EaseInCubic(float t) => t * t * t;

    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * MathF.Pow(t - 1f, 3f) + c1 * MathF.Pow(t - 1f, 2f);
    }
}
