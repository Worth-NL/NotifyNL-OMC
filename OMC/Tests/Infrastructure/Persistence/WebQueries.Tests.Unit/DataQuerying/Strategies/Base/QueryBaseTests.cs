// © 2026, Worth Systems.

using Moq;
using NUnit.Framework;
using System.Net;
using WebQueries.DataQuerying.Models.Responses;
using WebQueries.DataQuerying.Strategies.Base;
using WebQueries.DataQuerying.Strategies.Interfaces;
using WebQueries.DataSending.Clients.Enums;
using WebQueries.DataSending.Interfaces;
using ZgwModels.Mapping.Models.POCOs.OpenKlant.v2;
using ZgwModels.Serialization.Interfaces;

namespace WebQueries.Tests.Unit.DataQuerying.Strategies.Base
{
    [TestFixture]
    public sealed class QueryBaseTests
    {
        private static readonly Uri s_uri = new("https://test.domain/api/v1/resource/1");

        private static IQueryBase GetQueryBase(HttpRequestResponse getResponse)
        {
            Mock<IHttpNetworkService> networkService = new(MockBehavior.Strict);
            networkService
                .Setup(mock => mock.GetAsync(It.IsAny<HttpClientTypes>(), It.IsAny<Uri>()))
                .ReturnsAsync(getResponse);
            networkService
                .Setup(mock => mock.PostAsync(It.IsAny<HttpClientTypes>(), It.IsAny<Uri>(), It.IsAny<string>()))
                .ReturnsAsync(getResponse);

            return new QueryBase(new Mock<ISerializationService>(MockBehavior.Strict).Object, networkService.Object);
        }

        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.InternalServerError)]
        [TestCase(HttpStatusCode.Unauthorized)]
        public void ProcessGetAsync_FailedResponse_CarriesTheStatusCodeOnTheException(HttpStatusCode statusCode)
        {
            // NOTE: Callers tell "the resource is not there" (404, do not retry) from "the service failed" (retry)
            //       by this status code alone, so it has to survive into the exception.

            // Arrange
            IQueryBase queryBase = GetQueryBase(HttpRequestResponse.Failure("{}", statusCode));

            // Act & Assert
            HttpRequestException? exception = Assert.ThrowsAsync<HttpRequestException>(() =>
                queryBase.ProcessGetAsync<PartyResults>(HttpClientTypes.OpenKlant_v2, s_uri, "fallback"));

            Assert.That(exception!.StatusCode, Is.EqualTo(statusCode));
        }

        [Test]
        public void ProcessGetAsync_NoResponseAtAll_LeavesTheStatusCodeEmpty()
        {
            // NOTE: A timeout or DNS failure never produced a status. It must not read as a 404.

            // Arrange
            IQueryBase queryBase = GetQueryBase(HttpRequestResponse.Failure("The operation was canceled."));

            // Act & Assert
            HttpRequestException? exception = Assert.ThrowsAsync<HttpRequestException>(() =>
                queryBase.ProcessGetAsync<PartyResults>(HttpClientTypes.OpenKlant_v2, s_uri, "fallback"));

            Assert.That(exception!.StatusCode, Is.Null);
        }

        [Test]
        public void ProcessPostAsync_FailedResponse_CarriesTheStatusCodeOnTheException()
        {
            // Arrange
            IQueryBase queryBase = GetQueryBase(HttpRequestResponse.Failure("{}", HttpStatusCode.BadRequest));

            // Act & Assert
            HttpRequestException? exception = Assert.ThrowsAsync<HttpRequestException>(() =>
                queryBase.ProcessPostAsync<PartyResults>(HttpClientTypes.OpenKlant_v2, s_uri, "{}", "fallback"));

            Assert.That(exception!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}
