using UnityEngine;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 保存一类声音固定的资源、输出与播放参数。
    /// </summary>
    [CreateAssetMenu(menuName = "StreamRoar/Audio/Audio Preset")]
    public sealed class AudioPreset : ScriptableObject
    {
        [SerializeField, Tooltip("音频资源 key")]
        string m_AssetKey;

        [SerializeField, Tooltip("输出总线")]
        AudioBus m_Bus;

        [SerializeField, Tooltip("是否循环播放")]
        bool m_Loop;

        [SerializeField, Range(0f, 1f), Tooltip("单次播放音量")]
        float m_Volume = 1f;

        [SerializeField, Range(0.1f, 3f), Tooltip("单次播放音高")]
        float m_Pitch = 1f;

        [SerializeField, Min(0f), Tooltip("3D 音频开始衰减的距离")]
        float m_MinDistance = 1f;

        [SerializeField, Min(0f), Tooltip("3D 音频不再衰减的距离")]
        float m_MaxDistance = 30f;

        public string AssetKey => m_AssetKey;
        public AudioBus Bus => m_Bus;
        public bool Loop => m_Loop;
        public float Volume => m_Volume;
        public float Pitch => m_Pitch;
        public float MinDistance => m_MinDistance;
        public float MaxDistance => m_MaxDistance;
    }
}
