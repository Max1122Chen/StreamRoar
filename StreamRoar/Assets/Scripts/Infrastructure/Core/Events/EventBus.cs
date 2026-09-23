using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 按事件类型和可选作用域分发的同步事件总线。
    /// </summary>
    public sealed class EventBus : IEventBus
    {
        readonly Dictionary<Type, object> m_Channels = new();
        readonly Dictionary<ScopedEventKey, object> m_ScopedChannels = new();

        /// <summary>
        /// 订阅指定类型事件。
        /// </summary>
        public void Subscribe<TEvent>(IEventListener<TEvent> listener) where TEvent : struct, IEvent
        {
            Type eventType = typeof(TEvent);
            if (!m_Channels.TryGetValue(eventType, out object channel))
            {
                channel = new EventChannel<TEvent>();
                m_Channels.Add(eventType, channel);
            }

            ((EventChannel<TEvent>)channel).Subscribe(listener);
        }

        /// <summary>
        /// 订阅指定作用域的类型事件。
        /// </summary>
        public void Subscribe<TEvent>(object scope, IEventListener<TEvent> listener) where TEvent : struct, IEvent
        {
            ScopedEventKey key = new(scope, typeof(TEvent));
            if (!m_ScopedChannels.TryGetValue(key, out object channel))
            {
                channel = new EventChannel<TEvent>();
                m_ScopedChannels.Add(key, channel);
            }

            ((EventChannel<TEvent>)channel).Subscribe(listener);
        }

        /// <summary>
        /// 取消指定类型事件订阅。
        /// </summary>
        public void Unsubscribe<TEvent>(IEventListener<TEvent> listener) where TEvent : struct, IEvent
        {
            if (m_Channels.TryGetValue(typeof(TEvent), out object channel))
            {
                EventChannel<TEvent> eventChannel = (EventChannel<TEvent>)channel;
                eventChannel.Unsubscribe(listener);
                if (eventChannel.IsEmpty)
                    m_Channels.Remove(typeof(TEvent));
            }
        }

        /// <summary>
        /// 取消指定作用域的类型事件订阅。
        /// </summary>
        public void Unsubscribe<TEvent>(object scope, IEventListener<TEvent> listener) where TEvent : struct, IEvent
        {
            ScopedEventKey key = new(scope, typeof(TEvent));
            if (!m_ScopedChannels.TryGetValue(key, out object channel))
                return;

            EventChannel<TEvent> eventChannel = (EventChannel<TEvent>)channel;
            eventChannel.Unsubscribe(listener);
            if (eventChannel.IsEmpty)
                m_ScopedChannels.Remove(key);
        }

        /// <summary>
        /// 按订阅顺序同步发布事件。
        /// </summary>
        public void Publish<TEvent>(TEvent eventData) where TEvent : struct, IEvent
        {
            if (m_Channels.TryGetValue(typeof(TEvent), out object channel))
            {
                EventChannel<TEvent> eventChannel = (EventChannel<TEvent>)channel;
                eventChannel.Publish(eventData);
                if (eventChannel.IsEmpty)
                    m_Channels.Remove(typeof(TEvent));
            }
        }

        /// <summary>
        /// 同步发布指定作用域的类型事件。
        /// </summary>
        public void Publish<TEvent>(object scope, TEvent eventData) where TEvent : struct, IEvent
        {
            ScopedEventKey key = new(scope, typeof(TEvent));
            if (!m_ScopedChannels.TryGetValue(key, out object channel))
                return;

            EventChannel<TEvent> eventChannel = (EventChannel<TEvent>)channel;
            eventChannel.Publish(eventData);
            if (eventChannel.IsEmpty)
                m_ScopedChannels.Remove(key);
        }

        /// <summary>
        /// 清除全部事件订阅。
        /// </summary>
        public void Clear()
        {
            m_Channels.Clear();
            m_ScopedChannels.Clear();
        }

        readonly struct ScopedEventKey : IEquatable<ScopedEventKey>
        {
            readonly object m_Scope;
            readonly Type m_EventType;

            public ScopedEventKey(object scope, Type eventType)
            {
                m_Scope = scope;
                m_EventType = eventType;
            }

            public bool Equals(ScopedEventKey other)
            {
                return ReferenceEquals(m_Scope, other.m_Scope) && m_EventType == other.m_EventType;
            }

            public override bool Equals(object obj)
            {
                return obj is ScopedEventKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return RuntimeHelpers.GetHashCode(m_Scope) * 397 ^ m_EventType.GetHashCode();
                }
            }
        }

        sealed class EventChannel<TEvent> where TEvent : struct, IEvent
        {
            List<IEventListener<TEvent>> m_Listeners = new();
            List<IEventListener<TEvent>> m_PendingListeners = new();

            int m_PublishDepth;
            bool m_HasPendingChanges;

            public bool IsEmpty => m_PublishDepth == 0 && m_Listeners.Count == 0;

            public void Subscribe(IEventListener<TEvent> listener)
            {
                List<IEventListener<TEvent>> listeners = GetMutableListeners();
                if (!listeners.Contains(listener))
                    listeners.Add(listener);
            }

            public void Unsubscribe(IEventListener<TEvent> listener)
            {
                GetMutableListeners().Remove(listener);
            }

            public void Publish(TEvent eventData)
            {
                // 发布期间只修改待生效列表，当前事件始终使用稳定的监听顺序。
                m_PublishDepth++;
                try
                {
                    for (int i = 0; i < m_Listeners.Count; i++)
                        m_Listeners[i].OnEvent(eventData);
                }
                finally
                {
                    m_PublishDepth--;
                    if (m_PublishDepth == 0 && m_HasPendingChanges)
                    {
                        (m_Listeners, m_PendingListeners) = (m_PendingListeners, m_Listeners);
                        m_PendingListeners.Clear();
                        m_HasPendingChanges = false;
                    }
                }
            }

            List<IEventListener<TEvent>> GetMutableListeners()
            {
                if (m_PublishDepth == 0)
                    return m_Listeners;

                if (!m_HasPendingChanges)
                {
                    m_PendingListeners.AddRange(m_Listeners);
                    m_HasPendingChanges = true;
                }

                return m_PendingListeners;
            }
        }
    }
}
