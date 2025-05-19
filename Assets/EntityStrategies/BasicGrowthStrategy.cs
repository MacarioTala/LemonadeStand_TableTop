using System.Collections.Generic;
using System.Linq;

public class BasicGrowthStrategy : iStrategy
{
    //Manifestation of preferences
    float _aggressionLevel = 0.2f;
    public float GetAggressionLevel() => _aggressionLevel;
    public List<ActionContext> GenerateActionContexts(iCompany company)
    {
        throw new System.NotImplementedException();
    }

    public void GenerateGoals(iCompany company)
    {
        var goals = new List<Goal>();
        var doubleCashGoal = new Goal("Double Initial Cash",
                                      "Double the initial cash of the company",
                                      null,
                                      (c,g) => g.SetOriginalValue("InitialCash",c.GetCash())
                                      );
        doubleCashGoal.IsGoalMet = c=>c.GetCash() >= (decimal)doubleCashGoal.GetOriginalValue<decimal>("InitialCash")*2;
        company.Goals.Add(doubleCashGoal);

        var tenLemonadeGoal = new Goal("Have 10 Lemonade",
                                      "Have 10 Lemonade in stock",
                                      null,
                                      (c, g) => g.SetOriginalValue("InitialLemonade", 0)
                                      )
        {
            IsGoalMet = c => c.GetInventory().GetInventoryEntriesByGood("Lemonade").Sum(e => e.quantity) >= 10
        };
        company.Goals.Add(tenLemonadeGoal);
    }

    public void PerformStrategy(ActionContext context)
    {
        switch(context.Action)
        {
            case ActionEnum.QueueTradeBuy:
                context.TradeToSubmit.Buyer.QueueOrder(context);
                break;

            case ActionEnum.QueueTradeSell:
                context.TradeToSubmit.Seller.QueueOrder(context);
                break;

            case ActionEnum.MakeRecipe:
                context.RecipeMaker.MakeRecipe(context);
                break;
            
            case ActionEnum.PublishBidAsk:
                var publisher = (Company)context.SubmittingCompany;
                publisher.SubmitBidAskSpreadToMarket(context);
                break;

            default :
                throw new ContextException("Action not supported");
        }
    }

    public void PerformStrategy(iCompany company)
    {
        throw new System.NotImplementedException();
    }

    public LemonadeStandResultObject SetAggressionLevel(float aggressionLevel)
    {
        _aggressionLevel = aggressionLevel;
        return LemonadeStandResultObject.Success();
    }
}
