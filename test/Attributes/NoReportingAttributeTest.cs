using NUnit.Framework;
using NValidate.Attributes;
using System.Reflection;

namespace NValidate.Tests.Attributes
{
    [TestFixture]
    public class NoReportingAttributeTest
    {
        [NoReporting]
        class ClassWithAttribute { }

        class ClassWithoutAttribute { }

        [NoReporting]
        public void FunctionWithAttribute() { }

        public void FunctionWithoutAttribute() { }

        [Test]
        public void HasAttributeForClass()
        {
            Assert.That(NoReportingAttribute.HasAttribute(typeof(ClassWithAttribute).GetTypeInfo()), Is.True);
            Assert.That(NoReportingAttribute.HasAttribute(typeof(ClassWithoutAttribute).GetTypeInfo()), Is.False);
        }

        [Test]
        public void HasAttributeForFunction()
        {
            Assert.That(NoReportingAttribute.HasAttribute(typeof(NoReportingAttributeTest).GetMethod("FunctionWithAttribute")), Is.True);
            Assert.That(NoReportingAttribute.HasAttribute(typeof(NoReportingAttributeTest).GetMethod("FunctionWithoutAttribute")), Is.False);
        }
    }
}
