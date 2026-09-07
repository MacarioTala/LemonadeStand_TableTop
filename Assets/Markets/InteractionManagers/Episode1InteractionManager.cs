public class Episode1InteractionManager : InteractionManagerBase
{
   
    public override void MarketsProvideLiquidityOfLastResort()
    {
         //Provide buy side liquidity
        //Markets attempt to sell all inventory
        foreach(var inventoryItem in _market.GetInventory().GetInventoryEntries())
        {
            var sellOrder = new Order(null, _market, inventoryItem.good, inventoryItem.quantity, inventoryItem.PriceOfGood)
            {
                SubmittingCompany = _market
            };
            var sellContext = new ActionContext()
                    { 
                        TradeToSubmit = sellOrder,
                        MarketToSubmitTo = _market,
                        Period = _market.CurrentPeriod,
                        SubmittingCompany = _market
                    };
            _market.QueueOrder(sellContext);
        }
    }
}