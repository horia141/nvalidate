using NUnit.Framework;

namespace NValidate.Tests
{
    [TestFixture]
    public class ExceptionsAttributeTest
    {
        [Test]
        public void GetExceptionsSetEmpty()
        {
            var attribute = new ExceptionsAttribute();

            Assert.That(attribute.GetExceptionsSet(), Is.Empty);
        }

        [Test]
        public void GetExceptionsSetOneElement()
        {
            var attribute = new ExceptionsAttribute(10);

            Assert.That(attribute.GetExceptionsSet(), Is.EquivalentTo(new[] { 10 }));
        }

        [Test]
        public void GetExceptionsSetSeveralElements()
        {
            var attribute = new ExceptionsAttribute(10, 20, 30);

            Assert.That(attribute.GetExceptionsSet(), Is.EquivalentTo(new[] { 30, 20, 10 }));
        }

        [Exceptions(10, 20, 30)]
        public void FunctionWithAttribute() { }

        public void FunctionWithoutAttribute() { }

        [Test]
        public void HasAttribute()
        {
            Assert.That(ExceptionsAttribute.HasAttribute(typeof(ExceptionsAttributeTest).GetMethod("FunctionWithAttribute")), Is.True);
            Assert.That(ExceptionsAttribute.HasAttribute(typeof(ExceptionsAttributeTest).GetMethod("FunctionWithoutAttribute")), Is.False);
        }
    }
}
