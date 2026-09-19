using System;
using UnityEngine;
using Luban;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 从二进制数据源读取 Luban 表的二进制缓冲区。
    /// </summary>
    public sealed class LubanBinaryConfigLoader
    {
        readonly Func<string, byte[]> m_LoadBytes;

        /// <summary>
        /// 使用外部字节读取委托创建加载器。
        /// </summary>
        public LubanBinaryConfigLoader(Func<string, byte[]> loadBytes)
        {
            m_LoadBytes = loadBytes ?? throw new ArgumentNullException(nameof(loadBytes));
        }

        /// <summary>
        /// 使用运行时资源提供器创建加载器。
        /// </summary>
        public LubanBinaryConfigLoader(IAssetProvider assets)
            : this(tableName => assets.Load<TextAsset>(tableName).bytes)
        {
        }

        /// <summary>
        /// 从数据源读取指定表的二进制缓冲区。
        /// </summary>
        public ByteBuf Load(string tableName)
        {
            byte[] bytes = m_LoadBytes(tableName);
            if (bytes == null || bytes.Length == 0)
                throw new InvalidOperationException($"Luban 表数据为空：{tableName}");

            return new ByteBuf(bytes);
        }
    }
}
