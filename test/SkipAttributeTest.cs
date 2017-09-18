using NUnit.Framework;
using System.Reflection;

namespace NValidate.Tests
{
    [TestFixture]
    public class SkipAttributeTest
    {
        [Skip]
        class ClassWithAttribute { }

        class ClassWithoutAttribute { }

        [Skip]
        public void FunctionWithAttribute() { }

        public void FunctionWithoutAttribute() { }

        [Test]
        public void HasAttributeForClass()
        {
            Assert.That(SkipAttribute.HasAttribute(typeof(ClassWithAttribute).GetTypeInfo()), Is.True);
            Assert.That(SkipAttribute.HasAttribute(typeof(ClassWithoutAttribute).GetTypeInfo()), Is.False);
        }

        [Test]
        public void HasAttributeForFunction()
        {
            Assert.That(SkipAttribute.HasAttribute(typeof(SkipAttributeTest).GetMethod("FunctionWithAttribute")), Is.True);
            Assert.That(SkipAttribute.HasAttribute(typeof(SkipAttributeTest).GetMethod("FunctionWithoutAttribute")), Is.False);
        }
    }
}
