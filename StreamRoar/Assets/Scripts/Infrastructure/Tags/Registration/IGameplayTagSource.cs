namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// Tag 数据源扩展点：Native →（未来）Config。仅在 Manager 构造期间调用。
    /// </summary>
    public interface IGameplayTagSource
    {
        void Collect(IGameplayTagRegistration registration);
    }
}
