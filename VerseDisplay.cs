namespace BladeProclamations;

public sealed class VerseDisplay
{
    public string ActionName { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Verse { get; private set; } = string.Empty;
    public VerseTheme Theme { get; private set; }
    public DateTime StartedAtUtc { get; private set; }
    public float DurationSeconds { get; private set; }

    public bool Active => RemainingSeconds > 0;
    public float RemainingSeconds => DurationSeconds - (float)(DateTime.UtcNow - StartedAtUtc).TotalSeconds;
    public float ElapsedSeconds => Math.Max(0f, (float)(DateTime.UtcNow - StartedAtUtc).TotalSeconds);

    public void Show(QuoteEntry quote, VerseTheme theme, float durationSeconds)
    {
        ActionName = quote.ActionName;
        Title = quote.Title;
        Verse = quote.Verse;
        Theme = quote.ThemeOverride ?? theme;
        DurationSeconds = Math.Max(1f, durationSeconds);
        StartedAtUtc = DateTime.UtcNow;
    }

    public float Alpha
    {
        get
        {
            if (!Active)
                return 0f;

            var elapsed = (float)(DateTime.UtcNow - StartedAtUtc).TotalSeconds;
            var fadeIn = Math.Clamp(elapsed / 0.18f, 0f, 1f);
            var fadeOut = Math.Clamp(RemainingSeconds / 0.65f, 0f, 1f);
            return Math.Min(fadeIn, fadeOut);
        }
    }
}
