using NUnit.Framework;
using System;

namespace NValidate.Tests
{
    [AttributeUsage(AttributeTargets.Parameter)]
    class OneAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Parameter)]
    class TwoAttribute : Attribute { }

    class TestBase { }

    class TestDerived : TestBase { }

    [TestFixture]
    public class EnvironTest
    {
        [Test]
        public void Get()
        {
            var environ = new EnvironBuilder().Add<string>("hello").Build();
            Assert.That(environ.Get<string>(), Is.EqualTo("hello"));
        }

        [Test]
        public void GetMoreComplex()
        {
            var environ = new EnvironBuilder()
                .Add<string>("hello")
                .Add<int>(10)
                .Add<ValueTuple<string, int>>(("hello", 10))
                .Build();

            Assert.That(environ.Get<string>(), Is.EqualTo("hello"));
            Assert.That(environ.Get<int>(), Is.EqualTo(10));
            Assert.That(environ.Get<ValueTuple<string, int>>(), Is.EqualTo(("hello", 10)));
        }

        [Test]
        public void GetWithAttribute()
        {
            var environ = new EnvironBuilder()
                .Add<OneAttribute, string>("hello")
                .Add<TwoAttribute, string>("world")
                .Add<string>("hello world")
                .Build();

            Assert.That(environ.Get<OneAttribute, string>(), Is.EqualTo("hello"));
            Assert.That(environ.Get<TwoAttribute, string>(), Is.EqualTo("world"));
            Assert.That(environ.Get<string>(), Is.EqualTo("hello world"));
        }

        [Test]
        public void GetWithExtractor()
        {
            var environ = new EnvironBuilder()
                .Add<bool>(true)
                .Add<int>(10)
                .AddExtractor<string>((env) => $"{env.Get<bool>()}-{env.Get<int>()}")
                .Build();

            Assert.That(environ.Get<string>(), Is.EqualTo("True-10"));
        }

        [Test]
        public void GetWithExtractorAndAttribute()
        {
            var environ = new EnvironBuilder()
                .Add<bool>(true)
                .Add<int>(10)
                .AddExtractor<OneAttribute, string>((env) => $"{env.Get<bool>()}-{env.Get<int>()}")
                .AddExtractor<TwoAttribute, string>((env) => $"{env.Get<bool>()}+{env.Get<int>()}")
                .Build();

            Assert.That(environ.Get<OneAttribute, string>(), Is.EqualTo("True-10"));
            Assert.That(environ.Get<TwoAttribute, string>(), Is.EqualTo("True+10"));
        }

        [Test]
        public void GetFails()
        {
            var environ = new EnvironBuilder().Build();

            Assert.That(environ.Get<bool>(), Is.False);
            Assert.That(environ.Get<int>, Is.Zero);
            Assert.That(environ.Get<DateTime>, Is.EqualTo(default(DateTime)));
            Assert.That(environ.Get<string>(), Is.Null);
        }

        [Test]
        public void GetFailsWithAttribute()
        {
            var environ = new EnvironBuilder()
                .Add<OneAttribute, bool>(true)
                .Add<TwoAttribute, string>("true")
                .Build();

            Assert.That(environ.Get<bool>(), Is.False);
            Assert.That(environ.Get<TwoAttribute, bool>(), Is.False);
            Assert.That(environ.Get<string>(), Is.Null);
            Assert.That(environ.Get<OneAttribute, string>(), Is.Null);
        }

        [Test]
        public void GetWithHierarchyFails()
        {
            var environ = new EnvironBuilder()
                .Add(new TestDerived())
                .Build();

            Assert.That(environ.Get<TestBase>(), Is.Null);
        }

        [Test]
        public void GetWithExplicitHierarchyFails()
        {
            var derived = new TestDerived();
            var environ = new EnvironBuilder()
                .Add<TestBase>(derived)
                .Build();

            Assert.That(environ.Get<TestBase>(), Is.SameAs(derived));
        }

        [Test]
        public void Extend()
        {
            var environ = new EnvironBuilder().Build();

            var newEnviron = environ.Extend<string>("hello");

            Assert.That(newEnviron.Get<string>(), Is.EqualTo("hello"));
        }

        [Test]
        public void ExtendWithAttribute()
        {
            var environ = new EnvironBuilder().Build();

            var newEnviron = environ
                .Extend<OneAttribute, string>("hello")
                .Extend<TwoAttribute, string>("world");

            Assert.That(newEnviron.Get<OneAttribute, string>(), Is.EqualTo("hello"));
            Assert.That(newEnviron.Get<TwoAttribute, string>(), Is.EqualTo("world"));
        }

        [Test]
        public void ExtendProducesANewObject()
        {
            var environ = new EnvironBuilder().Build();

            var newEnviron = environ.Extend<string>("hello");

            Assert.That(newEnviron, Is.Not.SameAs(environ));
            Assert.That(environ.Get<string>(), Is.Null);
        }

        [Test]
        public void ExtendShadowsPreviousAssociation()
        {
            var environ = new EnvironBuilder()
                .Add<string>("hello")
                .Build();

            var newEnviron = environ.Extend<string>("world");

            Assert.That(newEnviron.Get<string>(), Is.EqualTo("world"));
        }

        [Test]
        public void ExtendWithHierarchyFails()
        {
            var environ = new EnvironBuilder().Build();
            var newEnviron = environ.Extend(new TestDerived());

            Assert.That(newEnviron.Get<TestBase>(), Is.Null);
        }

        [Test]
        public void ExtendWithExplicitHierarchyFails()
        {
            var derived = new TestDerived();
            var environ = new EnvironBuilder().Build();
            var newEnviron = environ.Extend<TestBase>(derived);

            Assert.That(newEnviron.Get<TestBase>(), Is.SameAs(derived));
        }

        [Test]
        public void ExtendWithAttributeShadowsPreviousAssociation()
        {
            var environ = new EnvironBuilder()
                .Add<OneAttribute, string>("hello")
                .Build();

            var newEnviron = environ.Extend<OneAttribute, string>("world");

            Assert.That(newEnviron.Get<OneAttribute, string>(), Is.EqualTo("world"));
        }

        [Test]
        public void ResolveParameters()
        {
            Func<string, int, string> testFn = (string foo, int bar) => $"{foo}-{bar}";
            var environ = new EnvironBuilder()
                .Add<string>("hello")
                .Add<int>(10)
                .Build();

            var resolvedTestFnParams = environ.ResolveParameters(testFn.Method.GetParameters());
            Assert.That(resolvedTestFnParams, Is.Not.Null);
            Assert.That(resolvedTestFnParams, Has.Length.EqualTo(2));
            Assert.That(resolvedTestFnParams[0], Is.EqualTo("hello"));
            Assert.That(resolvedTestFnParams[1], Is.EqualTo(10));
        }

        public string TestFn([One] string foo, [Two] string bar) => $"{foo}-{bar}";

        [Test]
        public void ResolveParametersWithAttribute()
        {
            var environ = new EnvironBuilder()
                .Add<OneAttribute, string>("hello")
                .Add<TwoAttribute, string>("world")
                .Build();

            var resolvedTestFnParams = environ.ResolveParameters(typeof(EnvironTest).GetMethod("TestFn").GetParameters());
            Assert.That(resolvedTestFnParams, Is.Not.Null);
            Assert.That(resolvedTestFnParams, Has.Length.EqualTo(2));
            Assert.That(resolvedTestFnParams[0], Is.EqualTo("hello"));
            Assert.That(resolvedTestFnParams[1], Is.EqualTo("world"));
        }

        [Test]
        public void ResolveParametersFails()
        {
            Func<string, int, string> testFn = (string foo, int bar) => $"{foo}-{bar}";
            var environ = new EnvironBuilder().Build();

            Assert.That(
                () => environ.ResolveParameters(testFn.Method.GetParameters()),
                Throws.InstanceOf<CannotFindAssociationException>().With.Property("Message").EqualTo("Cannot translate parameter \"foo\""));
        }

        [Test]
        public void DoubleAddFails()
        {
            Assert.That(() =>
                new EnvironBuilder()
                    .Add<string>("a")
                    .Add<string>("b")
                    .Build(),
                Throws.InstanceOf<AssociationExistsException>());
            Assert.That(() =>
                new EnvironBuilder()
                    .Add<OneAttribute, string>("a")
                    .Add<OneAttribute, string>("b")
                    .Build(),
                Throws.InstanceOf<AssociationExistsException>());
        }

        [Test]
        public void DoubleAddExtractorFails()
        {
            Assert.That(() =>
                new EnvironBuilder()
                    .AddExtractor<string>(env => "foo")
                    .AddExtractor<string>(env => "bar")
                    .Build(),
                Throws.InstanceOf<AssociationExistsException>());
            Assert.That(() =>
                new EnvironBuilder()
                    .AddExtractor<OneAttribute, string>(env => "foo")
                    .AddExtractor<OneAttribute, string>(env => "bar")
                    .Build(),
                Throws.InstanceOf<AssociationExistsException>());
        }

        [Test]
        public void DoubleAddAndAddExtractorFails()
        {
            Assert.That(() =>
                new EnvironBuilder()
                    .Add<string>("foo")
                    .AddExtractor<string>(env => "bar")
                    .Build(),
                Throws.InstanceOf<AssociationExistsException>());
            Assert.That(() =>
                new EnvironBuilder()
                    .Add<OneAttribute, string>("foo")
                    .AddExtractor<OneAttribute, string>(env => "bar")
                    .Build(),
                Throws.InstanceOf<AssociationExistsException>());
        }

        [Test]
        public void DoubleAddExtractorAndAddFails()
        {
            Assert.That(() =>
                new EnvironBuilder()
                    .AddExtractor<string>(env => "foo")
                    .Add<string>("bar")
                    .Build(),
                Throws.InstanceOf<AssociationExistsException>());
            Assert.That(() =>
                new EnvironBuilder()
                    .AddExtractor<OneAttribute, string>(env => "foo")
                    .Add<OneAttribute, string>("bar")
                    .Build(),
                Throws.InstanceOf<AssociationExistsException>());
        }
    }
}
