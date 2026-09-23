namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 强类型同步事件总线。
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// 订阅指定类型事件。
        /// </summary>
        void Subscribe<TEvent>(IEventListener<TEvent> listener) where TEvent : struct, IEvent;

        /// <summary>
        /// 订阅指定作用域的类型事件。
        /// </summary>
        void Subscribe<TEvent>(object scope, IEventListener<TEvent> listener) where TEvent : struct, IEvent;

        /// <summary>
        /// 取消指定类型事件订阅。
        /// </summary>
        void Unsubscribe<TEvent>(IEventListener<TEvent> listener) where TEvent : struct, IEvent;

        /// <summary>
        /// 取消指定作用域的类型事件订阅。
        /// </summary>
        void Unsubscribe<TEvent>(object scope, IEventListener<TEvent> listener) where TEvent : struct, IEvent;

        /// <summary>
        /// 同步发布指定类型事件。
        /// </summary>
        void Publish<TEvent>(TEvent eventData) where TEvent : struct, IEvent;

        /// <summary>
        /// 同步发布指定作用域的类型事件。
        /// </summary>
        void Publish<TEvent>(object scope, TEvent eventData) where TEvent : struct, IEvent;

        /// <summary>
        /// 清除全部事件订阅。
        /// </summary>
        void Clear();
    }
}
