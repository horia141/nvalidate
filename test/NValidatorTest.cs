using NUnit.Framework;


namespace NValidate.Tests
{
    [TestFixture]
    public class NValidatorTest
    {
        [Test]
        public void AddFoo()
        {
	        Assert.That(NValidator.AddFoo("bar"), Is.EqualTo("bar-foo"));
        }
    }
}
