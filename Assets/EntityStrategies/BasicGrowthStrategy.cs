using System.Collections.Generic;

public class BasicGrowthStrategy : iStrategy
{
    public List<ActionContext> GenerateActionContexts(iCompany company)
    {
        throw new System.NotImplementedException();
    }

    public List<Goal> GenerateGoals(iCompany company)
    {
        throw new System.NotImplementedException();
    }

    public void PerformStrategy(ActionContext context)
    {
        switch(context.Action)
        {
            case ActionEnum.QueueTradeBuy:
                context.Buyer.QueueTrade(context);
                break;

            case ActionEnum.QueueTradeSell:
                context.Seller.QueueTrade(context);
                break;

            case ActionEnum.MakeRecipe:
                context.RecipeMaker.MakeRecipe(context);
                break;
            
            case ActionEnum.PublishBidAsk:
                context.SubmittingCompany.SubmitBidAskSpreadToMarket(context);
                break;

            default :
                throw new ContextException("Action not supported");
        }
    }
}
