// © 2026, Worth Systems.

using NUnit.Framework;
using WebQueries.Register;

namespace WebQueries.Tests.Unit.Register
{
    [TestFixture]
    public sealed class ContactMomentContentTests
    {
        [TestCase(0)]
        [TestCase(999)]
        [TestCase(1000)]
        public void Truncate_UpTo1000Characters_StaysUnchanged(int length)
        {
            string text = new('x', length);

            Assert.That(ContactMomentContent.Truncate(text), Is.EqualTo(text));
        }

        [TestCase(1001)]
        [TestCase(5000)]
        public void Truncate_LongerThan1000Characters_IsCutTo997PlusThreeDots(int length)
        {
            string result = ContactMomentContent.Truncate(new string('x', length));

            Assert.That(result, Is.EqualTo($"{new string('x', 997)}..."));
        }

        [Test]
        public void Truncate_CutFallsInsideASurrogatePair_DropsTheWholePair()
        {
            // "😀" is two UTF-16 chars; put its high surrogate at index 996, the last one kept
            string text = $"{new string('x', 996)}😀{new string('x', 10)}";

            string result = ContactMomentContent.Truncate(text);

            Assert.That(result, Is.EqualTo($"{new string('x', 996)}..."));
        }

        [Test]
        public void Truncate_CustomMaxLength_IsHonoured()
        {
            Assert.That(ContactMomentContent.Truncate("abcdefghij", 6), Is.EqualTo("abc..."));
        }

        [TestCase(3, "...")]
        [TestCase(1, ".")]
        [TestCase(0, "")]
        [TestCase(-5, "")]
        public void Truncate_MaxLengthNoRoomForText_ReturnsOnlyDots(int maxLength, string expected)
        {
            Assert.That(ContactMomentContent.Truncate("abcdefghij", maxLength), Is.EqualTo(expected));
        }
    }
}
