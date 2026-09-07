using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeliveryVan:MonoBehaviour
{
    List<Good> GoodsMasterList=new();
    readonly Inventory AvailableInventory = new();
    private void Awake()
    {
        LoadGoods();
        LoadElementCharacteristicsFromResources();
    }

    public List<InventoryEntry> GetCurrentDelivery(int period)
    {
        RollForAvailableInventory(period);
        return AvailableInventory.GetInventoryEntries();
    }

    private void RollForAvailableInventory(int period)
    {
        AvailableInventory.Clear();
        List<string> alreadyRolled=new();

        for (var i=0;i<3;i++)
        {
            var selectedCandidate = RollCandidate(alreadyRolled);
            var numAppearing = MathHelper.GetNumAppearingFromRarity(selectedCandidate.GetRarity());

            var candidatePriceBand = selectedCandidate.GetPriceBand();
            var candidatePrice = UnityEngine.Random.Range(candidatePriceBand.Min,candidatePriceBand.Max+1);
            var inventoryEntry = new InventoryEntry(selectedCandidate,numAppearing,candidatePrice,period);

            AvailableInventory.AddInventoryEntry(inventoryEntry);
            alreadyRolled.Add(inventoryEntry.good.GoodName);
        }
    }

    private Good RollCandidate(List<string> alreadyRolled)
    {
        while(true)
        {
            var roll = MathHelper.RollD(1,100);
            var rolledRarity = MathHelper.GetRarityFromPercentage(roll);

            var candidates = GoodsMasterList
                        .Where(x=> x.GetRarity()==rolledRarity)
                        .Where(x=> !alreadyRolled.Contains(x.GoodName))
                        .ToList();
            
            if(candidates.Count==0) continue;

            return candidates[UnityEngine.Random.Range(0,candidates.Count)];
        }
    }

    #region load stuff from resources
    private void LoadGoods()
    {
        GoodsMasterList= Resources.LoadAll<Good>("Goods").Where(x=>x.IsProducedGood==false).ToList();
    }

    private void LoadElementCharacteristicsFromResources()
    {
        const string elementsPath = "ElementaryGoodsList";
        var elementsCSV = Resources.Load<TextAsset>(elementsPath);
        var lines = elementsCSV.text.Split('\n');

        foreach(var line in lines.Skip(1))
        {
            var columns = line.Split(",");
            var goodName = columns[0].Trim();

            var currentGood = GoodsMasterList.FirstOrDefault(x=>x.GoodName == goodName);

            //Set Good characteristics here
            Enum.TryParse<RarityEnum>(columns[2].Trim(),out var rarity);
            int.TryParse(columns[3].Trim(),out var minPrice);
            int.TryParse(columns[4].Trim(),out var maxPrice);

            currentGood.SetRarity(rarity);
            currentGood.SetPriceBand(minPrice,maxPrice);
        }
    }
    #endregion
}