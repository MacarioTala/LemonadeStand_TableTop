public class InventoryEntry
{
    public Good good;
    public int quantity;
    public decimal Cost;
    public decimal Price;
    private Recipe recipe;
    public int RemainingDelay=0;
    public int PeriodAcquired;
    public Recipe GetRecipe()=> good.IsProducedGood?recipe:null;
    public void SetRecipe(Recipe recipe)
    {
        if(good.IsProducedGood)
        {
            this.recipe=recipe;
        }
        else
        {
            throw new System.Exception("This good is not a produced good");
        }
    }
    
    public InventoryEntry(Good good, int quantity, decimal acquisition_price, int period)
    {
        this.good = good;
        this.quantity = quantity;
        Cost = acquisition_price;
        PeriodAcquired = period;
        RemainingDelay=good.DeliveryDelay;
    }

    public void SetPrice(decimal price)
    {
        Price = price;
    }
    public override string ToString()
    {
        return $"{good.GoodName} {quantity} units at {Cost} ";
    }
}