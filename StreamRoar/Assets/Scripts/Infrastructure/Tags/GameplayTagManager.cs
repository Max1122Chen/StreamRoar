using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// GameplayTag 权威注册表。构造时收集各 Source 后密封，禁止运行时动态注册。
    /// </summary>
    public sealed class GameplayTagManager : IGameplayTagManager, IGameplayTagRegistration
    {
        static readonly Regex s_PathPattern = new(
            @"\A[A-Za-z][A-Za-z0-9_]*(\.[A-Za-z][A-Za-z0-9_]*)*\z",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        readonly Dictionary<string, int> m_PathToId = new(StringComparer.Ordinal);
        readonly Dictionary<int, TagNode> m_Nodes = new();
        readonly List<GameplayTag> m_AllTags = new();
        bool m_Sealed;
        int m_NextId = 1;

        public GameplayTagManager(IEnumerable<IGameplayTagSource> sources)
        {
            if (sources == null)
                throw new ArgumentNullException(nameof(sources));

            foreach (IGameplayTagSource source in sources)
            {
                if (source == null)
                    throw new ArgumentException("Tag source 不能为 null。", nameof(sources));
                source.Collect(this);
            }

            m_Sealed = true;
        }

        /// <summary>测试与工具用：仅使用给定 Source 构建。</summary>
        public static GameplayTagManager Create(params IGameplayTagSource[] sources)
        {
            return new GameplayTagManager(sources);
        }

        void IGameplayTagRegistration.Register(string path)
        {
            if (m_Sealed)
                throw new InvalidOperationException("GameplayTagManager 已密封，禁止运行时注册 Tag。");

            ValidatePath(path);
            EnsurePathHierarchy(path);
        }

        public GameplayTag RequestTag(string path)
        {
            if (!TryRequestTag(path, out GameplayTag tag))
                throw new ArgumentException($"未注册的 GameplayTag：'{path}'。", nameof(path));
            return tag;
        }

        public bool TryRequestTag(string path, out GameplayTag tag)
        {
            tag = GameplayTag.Invalid;
            if (string.IsNullOrEmpty(path) || !s_PathPattern.IsMatch(path))
                return false;

            if (!m_PathToId.TryGetValue(path, out int id))
                return false;

            tag = new GameplayTag(id);
            return true;
        }

        public bool IsValid(GameplayTag tag) => tag.IsValid && m_Nodes.ContainsKey(tag.Id);

        public string GetPath(GameplayTag tag)
        {
            return m_Nodes.TryGetValue(tag.Id, out TagNode node) ? node.Path : null;
        }

        public bool IsChildOf(GameplayTag tag, GameplayTag ancestor)
        {
            if (!IsValid(tag) || !IsValid(ancestor))
                return false;
            if (tag.Id == ancestor.Id)
                return true;

            int current = m_Nodes[tag.Id].ParentId;
            while (current > 0)
            {
                if (current == ancestor.Id)
                    return true;
                current = m_Nodes[current].ParentId;
            }

            return false;
        }

        public IReadOnlyList<GameplayTag> GetAllTags() => m_AllTags;

        void EnsurePathHierarchy(string fullPath)
        {
            string[] segments = fullPath.Split('.');
            string cumulative = null;
            int parentId = 0;

            for (int i = 0; i < segments.Length; i++)
            {
                cumulative = cumulative == null ? segments[i] : cumulative + "." + segments[i];
                bool isLeaf = i == segments.Length - 1;

                if (m_PathToId.TryGetValue(cumulative, out int existingId))
                {
                    parentId = existingId;
                    if (isLeaf)
                    {
                        TagNode existing = m_Nodes[existingId];
                        if (existing.IsImplicit)
                            m_Nodes[existingId] = existing.AsExplicit();
                    }

                    continue;
                }

                int id = m_NextId++;
                bool implicitNode = !isLeaf;
                m_PathToId.Add(cumulative, id);
                m_Nodes.Add(id, new TagNode(cumulative, parentId, implicitNode));
                m_AllTags.Add(new GameplayTag(id));
                parentId = id;
            }
        }

        static void ValidatePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Tag 路径不能为空。", nameof(path));
            if (!s_PathPattern.IsMatch(path))
                throw new ArgumentException(
                    "Tag 路径须为以点分隔的标识符段（字母开头，可含数字与下划线），例如 State.Debuff.Burn。",
                    nameof(path));
        }

        readonly struct TagNode
        {
            public readonly string Path;
            public readonly int ParentId;
            public readonly bool IsImplicit;

            public TagNode(string path, int parentId, bool isImplicit)
            {
                Path = path;
                ParentId = parentId;
                IsImplicit = isImplicit;
            }

            public TagNode AsExplicit() => new(Path, ParentId, false);
        }
    }
}
