using UnityEngine;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 动态世界 Widget 的注册对象。
    /// </summary>
    public interface IWorldWidget
    {
        string WidgetId { get; }
        Transform Anchor { get; }
        Vector3 Offset { get; }

        /// <summary>
        /// 绑定 UI 实例。
        /// </summary>
        void Bind(GameObject instance);

        /// <summary>
        /// 解除 UI 实例绑定。
        /// </summary>
        void Unbind();
    }
}
