namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// 标识一次音频播放，可用于主动停止仍在运行的声音。
    /// </summary>
    public readonly struct AudioHandle
    {
        internal AudioHandle(int id)
        {
            Id = id;
        }

        internal int Id { get; }

        /// <summary>
        /// 是否持有一个可查询的播放标识。
        /// </summary>
        public bool IsValid => Id > 0;
    }
}
