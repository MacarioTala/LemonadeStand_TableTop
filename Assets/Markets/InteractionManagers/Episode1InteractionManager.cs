using Unity.VisualScripting.YamlDotNet.Core;

public class Episode1InteractionManager : InteractionManagerBase
{
   
    public override void MarketsProvideLiquidityOfLastResort()
    {
         //Provide buy side liquidity
        //Markets attempt to sell all inventory
        foreach(var inventoryItem in _market.GetInventory().GetInventoryEntries())
        {
            var sellOrder = new Order(null, _market, inventoryItem.good, inventoryItem.quantity, inventoryItem.Price)
            {
                SubmittingCompany = _market
            };
            var sellContext = new ActionContext()
                    { 
                        TradeToSubmit = sellOrder,
                        MarketToSubmitTo = _market,
                        Period = _market.CurrentPeriod  
                    };
            _market.QueueOrder(sellContext);
        }
    }
}