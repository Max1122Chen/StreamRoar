using System;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 已注册 GameplayTag 的轻量句柄；相等比较基于稳定 Id。
    /// </summary>
    public readonly struct GameplayTag : IEquatable<GameplayTag>
    {
        public static readonly GameplayTag Invalid = default;

        readonly int m_Id;

        internal GameplayTag(int id)
        {
            m_Id = id;
        }

        /// <summary>内部稳定 Id；0 表示无效。</summary>
        public int Id => m_Id;

        public bool IsValid => m_Id > 0;

        public bool Equals(GameplayTag other) => m_Id == other.m_Id;

        public override bool Equals(object obj) => obj is GameplayTag other && Equals(other);

        public override int GetHashCode() => m_Id;

        public static bool operator ==(GameplayTag left, GameplayTag right) => left.m_Id == right.m_Id;

        public static bool operator !=(GameplayTag left, GameplayTag right) => left.m_Id != right.m_Id;

        public override string ToString() => IsValid ? $"GameplayTag({m_Id})" : "GameplayTag(Invalid)";
    }
}
