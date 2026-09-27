using System;
using System.Collections.Generic;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// Shipping / 非 Development 下的空实现。
    /// </summary>
    public sealed class NoopDebugConsole : IDebugConsole
    {
        static readonly IReadOnlyList<ConsoleOutputEntry> s_Empty = new ConsoleOutputEntry[0];

        public bool IsOpen => false;

        public void Open()
        {
        }

        public void Close()
        {
        }

        public void Toggle()
        {
        }

        public void WriteLine(string message)
        {
        }

        public void WriteError(string message)
        {
        }

        public void ClearOutput()
        {
        }

        public bool TryExecute(string line, out string error)
        {
            error = "Debug console disabled.";
            return false;
        }

        public void Register(IConsoleCommand command)
        {
        }

        public IReadOnlyList<ConsoleOutputEntry> GetOutputSnapshot() => s_Empty;

        public bool TryHistoryPrevious(out string line)
        {
            line = null;
            return false;
        }

        public bool TryHistoryNext(out string line)
        {
            line = null;
            return false;
        }

        public IReadOnlyList<string> GetCompletionCandidates(string line) => s_EmptyNames;

        public string SuggestCompletion(string line, int selectedCandidateIndex = 0) => null;

        public bool TryGetCommandHelp(string name, out string help)
        {
            help = null;
            return false;
        }

        static readonly IReadOnlyList<string> s_EmptyNames = Array.Empty<string>();
    }
}
