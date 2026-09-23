using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("StreamRoar.Tests.EditMode")]

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 轻量服务定位器：用于运行时服务注册与解析。
    /// </summary>
    public static class ServiceLocator
    {
        static readonly Dictionary<Type, object> s_Services = new();

        public static void Register<T>(T service) where T : class
        {
            s_Services[typeof(T)] = service;
        }

        public static void Unregister<T>(T service) where T : class
        {
            Type key = typeof(T);
            if (!s_Services.TryGetValue(key, out object current))
                return;

            if (ReferenceEquals(current, service))
                s_Services.Remove(key);
        }

        public static T Resolve<T>() where T : class
        {
            if (s_Services.TryGetValue(typeof(T), out object service))
                return service as T;

            return null;
        }

        /// <summary>
        /// 仅供 EditMode 测试隔离使用；生产代码禁止调用。
        /// </summary>
        internal static void ClearForTests()
        {
            s_Services.Clear();
        }
    }
}
