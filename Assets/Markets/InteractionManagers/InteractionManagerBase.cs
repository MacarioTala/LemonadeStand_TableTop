using System.Linq;
using UnityEngine;
public abstract class InteractionManagerBase : iMarketInteractionManager
{
    protected Market _market;

    public virtual void EvaluateParticipantCollapse(EconAgent company)
    {
        if (company.IsBankrupt())
        {
            TheEconomy.Instance.HandleParticipantCollapse(_market, company);
        }
        if (company is PopulationAgent populationCompany)
        {
            if (populationCompany.IsMaxEnnui())
            {
                TheEconomy.Instance.HandleParticipantCollapse(_market, company);
            }
        }
    }

    public abstract void MarketsProvideLiquidityOfLastResort();
    
    public virtual void PopulationsAct(int period)
    {
        var marketParticipants = _market.GetMarketParticipants()
                                .OfType<PopulationAgent>()
                                .Where(x=>!x.IsPlayer)
                                .ToList();

        if (marketParticipants.Count() == 0)
        {
            Debug.Log($"Market {_market.Name} has no populations");
        }
        foreach (var participant in marketParticipants)
        {
            participant.PerformStrategy();
        }
    }

    public virtual LemonadeStandResultObject QueueMarketOrder(ActionContext context)
    {
        context.TradeToSubmit.SubmittingCompany = _market;
        var queueResult = QueueOrder(context);
        if (!queueResult.Equals(LemonadeStandResultObject.Success()))
            return queueResult;
        return LemonadeStandResultObject.Success();
    }

    public virtual LemonadeStandResultObject QueueOrder(ActionContext context)
    {
        var contextValidationResult = context.ContainsValidTrade();
        if (!contextValidationResult.Equals(LemonadeStandResultObject.Success()))
            return context.ContainsValidTrade();

        var queueResult = _market.TradeProcessor.QueueOrder(context);
        if (!queueResult.Equals(LemonadeStandResultObject.Success()))
            return queueResult;
        //Record the order
        _market.LogOrder(context.TradeToSubmit, context.Period);

        return LemonadeStandResultObject.Success();
    }

    public virtual void RegisterMarketParticipant(EconAgent marketParticipant)
    {
        var participants = _market.GetMarketParticipants();
         if (!participants.Contains(marketParticipant))
        {
            participants.Add(marketParticipant);
            marketParticipant.SetMarket(_market);
        }
        else
        {
            throw new TheEconomy_CompanyException($"Company {marketParticipant.Name} of type {marketParticipant.GetType()} already in Market {_market.MarketId}");
        }
        TheEconomy.Instance.RegisterEconomicAgent(marketParticipant);
    }

    public virtual LemonadeStandResultObject RemoveMarketParticipant(EconAgent marketParticipant)
    {
        var participants = _market.GetMarketParticipants();
        if (participants.Contains(marketParticipant))
        {
            participants.Remove(marketParticipant);
            marketParticipant.LeaveMarket();
            return LemonadeStandResultObject.Success();
        }
        return LemonadeStandResultObject.Failure(ResultTypeEnum.CompanyNotFound, $"Company {marketParticipant.Name} not found in Market {_market.Name}");
    }

    public void SetMarket(Market market)
        =>_market = market;

    public virtual void UpdateCompanyStatuses(int period)
    {
        var participantCopyforIteration = _market.GetMarketParticipants().ToList();
        foreach (var agent in participantCopyforIteration)
        {
            //Update Company Statuses
            agent.ExpireGoods(period);
            agent.SubtractFixedCostsForPeriod(period);
            agent.UpdateCurrentPeriod(period + 1);
            EvaluateParticipantCollapse(agent);
        }
    }
}