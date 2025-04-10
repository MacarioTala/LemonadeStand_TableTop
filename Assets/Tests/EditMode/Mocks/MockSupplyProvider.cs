
using System;
using System.Collections.Generic;

public class MockSupplyProvider : iSupplyProvider
{
    Market _market;
    public void Initialize(Market market)
    {
        _market = market;
    }
    public List<(Good Good, int Quantity, decimal Price)> _supplyInPeriod = new();
    public List<(Good Good, int Quantity, decimal Price)> GetSupplyInPeriod(int period)
    {
        return _supplyInPeriod;
    }
    public void AddSupply(Good good, int quantity, decimal price)
    {
        _supplyInPeriod.Add((good, quantity, price));
    }

    public LemonadeStandResultObject SupplyGoods()
    {
        throw new System.NotImplementedException();
    }
}
