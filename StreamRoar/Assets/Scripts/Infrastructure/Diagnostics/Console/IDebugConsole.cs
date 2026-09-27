using System.Collections.Generic;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 开发期 Debug Console 门面：开闭、输出、执行、命令注册。
    /// </summary>
    public interface IDebugConsole : IConsoleWriter
    {
        bool IsOpen { get; }

        void Open();

        void Close();

        void Toggle();

        void ClearOutput();

        bool TryExecute(string line, out string error);

        void Register(IConsoleCommand command);

        IReadOnlyList<ConsoleOutputEntry> GetOutputSnapshot();

        bool TryHistoryPrevious(out string line);

        bool TryHistoryNext(out string line);

        /// <summary>命令名阶段的前缀候选（智能提示）；无则空列表。</summary>
        IReadOnlyList<string> GetCompletionCandidates(string line);

        /// <summary>Tab：接受选中候选或最长公共前缀；无法补全时返回 null。</summary>
        string SuggestCompletion(string line, int selectedCandidateIndex = 0);

        /// <summary>提示条展示用；未知命令返回 false。</summary>
        bool TryGetCommandHelp(string name, out string help);
    }
}
