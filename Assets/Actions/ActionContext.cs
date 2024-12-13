public class ActionContext
{
    //General
    public int Period;
    public ActionEnum Action;
    
    //Make Recipe
    public Company RecipeMaker;
    public Recipe Recipe;
    public int QuantityToMake;

    //QueueTrade/transactions
    public iCompany Buyer;
    public iCompany Seller;
    public Good GoodToBuy;
    public int Quantity;
    public decimal Price;
    public bool IsBuy;
    public Order TradeToSubmit;

    //Submit Bid/Ask
    public Market MarketToSubmitTo;
    public Company SubmittingCompany;
    public Good GoodToSubmit;
    public decimal BidToSubmit;
    public decimal AskToSubmit;

    public LemonadeStandResultObject DoesContextContainValidTrade()
    {
        if (TradeToSubmit == null) return LemonadeStandResultObject.Failure(ResultTypeEnum.ContextHasNoTrade, "Context has no trade");
        return LemonadeStandResultObject.Success();
    }

    public LemonadeStandResultObject DoesContextContainValidBidAskSpread()
    {
         if(BidToSubmit==0) return LemonadeStandResultObject.Failure(ResultTypeEnum.SpreadHasNoBid, "Spread has no bid");
         if(AskToSubmit==0) return LemonadeStandResultObject.Failure(ResultTypeEnum.SpreadHasNoAsk, "Spread has no ask");
         if(GoodToSubmit==null) return LemonadeStandResultObject.Failure(ResultTypeEnum.SpreadHasNoGood, "Spread has no good");
         if(MarketToSubmitTo==null) return LemonadeStandResultObject.Failure(ResultTypeEnum.MarketNotSet, "Market not set");
        return LemonadeStandResultObject.Success();
    }
}

public class ContextException : System.Exception
{
    public ContextException(string message) : base(message)
    {
    }
}