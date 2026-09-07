// © 2024, Worth Systems.

using Common.Extensions;
using Common.Models.Messages.Details;
using Common.Models.Messages.Details.Base;
using NUnit.Framework;

namespace Common.Tests.Unit.Extensions
{
    [TestFixture]
    public sealed class DetailsExtensionsTests
    {
        #region Trim()
        [Test]
        public void Trim_ReturnsBaseSimpleDetails_WithSameMessage()
        {
            ErrorDetails enhanced = new("Test message", "some case", ["reason1"]);

            BaseSimpleDetails result = enhanced.Trim();

            Assert.That(result.Message, Is.EqualTo("Test message"));
        }

        [Test]
        public void Trim_DropsTheCasesAndReasons()
        {
            ErrorDetails enhanced = new("msg", "case A", ["r1", "r2"]);

            BaseSimpleDetails result = enhanced.Trim();

            // Trimmed result is SimpleDetails — no Cases/Reasons properties
            Assert.That(result, Is.Not.InstanceOf<BaseEnhancedDetails>());
        }

        [Test]
        public void Trim_EmptyDetails_ReturnsSimpleDetailsWithEmptyMessage()
        {
            BaseSimpleDetails result = ErrorDetails.Empty.Trim();

            Assert.That(result, Is.Not.Null);
        }
        #endregion

        #region Expand()
        [Test]
        public void Expand_ReturnsBaseEnhancedDetails_WithSameMessage()
        {
            SimpleDetails simple = new("Expand this");

            BaseEnhancedDetails result = simple.Expand();

            Assert.That(result.Message, Is.EqualTo("Expand this"));
        }

        [Test]
        public void Expand_ResultHasEmptyCasesAndReasons()
        {
            SimpleDetails simple = new("msg");

            BaseEnhancedDetails result = simple.Expand();

            Assert.Multiple(() =>
            {
                Assert.That(result.Cases, Is.EqualTo(string.Empty));
                Assert.That(result.Reasons, Is.Empty);
            });
        }
        #endregion

        #region Round-trip
        [Test]
        public void TrimThenExpand_PreservesMessage()
        {
            ErrorDetails original = new("Round-trip message", "case", ["r"]);

            BaseEnhancedDetails result = original.Trim().Expand();

            Assert.That(result.Message, Is.EqualTo("Round-trip message"));
        }
        #endregion
    }
}
