using System.Collections.Generic;

public interface iMarketDataManager
{
    List<MarketData> PublishMarketData(Market market);
    void PublishSpreadToMarket(ActionContext context);
    
}