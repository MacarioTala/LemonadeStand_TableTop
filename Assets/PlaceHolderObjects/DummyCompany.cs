using System.Collections.Generic;
using UnityEngine;

public class DummyCompany : iEconAgent
{
    public string Name {get; set;}="Raw Materials Source";
    public int InfiniteCash => 10000000;
        
    public Inventory Inventory{get; private set;} = new Inventory();

    public DummyCompany()
    {
        foreach (var good in new List<Good> {Good.CreateInstance("Lemon", new PriceBand(1, 2), RarityEnum.Common),
                                             Good.CreateInstance("Water", new PriceBand(1, 1), RarityEnum.Common),
                                             Good.CreateInstance("Sugar", new PriceBand(1, 3), RarityEnum.Common),
                                             Good.CreateInstance("Lemonade", new PriceBand(2, 4), RarityEnum.Uncommon)})
        {
            Inventory.AddGood(new InventoryEntry(good, 1000000, 1, 0));
        }
    }

    public int GetCash()
    {
        return InfiniteCash;
    }

    public List<FixedCostInstance> FixedCosts { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public iFixedCostStrategy FixedCostStrategy { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public List<Goal> Goals { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public int CurrentPeriod { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public int StartingPeriod { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public void BuyGood(Good good, int quantity, decimal price, int period)
    {
        throw new System.NotImplementedException();
    }

    public int CalculateFixedCostsForPeriod(int period)
    {
        throw new System.NotImplementedException();
    }

    public void CheckAgentGoals()
    {
        throw new System.NotImplementedException();
    }

    public void CompleteGoal(Goal goal)
    {
        throw new System.NotImplementedException();
    }

    public void ExpireGoods(int period)
    {
        throw new System.NotImplementedException();
    }

    public Inventory GetInventory()
    {
        return Inventory;
    }

    public LemonadeStandResultObject QueueOrder(ActionContext context)
    {
        throw new System.NotImplementedException();
    }

    public void SellGood(Good good, int quantity, decimal price, int period)
    {
        throw new System.NotImplementedException();
    }

    public void UpdateCurrentPeriod(int period)
    {
        throw new System.NotImplementedException();
    }

    public void SetCash(int newCash)
    {
        Debug.Log("Null cash transaction for dummy company");
    }

    public void SetStrategy(iStrategy strategy)
    {
        throw new System.NotImplementedException();
    }

    public iStrategy GetStrategy()
    {
        throw new System.NotImplementedException();
    }

    public DemandData GetDemandFor(Good good)
    {
        throw new System.NotImplementedException();
    }

    public void SetAggressionLevel(decimal aggressionLevel)
    {
        throw new System.NotImplementedException();
    }

    public decimal GetAggressionLevel()
    {
        throw new System.NotImplementedException();
    }
}
