namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 对象池生命周期回调。
    /// </summary>
    public interface IPoolLifecycle
    {
        /// <summary>
        /// 对象被取出时调用。可以在这里重置对象状态，准备好被使用。
        /// </summary>
        void OnPoolRent();

        /// <summary>
        /// 对象被归还时调用。可以在这里清理对象状态，准备好被下次使用。
        /// </summary>
        void OnPoolReturn();
    }
}
