namespace StreamRoar.Infrastructure
{
    public interface IVfxService
    {
        PooledVfx Play(VfxPreset preset, in VfxSpawnRequest request);
    }
}
