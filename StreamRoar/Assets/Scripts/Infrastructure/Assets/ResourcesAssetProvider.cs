using System;
using UnityEngine;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 基于 Resources 的资源访问实现。
    /// </summary>
    public sealed class ResourcesAssetProvider : IAssetProvider
    {
        public T Load<T>(string key) where T : UnityEngine.Object
        {
            key = key.Replace('\\', '/');
            T asset = Resources.Load<T>(key);
            if (!asset)
                throw new InvalidOperationException($"Resources 未找到资源：{key} ({typeof(T).Name})");

            return asset;
        }

        public T[] LoadAll<T>(string path) where T : UnityEngine.Object
        {
            path = path.Replace('\\', '/').Trim('/');
            return Resources.LoadAll<T>(path);
        }

        public T LoadComponent<T>(string key) where T : Component
        {
            GameObject prefab = Load<GameObject>(key);
            T component = prefab.GetComponent<T>();
            if (!component)
                throw new InvalidOperationException($"Resources prefab 缺少组件：{key} ({typeof(T).Name})");

            return component;
        }
    }
}
