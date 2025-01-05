public interface iConsumptionManager
{
    void AdjustDemand (ActionContext context);
    LemonadeStandResultObject FulfillDemand (Market market);
}