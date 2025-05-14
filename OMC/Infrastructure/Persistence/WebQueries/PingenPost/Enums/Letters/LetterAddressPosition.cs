using System.Text.Json.Serialization;

// ReSharper disable InconsistentNaming

namespace WebQueries.PingenPost.Enums.Letters;

/// <summary>
/// Letter address position. <see href="https://api.pingen.com/documentation#tag/letters.general/operation/letters.show">API Doc - Letter details</see>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LetterAddressPosition
{
    /// <summary>
    /// Address position left
    /// </summary>
    left,

    /// <summary>
    /// Address position right
    /// </summary>
    right
}
