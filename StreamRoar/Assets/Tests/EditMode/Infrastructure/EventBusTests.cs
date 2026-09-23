using NUnit.Framework;
using StreamRoar.Infrastructure;

namespace StreamRoar.Tests.EditMode.Infrastructure
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
            var listener = new RecordingListener();
            m_Bus.Subscribe<SampleEvent>(listener);

            m_Bus.Publish(new SampleEvent { Value = 7 });

            Assert.That(listener.Count, Is.EqualTo(1));
            Assert.That(listener.LastValue, Is.EqualTo(7));
        }

        [Test]
        public void Publish_Scoped_DoesNotNotifyOtherScope()
        {
            var scopeA = new object();
            var scopeB = new object();
            var listenerA = new RecordingListener();
            var listenerB = new RecordingListener();
            m_Bus.Subscribe(scopeA, listenerA);
            m_Bus.Subscribe(scopeB, listenerB);

            m_Bus.Publish(scopeA, new SampleEvent { Value = 3 });

            Assert.That(listenerA.Count, Is.EqualTo(1));
            Assert.That(listenerB.Count, Is.EqualTo(0));
        }

        [Test]
        public void Unsubscribe_StopsFurtherNotifications()
        {
            var listener = new RecordingListener();
            m_Bus.Subscribe<SampleEvent>(listener);
            m_Bus.Unsubscribe<SampleEvent>(listener);

            m_Bus.Publish(new SampleEvent { Value = 1 });

            Assert.That(listener.Count, Is.EqualTo(0));
        }

        [Test]
        public void Clear_RemovesAllSubscriptions()
        {
            var listener = new RecordingListener();
            m_Bus.Subscribe<SampleEvent>(listener);
            m_Bus.Clear();

            m_Bus.Publish(new SampleEvent { Value = 2 });

            Assert.That(listener.Count, Is.EqualTo(0));
        }

        struct SampleEvent : IEvent
        {
            public int Value;
        }

        sealed class RecordingListener : IEventListener<SampleEvent>
        {
            public int Count { get; private set; }
            public int LastValue { get; private set; }

            public void OnEvent(SampleEvent eventData)
            {
                Count++;
                LastValue = eventData.Value;
            }
        }
    }
}
