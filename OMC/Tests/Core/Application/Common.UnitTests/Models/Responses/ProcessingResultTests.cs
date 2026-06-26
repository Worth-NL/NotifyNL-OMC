// © 2024, Worth Systems.

using Common.Enums.Responses;
using Common.Models.Messages.Details;
using Common.Models.Responses;
using NUnit.Framework;
using System.Net;

namespace Common.Tests.Unit.Models.Responses
{
    [TestFixture]
    public sealed class ProcessingResultTests
    {
        private const string TestDescription = "Something happened";
        private static readonly object TestJson = new { Key = "Value" };

        #region Factory methods — status
        [Test]
        public void Success_SetsStatusToSuccess()
        {
            ProcessingResult result = ProcessingResult.Success(TestDescription);
            Assert.That(result.Status, Is.EqualTo(ProcessingStatus.Success));
        }

        [Test]
        public void Skipped_SetsStatusToSkipped()
        {
            ProcessingResult result = ProcessingResult.Skipped(TestDescription);
            Assert.That(result.Status, Is.EqualTo(ProcessingStatus.Skipped));
        }

        [Test]
        public void Aborted_SetsStatusToAborted()
        {
            ProcessingResult result = ProcessingResult.Aborted(TestDescription);
            Assert.That(result.Status, Is.EqualTo(ProcessingStatus.Aborted));
        }

        [Test]
        public void NotPossible_SetsStatusToNotPossible()
        {
            ProcessingResult result = ProcessingResult.NotPossible(TestDescription);
            Assert.That(result.Status, Is.EqualTo(ProcessingStatus.NotPossible));
        }

        [Test]
        public void Failure_SetsStatusToFailure()
        {
            ProcessingResult result = ProcessingResult.Failure(TestDescription);
            Assert.That(result.Status, Is.EqualTo(ProcessingStatus.Failure));
        }

        [Test]
        public void Unknown_SetsStatusToFailure()
        {
            ProcessingResult result = ProcessingResult.Unknown(TestDescription);
            Assert.That(result.Status, Is.EqualTo(ProcessingStatus.Failure));
        }
        #endregion

        #region Factory methods — description (no json)
        [Test]
        public void Success_WithoutJson_DescriptionIsPlainMessage()
        {
            ProcessingResult result = ProcessingResult.Success(TestDescription);
            Assert.That(result.Description, Is.EqualTo(TestDescription));
        }

        [Test]
        public void Failure_WithoutJson_DescriptionIsPlainMessage()
        {
            ProcessingResult result = ProcessingResult.Failure(TestDescription);
            Assert.That(result.Description, Is.EqualTo(TestDescription));
        }
        #endregion

        #region Factory methods — description (with json)
        [Test]
        public void Success_WithJson_DescriptionContainsBoth()
        {
            ProcessingResult result = ProcessingResult.Success(TestDescription, TestJson);
            Assert.That(result.Description, Does.Contain(TestDescription));
            Assert.That(result.Description, Does.Contain(TestJson.ToString()!));
        }

        [Test]
        public void Failure_WithJson_DescriptionContainsBoth()
        {
            ProcessingResult result = ProcessingResult.Failure(TestDescription, TestJson);
            Assert.That(result.Description, Does.Contain(TestDescription));
            Assert.That(result.Description, Does.Contain(TestJson.ToString()!));
        }
        #endregion

        #region Factory methods — default details
        [Test]
        public void Success_WithoutExplicitDetails_UsesInfoDetailsEmpty()
        {
            ProcessingResult result = ProcessingResult.Success(TestDescription);
            Assert.That(result.Details, Is.EqualTo(InfoDetails.Empty));
        }

        [Test]
        public void Skipped_WithoutExplicitDetails_UsesInfoDetailsEmpty()
        {
            ProcessingResult result = ProcessingResult.Skipped(TestDescription);
            Assert.That(result.Details, Is.EqualTo(InfoDetails.Empty));
        }

        [Test]
        public void Aborted_WithoutExplicitDetails_UsesErrorDetailsEmpty()
        {
            ProcessingResult result = ProcessingResult.Aborted(TestDescription);
            Assert.That(result.Details, Is.EqualTo(ErrorDetails.Empty));
        }

        [Test]
        public void Failure_WithoutExplicitDetails_UsesErrorDetailsEmpty()
        {
            ProcessingResult result = ProcessingResult.Failure(TestDescription);
            Assert.That(result.Details, Is.EqualTo(ErrorDetails.Empty));
        }
        #endregion

        #region Factory methods — explicit details
        [Test]
        public void Success_WithExplicitDetails_UsesProvidedDetails()
        {
            ErrorDetails customDetails = new("msg", "case", ["reason"]);
            ProcessingResult result = ProcessingResult.Success(TestDescription, details: customDetails);
            Assert.That(result.Details, Is.EqualTo(customDetails));
        }

        [Test]
        public void Failure_WithExplicitDetails_UsesProvidedDetails()
        {
            InfoDetails customDetails = new("msg", "case", ["reason"]);
            ProcessingResult result = ProcessingResult.Failure(TestDescription, details: customDetails);
            Assert.That(result.Details, Is.EqualTo(customDetails));
        }
        #endregion
    }
}
