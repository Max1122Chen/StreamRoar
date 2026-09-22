# 配置与生成边界

## 当前内容

Luban 配置位于仓库同级 `../config/`，工具在 `../tools/`，不包含在本 Git 仓库内。
当前只有 `Defines/test.xml`、`Data/test.xlsx` 定义的 TbTest，行类型 TestRow 包含 Id、Name、Value。
生成命名空间为 `StreamRoar.Configs`；没有角色原型、战斗数值或生成记录等业务表。

| 内容 | 位置，相对仓库根 |
| --- | --- |
| Luban 配置与模板 | `../config/luban.conf`、`../config/Templates/` |
| 导出入口 | `../tools/MAC_ExportConfig.sh`、`../tools/WIN_ExportConfig.bat` |
| 只读检查入口 | `../tools/scripts/MAC_CheckConfig.sh`、`../tools/scripts/WIN_CheckConfig.bat` |
| 生成 C# | `StreamRoar/Assets/ScriptsGenerated/Configs/` |
| 生成二进制 | `StreamRoar/Assets/ResourcesAssets/Configs/` |

导出入口使用仓库同级 config 和 tools，并写入上表两个生成目录。只修改源 Schema、表格或模板，
通过导出更新产物，不手改生成代码。独立克隆仓库后，先确认同级工具和源表已经准备齐全。

## 查询与资源的分工

- IAssetProvider 读取 Unity 资源；LubanBinaryConfigLoader 通过它取得 TextAsset 字节并返回 ByteBuf。
- 生成的 Tables 负责解析 TbTest，并通过 GetAllTables 暴露表索引。
- RuntimeConfigProvider 按表类型提供 GetTable；行查询使用生成表 API。
- Infrastructure 不引用生成业务表类型；构造 Tables、注册和注销配置服务属于组合根的接入职责。

**当前 ApplicationController 尚未执行配置装配。** 导出了 TbTest.bytes 不等于运行时已加载配置，
直接 Resolve<IConfigProvider>() 当前会返回 null。API 参考中的调用片段用于完成装配后的消费者。

YooAsset Collector 收集 `Assets/ResourcesAssets`，使用 AddressByFileName；当前 Tables 请求 `TbTest`。
修改文件名或收集规则时检查地址唯一性和表加载 key，不能把 ResourcesAssets 路径当作 Resources.Load 路径。

## 作者数据与验证

Prefab 管接线，Unity 资产管资源与作者配置，表格管已接入的数据；同一字段只保留一个权威来源。
新增业务表时根据实际消费者确定字段、约束和 ID，当前测试表不构成通用业务 Schema。

Schema 或源表变更按需检查导出、数据加载及受影响字段效果；启动接入变更还需验证注册时机和退出清理。
具体验证范围见 [Verification](Verification.md)，本说明不宣称配置运行链已通过 Play Mode。
