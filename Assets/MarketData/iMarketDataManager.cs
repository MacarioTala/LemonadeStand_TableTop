using System.Collections.Generic;

public interface iMarketDataManager
{
    List<MarketData> PublishMarketData();
    void PublishSpreadToMarket(ActionContext context);
    
}