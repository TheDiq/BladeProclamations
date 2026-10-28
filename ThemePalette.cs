using System.Numerics;

namespace BladeProclamations;

public readonly record struct ThemePalette(
    Vector4 Background,
    Vector4 BackgroundInner,
    Vector4 Border,
    Vector4 Accent,
    Vector4 Title,
    Vector4 Text,
    string LeftOrnament,
    string RightOrnament)
{
    public static ThemePalette Get(VerseTheme theme) => theme switch
    {
        VerseTheme.MoonlitElegance => new(
            new(0.025f, 0.045f, 0.095f, 1f),
            new(0.07f, 0.11f, 0.20f, 1f),
            new(0.48f, 0.70f, 0.98f, 1f),
            new(0.77f, 0.88f, 1.00f, 1f),
            new(0.83f, 0.92f, 1.00f, 1f),
            new(0.93f, 0.96f, 1.00f, 1f),
            "《 月影 》",
            "《 静寂 》"),

        VerseTheme.VoidTestament => new(
            new(0.035f, 0.015f, 0.055f, 1f),
            new(0.10f, 0.035f, 0.15f, 1f),
            new(0.62f, 0.28f, 0.92f, 1f),
            new(0.93f, 0.22f, 0.73f, 1f),
            new(0.96f, 0.63f, 1.00f, 1f),
            new(0.95f, 0.89f, 1.00f, 1f),
            "† ABYSS †",
            "† TESTAMENT †"),

        _ => new(
            new(0.075f, 0.012f, 0.018f, 1f),
            new(0.17f, 0.025f, 0.035f, 1f),
            new(0.88f, 0.12f, 0.16f, 1f),
            new(1.00f, 0.45f, 0.18f, 1f),
            new(1.00f, 0.76f, 0.58f, 1f),
            new(1.00f, 0.92f, 0.88f, 1f),
            "《 BLOOD MOON 》",
            "《 FINAL VERSE 》"),
    };
}
