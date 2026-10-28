namespace BladeProclamations;

public static class QuoteCatalog
{
    // PvE action row IDs current for Dawntrail 7.5 / Dalamud API 15.
    public const uint Higanbana = 7489;
    public const uint MeikyoShisui = 7499;
    public const uint HissatsuGuren = 7496;
    public const uint HissatsuSenei = 16481;
    public const uint Ikishoten = 16482;
    public const uint Shoha = 16487;
    public const uint OgiNamikiri = 25781;
    public const uint KaeshiNamikiri = 25782;
    public const uint Tengentsu = 36962;
    public const uint TendoSetsugekka = 41454;
    public const uint TendoKaeshiSetsugekka = 41455;
    public const uint Zanshin = 41577;

    public static Dictionary<uint, QuoteEntry> CreateDefaults() => new()
    {
        [MeikyoShisui] = Entry(
            "Meikyo Shisui",
            "RESTRICTION ART — THREEFOLD SEAL",
            "Technique is merely a chain placed upon power. I hereby cast it aside."),

        [Ikishoten] = Entry(
            "Ikishoten",
            "BLOOD OF THE FORGOTTEN WAR",
            "My blood remembers a war this age has forgotten. Now awaken—and bear witness."),

        [Higanbana] = Entry(
            "Higanbana",
            "FLOWER OF THE AFTERLIFE",
            "Bloom within the wound my blade has opened. Do not fear the pain; it is only death taking root."),

        [TendoSetsugekka] = Entry(
            "Tendo Setsugekka",
            "HEAVENLY SNOW • MOON • FLOWER",
            "Three deaths beneath one silent heaven. When snow, moon, and flower become one—the firmament is divided."),

        [TendoKaeshiSetsugekka] = Entry(
            "Tendo Kaeshi Setsugekka",
            "REFLECTION OF THE SILENT HEAVEN",
            "Fool… did you mistake the first cut for the end? A reflection must repeat the death it witnessed."),

        [HissatsuSenei] = Entry(
            "Hissatsu: Senei",
            "SECRET SWORD — SENEI",
            "Your defeat occurred three heartbeats ago. Your body simply has not realized it."),

        [HissatsuGuren] = Entry(
            "Hissatsu: Guren",
            "SCARLET DRAGON CREMATION",
            "A path does not exist before my blade. Scarlet dragon—consume everything and carve one."),

        [OgiNamikiri] = Entry(
            "Ogi Namikiri",
            "FORBIDDEN HORIZON — NAMIKIRI",
            "O blackened moon, witness the stroke that divides heaven from earth."),

        [KaeshiNamikiri] = Entry(
            "Kaeshi: Namikiri",
            "SECOND HORIZON — WORLD'S END",
            "One horizon was insufficient. I shall sever that which lies beyond existence itself."),

        [Shoha] = Entry(
            "Shoha",
            "THOUSAND-BATTLE REQUIEM",
            "Every victory, every wound, every regret—a thousand silent battles converge within this single moment."),

        [Zanshin] = Entry(
            "Zanshin",
            "THE INTENTION THAT REMAINS",
            "The blade has returned to its sheath… but the killing intent remains."),

        [Tengentsu] = Entry(
            "Tengentsu",
            "MOONLIT AEGIS",
            "The moon cannot be wounded by those condemned to crawl beneath it."),
    };

    public static void MergeMissingDefaults(Dictionary<uint, QuoteEntry> target)
    {
        foreach (var pair in CreateDefaults())
            target.TryAdd(pair.Key, pair.Value);
    }

    private static QuoteEntry Entry(string actionName, string title, string verse) => new()
    {
        ActionName = actionName,
        Title = title,
        Verse = verse,
        Enabled = true,
    };
}
