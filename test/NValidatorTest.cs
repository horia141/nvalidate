using System;
using Xunit;

using NValidate.Internal;

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
