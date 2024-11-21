public class InventoryEntry
{
    public Good good;
    public int quantity;
    public decimal acquisition_price;

    public int PeriodAcquired;

    public InventoryEntry(Good good, int quantity, decimal acquisition_price, int period)
    {
        this.good = good;
        this.quantity = quantity;
        this.acquisition_price = acquisition_price;
        PeriodAcquired = period;
    }

    public override string ToString()
    {
        return $"{good.good_name} {quantity} units at {acquisition_price} ";
    }
}