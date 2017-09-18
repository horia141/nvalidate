using NUnit.Framework;
using NValidate.Attributes;

namespace NValidate.Tests
{
    [TestFixture]
    public class ValidatorTemplateAttributeTest
    {
        [ValidatorTemplate]
        public void FunctionWithAttribute() { }

        public void FunctionWithoutAttribute() { }

        [Test]
        public void HasAttribute()
        {
            Assert.That(ValidatorTemplateAttribute.HasAttribute(typeof(ValidatorTemplateAttributeTest).GetMethod("FunctionWithAttribute")), Is.True);
            Assert.That(ValidatorTemplateAttribute.HasAttribute(typeof(ValidatorTemplateAttributeTest).GetMethod("FunctionWithoutAttribute")), Is.False);
        }
    }
}
