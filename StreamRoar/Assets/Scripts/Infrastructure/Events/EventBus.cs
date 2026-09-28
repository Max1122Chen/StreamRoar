using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;   // 提供 RuntimeHelpers.GetHashCode 等底层方法

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 按事件类型和可选作用域分发的同步事件总线。
    /// </summary>
    public sealed class EventBus : IEventBus
    {
        // ===== 两个核心字典 =====
        // 全局事件通道：key = 事件类型，value = 该类型的 EventChannel（用 object 存是为了做"类型擦除"）
        readonly Dictionary<Type, object> m_Channels = new();
        // 作用域事件通道：key = (作用域对象, 事件类型) 的组合键，value = 通道
        readonly Dictionary<ScopedEventKey, object> m_ScopedChannels = new();

        // ================= 订阅：全局 =================
        public void Subscribe<TEvent>(Action<TEvent> listener) where TEvent : struct, IEvent
        {
            Type eventType = typeof(TEvent);                    // 取出事件的类型对象
            if (!m_Channels.TryGetValue(eventType, out object channel))   // 这个类型还没有通道？
            {
                channel = new EventChannel<TEvent>();           // 没有就新建一个强类型通道
                m_Channels.Add(eventType, channel);             // 存进字典
            }

            ((EventChannel<TEvent>)channel).Subscribe(listener); // 转回强类型，往里加监听者
        }

        // ================= 订阅：指定作用域 =================
        public void Subscribe<TEvent>(object scope, Action<TEvent> listener) where TEvent : struct, IEvent
        {
            ScopedEventKey key = new(scope, typeof(TEvent));    // 用"作用域+事件类型"做组合键
            if (!m_ScopedChannels.TryGetValue(key, out object channel))   // 这个作用域下没这个事件的通道？
            {
                channel = new EventChannel<TEvent>();           // 新建
                m_ScopedChannels.Add(key, channel);             // 存进去
            }

            ((EventChannel<TEvent>)channel).Subscribe(listener); // 加入监听者
        }

        // ================= 退订：全局 =================
        public void Unsubscribe<TEvent>(Action<TEvent> listener) where TEvent : struct, IEvent
        {
            if (m_Channels.TryGetValue(typeof(TEvent), out object channel))  // 找到该类型的通道
            {
                EventChannel<TEvent> eventChannel = (EventChannel<TEvent>)channel;
                eventChannel.Unsubscribe(listener);            // 从通道里移除这个监听者
                if (eventChannel.IsEmpty)                      // 通道空了？
                    m_Channels.Remove(typeof(TEvent));         // 连通道一起删掉，省内存
            }
        }

        // ================= 退订：指定作用域 =================
        public void Unsubscribe<TEvent>(object scope, Action<TEvent> listener) where TEvent : struct, IEvent
        {
            ScopedEventKey key = new(scope, typeof(TEvent));
            if (!m_ScopedChannels.TryGetValue(key, out object channel))   // 没找到就直接返回
                return;

            EventChannel<TEvent> eventChannel = (EventChannel<TEvent>)channel;
            eventChannel.Unsubscribe(listener);
            if (eventChannel.IsEmpty)
                m_ScopedChannels.Remove(key);
        }

        // ================= 发布：全局 =================
        public void Publish<TEvent>(TEvent eventData) where TEvent : struct, IEvent
        {
            if (m_Channels.TryGetValue(typeof(TEvent), out object channel))  // 找到该类型通道
            {
                EventChannel<TEvent> eventChannel = (EventChannel<TEvent>)channel;
                eventChannel.Publish(eventData);             // 通知通道里所有监听者
                if (eventChannel.IsEmpty)                    // 如果发布后通道空了（监听者都退订了）
                    m_Channels.Remove(typeof(TEvent));       // 删掉空通道
            }
        }

        // ================= 发布：指定作用域 =================
        public void Publish<TEvent>(object scope, TEvent eventData) where TEvent : struct, IEvent
        {
            ScopedEventKey key = new(scope, typeof(TEvent));
            if (!m_ScopedChannels.TryGetValue(key, out object channel))   // 这个作用域没人监听 → 直接返回
                return;

            EventChannel<TEvent> eventChannel = (EventChannel<TEvent>)channel;
            eventChannel.Publish(eventData);
            if (eventChannel.IsEmpty)
                m_ScopedChannels.Remove(key);
        }

        // ================= 清空全部 =================
        public void Clear()
        {
            m_Channels.Clear();        // 清空全局通道
            m_ScopedChannels.Clear();  // 清空作用域通道
        }

        // ================= 作用域组合键 =================
        // 一个 readonly struct，用来当 m_ScopedChannels 的字典 key
        readonly struct ScopedEventKey : IEquatable<ScopedEventKey>
        {
            readonly object m_Scope;    // 作用域对象（谁在监听）
            readonly Type m_EventType;  // 事件类型

            public ScopedEventKey(object scope, Type eventType)
            {
                m_Scope = scope;        // 存作用域对象
                m_EventType = eventType; // 存事件类型
            }

            // 判断两个 key 是否相等：作用域对象是同一个引用 且 事件类型相同
            public bool Equals(ScopedEventKey other)
            {
                return ReferenceEquals(m_Scope, other.m_Scope) && m_EventType == other.m_EventType;
                // ReferenceEquals：比较的是"是不是同一个对象"，不是内容
            }

            public override bool Equals(object obj)
            {
                return obj is ScopedEventKey other && Equals(other);  // 先判类型再比较
            }

            // 字典靠 GetHashCode 快速定位桶，这里把两个字段的哈希混合起来
            public override int GetHashCode()
            {
                unchecked
                {
                    return RuntimeHelpers.GetHashCode(m_Scope) * 397 ^ m_EventType.GetHashCode();
                    // RuntimeHelpers.GetHashCode：取对象引用本身的哈希（不受重写影响）
                    // * 397 ^：一种常见的哈希混合技巧，减少冲突
                }
            }
        }

        // ================= 真正的"事件通道" =================
        // 一个类型对应一个通道。通道里维护该类型的所有监听者。
        sealed class EventChannel<TEvent> where TEvent : struct, IEvent
        {
            List<Action<TEvent>> m_Listeners = new();        // 当前生效的监听者列表
            List<Action<TEvent>> m_PendingListeners = new(); // 待生效的监听者列表（发布期间暂存）

            int m_PublishDepth;      // 发布嵌套深度（防止嵌套发布时提前提交修改）
            bool m_HasPendingChanges; // 是否有"发布期间的待生效修改"

            // 通道是否为空：不在发布中 且 监听者列表为空
            public bool IsEmpty => m_PublishDepth == 0 && m_Listeners.Count == 0;

            // 订阅：往监听者列表加一个（去重）
            public void Subscribe(Action<TEvent> listener)
            {
                List<Action<TEvent>> listeners = GetMutableListeners(); // 拿到"当前可写"的列表
                if (!listeners.Contains(listener))   // 避免重复订阅
                    listeners.Add(listener);
            }

            // 退订：移除监听者
            public void Unsubscribe(Action<TEvent> listener)
            {
                GetMutableListeners().Remove(listener);
            }

            // 发布：遍历监听者，逐个通知
            public void Publish(TEvent eventData)
            {
                // 发布期间只改"待生效列表"，当前遍历用的列表始终保持稳定
                m_PublishDepth++;                 // 深度 +1（进入一层发布）
                try
                {
                    for (int i = 0; i < m_Listeners.Count; i++)
                        m_Listeners[i]?.Invoke(eventData);   // 调用每个订阅回调
                }
                finally
                {
                    m_PublishDepth--;              // 深度 -1（退出一层发布）
                    // 只有回到最外层（深度归零）且确实有修改，才把待生效列表交换成正式列表
                    if (m_PublishDepth == 0 && m_HasPendingChanges)
                    {
                        (m_Listeners, m_PendingListeners) = (m_PendingListeners, m_Listeners); // 交换两个列表
                        m_PendingListeners.Clear();    // 清空旧的
                        m_HasPendingChanges = false;   // 标记已处理
                    }
                }
            }

            // 拿到"当前可写"的列表：
            // 不在发布中 → 直接返回正式列表（立即生效）
            // 发布中      → 返回待生效列表（先暂存，发布完才生效），避免遍历中修改集合
            List<Action<TEvent>> GetMutableListeners()
            {
                if (m_PublishDepth == 0)
                    return m_Listeners;          // 没在发布，直接改正式列表

                if (!m_HasPendingChanges)        // 第一次在发布中修改
                {
                    m_PendingListeners.AddRange(m_Listeners); // 把正式列表拷贝一份到待生效列表
                    m_HasPendingChanges = true;   // 标记有修改
                }

                return m_PendingListeners;       // 返回待生效列表
            }
        }
    }
}
