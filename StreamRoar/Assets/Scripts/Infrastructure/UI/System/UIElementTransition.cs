using System.Collections;
using UnityEngine;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// UI 元素开关场动画。
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    [DisallowMultipleComponent]
    public sealed class UIElementTransition : MonoBehaviour
    {
        [Header("开场动画")]
        [Tooltip("开场缩放起点。")]
        [Min(0.01f)]
        [SerializeField]
        float m_OpenFromScale = 0.96f;

        [Tooltip("开场透明起点。")]
        [Range(0f, 1f)]
        [SerializeField]
        float m_OpenFromAlpha = 0f;

        [Tooltip("开场时长（秒）。")]
        [Min(0.01f)]
        [SerializeField]
        float m_OpenDuration = 0.2f;

        [Header("关场动画")]
        [Tooltip("关场缩放终点。")]
        [Min(0.01f)]
        [SerializeField]
        float m_CloseToScale = 0.96f;

        [Tooltip("关场透明终点。")]
        [Range(0f, 1f)]
        [SerializeField]
        float m_CloseToAlpha = 0f;

        [Tooltip("关场时长（秒）。")]
        [Min(0.01f)]
        [SerializeField]
        float m_CloseDuration = 0.16f;

        [Header("播放设置")]
        [Tooltip("是否忽略 Time.timeScale。")]
        [SerializeField]
        bool m_UseUnscaledTime = true;

        RectTransform m_Target;
        CanvasGroup m_CanvasGroup;

        void Awake()
        {
            m_Target = transform as RectTransform;
            m_CanvasGroup = GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// 播放 UI 元素开场动画并恢复交互。
        /// </summary>
        public IEnumerator PlayOpen()
        {
            PrepareForOpen();
            yield return PlayRoutine(m_OpenFromScale, 1f, m_OpenFromAlpha, 1f, m_OpenDuration);

            m_CanvasGroup.interactable = true;
            m_CanvasGroup.blocksRaycasts = true;
        }

        /// <summary>
        /// 关闭交互并播放 UI 元素退场动画。
        /// </summary>
        public IEnumerator PlayClose()
        {
            m_CanvasGroup.interactable = false;
            m_CanvasGroup.blocksRaycasts = false;

            yield return PlayRoutine(
                m_Target.localScale.x,
                m_CloseToScale,
                m_CanvasGroup.alpha,
                m_CloseToAlpha,
                m_CloseDuration);
        }

        public void PrepareForOpen()
        {
            m_Target.localScale = Vector3.one * m_OpenFromScale;
            m_CanvasGroup.alpha = m_OpenFromAlpha;
            m_CanvasGroup.interactable = false;
            m_CanvasGroup.blocksRaycasts = false;
        }

        IEnumerator PlayRoutine(float fromScale, float toScale, float fromAlpha, float toAlpha, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += m_UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = EaseOutCubic(t);
                m_Target.localScale = Vector3.one * Mathf.Lerp(fromScale, toScale, eased);
                m_CanvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, eased);
                yield return null;
            }

            m_Target.localScale = Vector3.one * toScale;
            m_CanvasGroup.alpha = toAlpha;
        }

        static float EaseOutCubic(float value)
        {
            float inverse = 1f - value;
            return 1f - inverse * inverse * inverse;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            m_OpenFromScale = Mathf.Max(0.01f, m_OpenFromScale);
            m_OpenDuration = Mathf.Max(0.01f, m_OpenDuration);
            m_CloseToScale = Mathf.Max(0.01f, m_CloseToScale);
            m_CloseDuration = Mathf.Max(0.01f, m_CloseDuration);
        }
#endif
    }
}
