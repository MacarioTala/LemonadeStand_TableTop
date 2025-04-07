using System.Collections.Generic;
using Unity.VisualScripting.YamlDotNet.Core;

public interface iSupplyProvider
{
    /// <summary>
    ///     Methods for the market to supply goods outside of Companies
    /// </summary>
    
    public LemonadeStandResultObject SupplyGoods(Market market);
    public List<(Good Good,int Quantity,float Price)> GetSupplyInPeriod(Market market, int period);
}