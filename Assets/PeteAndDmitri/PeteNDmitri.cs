using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PeteNDmitri:MonoBehaviour
{
    readonly Inventory TotalInventory = new();
    readonly Inventory SuitCaseInventory = new();
    private Country country;
    private Good ether;
    [SerializeField] int gold;
    [SerializeField] int percentRare;
    [SerializeField] int percentUncommon;
    [SerializeField] private Suitcase suitcase;

    private void Awake()
    {
        Debug.Assert(percentRare+percentUncommon<=100,"Pete and Dmitri inventory preferences exceed 100%");

        country=GameRoot.Instance.Country;
        ether=Resources.Load<Good>("Goods/Ether");
    }

    public void GetStockThisTurn()
    {
        var stockpileToConsider = country.GetNeighbourhoods()
                                         .SelectMany(x=>x.StockPile
                                                .Select(kvp => new NeighbourhoodStockpile()
                                                {
                                                    Neighbourhood = x,
                                                    Good = kvp.Key,
                                                    InventoryEntry = kvp.Value
                                                }
                                                )
                                         );
        var temp = stockpileToConsider.ToList();

        var stockpileWithEther= stockpileToConsider.FirstOrDefault(x=>x.Good == ether);
        if(stockpileWithEther != null)
            BuyFromNeighbourhood(stockpileWithEther.Neighbourhood,ether,1);
            
        //Create post-ether budget
        //Currently, only ether is very rare, so there's no separate budget for very rare
        int rareBudget= (int)Math.Round((decimal)gold*percentRare/100);
        int uncommonBudget = (int)Math.Round((decimal)gold*percentUncommon/100);
        int commonBudget = gold - (rareBudget+uncommonBudget);
        
        //Spend on rare
        var rareStockPile = stockpileToConsider.Where(x=>x.Good.GetRarity()==RarityEnum.Rare);
        BuyInventory(rareStockPile,rareBudget);

        //Spend on uncommon
        var uncommonStockPile = stockpileToConsider.Where(x=>x.Good.GetRarity()==RarityEnum.Uncommon);
        BuyInventory(uncommonStockPile,uncommonBudget);

        //Spend rest on common
        var commonStockPile = stockpileToConsider.Where(x=>x.Good.GetRarity()==RarityEnum.Common);
        BuyInventory(commonStockPile,commonBudget);
    }

    private void BuyInventory(IEnumerable<NeighbourhoodStockpile> stockpiles, int budget)
    {
        while(true)
        {
            var cheapestSources = stockpiles
                                    .Where(x=>x.Neighbourhood.StockPile.ContainsKey(x.Good))
                                    .Where(x=>x.InventoryEntry.quantity>0)
                                    .GroupBy(x=>x.Good)
                                    .Select(
                                        group=>group
                                        .OrderBy(x=>x.InventoryEntry.PriceOfGood)
                                        .First()
                                    ).ToList();

            if(!cheapestSources.Any()) return;//neighbourhoods have no stockpiles

            var cheapestGoodPrice = cheapestSources.Min(x=>x.InventoryEntry.PriceOfGood);

            if(budget< cheapestGoodPrice) return;

            //Buy till budget is exhausted or goods are gone
            foreach(var source in cheapestSources)
            {
                if(!source.Neighbourhood.StockPile.TryGetValue(source.Good, out var stock)) continue;

                var pricePerUnit = stock.PriceOfGood;

                if(
                    budget>=pricePerUnit 
                    && 
                    BuyFromNeighbourhood(source.Neighbourhood,source.Good,1)
                    )
                    budget-= pricePerUnit;
            }
        }
    }

    private bool BuyFromNeighbourhood(Neighbourhood neighbourhood,Good good,int qty)
    {
        if(!neighbourhood.StockPile.TryGetValue(good,out var stock)) return false;
        
        var price = stock.PriceOfGood;
        var totalPrice = price*qty;

        if(totalPrice>gold) return false;
        
        if(neighbourhood.SellGood(good,qty)==LemonadeStandResultObject.Success())
        {
            gold-=totalPrice;
            var entry = new InventoryEntry(good,qty,price,TheEconomy.Instance.TradingPeriod);
            TotalInventory.AddInventoryEntry(entry);
            return true;
        }
        return false;
    }

    public List<InventoryEntry> GetCurrentDelivery(int period)
    {
        SuitCaseInventory.Clear();
        List<string> alreadyRolled=new();

        var numCandidates = Math.Min(TotalInventory.GetInventoryEntries().Select(x=>x.good).Distinct().Count(),3);

        for (var i=0;i<numCandidates;i++)
        {
            var selectedCandidate = RollCandidate(alreadyRolled);

            var quantityToOffer = Math.Max((int)Math.Round((decimal)selectedCandidate.quantity/4),1);//TODO: Remove hardcode when better algo

            var margin = Math.Max(selectedCandidate.PriceOfGood*.1,1);
            var candidatePrice = (int)Math.Round(selectedCandidate.PriceOfGood+margin);
            var inventoryEntry = new InventoryEntry(selectedCandidate.good,quantityToOffer,candidatePrice,period);

            SuitCaseInventory.AddInventoryEntry(inventoryEntry);
            alreadyRolled.Add(inventoryEntry.good.GoodName);
        }
        return SuitCaseInventory.GetInventoryEntries();
    }

    private InventoryEntry RollCandidate(List<string> alreadyRolled)
    {
        while(true)
        {
            var roll = MathHelper.RollD(1,100);
            var rolledRarity = MathHelper.GetRarityFromPercentage(roll);

            var candidates = TotalInventory.GetInventoryEntries()
                        .Where(x=> x.good.GetRarity()==rolledRarity)
                        .Where(x=> !alreadyRolled.Contains(x.good.GoodName))
                        .ToList();
            
            if(candidates.Count==0) continue;

            return candidates[UnityEngine.Random.Range(0,candidates.Count)];
        }
    }

    internal void SellGood(InventoryEntry entry)
    {
        var totalPrice = entry.quantity*entry.PriceOfGood;
        gold+=totalPrice;
        TotalInventory.TryConsumeGood(entry.good.GoodName,entry.quantity);
    }

    #region Domains
    #endregion
    #region Private objects
    private class NeighbourhoodStockpile{
        public Neighbourhood Neighbourhood;
        public Good Good;
        public InventoryEntry InventoryEntry;
    }
    #endregion
}