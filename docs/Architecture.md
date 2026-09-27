# StreamRoar 当前架构

当前已提供资源、事件、时间、对象池、UI、VFX、Audio、Save、场景导航与 GameplayTag 基础设施。
Gameplay 尚无具体业务脚本；基础设施中存在接口或实现，不代表已完成业务接入。

## 启动与服务

Unity 项目根是 `StreamRoar/`，当前版本为 2022.3.62f3c1。
`Launch.unity` 的 GameRoot 挂载 ApplicationController 与 UIRuntimeBootstrap，并跨场景保留。

```text
ApplicationController
├── Awake：注册 Timer / GameTime / EventBus / Assets / Audio / Save / Vfx / Scene / GameplayTag
├── Start：等待 YooAsset 就绪 -> Boot UI -> 加载 SampleScene
├── Update：启动完成后驱动 Timer / GameTime / Audio
└── OnDestroy：关闭 UI、注销服务、释放场景监听/事件/VFX/音频/资源并恢复时间
```

启动代码见 [ApplicationController](../StreamRoar/Assets/Scripts/ApplicationLifecycle/ApplicationController.cs)。
Launch 当前使用 DefaultPackage、ResourcesAssets 根与 EditorSimulate；Player 固定使用 Offline。
ResourcesAssetProvider 仅适用于 `Assets/Resources`，不能直接读取现有 ResourcesAssets 布局。

UIRuntimeBootstrap 创建 UIRuntime，添加 UIElementManager 与 UIManager。
UIManager 完成 UI 启动，并将 UIElementManager 注册为 IUIService；退出时负责注销。
UI 配置把 SampleScene 映射到 Gameplay Scope，目前没有注册任何屏幕元素或 Widget。

编辑器的 SceneBootstrapper 默认在进入 Play Mode 时打开 Build Settings 第一场景，当前为 Launch，
退出后恢复之前的场景。该行为可在 `StreamRoar/Play` 菜单关闭；需要服务的验证仍应从 Launch 开始。

## 尚未装配的模块

- 已有 IConfigProvider、RuntimeConfigProvider、LubanBinaryConfigLoader 及生成的测试表，
  ApplicationController 尚未构造 Tables 或注册 IConfigProvider，见 [配置说明](Configuration_Architecture.md)。
- 已有 ICursorService / CursorService，但 ApplicationController 尚未注册、Tick 或转发焦点变化。
- UI 代码支持世界 Widget 与 IgnoreDepth；当前 UISystemConfig 没有世界空间层或 Widget 配置。

这些是当前能力边界，接入由具体需求决定。服务所有权见 [UI 与基础设施](UI_and_Infrastructure.md)。
