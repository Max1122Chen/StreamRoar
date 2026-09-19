---
name: infrastructure-usage
description: 使用项目 Infrastructure API 接入资源、配置、事件、计时、对象池、UI、VFX、音频、存档和场景服务时使用。
---

# Infrastructure API Reference

- Namespace: `StreamRoar.Infrastructure`
- Source: `StreamRoar/Assets/Scripts/Infrastructure`
- 必需依赖缺失时直接暴露错误，不增加默认资源、静默重试或空捕获。

## ServiceLocator

应用级服务注册表。服务注册和注销由组合根负责。

| API | 说明 |
| --- | --- |
| `Register<T>(T service)` | 按类型注册或替换服务。 |
| `Resolve<T>()` | 获取已注册的服务。 |
| `Unregister<T>(T service)` | 注册项仍指向同一实例时将其移除。 |

```csharp
ServiceLocator.Register<IEventBus>(eventBus);
IEventBus events = ServiceLocator.Resolve<IEventBus>();
ServiceLocator.Unregister<IEventBus>(eventBus);
```

## IAssetProvider

按 key 加载运行时资源，实例化仍由调用方负责。

| API | 返回值 |
| --- | --- |
| `Load<T>(string key)` | `T` 类型资源。 |
| `LoadAll<T>(string path)` | 指定目录中的全部 `T` 类型资源。 |
| `LoadComponent<T>(string key)` | Prefab 根节点上的 `T` 组件。 |

```csharp
IAssetProvider assets = ServiceLocator.Resolve<IAssetProvider>();
GameObject prefab = assets.Load<GameObject>(assetKey);
AudioBus[] buses = assets.LoadAll<AudioBus>("Audio/Buses");
RectTransform view = assets.LoadComponent<RectTransform>(assetKey);
```

## IConfigProvider

按表契约获取运行时配置表。

| API | 返回值 |
| --- | --- |
| `GetTable<TTable>()` | 已注册的 `TTable` 配置表。 |

```csharp
IConfigProvider configs = ServiceLocator.Resolve<IConfigProvider>();
TTable table = configs.GetTable<TTable>();
```

## IEventBus

强类型同步事件。跨对象通知使用全局订阅，单个所有者的状态使用 scope 订阅。

| API | 说明 |
| --- | --- |
| `Subscribe<TEvent>(listener)` | 订阅全局事件。 |
| `Subscribe<TEvent>(scope, listener)` | 订阅指定 scope 的事件。 |
| `Unsubscribe<TEvent>(listener)` | 取消全局订阅。 |
| `Unsubscribe<TEvent>(scope, listener)` | 取消指定 scope 的订阅。 |
| `Publish<TEvent>(eventData)` | 发布全局事件。 |
| `Publish<TEvent>(scope, eventData)` | 在指定 scope 发布事件。 |
| `Clear()` | 清除全部订阅。 |

```csharp
events.Subscribe<ChangedEvent>(this);
events.Publish(new ChangedEvent(value));
events.Unsubscribe<ChangedEvent>(this);

events.Subscribe<ChangedEvent>(owner, this);
events.Publish(owner, new ChangedEvent(value));
events.Unsubscribe<ChangedEvent>(owner, this);
```

## ITimerService

调度延迟和循环回调。调用方持有 `TimerHandle` 并负责取消。

| API | 说明 |
| --- | --- |
| `Delay(owner, duration, handle, callback)` | 在 `duration` 后执行一次。 |
| `Every(owner, interval, handle, callback)` | 按 `interval` 循环执行。 |
| `Remove(handle)` | 移除单个计时器。 |
| `RemoveByOwner(owner)` | 移除指定 owner 的全部计时器。 |
| `Tick(deltaTime)` | 推进计时器，由组合根调用。 |
| `Clear()` | 移除全部计时器。 |

```csharp
readonly TimerHandle m_Timer = new();

timerService.Delay(this, delay, m_Timer, OnElapsed);
timerService.Every(this, interval, m_Timer, OnTick);
timerService.Remove(m_Timer);
```

## IGameTimeService

统一控制暂停和短时慢动作，使用未缩放时间推进。

| API | 说明 |
| --- | --- |
| `IsPaused` | 当前暂停状态。 |
| `SetPaused(bool paused)` | 暂停或恢复。 |
| `PlaySlowMotion(timeScale, duration)` | 使用默认过渡播放慢动作。 |
| `PlaySlowMotion(timeScale, duration, startTransition, endTransition)` | 使用指定过渡播放慢动作。 |
| `Tick(unscaledDeltaTime)` | 推进慢动作，由组合根调用。 |
| `Reset()` | 恢复默认时间状态。 |

```csharp
gameTime.SetPaused(true);
gameTime.SetPaused(false);
gameTime.PlaySlowMotion(0.2f, 0.5f, 0.1f, 0.15f);
```

## PrefabPool<T>

复用 Prefab 组件。`T` 可实现 `IPoolLifecycle`，在租借和归还时重置状态。

| API | 说明 |
| --- | --- |
| `PrefabPool(assets, key, root, initialCapacity, minCachedCount)` | 创建对象池。 |
| `Prewarm()` | 创建初始缓存实例。 |
| `Get(parent)` | 在 `parent` 下租借实例。 |
| `Release(instance)` | 归还已租借实例。 |
| `Dispose()` | 销毁对象池持有的全部实例。 |

