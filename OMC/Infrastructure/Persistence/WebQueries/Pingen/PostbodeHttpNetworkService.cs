using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebQueries.Pingen
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class PostbodeHttpNetworkService : IPostbodeHttpNetworkService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        /// <summary>
        /// 
        /// </summary>
        public PostbodeHttpNetworkService()
        {
            _apiKey = "M6HoBiPBgLGjnQ5CBNF3f9TSqrDPadJ6Z0Y4Xe2x3063d725";
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://postbode.app/api/v2/")
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mailboxId"></param>
        /// <param name="filePath"></param>
        /// <param name="envelopeId"></param>
        /// <param name="countryCode"></param>
        /// <param name="registered"></param>
        /// <param name="sendDirect"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SendLetterAsync(string mailboxId, string filePath, int envelopeId,
            string countryCode,
            bool registered, bool sendDirect)
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(mailboxId), "mailbox_id");
            form.Add(new StringContent(envelopeId.ToString()), "envelope_id");
            form.Add(new StringContent(countryCode), "country");
            form.Add(new StringContent(registered.ToString().ToLower()), "registered");
            form.Add(new StringContent(sendDirect.ToString().ToLower()), "send_direct");

            var fileContent = new ByteArrayContent(File.ReadAllBytes(filePath));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
            form.Add(fileContent, "file", Path.GetFileName(filePath));

            HttpResponseMessage response = await _httpClient.PostAsync("postal", form);
            HttpResponseMessage response2 = await _httpClient.GetAsync(mailboxId + "/postals", CancellationToken.None);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Letter sent successfully.");
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to send letter. Status: {response.StatusCode}, Error: {error}");
            }
            return response;
        }
    }
}