
using System.Collections.Generic;

public class MockSupplyProvider : iSupplyProvider
{
    public List<(Good Good, int Quantity, float Price)> _supplyInPeriod = new();
    public List<(Good Good, int Quantity, float Price)> GetSupplyInPeriod(Market market, int period)
    {
        return _supplyInPeriod;
    }
    public void AddSupply(Good good, int quantity, float price)
    {
        _supplyInPeriod.Add((good, quantity, price));
    }

    public LemonadeStandResultObject SupplyGoods(Market market)
    {
        throw new System.NotImplementedException();
    }
}
