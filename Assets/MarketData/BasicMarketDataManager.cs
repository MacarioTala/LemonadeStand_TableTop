using System.Collections.Generic;
using System.Linq;

public class BasicMarketDataManager : iMarketDataManager
{
    public List<MarketData> PublishMarketData(Market market)
    {
        //Modify here to reflect things like poor public information
        return market.MarketData;
    }

    public void PublishSpreadToMarket(ActionContext context)
    {
        List<MarketData> Spread = context.MarketToSubmitTo.MarketData;
        var MarketToSubmitTo = context.MarketToSubmitTo;
        var good = context.GoodToSubmit;
        var bid = context.BidToSubmit;
        var ask = context.AskToSubmit;
        var submittingCompany = context.SubmittingCompany;

        var isGoodInMarketData = Spread.Any(x=>x.Good==good && x.Company.Equals(submittingCompany));
        if(isGoodInMarketData)
        {
            var marketData = Spread.First(x=>x.Good==good && x.Company.Equals(submittingCompany));
            marketData.Bid = bid;
            marketData.Ask = ask;
        }
        else
        {
            var data = new MarketData
            {
                Company = submittingCompany,
                Good = good,
                Bid = bid,
                Ask = ask
            };
            Spread.Add(data);
        }
    }
}
