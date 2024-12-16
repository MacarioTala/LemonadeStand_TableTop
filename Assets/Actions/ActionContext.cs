using System.Collections.Generic;

public class ActionContext
{
    //General
    public ActionEnum Action;
    public Market MarketToSubmitTo;
    public int Period;

    private iCompany _submittingCompany;
    public iCompany SubmittingCompany
    {
        get => _submittingCompany;
        set
        {
            _submittingCompany = value;
            if( TradeToSubmit is not null )
                { TradeToSubmit.SubmittingCompany = SubmittingCompany; }
        }
    }
    
    //Make Recipe
    public Company RecipeMaker;
    public Recipe Recipe;
    public int QuantityToMake;

    //QueueTrade/transactions
    public Order TradeToSubmit;
    public List<Order> CounterPartyOrders;

    //Submit Bid/Ask
    public Good GoodToSubmit;
    public decimal BidToSubmit;
    public decimal AskToSubmit;

    public LemonadeStandResultObject IsContextValid()
    {
        if (SubmittingCompany == null) return LemonadeStandResultObject.Failure(ResultTypeEnum.OrderHasNoSubmittingCompany, "Order has no submitting company");
        return LemonadeStandResultObject.Success();
    }
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