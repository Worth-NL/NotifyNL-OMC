using System.Net.Http;
using System.Threading.Tasks;

namespace WebQueries.Pingen
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPostbodeHttpNetworkService
    {
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
        Task<HttpResponseMessage> SendLetterAsync(string mailboxId, string filePath, int envelopeId,
            string countryCode,
            bool registered, bool sendDirect);
    }
}