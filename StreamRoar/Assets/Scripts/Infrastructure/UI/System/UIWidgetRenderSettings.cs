using System;
using UnityEngine;
using UnityEngine.UI;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 控制 Widget 图形的世界空间深度测试策略。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UIWidgetRenderSettings : MonoBehaviour
    {
        [SerializeField, Tooltip("忽略深度测试时使用的 UI 材质")]
        Material m_IgnoreDepthMaterial;

        Graphic[] m_Graphics;

        /// <summary>
        /// 为 Widget 图形应用忽略深度材质。
        /// </summary>
        public void Apply()
        {
            if (!m_IgnoreDepthMaterial)
                throw new InvalidOperationException($"{name} 未配置忽略深度材质。");

            m_Graphics ??= GetComponentsInChildren<Graphic>(true);
            if (m_Graphics.Length == 0)
                throw new InvalidOperationException($"{name} 没有可应用忽略深度材质的 Graphic。");

            for (int i = 0; i < m_Graphics.Length; i++)
                m_Graphics[i].material = m_IgnoreDepthMaterial;
        }
    }
}
