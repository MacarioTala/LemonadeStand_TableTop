public class InventoryEntry
{
    public Good good;
    public int quantity;
    public float acquisition_price;

    public InventoryEntry(Good good, int quantity, float acquisition_price)
    {
        this.good = good;
        this.quantity = quantity;
        this.acquisition_price = acquisition_price;
    }
}