using System;
using System.Collections.Generic;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 委托实现的命令，供元命令与测试使用。
    /// </summary>
    public sealed class DelegateConsoleCommand : IConsoleCommand
    {
        readonly Action<IReadOnlyList<string>, IConsoleWriter> m_Execute;

        public string Name { get; }
        public string Help { get; }
        public ConsoleArgSchema Schema { get; }

        public DelegateConsoleCommand(
            string name,
            string help,
            ConsoleArgSchema schema,
            Action<IReadOnlyList<string>, IConsoleWriter> execute)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Command name required.", nameof(name));
            Name = name.Trim();
            Help = help ?? string.Empty;
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            m_Execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public void Execute(IReadOnlyList<string> args, IConsoleWriter writer)
        {
            m_Execute(args, writer);
        }
    }
}
