using System;

namespace StreamRoar.Infrastructure
{
    public interface ITimerService
    {
        /// <summary>
        /// 延迟指定时间后执行一次。
        /// </summary>
        void Delay(object owner, float duration, TimerHandle handle, Action callback = null);

        /// <summary>
        /// 每隔指定时间重复执行。
        /// </summary>
        void Every(object owner, float interval, TimerHandle handle, Action callback);

        /// <summary>
        /// 移除指定计时器。
        /// </summary>
        bool Remove(TimerHandle handle);

        /// <summary>
        /// 移除指定 owner 注册的全部计时器，并返回移除数量。
        /// </summary>
        int RemoveByOwner(object owner);

        /// <summary>
        /// 推进全部计时器。
        /// </summary>
        void Tick(float deltaTime);

        /// <summary>
        /// 移除全部计时器。
        /// </summary>
        void Clear();
    }
}
