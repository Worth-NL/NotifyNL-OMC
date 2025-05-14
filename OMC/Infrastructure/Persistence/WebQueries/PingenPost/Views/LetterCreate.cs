using System.Text.Json.Serialization;
using PingenApiNet.Abstractions.Enums.Letters;
using WebQueries.PingenPost.Enums.Letters;

namespace WebQueries.PingenPost.Views;

/// <summary>
/// Letter create object to send to the API
/// </summary>
public sealed record LetterCreate
{
    /// <summary>
    /// Filename [ 5 .. 255 ] characters
    /// </summary>
    [JsonPropertyName("file_original_name")]
    public required string FileOriginalName { get; init; }

    /// <summary>
    /// File URL [ 1 .. 1000 ] characters
    /// </summary>
    [JsonPropertyName("file_url")]
    public required string? FileUrl { get; init; }

    /// <summary>
    /// File URL signature [ 1 .. 60 ] characters
    /// </summary>
    [JsonPropertyName("file_url_signature")]
    public required string? FileUrlSignature { get; init; }

    /// <summary>
    /// Address position
    /// </summary>
    [JsonPropertyName("address_position")]
    public required LetterAddressPosition AddressPosition { get; init; }

    /// <summary>
    /// Auto send
    /// </summary>
    [JsonPropertyName("auto_send")]
    public required bool AutoSend { get; init; }

    /// <summary>
    /// Delivery product. (Should be any of <see cref="LetterCreateDeliveryProduct"/>)
    /// NOTE: A more specific product can be used later at Letters/Send endpoint
    /// </summary>
    [JsonPropertyName("delivery_product")]
    public required string DeliveryProduct { get; init; }

    /// <summary>
    /// Print mode
    /// </summary>
    [JsonPropertyName("print_mode")]
    public required LetterPrintMode PrintMode { get; init; }

    /// <summary>
    /// Print spectrum
    /// </summary>
    [JsonPropertyName("print_spectrum")]
    public required LetterPrintSpectrum PrintSpectrum { get; init; }

    /// <summary>
    /// Meta data
    /// <br/> NOTE: This must only be set when <see cref="LetterSendDeliveryProduct.PostAgRegistered"/> or <see cref="LetterSendDeliveryProduct.PostAgAPlus"/> product used. Otherwise the API can fail at address validation when Zip code has more than 4 characters.
    /// </summary>
    [JsonPropertyName("meta_data")]
    public LetterMetaData? MetaData { get; init; }
}
