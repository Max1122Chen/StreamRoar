namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 按存档标识读写数据，不持有业务状态或决定保存时机。
    /// </summary>
    public interface ISaveService
    {
        /// <summary>
        /// 读取存档。仅在存档不存在时返回 false，读取或反序列化失败时抛出异常。
        /// </summary>
        bool TryLoad<T>(string saveId, out T data);

        /// <summary>
        /// 保存非空数据，覆盖同标识的已有存档。
        /// </summary>
        void Save<T>(string saveId, T data);

        /// <summary>
        /// 检查存档是否存在，不校验数据内容。
        /// </summary>
        bool Exists(string saveId);

        /// <summary>
        /// 删除存档。存档不存在时不执行操作。
        /// </summary>
        void Delete(string saveId);
    }
}
