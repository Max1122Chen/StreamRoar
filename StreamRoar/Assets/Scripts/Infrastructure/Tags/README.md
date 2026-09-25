# GameplayTag

分层语义标签（类 UE GameplayTag），供状态查询、后续 Console / 配置共用。

## 类型

| 类型 | 职责 |
|------|------|
| `GameplayTag` | 稳定 Id 句柄 |
| `GameplayTagContainer` | 集合；`Has(tag, includeChildren, manager)` 显式父子匹配 |
| `IGameplayTagManager` / `GameplayTagManager` | 注册表；构造后密封 |
| `IGameplayTagSource` | 扩展点：Native →（未来）Config |

## 用法

```csharp
var manager = ServiceLocator.Resolve<IGameplayTagManager>();
GameplayTag burn = manager.RequestTag(NativeGameplayTagSource.StateDebuffBurn);

var container = new GameplayTagContainer();
container.Add(burn);
bool hasDebuff = container.Has(manager.RequestTag(NativeGameplayTagSource.StateDebuff), includeChildren: true, manager);
```

## 约定

- 路径：`State.Debuff.Burn`（大小写敏感，段须字母开头）
- 禁止运行时动态注册；新增 Tag 通过 `IGameplayTagSource`
- 序列化推荐存完整路径字符串
- 注册顺序：Native → Config（Config 源尚未实现）
