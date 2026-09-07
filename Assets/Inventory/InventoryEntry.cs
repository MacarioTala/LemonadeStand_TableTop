using System;

public class InventoryEntry
{
    public Good good;
    public int quantity;
    public int? Cost;
    private int price;
    public int PriceOfGood {get =>price;}
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
            throw new Exception("This good is not a produced good");
        }
    }
    
    public InventoryEntry(Good good, int quantity, int? acquisitionPrice, int period)
    {
        this.good = good;
        this.quantity = quantity;
        Cost = acquisitionPrice;
        price = acquisitionPrice??0;
        PeriodAcquired = period;
        RemainingDelay=good.DeliveryDelay;
    }

    public void SetPrice(int price)
    {
        this.price = price;
    }
    public override string ToString()
    {
        return $"{good.GoodName} {quantity} units at {Cost} ";
    }

    public void CalculatePriceFromBand()
    {
        var priceBand = good.GetPriceBand();
        SetPrice((int)Math.Round(UnityEngine.Random.Range(priceBand.Min, (float)priceBand.Max)));
    }
}