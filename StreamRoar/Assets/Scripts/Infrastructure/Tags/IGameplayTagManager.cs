using System.Collections.Generic;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// Tag 注册表：路径解析、层级查询与调试列举。
    /// </summary>
    public interface IGameplayTagManager
    {
        /// <summary>严格解析已注册路径；未知或非法时抛出。</summary>
        GameplayTag RequestTag(string path);

        /// <summary>尝试解析；未知返回 false 且 tag 为 Invalid。</summary>
        bool TryRequestTag(string path, out GameplayTag tag);

        bool IsValid(GameplayTag tag);

        /// <summary>返回注册时的完整路径；无效 Tag 返回 null。</summary>
        string GetPath(GameplayTag tag);

        /// <summary>判断 <paramref name="tag"/> 是否为 <paramref name="ancestor"/> 本身或其子孙。</summary>
        bool IsChildOf(GameplayTag tag, GameplayTag ancestor);

        IReadOnlyList<GameplayTag> GetAllTags();
    }
}
