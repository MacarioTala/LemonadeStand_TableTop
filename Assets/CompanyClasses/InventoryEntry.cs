public class InventoryEntry
{
    public Good good;
    public int quantity;
    public decimal acquisition_price;

    public InventoryEntry(Good good, int quantity, decimal acquisition_price)
    {
        this.good = good;
        this.quantity = quantity;
        this.acquisition_price = acquisition_price;
    }

    public override string ToString()
    {
        return $"{good.good_name} {quantity} units at {acquisition_price} ";
    }
}