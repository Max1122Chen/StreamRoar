# 对象池使用说明

`PrefabPool<T>` 是一个轻量级 prefab 组件对象池。它通过 `IAssetProvider` 和资源 key 找到 prefab 组件，然后负责实例化、缓存、取出和归还。

## 基础用法

资源 key 使用资源后端约定：`Resources/VFX/HitSpark.prefab` 在 Resources 模式下对应 `VFX/HitSpark`，在 YooAsset 模式下使用 Collector 发布的资源地址。
由调用方在资源后端初始化完成后调用示例的 `Initialize`，传入资源服务与实际 key；不要在资源仍在初始化时预热。

```csharp
using StreamRoar.Infrastructure;
using UnityEngine;

public sealed class HitVfxSpawner : MonoBehaviour
{
    [SerializeField] Transform m_PoolRoot;

    PrefabPool<ParticleSystem> m_HitVfxPool;

    public void Initialize(IAssetProvider assets, string assetKey)
    {
        m_HitVfxPool = new PrefabPool<ParticleSystem>(
            assets,
            assetKey,
            m_PoolRoot,
            initialCapacity: 8,
            minCachedCount: 2
        );
        m_HitVfxPool.Prewarm();
    }

    public void Spawn(Vector3 position, Quaternion rotation, Transform parent = null)
    {
        ParticleSystem instance = m_HitVfxPool.Get(parent);

        instance.transform.SetPositionAndRotation(position, rotation);
        instance.Play();
    }

    public void Despawn(ParticleSystem instance)
    {
        m_HitVfxPool.Release(instance);
    }

    void OnDestroy()
    {
        m_HitVfxPool?.Dispose();
    }
}
```

## 生命周期回调

如果被池管理的组件需要在取出或归还时重置状态，可以实现 `IPoolLifecycle`。

```csharp
using StreamRoar.Infrastructure;
using UnityEngine;

public sealed class PooledProjectile : MonoBehaviour, IPoolLifecycle
{
    Rigidbody m_Rigidbody;

    void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    public void OnPoolRent()
    {
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;
    }

    public void OnPoolReturn()
    {
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;
    }
}
```

## 注意事项

- prefab 上必须挂着 `PrefabPool<T>` 的 `T` 组件，例如 `PrefabPool<ParticleSystem>` 对应 prefab 上的 `ParticleSystem`。
- `poolRoot` 可以为空；不为空时，归还的实例会重新挂到这个节点下。
- 池创建出来的实例不要直接 `Destroy`，应该调用 `Release`。
- `Release` 只接受当前由该池租借的实例；外部实例或重复归还会直接报错。
- `initialCapacity` 决定 `Prewarm` 会提前创建多少个实例。
- `minCachedCount` 决定压力下降后至少保留多少个空闲实例。
- 压力变高时，池会临时提高缓存目标；实例归还时会按时间间隔逐步裁掉多余缓存。
- 对于 VFX 这类共享播放入口，池参数应来自预设而不是每次播放请求；请求只携带这一次的运行时位姿与临时覆盖值。
