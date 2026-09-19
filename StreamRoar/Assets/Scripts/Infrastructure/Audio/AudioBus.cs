using UnityEngine;
using UnityEngine.Audio;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 定义播放输出与可控制音量参数之间的映射。
    /// </summary>
    [CreateAssetMenu(menuName = "StreamRoar/Audio/Audio Bus")]
    public sealed class AudioBus : ScriptableObject
    {
        [SerializeField, Tooltip("音频输出的 Mixer Group")]
        AudioMixerGroup m_Output;

        [SerializeField, Tooltip("Mixer 中公开的音量参数名")]
        string m_VolumeParameter;

        public AudioMixerGroup Output => m_Output;
        public string VolumeParameter => m_VolumeParameter;
    }
}
