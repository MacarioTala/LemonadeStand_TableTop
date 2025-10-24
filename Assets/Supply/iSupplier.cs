using System.Collections.Generic;

public interface iSupplier
{
    public void InitializeRarityQuantities(int common, int uncommon, int rare, int veryRare);
    LemonadeStandResultObject ReplenishSupplies();
    LemonadeStandResultObject SeedSuppliedGoods(List<Good> initialGoods);
    LemonadeStandResultObject SupplyGoods();
}