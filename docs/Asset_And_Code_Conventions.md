# 目录与命名

沿用现有职责目录；新目录由实际文件和消费者驱动。下面的 Assets 路径相对 Unity 根 `StreamRoar/`。

| 位置 | 内容 |
| --- | --- |
| `Assets/Scenes/` | Launch 与 SampleScene 场景组合 |
| `Assets/Settings/` | UISystemConfig、YooAsset Collector 等项目设置 |
| `Assets/ResourcesAssets/` | YooAsset 运行时资源，当前包含 Audio、Configs 与 UI 目录 |
| `Assets/Scripts/ApplicationLifecycle/` | 组合根与启动 |
| `Assets/Scripts/Gameplay/` | 业务代码接入位置，目前尚无业务脚本 |
| `Assets/Scripts/Infrastructure/` | 资源、配置、事件、计时、池、UI、VFX、Audio、Save、场景与光标 |
| `Assets/ScriptsGenerated/Configs/` | Luban 生成代码 |

## 命名与代码

- 命名空间沿用 StreamRoar；C# 文件与类型同名，一个文件一个 MonoBehaviour。
- 私有字段沿用 `m_`；容易误接的序列化字段用 Tooltip，非显然的公共契约与生命周期用简短注释说明。
- Prefab 与层级按职责命名；运行时资源地址遵循当前 Collector 规则，并保持唯一。
- 必需依赖在所有权边界报告缺失；正常不可执行返回明确结果，避免用兜底资源掩盖接线错误。
- 插件调用留在适配边界。扩展优先写项目代码，生成文件通过生成入口更新。
- 移动或重命名 Unity 资产保留对应 .meta 与 GUID，检查引用和场景覆盖。

配置源表与生成边界见 [Configuration](Configuration_Architecture.md)。
