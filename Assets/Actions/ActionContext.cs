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
    public Trade TradeToSubmit;

    //Submit Bid/Ask
    public Market MarketToSubmitTo;
    public Company SubmittingCompany;
    public Good GoodToSubmit;
    public decimal BidToSubmit;
    public decimal AskToSubmit;

}

public class ContextException : System.Exception
{
    public ContextException(string message) : base(message)
    {
    }
}