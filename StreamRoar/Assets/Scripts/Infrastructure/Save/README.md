# Save

按存档标识持久化数据，不管理业务状态或保存时机。

| 类型 | 职责 |
| --- | --- |
| `ISaveService` | 读取、保存、存在检查和删除的统一接口 |
| `JsonSaveService` | JSON 序列化、独立文件存储和临时文件替换 |

## 使用

```csharp
public sealed class ProgressData
{
    public int Level = 1;
    public Dictionary<string, int> Items = new();
}

ISaveService saves = new JsonSaveService(saveDirectory);
if (!saves.TryLoad("slot_01", out ProgressData progress))
    progress = new ProgressData();

progress.Items["potion"] = 3;
saves.Save("slot_01", progress);

bool exists = saves.Exists("slot_01");
saves.Delete("slot_01");
```

示例需要 `System.Collections.Generic` 和 `StreamRoar.Infrastructure` 命名空间。
组合根负责创建服务并注入消费者，存档目录由构造参数指定。

## 文件存储契约

- 标识对应 `<目录>/<标识>.json`。使用 `slot_01` 等稳定名称，不传入路径或扩展名；仅支持小写字母、数字、下划线和连字符，以字母或数字开头。避免操作系统保留文件名。
- `Save` 立即写盘。先完成序列化，再写临时文件，最后替换正式文件；不先删除旧存档。失败直接抛出异常，不自动重试或恢复默认数据。
- `TryLoad` 仅在文件或目录不存在时返回 `false`。空内容、空根对象、无法解析或转换的数据抛出异常。`Exists` 不验证内容。
- `Delete` 删除正式文件，不存在时无操作。失败写入可能留下 `.tmp` 文件，不参与读取，后续保存会覆盖它。
- 同步 API，同一目录使用单一写入方。没有内存缓存、退出自动保存、备份恢复或断电持久化保证。

## 数据与扩展

使用 Unity 官方 `com.unity.nuget.newtonsoft-json` 包。支持普通数据对象、列表、字典和标量，不要求 `[Serializable]`；默认序列化公开字段和属性。

存储数据而非运行时对象。Unity 对象使用稳定 ID 或资源标识表示，坐标等结构映射为数据字段，不直接序列化 GameObject、组件或 Unity 对象图。循环引用不支持，不启用类型名称反序列化。

数据字段约束、Schema Version、版本迁移和默认值由消费者定义。JSON 可解析不代表业务数据有效；必填成员可使用 `[JsonProperty(Required = Required.Always)]`。IL2CPP 构建需要验证实际数据类型的代码裁剪与 AOT 支持。

新增存储实现时实现 `ISaveService`，不增加透传服务层。设置可作为独立存档使用，但其生效逻辑不属于存档模块。云端异步、槽位界面与迁移管线尚未实现。