```csharp
var pool = new PrefabPool<PooledView>(
    assets, assetKey, poolRoot, initialCapacity, minCachedCount);

pool.Prewarm();
PooledView instance = pool.Get(parent);
pool.Release(instance);
pool.Dispose();
```

## IUIService

控制屏幕元素和世界 Widget。直接创建的 Widget 由 `UIElementManager` 释放。

| API | 说明 |
| --- | --- |
| `Open(string id)` | 打开当前作用域允许的屏幕元素。 |
| `Close(string id)` | 关闭已加载的屏幕元素。 |
| `CreateWidget(string id)` | 创建动态 Widget。 |
| `RegisterWorldWidget(IWorldWidget widget)` | 注册世界 Widget 数据源。 |
| `UnregisterWorldWidget(IWorldWidget widget)` | 注销并释放世界 Widget。 |
| `UIElementManager.ReleaseWidget(GameObject instance)` | 释放直接创建的 Widget。 |

```csharp
IUIService ui = ServiceLocator.Resolve<IUIService>();

ui.Open(elementId);
ui.Close(elementId);
ui.RegisterWorldWidget(worldWidget);
ui.UnregisterWorldWidget(worldWidget);

GameObject widget = elementManager.CreateWidget(widgetId);
elementManager.ReleaseWidget(widget);
```

## IVfxService

使用 `VfxPreset` 配置和单次播放的变换、速度、跟随参数播放特效。

| API | 返回值 |
| --- | --- |
| `Play(VfxPreset preset, in VfxSpawnRequest request)` | 当前播放的 `PooledVfx` 实例。 |

```csharp
var request = new VfxSpawnRequest(
    position,
    rotation,
    scale,
    speed,
    followTarget,
    VfxFollowMode.None);

IVfxService vfx = ServiceLocator.Resolve<IVfxService>();
PooledVfx instance = vfx.Play(preset, request);
```

## IAudioService

播放 2D、固定位置 3D 或跟随目标的音频，并控制 Mixer Bus 音量。

| API | 返回值 |
| --- | --- |
| `Play2D(AudioPreset preset)` | 本次播放的 `AudioHandle`。 |
| `PlayAt(AudioPreset preset, Vector3 position)` | 本次播放的 `AudioHandle`。 |
| `PlayFollow(AudioPreset preset, Transform target)` | 本次播放的 `AudioHandle`。 |
| `Stop(AudioHandle handle)` | 成功停止活动播放时返回 `true`。 |
| `SetVolume(AudioBus bus, float normalizedVolume)` | 无返回值，音量范围为 `0..1`。 |

```csharp
IAudioService audio = ServiceLocator.Resolve<IAudioService>();

audio.Play2D(uiPreset);
audio.PlayAt(hitPreset, hitPoint);

AudioHandle handle = audio.PlayFollow(loopPreset, target);
audio.Stop(handle);
audio.SetVolume(sfxBus, 0.8f);
```

## ISaveService

按存档标识同步读写完整数据对象，不维护业务状态或自动保存。

| API | 说明 |
| --- | --- |
| `TryLoad<T>(saveId, out data)` | 文件不存在返回 false，读取或转换失败抛出异常。 |
| `Save<T>(saveId, data)` | 立即写盘，覆盖同标识存档。 |
| `Exists(saveId)` | 检查文件存在，不校验内容。 |
| `Delete(saveId)` | 删除存档，不存在时无操作。 |

```csharp
ISaveService saves = ServiceLocator.Resolve<ISaveService>();
if (saves.TryLoad("slot_01", out ProgressData data))
    RestoreProgress(data);
saves.Save("slot_01", progress);
```

`ProgressData` 和恢复行为由消费者定义。支持普通对象、列表、字典和标量，不直接保存 Unity 对象图。
默认实现 `JsonSaveService`，目录由构造参数提供，应用入口使用 `persistentDataPath/Saves`。
标识使用 `slot_01` 等稳定名称，不传入文件路径。Schema 与迁移策略由消费者定义。

## ISceneNavigator

通过统一导航边界发起场景加载。

| API | 返回值 |
| --- | --- |
| `LoadScene(string sceneName, LoadSceneMode mode)` | 场景加载的 `AsyncOperation`。 |

```csharp
ISceneNavigator scenes = ServiceLocator.Resolve<ISceneNavigator>();
AsyncOperation operation = scenes.LoadScene(sceneName, LoadSceneMode.Single);
```

## ICursorService

控制光标锁定与显示。启动、帧更新和焦点变化由组合根驱动。

| API | 说明 |
| --- | --- |
| `IsLocked` | 当前锁定状态。 |
| `Lock()` | 锁定并隐藏光标。 |
| `Unlock()` | 解锁并显示光标。 |
| `SetLocked(bool locked)` | 显式设置锁定状态。 |
| `SetVisible(bool visible)` | 显式设置显示状态。 |
| `ApplyStartupState()` | 应用启动状态。 |
| `Tick()` | 刷新运行时状态。 |
| `HandleApplicationFocus(bool focus)` | 处理焦点变化。 |

```csharp
ICursorService cursor = ServiceLocator.Resolve<ICursorService>();

cursor.Lock();
cursor.Unlock();
cursor.SetLocked(locked);
cursor.SetVisible(visible);
```
