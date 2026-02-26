using UnityEngine;

public class DefaultMarketInteractionManager : InteractionManagerBase
{
    public override void MarketsProvideLiquidityOfLastResort()
    {
        //No-op: Markets do not provide liquidity in Default.
        //Only use this for integration testing when isolating various liquidity scenarios.
    }
}