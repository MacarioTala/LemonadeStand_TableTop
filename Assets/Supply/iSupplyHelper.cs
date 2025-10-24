using System.Collections.Generic;

public interface iSupplyHelper : iMarketAware
{
    /// <summary>
    ///     Methods for the market to calculate supply statistics
    /// </summary>
    public void Initialize(Market market);
    public List<(Good Good,int Quantity, decimal Price)> GetSupplyInPeriod(int period);

}