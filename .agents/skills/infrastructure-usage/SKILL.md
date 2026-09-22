---
name: infrastructure-usage
description: 接入或修改 StreamRoar Infrastructure 服务调用时，查阅项目 API 与生命周期。
---

# Infrastructure 接入

命名空间 `StreamRoar.Infrastructure`，源码在 `StreamRoar/Assets/Scripts/Infrastructure/`。

按当前接入的服务读取 [API Reference](references/api.md) 中对应标题即可，无需加载全部示例：

- 资源与配置：`IAssetProvider`、`IConfigProvider`
- 事件与时间：`IEventBus`、`ITimerService`、`IGameTimeService`
- UI 与表现：`IUIService`、`PrefabPool<T>`、`IVfxService`、`IAudioService`
- 存档与场景：`ISaveService`、`ISceneNavigator`
- 应用装配：`ServiceLocator`、`ICursorService`

签名和生命周期以当前接口及消费者为准。应用服务由组合根注册；接入时明确订阅、计时器、池对象与播放句柄的释放责任。
详细模块契约位于源码旁的 Audio、Save、Pooling README；仅在相关行为需要时读取。
