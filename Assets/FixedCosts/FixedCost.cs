public class FixedCost
{
    public string Description { get; set; }
    public FixedCostEnum FixedCostType { get; set; }
    public decimal Amount { get; set; }
    public int Frequency { get; set; }
    public int PeriodAcquired { get; set; }
}