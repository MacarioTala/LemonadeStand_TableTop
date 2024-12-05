public interface iDemandStrategy
{
    const int MinDemand = 0;
    const int MaxDemand = 10000;
    void AdjustDemand(Market market);
    void InitializeDemandForSpecificGood(Market market, Good good, int initialDemand, int minDemand = MinDemand, int maxDemand = MaxDemand);
    void InitializeDemand (Market market);
}