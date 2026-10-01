using System;
using System.Collections.Generic;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 命令注册表。查找不区分大小写；显示名保留注册时的写法。
    /// </summary>
    public sealed class ConsoleCommandRegistry
    {
        readonly Dictionary<string, IConsoleCommand> m_Commands =
            new(StringComparer.OrdinalIgnoreCase);

        readonly List<IConsoleCommand> m_Ordered = new();

        public int Count => m_Ordered.Count;

        public void Register(IConsoleCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (string.IsNullOrWhiteSpace(command.Name))
                throw new ArgumentException("Command name required.", nameof(command));

            string key = command.Name.Trim();
            if (m_Commands.ContainsKey(key))
                throw new InvalidOperationException($"Duplicate console command: '{key}'.");

            m_Commands.Add(key, command);
            m_Ordered.Add(command);
        }

        public bool TryGet(string name, out IConsoleCommand command)
        {
            command = null;
            if (string.IsNullOrWhiteSpace(name))
                return false;
            return m_Commands.TryGetValue(name.Trim(), out command);
        }

        public IReadOnlyList<IConsoleCommand> GetAll() => m_Ordered;

        public IEnumerable<string> GetNamesSorted()
        {
            var names = new List<string>(m_Ordered.Count);
            for (int i = 0; i < m_Ordered.Count; i++)
                names.Add(m_Ordered[i].Name);
            names.Sort(StringComparer.OrdinalIgnoreCase);
            return names;
        }
    }
}
