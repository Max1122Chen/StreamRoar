using UnityEngine;

namespace StreamRoar.Infrastructure
{
    [CreateAssetMenu(menuName = "StreamRoar/VFX/Vfx Preset")]
    public sealed class VfxPreset : ScriptableObject
    {
        [SerializeField, Tooltip("Resources 下的特效 prefab 路径")]
        string m_AssetKey;
        public string AssetKey => m_AssetKey;

        [SerializeField, Tooltip("实例生命周期")]
        VfxReuseMode m_ReuseMode = VfxReuseMode.Pooled;
        public VfxReuseMode ReuseMode => m_ReuseMode;

        [SerializeField, Min(0f), Tooltip("播放结束后归还秒数")]
        float m_ReleaseDelay;
        public float ReleaseDelay => m_ReleaseDelay;

        [SerializeField, Min(0), Tooltip("初始预热数量")]
        int m_InitialCapacity;
        public int InitialCapacity => m_InitialCapacity;

        [SerializeField, Min(0), Tooltip("最小缓存数量")]
        int m_MinCachedCount;
        public int MinCachedCount => m_MinCachedCount;

        public bool IsValid => !string.IsNullOrEmpty(m_AssetKey);
    }
}
