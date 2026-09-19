using System;
using System.Collections.Generic;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 基于 owner 和 handle 的运行时计时服务。
    /// </summary>
    public sealed class TimerService : ITimerService
    {
        readonly List<TimerTask> m_Timers = new();

        int m_NextId = 1;

        public void Delay(object owner, float duration, TimerHandle handle, Action callback = null)
        {
            AddTimer(owner, duration, false, handle, callback);
        }

        public void Every(object owner, float interval, TimerHandle handle, Action callback)
        {
            AddTimer(owner, interval, true, handle, callback);
        }

        public bool Remove(TimerHandle handle)
        {
            for (int i = 0; i < m_Timers.Count; i++)
            {
                if (m_Timers[i].Id != handle.Id)
                    continue;

                m_Timers[i].Remove();
                return true;
            }

            return false;
        }

        public int RemoveByOwner(object owner)
        {
            int removed = 0;
            for (int i = 0; i < m_Timers.Count; i++)
            {
                if (m_Timers[i].Removed || !ReferenceEquals(m_Timers[i].Owner, owner))
                    continue;

                m_Timers[i].Remove();
                removed++;
            }

            return removed;
        }

        public void Clear()
        {
            for (int i = 0; i < m_Timers.Count; i++)
                m_Timers[i].Remove();
        }

        public void Tick(float deltaTime)
        {
            int timerCount = m_Timers.Count;

            try
            {
                for (int i = 0; i < timerCount; i++)
                    m_Timers[i].Tick(deltaTime);
            }
            finally
            {
                RemoveMarkedTimers();
            }
        }

        void AddTimer(object owner, float duration, bool repeat, TimerHandle handle, Action callback)
        {
            Remove(handle);

            float interval = duration <= 0f ? 0f : duration;
            TimerTask timer = new TimerTask(m_NextId++, owner, interval, repeat, handle, callback);
            handle.Id = timer.Id;
            handle.Duration = interval;
            handle.RemainingTime = interval;
            m_Timers.Add(timer);
        }

        void RemoveMarkedTimers()
        {
            for (int i = m_Timers.Count - 1; i >= 0; i--)
            {
                if (m_Timers[i].Removed)
                    m_Timers.RemoveAt(i);
            }
        }

        static bool IsOwnerDestroyed(object owner)
        {
            return owner is UnityEngine.Object unityObject && !unityObject;
        }

        sealed class TimerTask
        {
            public readonly int Id;
            public readonly object Owner;
            public readonly float Interval;
            public readonly bool Repeat;
            public readonly TimerHandle Handle;
            public readonly Action Callback;

            public float RemainingTime;
            public bool Removed;

            public TimerTask(int id, object owner, float interval, bool repeat, TimerHandle handle, Action callback)
            {
                Id = id;
                Owner = owner;
                Interval = interval;
                Repeat = repeat;
                Handle = handle;
                Callback = callback;
                RemainingTime = interval;
            }

            public void Remove()
            {
                Removed = true;
                Handle.Clear();
            }

            public void Tick(float deltaTime)
            {
                if (Removed)
                    return;

                if (IsOwnerDestroyed(Owner))
                {
                    Remove();
                    return;
                }

                RemainingTime -= deltaTime;
                Handle.RemainingTime = RemainingTime;
                if (RemainingTime > 0f)
                    return;

                RemainingTime = 0f;
                if (Repeat)
                {
                    RemainingTime = Interval;
                    Handle.RemainingTime = RemainingTime;
                }
                else
                    Remove();

                Callback?.Invoke();
            }
        }
    }
}
