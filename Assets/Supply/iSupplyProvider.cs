using System;
using System.Collections.Generic;

public interface iSupplyProvider : iMarketAware
{
    /// <summary>
    ///     Methods for the market to supply goods outside of Companies
    /// </summary>
    public void Initialize(Market market);
    public LemonadeStandResultObject SupplyGoods();
    public List<(Good Good,int Quantity,Decimal Price)> GetSupplyInPeriod(int period);

}