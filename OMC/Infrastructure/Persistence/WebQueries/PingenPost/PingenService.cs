using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PingenApiNet.Abstractions.Enums.Letters;
using WebQueries.PingenPost.Enums.Letters;
using WebQueries.PingenPost.Views;

namespace WebQueries.PingenPost
{
    /// <summary>
    /// 
    /// </summary>
    public class PingenService
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private string? _accessToken;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        public PingenService(HttpClient httpClient, string clientId, string clientSecret)
        {
            _httpClient = httpClient;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        // Step 1: Authenticate and get access token
        /// <summary>
        /// 
        /// </summary>
        public async Task AuthenticateAsync()
        {
            var requestBody = new FormUrlEncodedContent([
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("client_secret", _clientSecret)
            ]);

            HttpResponseMessage response = await _httpClient.PostAsync("https://identity.pingen.com/auth/access-tokens", requestBody);
            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();
            TokenResponse? tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);

            _accessToken = tokenResponse?.AccessToken;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        // Step 2: Request file upload URL
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<FileUploadResponse?> RequestFileUploadUrlAsync()
        {
            HttpResponseMessage response = await _httpClient.GetAsync("https://api.pingen.com/file-upload");
            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();
            FileUploadResponse? b = null;
            try
            {
                b = JsonSerializer.Deserialize<FileUploadResponse>(responseContent);
            }
            catch (Exception e)
            {
                var a = e;
            }
            
            return b;
        }

        // Step 3: Upload file to the provided URL
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uploadUrl"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> UploadFileAsync(string? uploadUrl, string filePath)
        {
            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var content = new StreamContent(fileStream);
            using var httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.PutAsync(uploadUrl, content);
            response.EnsureSuccessStatusCode();
            return response;
        }

        // Step 4: Submit the letter
        /// <summary>
        /// 
        /// </summary>
        /// <param name="organisationId"></param>
        /// <param name="letterMetaData"></param>
        /// <param name="fileUrl"></param>
        /// <param name="fileSignature"></param>
        /// <param name="fileName"></param>
        public async Task SubmitLetterAsync(string organisationId, LetterMetaData letterMetaData, string? fileUrl,
            string? fileSignature, string fileName)
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
                        PrintSpectrum = LetterPrintSpectrum.color,
                        MetaData = letterMetaData
                    }
                }
            };


            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");

            HttpResponseMessage response = await _httpClient.PostAsync($"https://api.pingen.com/organisations/{organisationId}/letters", content);
            string errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(errorContent);
            response.EnsureSuccessStatusCode();
        }
    }

// Models for deserialization
    /// <summary>
    /// 
    /// </summary>
    public class TokenResponse
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; init; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FileUploadResponse
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("data")]
        public FileUploadData? Data { get; init; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FileUploadData
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; } // Maps to "type" in the JSON

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }   // Maps to "id" in the JSON

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("attributes")]
        public FileUploadAttributes? Attributes { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FileUploadAttributes
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("url")]
        public string? Url { get; set; } // Maps to "url" in the JSON

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("url_signature")]
        public string? UrlSignature { get; set; } // Maps to "url_signature" in the JSON
    }
}