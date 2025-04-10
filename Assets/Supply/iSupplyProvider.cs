using System;
using System.Collections.Generic;
using Unity.VisualScripting.YamlDotNet.Core;

public interface iSupplyProvider
{
    /// <summary>
    ///     Methods for the market to supply goods outside of Companies
    /// </summary>
    public void Initialize(Market market);
    public LemonadeStandResultObject SupplyGoods();
    public List<(Good Good,int Quantity,Decimal Price)> GetSupplyInPeriod(int period);

}