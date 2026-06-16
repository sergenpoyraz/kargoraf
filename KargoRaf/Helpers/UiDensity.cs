namespace KargoRaf.Helpers;

public enum UiDensity
{
    Normal,
    Compact,
    Dense
}

public sealed class UiDensityProfile
{
    public required double CardWidth { get; init; }
    public required double CardSpacing { get; init; }
    public required double CardPadding { get; init; }
    public required double SectionTitleFontSize { get; init; }
    public required double SectionBadgeSize { get; init; }
    public required double SectionCountFontSize { get; init; }
    public required double CompactAddButtonSize { get; init; }
    public required double PackageNameFontSize { get; init; }
    public required double PackageMetaFontSize { get; init; }
    public required double DeliverButtonHeight { get; init; }
    public required double DeliverButtonWidth { get; init; }
    public required double RowActionButtonSize { get; init; }
    public required double DeliverButtonFontSize { get; init; }
    public required double EmptyStateFontSize { get; init; }
    public required string Label { get; init; }
    public required string Hint { get; init; }
}

public static class UiDensityCatalog
{
    public const string SettingsKey = "card_density";
    public const string AutoScrollSettingsKey = "section_auto_scroll";

    public static UiDensity Parse(string? value) =>
        value?.ToLowerInvariant() switch
        {
            "normal" => UiDensity.Normal,
            "dense" or "dar" => UiDensity.Dense,
            _ => UiDensity.Compact
        };

    public static bool ParseAutoScroll(string? value, bool defaultValue = true) =>
        value?.ToLowerInvariant() switch
        {
            "false" or "0" or "hayir" or "hayır" => false,
            "true" or "1" or "evet" => true,
            _ => defaultValue
        };

    public static string ToAutoScrollStorage(bool enabled) => enabled ? "true" : "false";

    public static string ToStorage(UiDensity density) => density switch
    {
        UiDensity.Normal => "normal",
        UiDensity.Dense => "dense",
        _ => "compact"
    };

    public static UiDensityProfile GetProfile(UiDensity density) => density switch
    {
        UiDensity.Normal => Normal,
        UiDensity.Dense => Dense,
        _ => Compact
    };

    public static UiDensityProfile Normal { get; } = new()
    {
        CardWidth = 280,
        CardSpacing = 10,
        CardPadding = 12,
        SectionTitleFontSize = 15,
        SectionBadgeSize = 32,
        SectionCountFontSize = 12,
        CompactAddButtonSize = 34,
        PackageNameFontSize = 15,
        PackageMetaFontSize = 11,
        DeliverButtonHeight = 40,
        DeliverButtonWidth = 100,
        RowActionButtonSize = 36,
        DeliverButtonFontSize = 13,
        EmptyStateFontSize = 12,
        Label = "Normal",
        Hint = "Geniş kartlar — az bölüm, büyük ekran"
    };

    public static UiDensityProfile Compact { get; } = new()
    {
        CardWidth = 240,
        CardSpacing = 8,
        CardPadding = 10,
        SectionTitleFontSize = 14,
        SectionBadgeSize = 28,
        SectionCountFontSize = 11,
        CompactAddButtonSize = 30,
        PackageNameFontSize = 14,
        PackageMetaFontSize = 10,
        DeliverButtonHeight = 36,
        DeliverButtonWidth = 88,
        RowActionButtonSize = 32,
        DeliverButtonFontSize = 12,
        EmptyStateFontSize = 11,
        Label = "Sıkışık",
        Hint = "Önerilen — çoğu bakkal ekranı için dengeli"
    };

    public static UiDensityProfile Dense { get; } = new()
    {
        CardWidth = 200,
        CardSpacing = 6,
        CardPadding = 8,
        SectionTitleFontSize = 13,
        SectionBadgeSize = 24,
        SectionCountFontSize = 10,
        CompactAddButtonSize = 28,
        PackageNameFontSize = 13,
        PackageMetaFontSize = 9,
        DeliverButtonHeight = 34,
        DeliverButtonWidth = 80,
        RowActionButtonSize = 30,
        DeliverButtonFontSize = 11,
        EmptyStateFontSize = 10,
        Label = "Dar",
        Hint = "Çok bölüm veya düşük çözünürlük — daha fazla kart görünür"
    };

    public static IReadOnlyList<(UiDensity Density, UiDensityProfile Profile)> AllOptions { get; } =
    [
        (UiDensity.Normal, Normal),
        (UiDensity.Compact, Compact),
        (UiDensity.Dense, Dense)
    ];
}
