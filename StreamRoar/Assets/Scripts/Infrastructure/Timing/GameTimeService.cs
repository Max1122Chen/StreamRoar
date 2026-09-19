using UnityEngine;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 暂停优先，并用真实时间结束带平滑过渡的短时慢动作。
    /// </summary>
    public sealed class GameTimeService : IGameTimeService
    {
        const float DefaultTransitionDuration = 0.1f;

        bool m_IsPaused;
        float m_SlowMotionRemaining;
        float m_EndTransitionDuration;
        float m_CurrentScale = 1f;
        float m_TargetScale = 1f;
        float m_TransitionSpeed;
        bool m_IsEndingSlowMotion;

        public bool IsPaused => m_IsPaused;

        public void SetPaused(bool paused)
        {
            m_IsPaused = paused;
            ApplyTimeScale();
        }

        /// <summary>
        /// 使用默认过渡时间播放一段慢动作。
        /// </summary>
        public void PlaySlowMotion(float timeScale, float duration)
        {
            PlaySlowMotion(timeScale, duration, DefaultTransitionDuration, DefaultTransitionDuration);
        }

        /// <summary>
        /// 按真实时间播放慢动作，总时长包含进入和退出过渡。
        /// 目标倍率的保持时间约为总时长减去两段过渡时间。
        /// </summary>
        public void PlaySlowMotion(
            float timeScale,
            float duration,
            float startTransitionDuration,
            float endTransitionDuration)
        {
            m_SlowMotionRemaining = duration;
            m_EndTransitionDuration = endTransitionDuration;
            m_IsEndingSlowMotion = false;
            BeginTransition(timeScale, startTransitionDuration);
        }

        public void Tick(float unscaledDeltaTime)
        {
            if (m_SlowMotionRemaining > 0f)
            {
                m_SlowMotionRemaining -= unscaledDeltaTime;
                if (!m_IsEndingSlowMotion && m_SlowMotionRemaining <= m_EndTransitionDuration)
                {
                    m_IsEndingSlowMotion = true;
                    BeginTransition(1f, m_EndTransitionDuration);
                }

                if (m_SlowMotionRemaining <= 0f)
                    m_SlowMotionRemaining = 0f;
            }

            if (m_CurrentScale == m_TargetScale)
                return;

            m_CurrentScale = Mathf.MoveTowards(
                m_CurrentScale,
                m_TargetScale,
                m_TransitionSpeed * unscaledDeltaTime);
            ApplyTimeScale();
        }

        public void Reset()
        {
            m_IsPaused = false;
            m_SlowMotionRemaining = 0f;
            m_EndTransitionDuration = 0f;
            m_CurrentScale = 1f;
            m_TargetScale = 1f;
            m_TransitionSpeed = 0f;
            m_IsEndingSlowMotion = false;
            Time.timeScale = 1f;
        }

        void ApplyTimeScale()
        {
            Time.timeScale = m_IsPaused ? 0f : m_CurrentScale;
        }

        void BeginTransition(float targetScale, float duration)
        {
            m_TargetScale = targetScale;
            m_TransitionSpeed = Mathf.Abs(m_CurrentScale - targetScale) / duration;
        }
    }
}
