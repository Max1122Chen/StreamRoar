using System;
using System.Collections.Generic;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// Tag 集合。查询父子匹配时需传入 <see cref="IGameplayTagManager"/>，且 includeChildren 为显式参数。
    /// </summary>
    public sealed class GameplayTagContainer
    {
        readonly HashSet<int> m_TagIds = new();

        public int Count => m_TagIds.Count;

        public void Add(GameplayTag tag)
        {
            if (!tag.IsValid)
                throw new ArgumentException("不能向容器添加 Invalid Tag。", nameof(tag));
            m_TagIds.Add(tag.Id);
        }

        public bool Remove(GameplayTag tag) => tag.IsValid && m_TagIds.Remove(tag.Id);

        public void Clear() => m_TagIds.Clear();

        /// <summary>精确拥有该 Tag。</summary>
        public bool Has(GameplayTag tag) => tag.IsValid && m_TagIds.Contains(tag.Id);

        /// <summary>
        /// 是否拥有该 Tag；当 <paramref name="includeChildren"/> 为 true 时，
        /// 拥有其任意子孙 Tag 也视为命中。
        /// </summary>
        public bool Has(GameplayTag tag, bool includeChildren, IGameplayTagManager manager)
        {
            if (!tag.IsValid)
                return false;
            if (m_TagIds.Contains(tag.Id))
                return true;
            if (!includeChildren)
                return false;
            if (manager == null)
                throw new ArgumentNullException(nameof(manager));

            foreach (int ownedId in m_TagIds)
            {
                if (manager.IsChildOf(new GameplayTag(ownedId), tag))
                    return true;
            }

            return false;
        }

        public bool HasAny(IEnumerable<GameplayTag> tags, bool includeChildren, IGameplayTagManager manager)
        {
            if (tags == null)
                throw new ArgumentNullException(nameof(tags));

            foreach (GameplayTag tag in tags)
            {
                if (Has(tag, includeChildren, manager))
                    return true;
            }

            return false;
        }

        public bool HasAll(IEnumerable<GameplayTag> tags, bool includeChildren, IGameplayTagManager manager)
        {
            if (tags == null)
                throw new ArgumentNullException(nameof(tags));

            foreach (GameplayTag tag in tags)
            {
                if (!Has(tag, includeChildren, manager))
                    return false;
            }

            return true;
        }

        public IEnumerable<GameplayTag> Enumerate()
        {
            foreach (int id in m_TagIds)
                yield return new GameplayTag(id);
        }
    }
}
