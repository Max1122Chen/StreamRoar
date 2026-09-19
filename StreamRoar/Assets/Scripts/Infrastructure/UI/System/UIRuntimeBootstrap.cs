using System;
using UnityEngine;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 场景内 UI 运行时启动器。
    /// </summary>
    [DefaultExecutionOrder(-9000)]
    [DisallowMultipleComponent]
    public sealed class UIRuntimeBootstrap : MonoBehaviour
    {
        [SerializeField, Tooltip("UI 系统配置资产。")]
        UISystemConfig m_Config;

        [SerializeField, Tooltip("运行时根节点名称。")]
        string m_RuntimeRootName = "UIRuntime";

        UIManager m_UIManager;
        GameObject m_RuntimeRoot;

        /// <summary>
        /// 初始化 UI 运行时根节点与系统入口。
        /// </summary>
        public void Boot()
        {
            if (m_UIManager != null)
                return;

            if (m_Config == null)
                throw new InvalidOperationException("缺少 UISystemConfig。");
            if (string.IsNullOrWhiteSpace(m_RuntimeRootName))
                throw new InvalidOperationException("UI 运行时根节点名称不能为空。");

            m_RuntimeRoot = new GameObject(m_RuntimeRootName);
            m_RuntimeRoot.transform.SetParent(transform, false);

            var elementManager = m_RuntimeRoot.AddComponent<UIElementManager>();
            elementManager.SetConfig(m_Config);
            m_UIManager = m_RuntimeRoot.AddComponent<UIManager>();
            m_UIManager.BootRuntime();
        }

        /// <summary>
        /// 销毁 UI 运行时根节点。
        /// </summary>
        public void Shutdown()
        {
            if (!m_RuntimeRoot)
                return;

            Destroy(m_RuntimeRoot);
            m_RuntimeRoot = null;
            m_UIManager = null;
        }

        void OnDestroy()
        {
            Shutdown();
        }
    }
}
