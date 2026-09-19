#if UNITY_EDITOR
using System.Collections.Generic;
using StreamRoar.Infrastructure;
using UnityEditorInternal;
using UnityEngine;

namespace StreamRoar.Editor
{
    /// <summary>
    /// UI 系统配置校验器。
    /// </summary>
    public static class UISystemConfigValidator
    {
        /// <summary>
        /// 校验运行时必需的 UI 配置关系。
        /// </summary>
        public static bool Validate(UISystemConfig config, List<string> errors, List<string> warnings)
        {
            errors.Clear();
            warnings.Clear();

            if (config == null)
            {
                errors.Add("缺少 UISystemConfig 资产。");
                return false;
            }

            if (string.IsNullOrWhiteSpace(config.Root.RootName))
            {
                errors.Add("RootName 不能为空。");
            }

            if (string.IsNullOrWhiteSpace(config.PrefabSearchPath))
            {
                errors.Add("PrefabSearchPath 不能为空。");
            }

            if (config.Layers.Count == 0)
            {
                errors.Add("至少需要一个 Layer。");
            }

            var layerIds = new HashSet<string>();
            var sortingOrders = new HashSet<int>();
            var windowIds = new HashSet<string>();
            var popupIds = new HashSet<string>();
            var widgetIds = new HashSet<string>();
            var elementIds = new HashSet<string>();
            var assetKeys = new HashSet<string>();

            for (int i = 0; i < config.Layers.Count; i++)
            {
                UISystemConfig.LayerEntry layer = config.Layers[i];
                if (layer == null)
                {
                    errors.Add("存在空 Layer 条目。");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(layer.LayerId))
                {
                    errors.Add("Layer " + (i + 1) + " 的 LayerId 不能为空。");
                    continue;
                }

                if (!layerIds.Add(layer.LayerId))
                {
                    errors.Add("LayerId 重复: " + layer.LayerId);
                }

                if (!sortingOrders.Add(layer.SortingOrder))
                {
                    warnings.Add("SortingOrder 重复: " + layer.SortingOrder);
                }

            }

            if (!config.ContainsLayer(UISystemConfig.DefaultLayerIds.Normal))
            {
                warnings.Add("当前配置没有 Normal 层。");
            }

            if (HasRenderMode(config, UISystemConfig.UIRenderMode.WorldSpace) ||
                HasRenderMode(config, UISystemConfig.UIRenderMode.ScreenSpaceCamera))
            {
                ValidateCamera(config, errors, warnings);
            }

            for (int i = 0; i < config.Panels.Count; i++)
            {
                UISystemConfig.UIPanelEntry panel = config.Panels[i];
                if (panel == null)
                {
                    errors.Add("存在空 Panel 条目。");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(panel.Id))
                {
                    errors.Add("Panel ID 不能为空。");
                    continue;
                }
                if (panel.PrefabType != UISystemConfig.UIPrefabType.Panel)
                {
                    errors.Add("Panel 类型不匹配: " + panel.Id);
                }
                if (!elementIds.Add(panel.Id))
                    errors.Add("Panel ID 重复: " + panel.Id);

                if (string.IsNullOrWhiteSpace(panel.AssetKey))
                {
                    errors.Add("Panel 资源地址为空: " + panel.Id);
                }
                else if (!assetKeys.Add(panel.AssetKey))
                {
                    errors.Add("UI 资源地址重复: " + panel.AssetKey);
                }

                if (!config.ContainsLayer(panel.LayerId))
                {
                    errors.Add("Panel 层不存在: " + panel.Id + " -> " + panel.LayerId);
                }
            }

            for (int i = 0; i < config.Widgets.Count; i++)
            {
                UISystemConfig.UIWidgetEntry widget = config.Widgets[i];
                if (widget == null)
                {
                    errors.Add("存在空 Widget 条目。");
                    continue;
                }

                if (widget.InitialCapacity < 0 || widget.MinCachedCount < 0)
                {
                    errors.Add("Widget " + widget.Id + " 的 PoolConfig 不能小于 0。");
                }

                if (string.IsNullOrWhiteSpace(widget.Id))
                {
                    errors.Add("Widget ID 不能为空。");
                    continue;
                }
                if (widget.PrefabType != UISystemConfig.UIPrefabType.Widget)
                {
                    errors.Add("Widget 类型不匹配: " + widget.Id);
                }

                if (!widgetIds.Add(widget.Id))
                {
                    errors.Add("Widget ID 重复: " + widget.Id);
                }
                if (!elementIds.Add(widget.Id))
                {
                    errors.Add("UI ID 重复: " + widget.Id);
                }

                if (string.IsNullOrWhiteSpace(widget.AssetKey))
                {
                    errors.Add("Widget 资源地址为空: " + widget.Id);
                }
                else if (!assetKeys.Add(widget.AssetKey))
                {
                    errors.Add("UI 资源地址重复: " + widget.AssetKey);
                }

                if (!config.ContainsLayer(widget.LayerId))
                {
                    errors.Add("Widget 层不存在: " + widget.Id + " -> " + widget.LayerId);
                }
            }

            for (int i = 0; i < config.Windows.Count; i++)
            {
                UISystemConfig.UIWindowEntry window = config.Windows[i];
                if (window == null)
                {
                    errors.Add("存在空 Window 条目。");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(window.Id))
                {
                    errors.Add("Window ID 不能为空。");
                    continue;
                }
                if (window.PrefabType != UISystemConfig.UIPrefabType.Window)
                {
                    errors.Add("Window 类型不匹配: " + window.Id);
                }
                if (!windowIds.Add(window.Id) || !elementIds.Add(window.Id))
                {
                    errors.Add("Window ID 重复: " + window.Id);
                }

                if (string.IsNullOrWhiteSpace(window.AssetKey))
                {
                    errors.Add("Window 资源地址为空: " + window.Id);
                }
                else if (!assetKeys.Add(window.AssetKey))
                {
                    errors.Add("UI 资源地址重复: " + window.AssetKey);
                }

                if (!config.ContainsLayer(window.LayerId))
                {
                    errors.Add("Window 层不存在: " + window.Id + " -> " + window.LayerId);
                }
            }

            for (int i = 0; i < config.Popups.Count; i++)
            {
                UISystemConfig.UIPopupEntry popup = config.Popups[i];
                if (popup == null)
                {
                    errors.Add("存在空 Popup 条目。");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(popup.Id))
                {
                    errors.Add("Popup ID 不能为空。");
                    continue;
                }
                if (popup.PrefabType != UISystemConfig.UIPrefabType.Popup)
                {
                    errors.Add("Popup 类型不匹配: " + popup.Id);
                }
                if (!popupIds.Add(popup.Id) || !elementIds.Add(popup.Id))
                {
                    errors.Add("Popup ID 重复: " + popup.Id);
                }

                if (string.IsNullOrWhiteSpace(popup.AssetKey))
                {
                    errors.Add("Popup 资源地址为空: " + popup.Id);
                }
                else if (!assetKeys.Add(popup.AssetKey))
                {
                    errors.Add("UI 资源地址重复: " + popup.AssetKey);
                }

                if (!config.ContainsLayer(popup.LayerId))
                {
                    errors.Add("Popup 层不存在: " + popup.Id + " -> " + popup.LayerId);
                }
            }

            return errors.Count == 0;
        }

