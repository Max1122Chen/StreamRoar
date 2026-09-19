namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 统一管理暂停和短时慢动作的全局时间入口。
    /// </summary>
    public interface IGameTimeService
    {
        /// <summary>
        /// 当前是否暂停。
        /// </summary>
        bool IsPaused { get; }

        /// <summary>
        /// 设置暂停状态。
        /// </summary>
        void SetPaused(bool paused);

        /// <summary>
        /// 使用默认过渡时间播放一段慢动作。
        /// </summary>
        void PlaySlowMotion(float timeScale, float duration);

        /// <summary>
        /// 播放一段按真实时间结束的慢动作。总时长包含进入和退出过渡。
        /// 目标倍率的保持时间约为总时长减去两段过渡时间。
        /// </summary>
        void PlaySlowMotion(
            float timeScale,
            float duration,
            float startTransitionDuration,
            float endTransitionDuration);

        /// <summary>
        /// 使用未缩放时间推进慢动作。
        /// </summary>
        void Tick(float unscaledDeltaTime);

        /// <summary>
        /// 恢复默认时间状态。
        /// </summary>
        void Reset();
    }
}
