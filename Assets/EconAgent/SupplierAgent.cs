using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;


/// <summary>
///     SupplierAgents exist primarily to supply goods outside the production of companies
///     Use these to seed the world with initial values of goods where good.IsProducedGood == False
///     SupplierAgents can have their own goals, but will default to supplying the market as much as they can
/// </summary>
public class SupplierAgent : EconAgent, iSupplier
{
    bool hasProducedUnique = false;

    readonly Dictionary<RarityEnum, int> _maxGoodsProducedPerTurn = new();
    readonly HashSet<Good> _goodsSuppliedByThisSupplier = new();

    public HashSet<Good> GetSuppliedGoods()
    {
        return _goodsSuppliedByThisSupplier;
    }

    /// <summary>
    /// Call this method to set the number of goods this supplier can produce each turn 
    /// params are number per rarity.
    /// The actual supply/inventory manipulation is done in ReplenishSupplies
    /// </summary>
    public void InitializeRarityQuantities(int common, int uncommon, int rare, int veryRare)
    {
        _maxGoodsProducedPerTurn[RarityEnum.Common] = common;
        _maxGoodsProducedPerTurn[RarityEnum.Uncommon] = uncommon;
        _maxGoodsProducedPerTurn[RarityEnum.Rare] = rare;
        _maxGoodsProducedPerTurn[RarityEnum.Very_Rare] = veryRare;
        _maxGoodsProducedPerTurn[RarityEnum.Unique] = 1;
    }

    public LemonadeStandResultObject ReplenishSupplies()
    {
        var entries = GetInventory().GetInventoryEntries();
        foreach (var good in _goodsSuppliedByThisSupplier)
        {
            //check for over/undersupply here and adjust pricing
            var existingGood = entries.FirstOrDefault(x => x.good == good);
            var entry = new InventoryEntry(good, _maxGoodsProducedPerTurn[good.GetRarity()], good.GetPrice(), GetMarket().CurrentPeriod);
            if (good.GetRarity() == RarityEnum.Unique)
                {
                if (!hasProducedUnique)
                    {
                        hasProducedUnique = true;
                        GetInventory().AddGood(entry);    
                    }
                }
            else
            {
                if (existingGood is not null)
                    existingGood.quantity += _maxGoodsProducedPerTurn[existingGood.good.GetRarity()];
                else
                {
                    GetInventory().AddGood(entry);
                }
            }
        }

        return LemonadeStandResultObject.Success();
    }

    public LemonadeStandResultObject SupplyGoods()
    {
        var _market = GetMarket();
        foreach (var inventoryEntry in GetInventory().GetInventoryEntries())
        {
            var trade = new Order(null, this, inventoryEntry.good, inventoryEntry.quantity, inventoryEntry.good.GetPrice());
            var context = new ActionContext
            {
                TradeToSubmit = trade,
                MarketToSubmitTo = _market,
                Period = _market.CurrentPeriod
            };
            QueueOrder(context);
        }
        return LemonadeStandResultObject.Success();
    }

    public LemonadeStandResultObject SeedSuppliedGoods(List<Good> initialGoods)
    {
        foreach (Good good in initialGoods)
        {
            _goodsSuppliedByThisSupplier.Add(good);
        }
        return LemonadeStandResultObject.Success();
    }

    public static class SupplierAgentBuilder
    {
        public static EconAgentBuilder<SupplierAgent> Create()
            => EconAgentBuilder.For<SupplierAgent>();
    }
}