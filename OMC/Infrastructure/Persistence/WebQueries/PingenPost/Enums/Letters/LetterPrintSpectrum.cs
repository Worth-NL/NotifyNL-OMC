using System.Text.Json.Serialization;
// ReSharper disable InconsistentNaming

namespace PingenApiNet.Abstractions.Enums.Letters;

/// <summary>
/// Letter print spectrum. <see href="https://api.pingen.com/documentation#tag/letters.general/operation/letters.show">API Doc - Letter details</see>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LetterPrintSpectrum
{
    /// <summary>
    /// Print spectrum grayscale
    /// </summary>
    grayscale,

    /// <summary>
    /// Print spectrum color
    /// </summary>
    color
}
