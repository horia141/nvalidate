using System;
using Xunit;

using NValidate;

namespace NValidate.Tests
{
    public class NValidatorTest
    {
        [Fact]
        public void AddFoo()
        {
	    Assert.Equal(NValidator.AddFoo("bar"), "bar-foo");
        }
    }
}
