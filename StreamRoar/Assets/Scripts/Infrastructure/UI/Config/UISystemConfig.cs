using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// UI 系统统一配置。
    /// </summary>
    [CreateAssetMenu(fileName = "UISystemConfig", menuName = "StreamRoar/UI/UI System Config")]
    public sealed class UISystemConfig : ScriptableObject
    {
        public enum UIRenderMode
        {
            ScreenSpaceCamera = 0,
            ScreenSpaceOverlay = 1,
            WorldSpace = 2
        }

        public enum UICameraResolveMode
        {
            MainCamera = 0,
            Tag = 1
        }

        public enum UIPrefabType
        {
            Panel = 0,
            Widget = 1,
            Window = 2,
            Popup = 3
        }

        [Serializable]
        public sealed class RootSettings
        {
            [Tooltip("运行时 UI 根节点名")]
            public string RootName = "UIRoot";

            [Tooltip("屏幕相机解析方式")]
            public UICameraResolveMode CameraResolveMode = UICameraResolveMode.MainCamera;

            [Tooltip("当解析方式为 Tag 时使用的相机标签")]
            public string CameraTag = "MainCamera";

            [Tooltip("Screen Space Camera 的平面距离")]
            [Min(0.01f)]
            public float PlaneDistance = 100f;

            [Tooltip("CanvasScaler 参考分辨率")]
            public Vector2 ReferenceResolution = new Vector2(1920f, 1080f);

            [Range(0f, 1f)]
            [Tooltip("CanvasScaler 宽高匹配值")]
            public float MatchWidthOrHeight = 0.5f;
        }

        [Serializable]
        public sealed class LayerEntry
        {
            [Tooltip("逻辑层 ID，例如 Background / Popup")]
            public string LayerId = DefaultLayerIds.Normal;

            [Tooltip("Canvas 渲染模式")]
            public UIRenderMode RenderMode = UIRenderMode.ScreenSpaceOverlay;

            [Tooltip("Canvas sorting order")]
            public int SortingOrder = 200;

            [Tooltip("是否给该层添加 GraphicRaycaster")]
            public bool HasGraphicRaycaster = true;
        }

        [Serializable]
        public sealed class SceneScopeMapping
        {
            [Tooltip("场景名字（由 Build Settings 自动读取）")]
            public string SceneName = string.Empty;

            [Tooltip("当前场景绑定的 Scope")]
            public string ScopeId = string.Empty;
        }

        [Serializable]
        public abstract class UIPrefabBaseEntry
        {
            [Tooltip("Prefab 名称 / ID")]
            public string Id = string.Empty;

            [Tooltip("运行时资源地址")]
            public string AssetKey = string.Empty;

            [Tooltip("UI 类型")]
            public UIPrefabType PrefabType = UIPrefabType.Panel;

            [Tooltip("目标层级")]
            public string LayerId = string.Empty;

            [Tooltip("是否允许进入返回栈并响应 ESC")]
            public bool IsEscapable = true;
        }

        [Serializable]
        public sealed class UIPanelEntry : UIPrefabBaseEntry
        {
            public UIPanelEntry()
            {
                PrefabType = UIPrefabType.Panel;
            }
        }

        [Serializable]
        public sealed class UIWindowEntry : UIPrefabBaseEntry
        {
            public UIWindowEntry()
            {
                PrefabType = UIPrefabType.Window;
            }
        }

        [Serializable]
        public sealed class UIPopupEntry : UIPrefabBaseEntry
        {
            public UIPopupEntry()
            {
                PrefabType = UIPrefabType.Popup;
            }
        }

        [Serializable]
        public sealed class UIWidgetEntry : UIPrefabBaseEntry
        {
            [Tooltip("是否开启对象池")]
            public bool IsPooled;

            [Tooltip("是否忽略场景深度测试，始终显示在 Mesh 前")]
            public bool IgnoreDepth;

            [Tooltip("初始预热容量")]
            [Min(0)]
            public int InitialCapacity = 5;

            [Tooltip("最小常驻缓存数")]
            [Min(0)]
            public int MinCachedCount = 5;

            public UIWidgetEntry()
            {
                PrefabType = UIPrefabType.Widget;
            }
        }

        [Serializable]
        public sealed class ScopeElementSelection
        {
            [FormerlySerializedAs("PanelId")]
            [Tooltip("UI 元素 ID")]
            public string ElementId = string.Empty;

            [Tooltip("进入时是否预加载")]
            public bool Preload = true;

            [Tooltip("进入时是否自动打开")]
            public bool OpenOnEnter;
        }

        [Serializable]
        public sealed class ScopeElementGroup
        {
            [Tooltip("绑定的 Scope ID")]
            public string ScopeId = string.Empty;

            [FormerlySerializedAs("Panels")]
            [Tooltip("分配给该 Scope 的 UI 元素列表")]
            public List<ScopeElementSelection> Elements = new List<ScopeElementSelection>();
        }

        public const string DefaultLayerPrefix = "Layer_";
        public const float DefaultWorldScale = 0.01f;

        public static class DefaultLayerIds
        {
            public const string Background = "Background";
            public const string WorldOverlay = "WorldOverlay";
            public const string Normal = "Normal";
            public const string Popup = "Popup";
            public const string Top = "Top";
        }

        public static class DefaultScopeIds
        {
            public const string Menu = "Menu";
            public const string Gameplay = "Gameplay";
            public const string None = "None";
        }

        [Tooltip("UI 根配置")]
        public RootSettings Root = new RootSettings();

        [Tooltip("需要生成的 UI 层列表")]
        public List<LayerEntry> Layers = new List<LayerEntry>();

        [Tooltip("可供选择的 Scope 列表")]
        public List<string> Scopes =
            new List<string> { DefaultScopeIds.Menu, DefaultScopeIds.Gameplay, DefaultScopeIds.None };

        [Tooltip("场景到 Scope 的映射配置")]
        public List<SceneScopeMapping> SceneMappings = new List<SceneScopeMapping>();

        [Tooltip("Panel 注册信息")]
        public List<UIPanelEntry> Panels = new List<UIPanelEntry>();

        [Tooltip("Window 注册信息")]
        public List<UIWindowEntry> Windows = new List<UIWindowEntry>();

        [Tooltip("Popup 注册信息")]
        public List<UIPopupEntry> Popups = new List<UIPopupEntry>();

        [Tooltip("Widget 注册信息")]
        public List<UIWidgetEntry> Widgets = new List<UIWidgetEntry>();

        [Tooltip("UI Prefab 扫描路径")]
        public string PrefabSearchPath = "Assets/ResourcesAssets/UI";

        [FormerlySerializedAs("GlobalPanels")]
        [Tooltip("所有 Scope 均加载的 UI 元素")]
        public List<ScopeElementSelection> GlobalElements = new List<ScopeElementSelection>();

        [FormerlySerializedAs("ScopePanels")]
        [Tooltip("按 Scope 分配的 UI 元素")]
        public List<ScopeElementGroup> ScopeElements = new List<ScopeElementGroup>();

        /// <summary>
        /// 查找 UI 层配置。
        /// </summary>
        public bool TryGetLayer(string layerId, out LayerEntry entry)
        {
            entry = Layers.Find(layer => layer != null && layer.LayerId == layerId);
            return entry != null;
        }

        /// <summary>
        /// 查找面板配置。
        /// </summary>
        public bool TryGetPanel(string elementId, out UIPanelEntry entry)
        {
            entry = Panels.Find(panel => panel != null && panel.Id == elementId);
            return entry != null;
        }

        /// <summary>
        /// 查找窗口配置。
        /// </summary>
        public bool TryGetWindow(string windowId, out UIWindowEntry entry)
        {
            entry = Windows.Find(window => window != null && window.Id == windowId);
            return entry != null;
        }

        /// <summary>
        /// 查找弹窗配置。
        /// </summary>
        public bool TryGetPopup(string popupId, out UIPopupEntry entry)
        {
            entry = Popups.Find(popup => popup != null && popup.Id == popupId);
            return entry != null;
        }

        /// <summary>
        /// 查找可打开 UI 配置。
        /// </summary>
        public bool TryGetOpenable(string id, out UIPrefabBaseEntry entry)
        {
            if (TryGetPanel(id, out UIPanelEntry panel))
            {
                entry = panel;
                return true;
            }

            if (TryGetWindow(id, out UIWindowEntry window))
            {
                entry = window;
                return true;
            }

            if (TryGetPopup(id, out UIPopupEntry popup))
            {
                entry = popup;
                return true;
            }

            entry = null;
            return false;
        }

        /// <summary>
        /// 查找所有已注册 UI 元素。
        /// </summary>
        public bool TryGetElement(string id, out UIPrefabBaseEntry entry)
        {
            if (TryGetOpenable(id, out entry))
                return true;

            if (TryGetWidget(id, out UIWidgetEntry widget))
            {
                entry = widget;
                return true;
            }

            entry = null;
            return false;
        }

        /// <summary>
        /// 查找控件配置。
        /// </summary>
        public bool TryGetWidget(string widgetId, out UIWidgetEntry entry)
        {
            entry = Widgets.Find(widget => widget != null && widget.Id == widgetId);
            return entry != null;
        }

        /// <summary>
        /// 查找作用域面板组。
        /// </summary>
        public bool TryGetScopeElementGroup(string scopeId, out ScopeElementGroup entry)
        {
            entry = ScopeElements.Find(group => group != null && group.ScopeId == scopeId);
            return entry != null;
        }

        /// <summary>
        /// 查找场景绑定的 UI 作用域。
        /// </summary>
        public bool TryGetSceneScope(string sceneName, out string scopeId)
        {
            SceneScopeMapping mapping =
                SceneMappings.Find(item => item != null && item.SceneName == sceneName);
            scopeId = mapping?.ScopeId;
            return mapping != null;
        }

        /// <summary>
        /// 恢复项目默认 UI 配置。
        /// </summary>
        public void ResetToDefault()
        {
            PrefabSearchPath = "Assets/ResourcesAssets/UI";

            Root.RootName = "UIRoot";
            Root.CameraResolveMode = UICameraResolveMode.MainCamera;
            Root.CameraTag = "MainCamera";
            Root.PlaneDistance = 100f;
            Root.ReferenceResolution = new Vector2(1920f, 1080f);
            Root.MatchWidthOrHeight = 0.5f;

            Layers.Clear();
            Layers.Add(CreateLayer(DefaultLayerIds.Background, 100));
            Layers.Add(CreateLayer(DefaultLayerIds.WorldOverlay, 150));
            Layers.Add(CreateLayer(DefaultLayerIds.Normal, 200));
            Layers.Add(CreateLayer(DefaultLayerIds.Popup, 300));
            Layers.Add(CreateLayer(DefaultLayerIds.Top, 400));

            Scopes.Clear();
            Scopes.Add(DefaultScopeIds.Menu);
            Scopes.Add(DefaultScopeIds.Gameplay);
            Scopes.Add(DefaultScopeIds.None);

            SceneMappings.Clear();

            Panels.Clear();
            Windows.Clear();
            Popups.Clear();
            Widgets.Clear();
            GlobalElements.Clear();
            ScopeElements.Clear();
        }

        /// <summary>
        /// 检查指定 UI 层是否存在。
        /// </summary>
        public bool ContainsLayer(string layerId)
        {
            return TryGetLayer(layerId, out _);
        }

        /// <summary>
        /// 获取新增 UI 层的排序值。
        /// </summary>
        public int GetNextSortingOrder()
        {
            int maxSortingOrder = 0;
            for (int i = 0; i < Layers.Count; i++)
            {
                LayerEntry layer = Layers[i];
                if (layer != null && layer.SortingOrder > maxSortingOrder)
                {
                    maxSortingOrder = layer.SortingOrder;
                }
            }

            return maxSortingOrder + 100;
        }

        /// <summary>
        /// 添加一个使用下一排序值的 UI 层。
        /// </summary>
        public LayerEntry AddLayer()
        {
            LayerEntry layer = CreateLayer("Layer" + (Layers.Count + 1), GetNextSortingOrder());
            Layers.Add(layer);
            return layer;
        }

        static LayerEntry CreateLayer(string layerId, int sortingOrder)
        {
            return new LayerEntry
            {
                LayerId = layerId,
                RenderMode = UIRenderMode.ScreenSpaceOverlay,
                SortingOrder = sortingOrder,
                HasGraphicRaycaster = true
            };
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            Root.MatchWidthOrHeight = Mathf.Clamp01(Root.MatchWidthOrHeight);
            Root.ReferenceResolution.x = Mathf.Max(1f, Root.ReferenceResolution.x);
            Root.ReferenceResolution.y = Mathf.Max(1f, Root.ReferenceResolution.y);
            Root.PlaneDistance = Mathf.Max(1f, Root.PlaneDistance);

            for (int i = 0; i < Widgets.Count; i++)
            {
                UIWidgetEntry widget = Widgets[i];
                if (widget == null)
                    continue;

                widget.InitialCapacity = Mathf.Max(0, widget.InitialCapacity);
                widget.MinCachedCount = Mathf.Max(0, widget.MinCachedCount);
            }
        }
#endif
    }
}
