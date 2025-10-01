using System.Collections.Generic;
using UnityEngine;

namespace Sandbox
{
    public class MarketInitializer
    {
        private Market lemonadeMarket;
        private List<Good> testGoods;
        private TheEconomy lemonadeEconomy;
        
        public MarketInitializer(List<Good> goods, TheEconomy economy)
        {
            testGoods = goods;
            lemonadeEconomy = economy;
        }
        
        public Market InitializeMarket()
        {
            // Get a reference to the initial market
            lemonadeMarket = (Market)lemonadeEconomy.GetGlobalMarket();
           
            // Initialize the market with goods and demand
            foreach (var good in testGoods)
            {
                lemonadeMarket.InitializeDemandForSpecificGood(good, Random.Range(100, 1000));
                lemonadeMarket.GetInventory().AddGood(new InventoryEntry(good, 10000, 0.5m, 0));
            }
            
            Debug.Log("Market initialized successfully.");
            return lemonadeMarket;
        }
        
        public Market GetMarket() => lemonadeMarket;
    }
}
