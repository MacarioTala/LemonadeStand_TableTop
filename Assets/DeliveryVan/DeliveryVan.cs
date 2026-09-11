using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeliveryVan:MonoBehaviour
{
    IReadOnlyList<Good> GoodsMasterList;
    readonly Inventory AvailableInventory = new();
    private void Awake()
    {
        LoadGoods();
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

            return candidates[Random.Range(0,candidates.Count)];
        }
    }

    #region ETL
    private void LoadGoods()
        => GoodsMasterList=GameRoot.Instance.GetElements();
    #endregion
}