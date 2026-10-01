namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 命令执行时的输出写入面。
    /// </summary>
    public interface IConsoleWriter
    {
        void WriteLine(string message);

        void WriteError(string message);
    }
}
