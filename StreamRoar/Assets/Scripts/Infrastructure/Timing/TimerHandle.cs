using StreamRoar;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 计时器句柄，由 TimerService 写入、查询和清空。
    /// </summary>
    public sealed class TimerHandle
    {
        /// <summary>
        /// 当前计时器 Id。
        /// </summary>
        public int Id { get; internal set; } = GlobalConfig.Timer.InvalidId;

        /// <summary>
        /// 计时总时长。
        /// </summary>
        public float Duration { get; internal set; }

        /// <summary>
        /// 剩余时间。
        /// </summary>
        public float RemainingTime { get; internal set; }

        /// <summary>
        /// 是否有正在运行的计时器。
        /// </summary>
        public bool IsActive => Id != GlobalConfig.Timer.InvalidId;

        /// <summary>
        /// 归一化进度。
        /// </summary>
        public float Progress => Duration > 0f ? 1f - RemainingTime / Duration : 1f;

        internal void Clear()
        {
            Id = GlobalConfig.Timer.InvalidId;
            Duration = 0f;
            RemainingTime = 0f;
        }
    }
}
