using NUnit.Framework;
using System.Reflection;

namespace NValidate.Tests
{
    [TestFixture]
    public class ValidatorFixtureAttributeTest
    {
        [ValidatorFixture("WithAttribute")]
        class ClassWithAttribute { }

        class ClassWithoutAttribute { }

        [Test]
        public void Name()
        {
            var attribute = new ValidatorFixtureAttribute("The Attribute");

            Assert.That(attribute.Name, Is.EqualTo("The Attribute"));
        }

        [Test]
        public void HasAttribute()
        {
            Assert.That(ValidatorFixtureAttribute.HasAttribute(typeof(ClassWithAttribute).GetTypeInfo()), Is.True);
            Assert.That(ValidatorFixtureAttribute.HasAttribute(typeof(ClassWithoutAttribute).GetTypeInfo()), Is.False);
        }
    }
}
