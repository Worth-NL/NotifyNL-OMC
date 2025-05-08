using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PingenApiNet.Abstractions.Enums.Letters;
using PingenApiNet.Abstractions.Models.Letters.Embedded;
using PingenApiNet.Abstractions.Models.Letters.Views;

public class PingenService
{
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private string _accessToken;

    public PingenService(HttpClient httpClient, string clientId, string clientSecret)
    {
        _httpClient = httpClient;
        _clientId = clientId;
        _clientSecret = clientSecret;
    }

    // Step 1: Authenticate and get access token
    public async Task AuthenticateAsync()
    {
        var requestBody = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", _clientId),
            new KeyValuePair<string, string>("client_secret", _clientSecret)
        });

        var response = await _httpClient.PostAsync("https://identity.pingen.com/auth/access-tokens", requestBody);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);

        _accessToken = tokenResponse.AccessToken;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
    }

    // Step 2: Request file upload URL
    public async Task<FileUploadResponse> RequestFileUploadUrlAsync()
    {
        var response = await _httpClient.GetAsync("https://api.pingen.com/file-upload");
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<FileUploadResponse>(responseContent);
    }

    // Step 3: Upload file to the provided URL
    public async Task<HttpResponseMessage> UploadFileAsync(string uploadUrl, string filePath)
    {
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var content = new StreamContent(fileStream);
        using var httpClient = new HttpClient();
        var response = await httpClient.PutAsync(uploadUrl, content);
        response.EnsureSuccessStatusCode();
        return response;
    }

    // Step 4: Submit the letter
    public async Task SubmitLetterAsync(string organisationId, LetterMetaData letterMetaData, string fileUrl,
        string fileSignature, string fileName)
    {
        var payload = new
        {
            data = new 
            {
                type = "letters",
                attributes = new LetterCreate
                {
                    FileOriginalName = fileName,
                    FileUrl = fileUrl,
                    FileUrlSignature = fileSignature,
                    AddressPosition = LetterAddressPosition.left,
                    AutoSend = false,
                    DeliveryProduct = LetterCreateDeliveryProduct.Cheap,
                    PrintMode = LetterPrintMode.simplex,
                    PrintSpectrum = LetterPrintSpectrum.grayscale,
                    MetaData = letterMetaData
                }
            }
        };


        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

        var response = await _httpClient.PostAsync($"https://api.pingen.com/organisations/{organisationId}/letters", content);
        var errorContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine(errorContent);
        response.EnsureSuccessStatusCode();
    }
}

// Models for deserialization
public class TokenResponse
{
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
}

public class FileUploadResponse
{
    [JsonPropertyName("data")]
    public FileUploadData Data { get; set; }
}

public class FileUploadData
{
    [JsonPropertyName("type")]
    public string Type { get; set; } // Maps to "type" in the JSON

    [JsonPropertyName("id")]
    public string Id { get; set; }   // Maps to "id" in the JSON

    [JsonPropertyName("attributes")]
    public FileUploadAttributes Attributes { get; set; }
}

public class FileUploadAttributes
{
    [JsonPropertyName("url")]
    public string Url { get; set; } // Maps to "url" in the JSON

    [JsonPropertyName("url_signature")]
    public string UrlSignature { get; set; } // Maps to "url_signature" in the JSON
}