public interface iConsumptionManager
{
    void AdjustDemand (ActionContext context);
    void FulfillDemand (Market market);
}