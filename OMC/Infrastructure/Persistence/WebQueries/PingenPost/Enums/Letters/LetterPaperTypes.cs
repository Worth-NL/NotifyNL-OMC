namespace WebQueries.PingenPost.Enums.Letters;

/// <summary>
/// Letter paper type. <see href="https://api.pingen.com/documentation#tag/letters.general/operation/letters.price-calculator">API Doc - Letters price calculator</see>
/// </summary>
public static class LetterPaperTypes
{
    /// <summary>
    /// Normal paper
    /// </summary>
    public const string Normal = "normal";

    /// <summary>
    /// QR paper
    /// </summary>
    public const string Qr = "qr";

    /// <summary>
    /// IS paper
    /// </summary>
    public const string Is = "is";

    /// <summary>
    /// ISR paper
    /// </summary>
    public const string Isr = "isr";

    /// <summary>
    /// ISR+ paper
    /// </summary>
    public const string IsrPlus = "isr+";

    /// <summary>
    /// Sepa AT paper
    /// </summary>
    public const string SepaAt = "sepa_at";

    /// <summary>
    /// Sepa DE paper
    /// </summary>
    public const string SepaDe = "sepa_de";
}
