namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 强类型事件监听接口。
    /// </summary>
    public interface IEventListener<in TEvent> where TEvent : struct, IEvent
    {
        /// <summary>
        /// 接收指定类型事件。
        /// </summary>
        void OnEvent(TEvent eventData);
    }
}
