namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 构建期向 Manager 注册路径；运行时密封后不可再注册。
    /// </summary>
    public interface IGameplayTagRegistration
    {
        void Register(string path);
    }
}
