using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 统一管理 UI 元素与动态 Widget 生命周期。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UIElementManager : MonoBehaviour, IUIService
    {
        [Header("配置")]
        [Tooltip("统一 UI 系统配置")]
        [SerializeField]
        UISystemConfig m_Config;

        RectTransform m_Root;
        Transform m_WidgetPoolRoot;
        readonly Dictionary<string, UIElementHandle> m_LoadedElements = new Dictionary<string, UIElementHandle>();
        readonly Dictionary<string, PrefabPool<RectTransform>> m_WidgetPools =
            new Dictionary<string, PrefabPool<RectTransform>>();
        readonly Dictionary<GameObject, PrefabPool<RectTransform>> m_PooledWidgetInstances =
            new Dictionary<GameObject, PrefabPool<RectTransform>>();
        readonly HashSet<GameObject> m_TransientWidgetInstances = new HashSet<GameObject>();
        readonly List<IWorldWidget> m_WorldWidgetSources = new List<IWorldWidget>();
        readonly Dictionary<IWorldWidget, RectTransform> m_WorldWidgetInstances =
            new Dictionary<IWorldWidget, RectTransform>();
        readonly HashSet<string> m_AllowedElementIds = new HashSet<string>();
        readonly Dictionary<string, RectTransform> m_LayerRoots = new Dictionary<string, RectTransform>();
        readonly List<string> m_BackStack = new List<string>();
        readonly List<string> m_ReleaseBuffer = new List<string>(16);

        IAssetProvider m_AssetProvider;
        Camera m_RenderCamera;
        bool m_WorldWidgetsDirty;

        internal UISystemConfig Config => m_Config;

        void OnDestroy()
        {
            ReleaseAllWidgets();
        }

        /// <summary>
        /// 统一更新世界 Widget 的位置与朝向。
        /// </summary>
        void LateUpdate()
        {
            if (m_WorldWidgetsDirty && m_RenderCamera)
            {
                for (int i = 0; i < m_WorldWidgetSources.Count; i++)
                {
                    IWorldWidget source = m_WorldWidgetSources[i];
                    if (m_WorldWidgetInstances.ContainsKey(source))
                        continue;

                    RectTransform instance = (RectTransform)CreateWidget(source.WidgetId).transform;
                    instance.SetPositionAndRotation(
                        source.Anchor.position + source.Offset,
                        m_RenderCamera.transform.rotation);
                    m_WorldWidgetInstances.Add(source, instance);
                    source.Bind(instance.gameObject);
                }

                m_WorldWidgetsDirty = false;
            }

            if (!m_RenderCamera)
                return;

            Quaternion rotation = m_RenderCamera.transform.rotation;
            foreach (KeyValuePair<IWorldWidget, RectTransform> pair in m_WorldWidgetInstances)
            {
                IWorldWidget source = pair.Key;
                if (!source.Anchor)
                    continue;

                pair.Value.SetPositionAndRotation(source.Anchor.position + source.Offset, rotation);
            }
        }

        /// <summary>
        /// 注入 UI 系统配置。
        /// </summary>
        internal void SetConfig(UISystemConfig config)
        {
            m_Config = config;
        }

        /// <summary>
        /// 创建 UI 根节点、层级与 Canvas。
        /// </summary>
        internal void InitializeRuntime()
        {
            m_AssetProvider = ServiceLocator.Resolve<IAssetProvider>();
            EnsureRoot();
            EnsureLayerRoots();
            RefreshCanvasSettings();
        }

        /// <summary>
        /// 刷新各层 Canvas 参数，保证切换场景后仍然使用正确的相机和缩放规则。
        /// </summary>
        internal void RefreshCanvasSettings()
        {
            m_RenderCamera = ResolveRenderCamera();
            for (int i = 0; i < m_Config.Layers.Count; i++)
            {
                UISystemConfig.LayerEntry layer = m_Config.Layers[i];
                RectTransform root = m_LayerRoots[layer.LayerId];
                ApplyCanvasSettings(root.GetComponent<Canvas>(), root.GetComponent<CanvasScaler>(), layer, m_RenderCamera);
            }
        }

        /// <summary>
        /// 预加载指定 UI 元素。
        /// </summary>
        internal void Preload(IReadOnlyCollection<string> elementIds)
        {
            foreach (string elementId in elementIds)
            {
                if (!m_LoadedElements.ContainsKey(elementId))
                    CreateElement(elementId);
            }
        }

        /// <summary>
        /// 更新当前场景允许打开的 UI 元素。
        /// </summary>
        internal void SetAllowed(IReadOnlyCollection<string> elementIds)
        {
            m_AllowedElementIds.Clear();
            m_AllowedElementIds.UnionWith(elementIds);
        }

        /// <summary>
        /// 释放不属于当前场景作用域的 UI 元素。
        /// </summary>
        internal void ReleaseExcept(HashSet<string> keepElementIds)
        {
            StopAllCoroutines();
            NormalizeLoadedElementStates();

            m_ReleaseBuffer.Clear();
            foreach (string elementId in m_LoadedElements.Keys)
            {
                if (!keepElementIds.Contains(elementId))
                    m_ReleaseBuffer.Add(elementId);
            }

            for (int i = 0; i < m_ReleaseBuffer.Count; i++)
                ReleaseElement(m_ReleaseBuffer[i]);
        }

        /// <summary>
        /// 打开当前作用域允许的 UI 元素。
        /// </summary>
        public void Open(string elementId)
        {
            if (!CanOpenElement(elementId))
            {
                return;
            }

            if (!TryGetLoadedHandle(elementId, out UIElementHandle handle))
            {
                CreateElement(elementId);
                handle = m_LoadedElements[elementId];
            }

            if (handle.IsClosing)
            {
                handle.PendingShow = true;
                return;
            }

            if (!CanShow(handle))
            {
                return;
            }

            StartElementTransition(handle, ShowRoutine(handle));
        }

        /// <summary>
        /// 按配置层级创建动态 Widget 实例。
        /// </summary>
        public GameObject CreateWidget(string widgetId)
        {
            if (!m_Config.TryGetWidget(widgetId, out UISystemConfig.UIWidgetEntry entry))
                throw new InvalidOperationException($"UI Widget 未注册：{widgetId}");

            RectTransform parent = m_LayerRoots[entry.LayerId];
            GameObject instance;

            if (entry.IsPooled)
            {
                PrefabPool<RectTransform> pool = GetOrCreateWidgetPool(entry);
                RectTransform widget = pool.Get(parent);
                instance = widget.gameObject;
                m_PooledWidgetInstances[instance] = pool;
            }
            else
            {
                RectTransform prefab = m_AssetProvider.LoadComponent<RectTransform>(entry.AssetKey);
                instance = Instantiate(prefab, parent, false).gameObject;
                instance.name = widgetId;
                m_TransientWidgetInstances.Add(instance);
            }

            if (entry.IgnoreDepth)
            {
                UIWidgetRenderSettings renderSettings = instance.GetComponent<UIWidgetRenderSettings>();
                if (!renderSettings)
                {
                    throw new InvalidOperationException(
                        $"Widget '{widgetId}' 启用了 IgnoreDepth，但 prefab 根节点缺少 UIWidgetRenderSettings。");
                }

                renderSettings.Apply();
            }

            return instance;
        }

        /// <summary>
        /// 注册动态世界 Widget，实例创建统一延迟到 LateUpdate。
        /// </summary>
        public void RegisterWorldWidget(IWorldWidget widget)
        {
            if (m_WorldWidgetSources.Contains(widget))
                return;

            m_WorldWidgetSources.Add(widget);
            m_WorldWidgetsDirty = true;
        }

        /// <summary>
        /// 关闭指定 UI 元素。
        /// </summary>
        public void Close(string elementId)
        {
            if (!TryGetLoadedHandle(elementId, out UIElementHandle handle))
                return;

            handle.PendingShow = false;

            if (!CanHide(handle))
            {
                return;
            }

            StartElementTransition(handle, HideRoutine(handle));
        }

        /// <summary>
        /// 释放动态 Widget 实例。
        /// </summary>
        public void ReleaseWidget(GameObject instance)
        {
            if (!instance)
                return;

            if (m_PooledWidgetInstances.Remove(instance, out PrefabPool<RectTransform> pool))
            {
                pool.Release(instance.transform as RectTransform);
                return;
            }

            if (m_TransientWidgetInstances.Remove(instance))
                Destroy(instance);
        }

        /// <summary>
        /// 注销动态世界 Widget 并释放对应实例。
        /// </summary>
        public void UnregisterWorldWidget(IWorldWidget widget)
        {
            if (!m_WorldWidgetSources.Remove(widget))
                return;

            if (!m_WorldWidgetInstances.Remove(widget, out RectTransform instance))
                return;

            widget.Unbind();
            ReleaseWidget(instance.gameObject);
        }

        /// <summary>
        /// 关闭返回栈顶部允许响应 ESC 的 UI。
        /// </summary>
        internal bool TryCloseTopEscapable()
        {
            for (int i = m_BackStack.Count - 1; i >= 0; i--)
            {
                string elementId = m_BackStack[i];
                if (!TryGetLoadedHandle(elementId, out UIElementHandle handle))
                {
                    m_BackStack.RemoveAt(i);
                    continue;
                }

                if (!CanHide(handle) || !handle.IsEscapable)
                {
                    continue;
                }

                Close(elementId);
                return true;
            }

            return false;
        }

        void NormalizeLoadedElementStates()
        {
            foreach (KeyValuePair<string, UIElementHandle> pair in m_LoadedElements)
            {
                UIElementHandle handle = pair.Value;
                handle.IsClosing = false;
                handle.PendingShow = false;
                handle.TransitionRoutine = null;
                if (!handle.Instance)
                {
                    continue;
                }

                ApplyVisibleState(handle.Instance, handle.Instance.activeSelf);
            }
        }

        /// <summary>
        /// 从配置、层级和资源三步创建屏幕 UI 实例。
        /// </summary>
        void CreateElement(string elementId)
        {
            if (!m_Config.TryGetOpenable(elementId, out UISystemConfig.UIPrefabBaseEntry entry))
                throw new InvalidOperationException($"UI 元素未注册：{elementId}");

            RectTransform parent = m_LayerRoots[entry.LayerId];
            GameObject prefab = m_AssetProvider.Load<GameObject>(entry.AssetKey);
            GameObject instance = Instantiate(prefab, parent, false);
            RegisterLoadedElement(elementId, entry, instance);
        }

        /// <summary>
        /// 显示 UI 元素并处理进入动画。
        /// </summary>
        IEnumerator ShowRoutine(UIElementHandle handle)
        {
            handle.IsClosing = false;
            handle.PendingShow = false;
            ApplyVisibleState(handle.Instance, true);
            handle.Instance.SetActive(true);

            UIElementTransition transition = handle.Instance.GetComponent<UIElementTransition>();
            if (transition)
            {
                transition.PrepareForOpen();
                yield return transition.PlayOpen();
            }

            if (handle.IsEscapable && !m_BackStack.Contains(handle.ElementId))
            {
                m_BackStack.Add(handle.ElementId);
            }

            handle.TransitionRoutine = null;
        }

        /// <summary>
        /// 关闭 UI 元素并处理退出动画。
        /// </summary>
        IEnumerator HideRoutine(UIElementHandle handle)
        {
            handle.IsClosing = true;
            handle.PendingShow = false;

            UIElementTransition transition = handle.Instance.GetComponent<UIElementTransition>();
            if (transition)
            {
                yield return transition.PlayClose();
            }

            ApplyVisibleState(handle.Instance, false);
            handle.Instance.SetActive(false);
            m_BackStack.Remove(handle.ElementId);
            handle.IsClosing = false;
            handle.TransitionRoutine = null;

            if (handle.PendingShow && CanOpenElement(handle.ElementId))
            {
                StartElementTransition(handle, ShowRoutine(handle));
            }
        }

        void ReleaseElement(string elementId)
        {
            UIElementHandle handle = m_LoadedElements[elementId];
            if (handle.TransitionRoutine != null)
            {
                StopCoroutine(handle.TransitionRoutine);
                handle.TransitionRoutine = null;
            }

            m_BackStack.Remove(elementId);
            m_LoadedElements.Remove(elementId);
            Destroy(handle.Instance);
        }

        bool TryGetLoadedHandle(string elementId, out UIElementHandle handle)
        {
            return m_LoadedElements.TryGetValue(elementId, out handle);
        }

        bool CanOpenElement(string elementId)
        {
            if (!m_Config.TryGetOpenable(elementId, out UISystemConfig.UIPrefabBaseEntry entry))
                throw new InvalidOperationException($"UI 元素未注册：{elementId}");

            return m_AllowedElementIds.Contains(elementId) ||
                   entry.PrefabType == UISystemConfig.UIPrefabType.Window ||
                   entry.PrefabType == UISystemConfig.UIPrefabType.Popup;
        }

        static bool CanShow(UIElementHandle handle)
        {
            return handle.Instance && !handle.Instance.activeSelf && !handle.IsClosing;
        }

        static bool CanHide(UIElementHandle handle)
        {
            return handle.Instance && handle.Instance.activeSelf && !handle.IsClosing;
        }

        /// <summary>
        /// 将屏幕 UI 实例登记到运行时缓存中。
        /// </summary>
        void RegisterLoadedElement(string elementId, UISystemConfig.UIPrefabBaseEntry entry, GameObject instance)
        {
            instance.name = elementId;
            ApplyVisibleState(instance, false);
            instance.SetActive(false);

            m_LoadedElements[elementId] = new UIElementHandle
            {
                ElementId = elementId,
                Instance = instance,
                IsClosing = false,
                PendingShow = false,
                IsEscapable = entry.IsEscapable
            };
        }

        /// <summary>
        /// 创建 UI 根节点。
        /// </summary>
        void EnsureRoot()
        {
            GameObject rootObject = new GameObject(m_Config.Root.RootName, typeof(RectTransform));
            rootObject.transform.SetParent(transform, false);
            m_Root = rootObject.GetComponent<RectTransform>();
            StretchRoot(m_Root);

            GameObject poolObject = new GameObject("UIWidgetPool");
            poolObject.transform.SetParent(transform, false);
            m_WidgetPoolRoot = poolObject.transform;
        }

        /// <summary>
        /// 创建各个 UI 层级根节点。
        /// </summary>
        void EnsureLayerRoots()
        {
            for (int i = 0; i < m_Config.Layers.Count; i++)
            {
                UISystemConfig.LayerEntry layer = m_Config.Layers[i];
                GameObject layerObject = new GameObject(
                    UISystemConfig.DefaultLayerPrefix + layer.LayerId,
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler));
                if (layer.HasGraphicRaycaster)
                {
                    layerObject.AddComponent<GraphicRaycaster>();
                }

                layerObject.transform.SetParent(m_Root, false);
                RectTransform layerRoot = layerObject.GetComponent<RectTransform>();
                if (layer.RenderMode == UISystemConfig.UIRenderMode.WorldSpace)
                    ConfigureWorldLayer(layerRoot, m_Config.Root.ReferenceResolution);
                else
                    StretchRoot(layerRoot);

                Canvas canvas = layerObject.GetComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingOrder = layer.SortingOrder;
                m_LayerRoots.Add(layer.LayerId, layerRoot);
            }
        }

        void ApplyCanvasSettings(
            Canvas canvas, CanvasScaler scaler, UISystemConfig.LayerEntry layer, Camera renderCamera)
        {
            if (layer.RenderMode == UISystemConfig.UIRenderMode.WorldSpace)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = renderCamera;
                scaler.enabled = false;
                return;
            }

            scaler.enabled = true;
            if (layer.RenderMode == UISystemConfig.UIRenderMode.ScreenSpaceCamera)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = renderCamera;
            }
            else
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.worldCamera = null;
            }
            canvas.planeDistance = m_Config.Root.PlaneDistance;

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = m_Config.Root.ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = m_Config.Root.MatchWidthOrHeight;
        }

        static void ConfigureWorldLayer(RectTransform rect, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one * UISystemConfig.DefaultWorldScale;
        }

        Camera ResolveRenderCamera()
        {
            if (m_Config.Root.CameraResolveMode == UISystemConfig.UICameraResolveMode.Tag &&
                !string.IsNullOrWhiteSpace(m_Config.Root.CameraTag))
            {
                GameObject taggedObject = GameObject.FindWithTag(m_Config.Root.CameraTag);
                return taggedObject ? taggedObject.GetComponent<Camera>() : null;
            }

            return Camera.main;
        }

        static void StretchRoot(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localPosition = Vector3.zero;
        }

        static void ApplyVisibleState(GameObject panel, bool visible)
        {
            CanvasGroup group = EnsureCanvasGroup(panel);
            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }

        static CanvasGroup EnsureCanvasGroup(GameObject panel)
        {
            CanvasGroup group = panel.GetComponent<CanvasGroup>();
            if (!group)
            {
                group = panel.AddComponent<CanvasGroup>();
            }

            return group;
        }

        void StartElementTransition(UIElementHandle handle, IEnumerator routine)
        {
            if (handle.TransitionRoutine != null)
            {
                StopCoroutine(handle.TransitionRoutine);
            }

            handle.TransitionRoutine = StartCoroutine(routine);
        }

        PrefabPool<RectTransform> GetOrCreateWidgetPool(UISystemConfig.UIWidgetEntry entry)
        {
            if (m_WidgetPools.TryGetValue(entry.Id, out PrefabPool<RectTransform> pool))
                return pool;

            pool = new PrefabPool<RectTransform>(
                m_AssetProvider,
                entry.AssetKey,
                m_WidgetPoolRoot,
                entry.InitialCapacity,
                entry.MinCachedCount,
                entry.Id);
            m_WidgetPools.Add(entry.Id, pool);
            return pool;
        }

        void ReleaseAllWidgets()
        {
            foreach (KeyValuePair<IWorldWidget, RectTransform> pair in m_WorldWidgetInstances)
            {
                pair.Key.Unbind();
                ReleaseWidget(pair.Value.gameObject);
            }

            foreach (GameObject instance in m_TransientWidgetInstances)
            {
                if (instance)
                    Destroy(instance);
            }

            foreach (PrefabPool<RectTransform> pool in m_WidgetPools.Values)
                pool.Dispose();

            m_TransientWidgetInstances.Clear();
            m_PooledWidgetInstances.Clear();
            m_WidgetPools.Clear();
            m_WorldWidgetSources.Clear();
            m_WorldWidgetInstances.Clear();
        }
    }
}
