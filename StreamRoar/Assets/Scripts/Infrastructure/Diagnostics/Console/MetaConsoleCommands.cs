using System;
using System.Collections.Generic;
using System.Text;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// Console 内核元命令（help / clear / echo）。不含任何域命令。
    /// </summary>
    public static class MetaConsoleCommands
    {
        public static void RegisterAll(IDebugConsole console, ConsoleCommandRegistry registry)
        {
            if (console == null)
                throw new ArgumentNullException(nameof(console));
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            registry.Register(new DelegateConsoleCommand(
                "help",
                "List commands, or show help for one command.",
                ConsoleArgSchema.Optional(ConsoleArgType.String),
                (args, writer) => ExecuteHelp(registry, args, writer)));

            registry.Register(new DelegateConsoleCommand(
                "clear",
                "Clear console output.",
                ConsoleArgSchema.None,
                (_, __) => console.ClearOutput()));

            registry.Register(new DelegateConsoleCommand(
                "echo",
                "Echo arguments (smoke / pipeline).",
                ConsoleArgSchema.Variadic(0, ConsoleArgType.String),
                (args, writer) => writer.WriteLine(string.Join(" ", args))));
        }

        static void ExecuteHelp(ConsoleCommandRegistry registry, IReadOnlyList<string> args, IConsoleWriter writer)
        {
            if (args.Count == 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Commands:");
                foreach (string name in registry.GetNamesSorted())
                {
                    if (!registry.TryGet(name, out IConsoleCommand cmd))
                        continue;
                    sb.Append("  ").Append(cmd.Name);
                    if (!string.IsNullOrEmpty(cmd.Help))
                        sb.Append(" — ").Append(cmd.Help);
                    sb.AppendLine();
                }

                writer.WriteLine(sb.ToString().TrimEnd());
                return;
            }

            string query = args[0];
            if (!registry.TryGet(query, out IConsoleCommand command))
            {
                writer.WriteError($"Unknown command: {query}");
                return;
            }

            writer.WriteLine($"{command.Name} {command.Schema.Format()}");
            if (!string.IsNullOrEmpty(command.Help))
                writer.WriteLine(command.Help);
        }
    }
}
