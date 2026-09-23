using NUnit.Framework;
using StreamRoar.Infrastructure;

namespace StreamRoar.Tests.Editor.Infrastructure
{
    [TestFixture]
    [Category("Infrastructure")]
    public sealed class ServiceLocatorTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.ClearForTests();
        }

        [TearDown]
        public void TearDown()
        {
            ServiceLocator.ClearForTests();
        }

        [Test]
        public void Register_ThenResolve_ReturnsSameInstance()
        {
            var service = new SampleService();
            ServiceLocator.Register(service);

            Assert.That(ServiceLocator.Resolve<SampleService>(), Is.SameAs(service));
        }

        [Test]
        public void Unregister_MatchingInstance_RemovesService()
        {
            var service = new SampleService();
            ServiceLocator.Register(service);
            ServiceLocator.Unregister(service);

            Assert.That(ServiceLocator.Resolve<SampleService>(), Is.Null);
        }

        [Test]
        public void Unregister_DifferentInstance_KeepsRegisteredService()
        {
            var registered = new SampleService();
            var other = new SampleService();
            ServiceLocator.Register(registered);
            ServiceLocator.Unregister(other);

            Assert.That(ServiceLocator.Resolve<SampleService>(), Is.SameAs(registered));
        }

        [Test]
        public void Register_SameTypeTwice_ReplacesPrevious()
        {
            var first = new SampleService();
            var second = new SampleService();
            ServiceLocator.Register(first);
            ServiceLocator.Register(second);

            Assert.That(ServiceLocator.Resolve<SampleService>(), Is.SameAs(second));
        }

        sealed class SampleService
        {
        }
    }
}
