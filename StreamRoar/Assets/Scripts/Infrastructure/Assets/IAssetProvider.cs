using UnityEngine;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 运行时资源访问接口。
    /// </summary>
    public interface IAssetProvider
    {
        /// <summary>
        /// 按 key 加载 Unity 资源。
        /// </summary>
        T Load<T>(string key) where T : Object;

        /// <summary>
        /// 加载指定目录及子目录中的全部指定类型资源，空路径表示资源根目录。
        /// </summary>
        T[] LoadAll<T>(string path) where T : Object;

        /// <summary>
        /// 按 key 加载 prefab，并返回 prefab 上的指定组件。
        /// </summary>
        T LoadComponent<T>(string key) where T : Component;
    }
}
