public class SupplyDemandModifier : iPriceModifier
{
    public decimal Apply(decimal base_price, Good good, Market market)
    {
        var demand_data = market.MarketDemand;
        int demand = 0;
        if(demand_data.ContainsKey(good))
        {
            demand = demand_data[good].CurrentDemand;
        }
        var supply = market.GetTotalSupply(TheEconomy.Instance.tradingPeriod, good);
        var price_increase_threshold = good.price_increase_threshold;
        var price_decrease_threshold = good.price_decrease_threshold;
        var price_increment_rate = good.Get_price_increment_rate();
        
        var totalBought = market.GetTotalBought(TheEconomy.Instance.tradingPeriod, good);
        var totalSold = market.GetTotalSold(TheEconomy.Instance.tradingPeriod, good);

        if(totalBought >= price_increase_threshold)
        {
            base_price *= 1+price_increment_rate;
        }
        if(totalSold >= price_decrease_threshold)
        {
            base_price *= 1-price_increment_rate;
        }
        return base_price;
    }

}