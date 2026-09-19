using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 通过资源提供器加载音频，并管理播放实例及其回收。
    /// </summary>
    public sealed class AudioService : IAudioService, IDisposable
    {
        const float MinimumVolumeDb = -80f;

        readonly struct Playback
        {
            public Playback(int id, AudioSource source, Transform followTarget)
            {
                Id = id;
                Source = source;
                FollowsTarget = followTarget != null;
                FollowTarget = followTarget;
            }

            public int Id { get; }
            public AudioSource Source { get; }
            public bool FollowsTarget { get; }
            public Transform FollowTarget { get; }
        }

        readonly IAssetProvider m_Assets;
        readonly Transform m_Root;
        readonly ObjectPool<AudioSource> m_SourcePool;
        readonly List<Playback> m_ActivePlaybacks = new();
        int m_NextPlaybackId = 1;

        /// <summary>
        /// 创建音频服务，并使用指定节点承载池化的 AudioSource。
        /// </summary>
        public AudioService(IAssetProvider assets, Transform root)
        {
            m_Assets = assets ?? throw new ArgumentNullException(nameof(assets));
            m_Root = root ? root : throw new ArgumentNullException(nameof(root));
            m_SourcePool = new ObjectPool<AudioSource>(
                CreateSource,
                OnGetSource,
                OnReleaseSource,
                DestroySource);
        }

        /// <summary>
        /// 播放不受世界位置影响的 2D 声音。
        /// </summary>
        public AudioHandle Play2D(AudioPreset preset)
        {
            return Play(preset, Vector3.zero, false, null);
        }

        /// <summary>
        /// 在固定世界位置播放 3D 声音。
        /// </summary>
        public AudioHandle PlayAt(AudioPreset preset, Vector3 position)
        {
            return Play(preset, position, true, null);
        }

        /// <summary>
        /// 播放持续跟随目标位置的 3D 声音。
        /// </summary>
        public AudioHandle PlayFollow(AudioPreset preset, Transform target)
        {
            if (!target)
                throw new ArgumentNullException(nameof(target));

            return Play(preset, target.position, true, target);
        }

        /// <summary>
        /// 停止指定播放；播放已经结束时返回 false。
        /// </summary>
        public bool Stop(AudioHandle handle)
        {
            for (int i = m_ActivePlaybacks.Count - 1; i >= 0; i--)
            {
                if (m_ActivePlaybacks[i].Id != handle.Id)
                    continue;

                ReleaseAt(i);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 设置音频总线的归一化音量。
        /// </summary>
        public void SetVolume(AudioBus bus, float normalizedVolume)
        {
            RequireBus(bus);
            if (normalizedVolume < 0f || normalizedVolume > 1f)
                throw new ArgumentOutOfRangeException(nameof(normalizedVolume));

            float volumeDb = normalizedVolume > 0f
                ? Mathf.Max(MinimumVolumeDb, 20f * Mathf.Log10(normalizedVolume))
                : MinimumVolumeDb;

            if (!bus.Output.audioMixer.SetFloat(bus.VolumeParameter, volumeDb))
                throw new InvalidOperationException($"AudioBus 音量参数未公开：{bus.VolumeParameter}");
        }

        /// <summary>
        /// 更新跟随位置并回收已经自然结束的声音。
        /// </summary>
        public void Tick()
        {
            for (int i = m_ActivePlaybacks.Count - 1; i >= 0; i--)
            {
                Playback playback = m_ActivePlaybacks[i];
                if (playback.FollowsTarget && !playback.FollowTarget)
                {
                    ReleaseAt(i);
                    continue;
                }

                if (playback.FollowsTarget)
                    playback.Source.transform.position = playback.FollowTarget.position;

                if (!playback.Source.isPlaying)
                    ReleaseAt(i);
            }
        }

        /// <summary>
        /// 停止活动声音并销毁池内 AudioSource。
        /// </summary>
        public void Dispose()
        {
            for (int i = m_ActivePlaybacks.Count - 1; i >= 0; i--)
                ReleaseAt(i);

            m_SourcePool.Dispose();
        }

        AudioHandle Play(
            AudioPreset preset,
            Vector3 position,
            bool is3D,
            Transform followTarget)
        {
            RequireBus(preset.Bus);
            AudioClip clip = m_Assets.Load<AudioClip>(preset.AssetKey);
            AudioSource source = m_SourcePool.Get();
            ApplyPreset(source, preset, clip, position, is3D);

            int playbackId = m_NextPlaybackId++;
            m_ActivePlaybacks.Add(new Playback(playbackId, source, followTarget));
            source.Play();
            return new AudioHandle(playbackId);
        }

        void ReleaseAt(int index)
        {
            Playback playback = m_ActivePlaybacks[index];
            m_ActivePlaybacks.RemoveAt(index);
            m_SourcePool.Release(playback.Source);
        }

        /// <summary>
        /// 将预设参数与本次播放的空间参数写入 AudioSource。
        /// </summary>
        static void ApplyPreset(
            AudioSource source,
            AudioPreset preset,
            AudioClip clip,
            Vector3 position,
            bool is3D)
        {
            source.transform.position = position;
            source.clip = clip;
            source.outputAudioMixerGroup = preset.Bus.Output;
            source.loop = preset.Loop;
            source.volume = preset.Volume;
            source.pitch = preset.Pitch;
            source.spatialBlend = is3D ? 1f : 0f;
            if (is3D)
            {
                source.minDistance = preset.MinDistance;
                source.maxDistance = preset.MaxDistance;
            }
        }

        AudioSource CreateSource()
        {
            var gameObject = new GameObject("AudioSource");
            gameObject.transform.SetParent(m_Root, false);
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            gameObject.SetActive(false);
            return source;
        }

        static void OnGetSource(AudioSource source)
        {
            source.gameObject.SetActive(true);
        }

        void OnReleaseSource(AudioSource source)
        {
            source.Stop();
            source.clip = null;
            source.outputAudioMixerGroup = null;
            source.loop = false;
            source.volume = 1f;
            source.pitch = 1f;
            source.spatialBlend = 0f;
            source.transform.SetParent(m_Root, false);
            source.gameObject.SetActive(false);
        }

        static void DestroySource(AudioSource source)
        {
            UnityEngine.Object.Destroy(source.gameObject);
        }

        static void RequireBus(AudioBus bus)
        {
            if (!bus.Output)
                throw new InvalidOperationException($"AudioBus 缺少 Mixer Group：{bus.name}");
            if (string.IsNullOrWhiteSpace(bus.VolumeParameter))
                throw new InvalidOperationException($"AudioBus 缺少音量参数：{bus.name}");
        }
    }
}
