# Audio

通用运行时音频模块，负责音频资源加载、2D/3D 播放、Mixer 路由、音量控制与
`AudioSource` 复用。模块不定义音乐、音效或语音等业务分类，分类由 `AudioBus` 配置决定。

## 核心类型

| 类型 | 职责 |
| --- | --- |
| `IAudioService` | 提供播放、停止和总线音量控制接口 |
| `AudioService` | 管理资源加载、活动播放、位置跟随和 `AudioSource` 对象池 |
| `AudioPreset` | 保存资源 key、输出总线、循环、音量、音高和 3D 衰减距离 |
| `AudioBus` | 映射 `AudioMixerGroup` 与公开的 Mixer 音量参数 |
| `AudioHandle` | 标识一次播放，用于主动停止仍在运行的声音 |

## 播放方式

- `Play2D`：不参与距离衰减和空间定位，适用于界面声音与音乐。
- `PlayAt`：在固定世界位置播放 3D 声音，适用于命中、爆炸等瞬时声音。
- `PlayFollow`：持续跟随目标位置，适用于附着在移动对象上的循环声音。

```text
背景音乐、UI 音效     -> Play2D
脚步、命中、爆炸     -> PlayAt
引擎、蓄力、持续特效 -> PlayFollow
```

```csharp
audio.Play2D(uiPreset);
audio.PlayAt(hitPreset, hitPoint);

AudioHandle handle = audio.PlayFollow(loopPreset, target);
audio.Stop(handle);
```

非循环声音播放结束后自动归还对象池。循环声音由调用方保存 `AudioHandle`，并在所属生命周期
结束时调用 `Stop`。跟随目标销毁后，对应播放自动结束。

## 配置

1. 在 `AudioMixer` 中创建所需 Group，并公开对应的音量参数。
2. 为每个逻辑分类创建 `AudioBus`，配置 Mixer Group 和公开参数名。
3. 创建 `AudioPreset`，配置资源 key、Bus 与播放参数。
4. 调用方通过 `IAudioService` 播放，不直接创建或持有 `AudioSource`。

总线音量使用 `0..1` 的归一化值：

```csharp
audio.SetVolume(sfxBus, 0.8f);
```

资源 key、Bus、Mixer Group 或公开参数缺失属于配置错误，模块不会使用默认资源或替代路由。
