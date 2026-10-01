using System.Collections.Generic;
using System.Text;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 空白分词 + 双引号字符串；不做表达式求值。
    /// </summary>
    public static class ConsoleTokenizer
    {
        public static bool TryTokenize(string line, out List<string> tokens, out string error)
        {
            tokens = new List<string>();
            error = null;

            if (string.IsNullOrWhiteSpace(line))
                return true;

            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (inQuotes)
                {
                    if (c == '\\' && i + 1 < line.Length)
                    {
                        char next = line[i + 1];
                        if (next == '"' || next == '\\')
                        {
                            current.Append(next);
                            i++;
                            continue;
                        }
                    }

                    if (c == '"')
                    {
                        inQuotes = false;
                        continue;
                    }

                    current.Append(c);
                    continue;
                }

                if (c == '"')
                {
                    inQuotes = true;
                    continue;
                }

                if (char.IsWhiteSpace(c))
                {
                    if (current.Length > 0)
                    {
                        tokens.Add(current.ToString());
                        current.Clear();
                    }

                    continue;
                }

                current.Append(c);
            }

            if (inQuotes)
            {
                tokens = null;
                error = "Unclosed quote.";
                return false;
            }

            if (current.Length > 0)
                tokens.Add(current.ToString());

            return true;
        }
    }
}
