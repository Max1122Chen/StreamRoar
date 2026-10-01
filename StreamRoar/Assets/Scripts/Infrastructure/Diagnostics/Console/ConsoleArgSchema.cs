using System;

namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 命令参数签名：固定或可变 arity + 统一元素类型（MVP）。
    /// </summary>
    public sealed class ConsoleArgSchema
    {
        public static ConsoleArgSchema None { get; } = new(0, 0, ConsoleArgType.String);

        public int MinCount { get; }
        public int MaxCount { get; }
        public ConsoleArgType ElementType { get; }

        public ConsoleArgSchema(int minCount, int maxCount, ConsoleArgType elementType)
        {
            if (minCount < 0)
                throw new ArgumentOutOfRangeException(nameof(minCount));
            if (maxCount < minCount)
                throw new ArgumentOutOfRangeException(nameof(maxCount));

            MinCount = minCount;
            MaxCount = maxCount;
            ElementType = elementType;
        }

        public static ConsoleArgSchema Exact(int count, ConsoleArgType type) => new(count, count, type);

        public static ConsoleArgSchema Optional(ConsoleArgType type) => new(0, 1, type);

        public static ConsoleArgSchema Variadic(int minCount, ConsoleArgType type) =>
            new(minCount, int.MaxValue, type);

        public string Format()
        {
            if (MaxCount == 0)
                return "(no args)";
            if (MinCount == MaxCount)
                return $"<{ElementType.ToString().ToLowerInvariant()}> x{MinCount}";
            if (MaxCount == int.MaxValue)
                return $"<{ElementType.ToString().ToLowerInvariant()}>... (min {MinCount})";
            return $"<{ElementType.ToString().ToLowerInvariant()}> ({MinCount}..{MaxCount})";
        }
    }
}
