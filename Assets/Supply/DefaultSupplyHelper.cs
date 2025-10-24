using System.Collections.Generic;
using System.Linq;

public class DefaultSupplyHelper : iSupplyHelper
{
    Market _market;
    public List<(Good Good, int Quantity, decimal Price)> GetSupplyInPeriod(int period)
    {
        var ordersSubmittedInPeriod = _market.GetOrdersSubmittedInPeriod(period)
            .Where(x=> x.IsSell())
            .ToList();

        if (ordersSubmittedInPeriod.Count == 0) return null;
        var supply = new List<(Good Good, int Quantity, decimal Price)>();
        foreach (var order in ordersSubmittedInPeriod)
        {
            var good = order.Good;
            var quantity = order.Quantity;
            var price = order.Price;
            supply.Add(new(good, quantity, price));
        }
        return supply;
    }

    public void Initialize(Market market)
    {
        _market = market;
    }

    public void SetMarket(Market market)
    {
        _market = market;
    }   
}
