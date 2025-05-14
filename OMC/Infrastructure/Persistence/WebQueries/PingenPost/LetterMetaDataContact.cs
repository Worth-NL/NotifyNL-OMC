using System.Text.Json.Serialization;

namespace WebQueries.PingenPost;

/// <summary>
/// Contact address for <see cref="LetterMetaData.Recipient"/> and  <see cref="LetterMetaData.Sender"/>
/// </summary>
public sealed record LetterMetaDataContact
{
    /// <summary>
    /// Name
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Street
    /// </summary>
    [JsonPropertyName("street")]
    public required string Street { get; init; }

    /// <summary>
    /// Number
    /// </summary>
    [JsonPropertyName("number")]
    public required string Number { get; init; }

    /// <summary>
    /// Zip code
    /// </summary>
    [JsonPropertyName("zip")]
    public required string Zip { get; init; }

    /// <summary>
    /// City
    /// </summary>
    [JsonPropertyName("city")]
    public required string City { get; init; }

    /// <summary>
    /// Country
    /// </summary>
    [JsonPropertyName("country")]
    public required string Country { get; init; }
}
