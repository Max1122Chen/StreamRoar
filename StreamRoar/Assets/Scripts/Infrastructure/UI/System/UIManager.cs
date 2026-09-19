using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// UI 子系统主入口：根据当前场景作用域收敛允许元素与默认打开元素。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIElementManager))]
    public sealed class UIManager : MonoBehaviour, IEventListener<SceneLoadedEvent>
    {
        [Header("流程")]
        [Tooltip("按 ESC 时尝试关闭顶部可返回元素")]
        [SerializeField]
        bool m_EnableEscapeBack = true;

        readonly HashSet<string> m_KeepElementIds = new();
        readonly HashSet<string> m_PreloadElementIds = new();
        readonly HashSet<string> m_OpenOnEnterElementIds = new();

        UIElementManager m_ElementManager;
        IEventBus m_EventBus;
        bool m_Booted;

        UISystemConfig Config => m_ElementManager.Config;

        void Awake()
        {
            m_ElementManager = GetComponent<UIElementManager>();
        }

        /// <summary>
        /// 退出时注销 UI 服务与场景监听。
        /// </summary>
        void OnDestroy()
        {
            m_EventBus.Unsubscribe<SceneLoadedEvent>(this);
            ServiceLocator.Unregister<IUIService>(m_ElementManager);
        }

        void Update()
        {
            if (!m_EnableEscapeBack)
                return;

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                m_ElementManager.TryCloseTopEscapable();
        }

        /// <summary>
        /// 初始化 UI 元素运行时并应用当前场景 UI 作用域。
        /// </summary>
        internal void BootRuntime()
        {
            if (m_Booted)
                return;

            m_ElementManager.InitializeRuntime();
            m_EventBus = ServiceLocator.Resolve<IEventBus>();
            ServiceLocator.Register<IUIService>(m_ElementManager);
            ApplySceneScope(SceneManager.GetActiveScene().name, false);
            m_EventBus.Subscribe<SceneLoadedEvent>(this);
            m_Booted = true;
        }

        /// <summary>
        /// 场景切换后重新应用 UI 作用域。
        /// </summary>
        public void OnEvent(SceneLoadedEvent eventData)
        {
            m_ElementManager.RefreshCanvasSettings();
            ApplySceneScope(eventData.Scene.name, true);
        }

        /// <summary>
        /// 按场景作用域更新允许、预加载和默认打开的 UI 元素。
        /// </summary>
        void ApplySceneScope(string sceneName, bool releaseUnscopedElements)
        {
            string scopeId = Config.TryGetSceneScope(sceneName, out string mappedScopeId)
                                 ? mappedScopeId
                                 : UISystemConfig.DefaultScopeIds.None;
            m_KeepElementIds.Clear();
            m_PreloadElementIds.Clear();
            m_OpenOnEnterElementIds.Clear();
            AddElementSelections(Config.GlobalElements);

            if (Config.TryGetScopeElementGroup(scopeId, out UISystemConfig.ScopeElementGroup group))
                AddElementSelections(group.Elements);

            m_ElementManager.SetAllowed(m_KeepElementIds);
            if (releaseUnscopedElements)
                m_ElementManager.ReleaseExcept(m_KeepElementIds);

            m_ElementManager.Preload(m_PreloadElementIds);

            foreach (string elementId in m_KeepElementIds)
            {
                if (m_OpenOnEnterElementIds.Contains(elementId))
                    m_ElementManager.Open(elementId);
                else
                    m_ElementManager.Close(elementId);
            }
        }

        void AddElementSelections(IReadOnlyList<UISystemConfig.ScopeElementSelection> elementConfigs)
        {
            for (int i = 0; i < elementConfigs.Count; i++)
            {
                UISystemConfig.ScopeElementSelection elementConfig = elementConfigs[i];
                m_KeepElementIds.Add(elementConfig.ElementId);
                if (elementConfig.Preload)
                    m_PreloadElementIds.Add(elementConfig.ElementId);

                if (elementConfig.OpenOnEnter)
                    m_OpenOnEnterElementIds.Add(elementConfig.ElementId);
            }
        }
    }
}
