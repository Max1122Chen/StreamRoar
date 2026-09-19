using System;
using System.Collections.Generic;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 运行时配置表查询提供器。
    /// </summary>
    public sealed class RuntimeConfigProvider : IConfigProvider
    {
        readonly Dictionary<Type, object> m_Tables;

        /// <summary>
        /// 根据配置表项创建查询索引。
        /// </summary>
        public RuntimeConfigProvider(IEnumerable<KeyValuePair<Type, object>> tables)
        {
            m_Tables = new Dictionary<Type, object>();
            foreach (KeyValuePair<Type, object> table in tables)
                m_Tables.Add(table.Key, table.Value);
        }

        /// <summary>
        /// 按生成表类型获取配置表。
        /// </summary>
        public TTable GetTable<TTable>() where TTable : class
        {
            return (TTable)m_Tables[typeof(TTable)];
        }
    }
}
