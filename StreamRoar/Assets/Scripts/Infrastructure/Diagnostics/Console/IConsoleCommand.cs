using System.Collections.Generic;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 已注册的 Debug Console 命令。
    /// </summary>
    public interface IConsoleCommand
    {
        string Name { get; }

        string Help { get; }

        ConsoleArgSchema Schema { get; }

        void Execute(IReadOnlyList<string> args, IConsoleWriter writer);
    }
}
