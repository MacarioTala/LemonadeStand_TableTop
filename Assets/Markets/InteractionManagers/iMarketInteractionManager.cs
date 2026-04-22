public interface iMarketInteractionManager : iMarketAware
{
    void EvaluateParticipantCollapse(EconAgent company);
    void PopulationsAct(int period);
    void RegisterMarketParticipant(EconAgent marketParticipant);
    public LemonadeStandResultObject RemoveMarketParticipant(EconAgent company);
    LemonadeStandResultObject QueueOrder(ActionContext context);
    LemonadeStandResultObject QueueMarketOrder(ActionContext context);
    void UpdateCompanyStatuses(int period);
    void MarketsProvideLiquidityOfLastResort();
}