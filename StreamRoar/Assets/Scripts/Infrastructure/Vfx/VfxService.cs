using System;
using System.Collections.Generic;
using UnityEngine;

namespace StreamRoar.Infrastructure
{
    public sealed class VfxService : IVfxService, IDisposable
    {
        readonly IAssetProvider m_Assets;
        readonly ITimerService m_Timer;
        readonly Transform m_PoolRoot;
        readonly Dictionary<string, PrefabPool<PooledVfx>> m_Pools = new();
        readonly Dictionary<PooledVfx, TimerHandle> m_ReleaseTimers = new();

        public VfxService(IAssetProvider assets, ITimerService timer, Transform poolRoot)
        {
            m_Assets = assets;
            m_Timer = timer;
            m_PoolRoot = poolRoot;
        }

        public PooledVfx Play(VfxPreset preset, in VfxSpawnRequest request)
        {
            if (preset == null || !preset.IsValid)
                return null;

            if (preset.ReuseMode == VfxReuseMode.OneShot)
                return PlayOneShot(preset, request);

            return PlayPooled(preset, request);
        }

        public void Dispose()
        {
            foreach (KeyValuePair<PooledVfx, TimerHandle> pair in m_ReleaseTimers)
                m_Timer.Remove(pair.Value);

            foreach (KeyValuePair<string, PrefabPool<PooledVfx>> pair in m_Pools)
                pair.Value.Dispose();

            m_Pools.Clear();
            m_ReleaseTimers.Clear();
        }

        PooledVfx PlayPooled(VfxPreset preset, in VfxSpawnRequest request)
        {
            PrefabPool<PooledVfx> pool = GetOrCreatePool(preset);
            PooledVfx instance = pool.Get(m_PoolRoot);

            ApplyRequest(instance, request);
            ScheduleRelease(instance, preset.ReleaseDelay, () => pool.Release(instance));
            return instance;
        }

        PooledVfx PlayOneShot(VfxPreset preset, in VfxSpawnRequest request)
        {
            PooledVfx prefab = m_Assets.LoadComponent<PooledVfx>(preset.AssetKey);
            PooledVfx instance = UnityEngine.Object.Instantiate(prefab, m_PoolRoot, false);

            instance.gameObject.SetActive(false);
            ApplyRequest(instance, request);
            instance.gameObject.SetActive(true);
            instance.OnPoolRent();

            ScheduleRelease(instance, preset.ReleaseDelay, () => DestroyOneShot(instance));
            return instance;
        }

        static void ApplyRequest(PooledVfx instance, in VfxSpawnRequest request)
        {
            instance.transform.SetPositionAndRotation(request.Position, request.Rotation);
            instance.SetScale(request.Scale);
            instance.SetSpeed(request.Speed);
            instance.SetFollow(request.FollowTarget, request.FollowMode);
        }

        static void DestroyOneShot(PooledVfx instance)
        {
            UnityEngine.Object.Destroy(instance.gameObject);
        }

        void ScheduleRelease(PooledVfx instance, float releaseDelay, Action release)
        {
            TimerHandle timer = GetTimerHandle(instance);
            Action releaseInstance = () =>
            {
                release();
                m_ReleaseTimers.Remove(instance);
            };

            m_Timer.Every(instance, 0.05f, timer, () =>
            {
                if (instance && instance.IsAlive())
                    return;

                m_Timer.Remove(timer);
                if (releaseDelay <= 0f)
                {
                    releaseInstance();
                    return;
                }

                m_Timer.Delay(instance, releaseDelay, timer, releaseInstance);
            });
        }

        PrefabPool<PooledVfx> GetOrCreatePool(VfxPreset preset)
        {
            string assetKey = preset.AssetKey;
            if (m_Pools.TryGetValue(assetKey, out PrefabPool<PooledVfx> pool))
                return pool;

            pool = new PrefabPool<PooledVfx>(
                m_Assets,
                assetKey,
                m_PoolRoot,
                preset.InitialCapacity,
                preset.MinCachedCount);
            pool.Prewarm();
            m_Pools.Add(assetKey, pool);
            return pool;
        }

        TimerHandle GetTimerHandle(PooledVfx instance)
        {
            if (m_ReleaseTimers.TryGetValue(instance, out TimerHandle handle))
                return handle;

            handle = new TimerHandle();
            m_ReleaseTimers.Add(instance, handle);
            return handle;
        }
    }
}
