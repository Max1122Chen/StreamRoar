namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 运行时配置查询接口。
    /// </summary>
    /// <remarks>
    /// 配置以表为单位发布，表行查询由具体表契约负责。
    /// </remarks>
    public interface IConfigProvider
    {
        /// <summary>
        /// 按表契约获取运行时配置表。
        /// </summary>
        TTable GetTable<TTable>() where TTable : class;
    }
}
