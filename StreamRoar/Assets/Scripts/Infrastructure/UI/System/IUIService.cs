using UnityEngine;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 可打开 UI 显式开关服务。
    /// </summary>
    public interface IUIService
    {
        /// <summary>
        /// 打开当前作用域允许的 UI。
        /// </summary>
        void Open(string id);

        /// <summary>
        /// 关闭指定 UI。
        /// </summary>
        void Close(string id);

        /// <summary>
        /// 创建动态 Widget。
        /// </summary>
        GameObject CreateWidget(string id);

        /// <summary>
        /// 注册动态世界 Widget。
        /// </summary>
        void RegisterWorldWidget(IWorldWidget widget);

        /// <summary>
        /// 注销动态世界 Widget。
        /// </summary>
        void UnregisterWorldWidget(IWorldWidget widget);
    }
}
