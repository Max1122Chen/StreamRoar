# UI 与运行时基础设施

装配顺序见 [Architecture](Architecture.md)，调用示例见 [API Reference](../.agents/skills/infrastructure-usage/references/api.md)。
本页说明所有权与生命周期。

## 服务边界

| 服务 | 所有权与使用边界 |
| --- | --- |
| ServiceLocator | 应用级注册；重复注册覆盖，未注册 Resolve 返回 null，Unregister 只移除同一实例 |
| IAssetProvider | 加载 Unity Object，调用方负责实例化；YooAsset 实现持有缓存句柄并在 Dispose 释放 |
| IEventBus | 强类型同步事件；跨对象通知用全局，单个所有者的状态通知用对象 scope |
| ITimerService | 组合根按 deltaTime 推进；消费者持有 TimerHandle，在生命周期结束时移除 |
| IGameTimeService | 集中写 timeScale；按 unscaledDeltaTime 推进慢动作，暂停优先，Reset 恢复为 1 |
| ISceneNavigator | 加载场景并转发事件；当前未实现完整加载界面、取消和恢复流程 |
| IAudioService | 播放、跟随、Mixer 音量与 AudioSource 复用；循环声音由消费者 Stop |
| ISaveService | 同步读写完整数据；业务默认值、保存时机与版本迁移归消费者 |

上述服务由 ApplicationController 装配。IUIService 由 UIManager 注册，实际实现为 UIElementManager。
IConfigProvider 与 ICursorService 当前未接入启动；接口存在不代表可直接 Resolve，见 [Architecture](Architecture.md)。

## UI 装配与作用域

```text
ApplicationController -> UIRuntimeBootstrap.Boot -> UIRuntime
                                              ├── UIManager
                                              └── UIElementManager
```

UISystemConfig 定义 Root、CanvasScaler、层、Scope 和元素/Widget 资源及池配置。
UIManager 管 Scope、打开/关闭与 ESC 返回栈；UIElementManager 管实例、过渡和池。
当前配置见 [UISystemConfig.asset](../StreamRoar/Assets/Settings/UISystemConfig.asset)：
Background、Normal、Popup、Top 四个屏幕层；Launch 映射 None，SampleScene 映射 Gameplay。
Panels、Windows、Popups、Widgets、GlobalElements 和 ScopeElements 当前均为空。

世界 Widget 源实现 IWorldWidget，提供 WidgetId、Anchor、Offset、Bind 和 Unbind。
LateUpdate 处理创建、绑定及位置/朝向刷新，使用 World Space Canvas；接入前需补充对应世界层与 Widget 配置。
绑定与复用时由消费者重置业务显示值，注销时释放实例；直接 CreateWidget 的实例通过 ReleaseWidget 归还。

IgnoreDepth 由 Widget 条目显式开启，要求 Prefab 根的 UIWidgetRenderSettings 提供忽略深度材质。
当前没有配置使用该能力；开启后的可见性取决于实际材质，不能依赖 Canvas sortingOrder 消除 3D 遮挡。

## 资源与池

LoadAll<T> 查询目录及子目录，空路径表示资源根。两个 Provider 统一路径分隔符与首尾斜杠。
YooAsset 批量查询先加载主资源再判断类型，仅缓存匹配句柄，不匹配立即 Release；单项与批量加载复用缓存。
混合目录存在同步探测成本，优先查询职责明确的小目录。Release 表示释放引用，不保证底层资源立即卸载。

PrefabPool<T> 支持预热、按需扩容、压力缓存与空闲衰减，使用租借集合检查归还所有权。
外部或重复归还的有效实例会报错，销毁/null 实例归还为空操作。
消费者负责在租借/归还或 Bind/Unbind 中重置状态，详见 [Pooling README](../StreamRoar/Assets/Scripts/Infrastructure/Pooling/README.md)。

VfxPreset 保存资源 key、复用模式、释放延迟和容量；IVfxService.Play 接收本次位姿、速度与跟随参数。
VfxService 持有 PooledVfx 池与释放计时器，退出时清理。

## Audio 与 Save

[Audio README](../StreamRoar/Assets/Scripts/Infrastructure/Audio/README.md) 说明 AudioPreset、AudioBus、
2D/3D/跟随播放及停止责任。Mixer 参数缺失是配置错误；Audio 当前不负责偏好持久化。

[Save README](../StreamRoar/Assets/Scripts/Infrastructure/Core/Save/README.md) 说明 JSON 文件契约。
默认 JsonSaveService 目录为 persistentDataPath/Saves；先序列化和写临时文件，再替换正式文件。
缺失文件与损坏数据分开处理；退出只注销服务，不自动保存。

Settings、完整场景加载流程与资源生命周期演进按实际需求推进，待办见
[Infrastructure Roadmap](../StreamRoar/Assets/Scripts/Infrastructure/ROADMAP.md)；受影响路径按 [Verification](Verification.md) 验证。
