using System;
using NUnit.Framework;

using NValidate;

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
