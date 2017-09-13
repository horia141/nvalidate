using NUnit.Framework;
using System;

namespace NValidate.Tests
{
    [TestFixture]
    public class EnvironTest
    {
        [Test]
        public void GetByType()
        {
            var environ = new EnvironBuilder().AddModel("hello").Build();
            Assert.That(environ.GetByType(typeof(string)), Is.EqualTo("hello"));
        }

        [Test]
        public void GetByTypeMoreComplex()
        {
            var environ = new EnvironBuilder()
                .AddModel("hello")
                .AddModel(10)
                .AddModel(Tuple.Create("hello", 10))
                .Build();

            Assert.That(environ.GetByType(typeof(string)), Is.EqualTo("hello"));
            Assert.That(environ.GetByType(typeof(int)), Is.EqualTo(10));
            Assert.That(environ.GetByType(typeof(Tuple<string, int>)), Is.EqualTo(Tuple.Create("hello", 10)));
        }

        [Test]
        public void GetByTypeWithExtractor()
        {
            var environ = new EnvironBuilder()
                .AddModel(true)
                .AddModel(10)
                .AddModelExtractor<string>((env) => $"{env.Get<bool>()}-{env.Get<int>()}")
                .Build();

            Assert.That(environ.GetByType(typeof(string)), Is.EqualTo("True-10"));
        }

        [Test]
        public void GetByTypeFails()
        {
            var environ = new EnvironBuilder().Build();

            Assert.That(environ.GetByType(typeof(string)), Is.Null);
        }

        [Test]
        public void Get()
        {
            var environ = new EnvironBuilder().AddModel("hello").Build();
            Assert.That(environ.Get<string>(), Is.EqualTo("hello"));
        }

        [Test]
        public void GetMoreComplex()
        {
            var environ = new EnvironBuilder()
                .AddModel("hello")
                .AddModel(10)
                .AddModel(Tuple.Create("hello", 10))
                .Build();

            Assert.That(environ.Get<string>(), Is.EqualTo("hello"));
            Assert.That(environ.Get<int>(), Is.EqualTo(10));
            Assert.That(environ.Get<Tuple<string, int>>(), Is.EqualTo(Tuple.Create("hello", 10)));
        }

        [Test]
        public void GetWithExtractor()
        {
            var environ = new EnvironBuilder()
                .AddModel(true)
                .AddModel(10)
                .AddModelExtractor<string>((env) => $"{env.Get<bool>()}-{env.Get<int>()}")
                .Build();

            Assert.That(environ.Get<string>(), Is.EqualTo("True-10"));
        }

        [Test]
        public void GetFails()
        {
            var environ = new EnvironBuilder().Build();

            Assert.That(environ.Get<string>(), Is.Null);
        }

        [Test]
        public void ResolveParameters()
        {
            Func<string, int, string> testFn = (string foo, int bar) => $"{foo}-{bar}";
            var environ = new EnvironBuilder()
                .AddModel("hello")
                .AddModel(10)
                .Build();

            var resolvedTestFnParams = environ.ResolveParameters(testFn.Method.GetParameters());
            Assert.That(resolvedTestFnParams, Is.Not.Null);
            Assert.That(resolvedTestFnParams, Has.Length.EqualTo(2));
            Assert.That(resolvedTestFnParams[0], Is.EqualTo("hello"));
            Assert.That(resolvedTestFnParams[1], Is.EqualTo(10));
        }

        [Test]
        public void ResolveParametersFails()
        {
            Func<string, int, string> testFn = (string foo, int bar) => $"{foo}-{bar}";
            var environ = new EnvironBuilder().Build();

            Assert.That(() => environ.ResolveParameters(testFn.Method.GetParameters()), Throws.Exception.With.Property("Message").EqualTo("Cannot translate parameter \"foo\""));
        }
    }
}
