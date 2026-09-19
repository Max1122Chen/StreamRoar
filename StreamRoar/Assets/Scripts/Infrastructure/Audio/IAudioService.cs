using UnityEngine;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 提供与具体业务无关的音频播放、停止和总线音量控制。
    /// </summary>
    public interface IAudioService
    {
        /// <summary>
        /// 播放不受世界位置影响的 2D 声音。
        /// </summary>
        AudioHandle Play2D(AudioPreset preset);

        /// <summary>
        /// 在固定世界位置播放 3D 声音。
        /// </summary>
        AudioHandle PlayAt(AudioPreset preset, Vector3 position);

        /// <summary>
        /// 播放跟随目标位置的 3D 声音。
        /// </summary>
        AudioHandle PlayFollow(AudioPreset preset, Transform target);

        /// <summary>
        /// 停止指定播放；声音已经结束时返回 false。
        /// </summary>
        bool Stop(AudioHandle handle);

        /// <summary>
        /// 设置音频总线的归一化音量。
        /// </summary>
        void SetVolume(AudioBus bus, float normalizedVolume);
    }
}
