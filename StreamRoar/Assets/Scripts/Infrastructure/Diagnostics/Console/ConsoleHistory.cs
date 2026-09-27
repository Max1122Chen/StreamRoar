using System.Collections.Generic;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 命令历史；上/下浏览。提交后重置浏览游标。
    /// </summary>
    public sealed class ConsoleHistory
    {
        readonly List<string> m_Entries = new();
        readonly int m_Capacity;
        int m_BrowseIndex = -1;

        public ConsoleHistory(int capacity = 64)
        {
            m_Capacity = capacity > 0 ? capacity : 64;
        }

        public int Count => m_Entries.Count;

        public void Add(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return;

            string trimmed = line.TrimEnd();
            if (m_Entries.Count > 0 && m_Entries[m_Entries.Count - 1] == trimmed)
            {
                m_BrowseIndex = m_Entries.Count;
                return;
            }

            m_Entries.Add(trimmed);
            while (m_Entries.Count > m_Capacity)
                m_Entries.RemoveAt(0);

            m_BrowseIndex = m_Entries.Count;
        }

        public bool TryPrevious(out string line)
        {
            line = null;
            if (m_Entries.Count == 0)
                return false;

            if (m_BrowseIndex > 0)
                m_BrowseIndex--;
            else
                m_BrowseIndex = 0;

            line = m_Entries[m_BrowseIndex];
            return true;
        }

        public bool TryNext(out string line)
        {
            line = null;
            if (m_Entries.Count == 0)
                return false;

            if (m_BrowseIndex < m_Entries.Count - 1)
            {
                m_BrowseIndex++;
                line = m_Entries[m_BrowseIndex];
                return true;
            }

            m_BrowseIndex = m_Entries.Count;
            line = string.Empty;
            return true;
        }

        public void ResetBrowse()
        {
            m_BrowseIndex = m_Entries.Count;
        }
    }
}
