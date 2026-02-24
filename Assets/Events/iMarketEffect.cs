public interface iMarketEffect
{
    public void Apply(Market market);
    /// <summary>
    /// Saves the original state of the effect. This is used to reset the effect after it has been applied.
    /// Note: should be Noop in effects that modifyEffects outside themselves 
    /// like ModifyMonsterEffect, because those effects apply to other iMarketEvents that have lists of effects.
    /// </summary>
    public void Reset();
}
