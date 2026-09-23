# 按改动选择验证

下面是验证菜单，按受影响行为选择；“存在实现”与“已验证运行”分别描述。

自动化 EditMode 约定与跑法见 [Testing](Testing.md)。静态检查和 `dotnet build` 不能替代 Unity Test Runner 或 Play Mode 证据。

| 改动 | 合适的证据 |
| --- | --- |
| 纯文档、技能说明 | 路径和链接、代码事实、指令是否冲突；无需启动 Unity |
| Infrastructure Core 契约（Locator / EventBus / Save 等） | **优先** `Assets/Tests/Editor` 相关 EditMode 用例全绿；再按需补手工路径 |
| 其他 C# 行为 | Unity 导入/编译，以及受影响的最小 Editor 或 Play Mode 路径 |
| Prefab、场景等序列化资产 | Unity 加载/反序列化、受影响引用与运行行为 |
| 运行时生命周期 | 创建、禁用/释放、重新绑定或复用中受影响的路径 |
| 配置 Schema/源表 | 导出或校验、消费端加载，以及变更字段的效果 |

相关检查通过后，仅因新变化、失败或未解决风险扩大范围。无法运行 Unity 时报告具体已做检查与待验证项；
rg、YAML 和 dotnet build 均不证明运行时正确。MSB3644 表示缺少引用程序集，不等于 Unity 编译失败。

## 自动化（EditMode）

- 路径：`Assets/Tests/Editor/`（Editor 程序集，不改动 Infrastructure 目录结构）
- 入口：Window → General → Test Runner → EditMode
- 样板覆盖：`ServiceLocator`、`EventBus`、`JsonSaveService`
- 规则全文：[Testing](Testing.md)

## 运行入口

打开本仓库的 `StreamRoar/`，确认工具返回的目标项目路径，编辑器版本以 ProjectVersion.txt 为准。
从 Launch 启动，等待 YooAsset 和 UI 就绪并进入 SampleScene。直接运行 SampleScene 不能替代服务启动验证。
退出后检查 Console 新增问题，以及是否意外保存了场景或资产。

## 专项路径

| 领域 | 关键观察点 |
| --- | --- |
| 启动 | 服务注册、YooAsset 就绪、UI 启动和场景加载；退出时注销、资源释放、timeScale 恢复 |
| 事件与时间 | scope 隔离、订阅/取消配对；计时器结束或取消；暂停及慢动作按预期时间域推进 |
| UI/池 | Scope、开关竞态、注册/注销、复用状态重置；世界 Widget 需先配置世界空间层和 Widget 条目 |
| 资源 | 空目录与子目录查询、类型过滤、缓存复用和释放；EditorSimulate 不替代 Offline 包验证 |
| VFX | 复用模式、延迟释放、跟随目标失效和池复用后状态 |
| Audio | 播放/停止、跟随目标失效、循环音频释放、Mixer 路由与音量 |
| Save | 缺失与损坏数据区分、覆盖失败保留旧文件；用独立临时数据，目标平台/AOT 按需求验证；契约回归见 EditMode |
| 配置接入 | 接入 Tables 构造及 IConfigProvider 注册后，再验证 TbTest 加载、查询和退出注销 |

Prefab 专项按改动检查组件、挂点、层、嵌套实例、变体与场景覆盖。
当前 UI 配置没有屏幕元素或 Widget，进入 SampleScene 本身不能证明相关 UI 行为通过。

本文件提供可复用验证路径，没有引用其他项目的通过记录。普通任务在交付中说明本次结果即可，无需追加执行日志。
