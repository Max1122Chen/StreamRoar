using System;
using System.Collections.Generic;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 命令名前缀候选与 Tab 补全（参数补全后置）。
    /// </summary>
    public static class ConsoleCompletion
    {
        public const int DefaultMaxCandidates = 8;

        /// <summary>
        /// Tab / LCP 收集上限（避免无界分配；命令集通常远小于此）。
        /// </summary>
        const int SuggestCollectLimit = 256;

        /// <summary>
        /// 命令名阶段的前缀匹配列表（已排序）；有空白（进入参数）或空前缀时返回空。
        /// </summary>
        public static IReadOnlyList<string> GetCandidates(
            string line,
            ConsoleCommandRegistry registry,
            int maxCandidates = DefaultMaxCandidates)
        {
            if (registry == null || string.IsNullOrEmpty(line))
                return Array.Empty<string>();

            if (line.IndexOfAny(new[] { ' ', '\t' }) >= 0)
                return Array.Empty<string>();

            string prefix = line.Trim();
            if (prefix.Length == 0)
                return Array.Empty<string>();

            int limit = maxCandidates > 0 ? maxCandidates : DefaultMaxCandidates;
            // 切勿把 limit 当作 List 容量：曾用 int.MaxValue 导致 OOM。
            var matches = new List<string>(Math.Min(limit, DefaultMaxCandidates));
            foreach (string name in registry.GetNamesSorted())
            {
                if (!name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    continue;
                matches.Add(name);
                if (matches.Count >= limit)
                    break;
            }

            return matches;
        }

        /// <summary>
        /// Tab：唯一匹配返回全名；多匹配返回最长公共前缀；无匹配返回 null。
        /// </summary>
        public static string Suggest(string line, ConsoleCommandRegistry registry)
        {
            IReadOnlyList<string> matches = GetCandidates(line, registry, SuggestCollectLimit);
            if (matches.Count == 0)
                return null;
            if (matches.Count == 1)
                return matches[0];

            return LongestCommonPrefix(matches);
        }

        /// <summary>
        /// 接受指定下标的候选；越界时回退到 Suggest。
        /// </summary>
        public static string Accept(string line, ConsoleCommandRegistry registry, int selectedIndex)
        {
            // 与 View 可见候选一致即可；勿无无界 limit。
            IReadOnlyList<string> matches = GetCandidates(line, registry, DefaultMaxCandidates);
            if (matches.Count == 0)
                return null;
            if (selectedIndex >= 0 && selectedIndex < matches.Count)
                return matches[selectedIndex];
            return Suggest(line, registry);
        }

        static string LongestCommonPrefix(IReadOnlyList<string> values)
        {
            if (values.Count == 0)
                return string.Empty;

            string first = values[0];
            int len = first.Length;
            for (int i = 1; i < values.Count; i++)
            {
                int j = 0;
                string other = values[i];
                while (j < len && j < other.Length &&
                       char.ToLowerInvariant(first[j]) == char.ToLowerInvariant(other[j]))
                {
                    j++;
                }

                len = j;
                if (len == 0)
                    return string.Empty;
            }

            return first.Substring(0, len);
        }
    }
}
