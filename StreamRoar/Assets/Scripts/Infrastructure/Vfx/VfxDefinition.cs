using System;
using UnityEngine;

namespace StreamRoar.Infrastructure
{
    [Serializable]
    public abstract class VfxDefinition
    {
        [SerializeField, Tooltip("VFX 预设")]
        VfxPreset m_Preset;
        public VfxPreset Preset => m_Preset;
        public bool IsValid => m_Preset != null && m_Preset.IsValid;

        public string AssetKey => m_Preset.AssetKey;
        public VfxReuseMode ReuseMode => m_Preset.ReuseMode;
        public float ReleaseDelay => m_Preset.ReleaseDelay;
        public int InitialCapacity => m_Preset.InitialCapacity;
        public int MinCachedCount => m_Preset.MinCachedCount;

        [SerializeField, Min(0.1f), Tooltip("特效缩放倍数")]
        float m_Scale = 1f;
        public float Scale => Mathf.Max(0.1f, m_Scale);

        [SerializeField, Min(0.1f), Tooltip("播放速度")]
        float m_Speed = 1f;
        public float Speed => m_Speed;
    }
}