        static bool HasRenderMode(UISystemConfig config, UISystemConfig.UIRenderMode renderMode)
        {
            for (int i = 0; i < config.Layers.Count; i++)
            {
                UISystemConfig.LayerEntry layer = config.Layers[i];
                if (layer != null && layer.RenderMode == renderMode)
                    return true;
            }

            return false;
        }

        static void ValidateCamera(UISystemConfig config, List<string> errors, List<string> warnings)
        {
            if (config.Root.CameraResolveMode == UISystemConfig.UICameraResolveMode.MainCamera)
            {
                if (Camera.main == null)
                {
                    warnings.Add("当前场景没有 MainCamera，预览时可能看不到 Camera 或 World Space 结果。");
                }

                return;
            }

            if (string.IsNullOrWhiteSpace(config.Root.CameraTag))
            {
                errors.Add("CameraTag 不能为空。");
                return;
            }

            string[] tags = InternalEditorUtility.tags;
            bool foundTag = false;
            for (int i = 0; i < tags.Length; i++)
            {
                if (tags[i] == config.Root.CameraTag)
                {
                    foundTag = true;
                    break;
                }
            }

            if (!foundTag)
            {
                errors.Add("CameraTag 不存在: " + config.Root.CameraTag);
                return;
            }

            if (GameObject.FindWithTag(config.Root.CameraTag) == null)
            {
                warnings.Add("当前场景没有找到相机标签对象: " + config.Root.CameraTag);
            }
        }
    }
}
#endif
