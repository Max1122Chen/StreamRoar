namespace StreamRoar.Infrastructure
{
    /// <summary>
    /// C# 原生 Tag 种子。后续玩法可扩展本类或另增 IGameplayTagSource。
    /// </summary>
    public sealed class NativeGameplayTagSource : IGameplayTagSource
    {
        public const string State = "State";
        public const string StateDebuff = "State.Debuff";
        public const string StateDebuffBurn = "State.Debuff.Burn";
        public const string StateBuff = "State.Buff";
        public const string StateBuffHaste = "State.Buff.Haste";
        public const string Ability = "Ability";
        public const string AbilityAttack = "Ability.Attack";

        public void Collect(IGameplayTagRegistration registration)
        {
            registration.Register(StateDebuffBurn);
            registration.Register(StateBuffHaste);
            registration.Register(AbilityAttack);
        }
    }
}
