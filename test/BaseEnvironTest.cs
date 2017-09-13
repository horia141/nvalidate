using NUnit.Framework;


namespace NValidate.Tests
{
    [TestFixture]
    public class BaseEnvironTest
    {
        [Test]
        public void AddProducesALinkedEnviron()
        {
            var baseEnviron = new BaseEnvironBuilder().Build();
            var newEnviron = baseEnviron.Add("hello");
            Assert.That(newEnviron, Is.AssignableTo(typeof(LinkedEnviron)));
        }
    }
}
