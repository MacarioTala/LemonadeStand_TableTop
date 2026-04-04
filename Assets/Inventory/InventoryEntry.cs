using System;

public class InventoryEntry
{
    public Good good;
    public int quantity;
    public decimal Cost;
    private decimal price;
    public decimal Price {get =>Math.Round(price,3,MidpointRounding.AwayFromZero);}
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
        this.price = price;
    }
    public override string ToString()
    {
        return $"{good.GoodName} {quantity} units at {Cost} ";
    }

    public void CalculatePriceFromBand()
    {
        var priceBand = good.Get_price_band();
        SetPrice((decimal)UnityEngine.Random.Range((float)priceBand.Min, (float)priceBand.Max));
    }
}