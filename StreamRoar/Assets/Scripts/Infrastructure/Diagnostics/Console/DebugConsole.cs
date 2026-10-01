using System.Collections.Generic;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// Debug Console 实现：缓冲、历史、执行编排。
    /// </summary>
    public sealed class DebugConsole : IDebugConsole
    {
        readonly ConsoleCommandRegistry m_Registry = new();
        readonly ConsoleExecutor m_Executor;
        readonly ConsoleHistory m_History;
        readonly List<ConsoleOutputEntry> m_Output = new();
        readonly int m_OutputCapacity;

        public bool IsOpen { get; private set; }

        public ConsoleCommandRegistry Registry => m_Registry;

        public DebugConsole(int outputCapacity = 512, int historyCapacity = 64)
        {
            m_OutputCapacity = outputCapacity > 0 ? outputCapacity : 512;
            m_History = new ConsoleHistory(historyCapacity);
            m_Executor = new ConsoleExecutor(m_Registry);
        }

        public void Open() => IsOpen = true;

        public void Close() => IsOpen = false;

        public void Toggle() => IsOpen = !IsOpen;

        public void WriteLine(string message)
        {
            Append(ConsoleOutputKind.Info, message);
        }

        public void WriteError(string message)
        {
            Append(ConsoleOutputKind.Error, message);
        }

        public void ClearOutput()
        {
            m_Output.Clear();
        }

        public void Register(IConsoleCommand command)
        {
            m_Registry.Register(command);
        }

        public bool TryExecute(string line, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(line))
                return true;

            Append(ConsoleOutputKind.Input, "> " + line.TrimEnd());
            m_History.Add(line);
            return m_Executor.TryExecute(line, this, out error);
        }

        public IReadOnlyList<ConsoleOutputEntry> GetOutputSnapshot() => m_Output;

        public bool TryHistoryPrevious(out string line) => m_History.TryPrevious(out line);

        public bool TryHistoryNext(out string line) => m_History.TryNext(out line);

        public IReadOnlyList<string> GetCompletionCandidates(string line) =>
            ConsoleCompletion.GetCandidates(line, m_Registry);

        public string SuggestCompletion(string line, int selectedCandidateIndex = 0) =>
            ConsoleCompletion.Accept(line, m_Registry, selectedCandidateIndex);

        public bool TryGetCommandHelp(string name, out string help)
        {
            help = null;
            if (!m_Registry.TryGet(name, out IConsoleCommand command))
                return false;
            help = command.Help;
            return true;
        }

        void Append(ConsoleOutputKind kind, string text)
        {
            m_Output.Add(new ConsoleOutputEntry(kind, text ?? string.Empty));
            while (m_Output.Count > m_OutputCapacity)
                m_Output.RemoveAt(0);
        }
    }
}
