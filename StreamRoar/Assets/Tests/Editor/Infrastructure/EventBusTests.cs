using NUnit.Framework;
using System;
using StreamRoar.Infrastructure;

namespace StreamRoar.Tests.Editor.Infrastructure
{
    [TestFixture]
    [Category("Infrastructure")]
    public sealed class EventBusTests
    {
        EventBus m_Bus;

        [SetUp]
        public void SetUp()
        {
            m_Bus = new EventBus();
        }

        [TearDown]
        public void TearDown()
        {
            m_Bus.Clear();
        }

        [Test]
        public void Publish_NotifiesGlobalSubscriber()
        {
            int count = 0;
            int lastValue = 0;
            m_Bus.Subscribe<SampleEvent>(e => { count++; lastValue = e.Value; });

            m_Bus.Publish(new SampleEvent { Value = 7 });

            Assert.That(count, Is.EqualTo(1));
            Assert.That(lastValue, Is.EqualTo(7));
        }

        [Test]
        public void Publish_Scoped_DoesNotNotifyOtherScope()
        {
            var scopeA = new object();
            var scopeB = new object();
            int countA = 0;
            int countB = 0;
            m_Bus.Subscribe<SampleEvent>(scopeA, e => countA++);
            m_Bus.Subscribe<SampleEvent>(scopeB, e => countB++);

            m_Bus.Publish(scopeA, new SampleEvent { Value = 3 });

            Assert.That(countA, Is.EqualTo(1));
            Assert.That(countB, Is.EqualTo(0));
        }

        [Test]
        public void Unsubscribe_StopsFurtherNotifications()
        {
            int count = 0;
            Action<SampleEvent> handler = e => count++;
            m_Bus.Subscribe<SampleEvent>(handler);
            m_Bus.Unsubscribe<SampleEvent>(handler);

            m_Bus.Publish(new SampleEvent { Value = 1 });

            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public void Clear_RemovesAllSubscriptions()
        {
            int count = 0;
            m_Bus.Subscribe<SampleEvent>(e => count++);
            m_Bus.Clear();

            m_Bus.Publish(new SampleEvent { Value = 2 });

            Assert.That(count, Is.EqualTo(0));
        }

        struct SampleEvent : IEvent
        {
            public int Value;
        }
    }
}
