public class FixedCostLedgerEntry
{
    public FixedCostInstance FixedCost;
    public int Period;

    public FixedCostLedgerEntry(FixedCostInstance cost,int period)
    {
        FixedCost=cost;
        Period = period;
    }
}