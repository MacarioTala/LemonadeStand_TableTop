using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sandbox
{
    public class SimulationReporter
    {
        private List<Good> testGoods;
        private List<EconAgent> companies;
        private Market lemonadeMarket;
        
        public SimulationReporter(List<Good> goods, List<EconAgent> companiesList, Market market)
        {
            testGoods = goods;
            companies = companiesList;
            lemonadeMarket = market;
        }
        
        public void GenerateSummary(int totalNumberOfCycles, int tradesPerCycle)
        {
            Debug.Log("=== SIMULATION SUMMARY ===");
            Debug.Log($"Total Cycles: {totalNumberOfCycles}");
            Debug.Log($"Trades per Cycle: {tradesPerCycle}");
            Debug.Log($"Number of Companies: {companies.Count}");
            
            // Log final market state
            Debug.Log("=== MARKET SUMMARY ===");
            foreach (var good in testGoods)
            {
                var inventoryEntry = lemonadeMarket.GetInventory().GetInventoryEntriesByGood(good.GoodName);
                var totalQuantity = inventoryEntry.Sum(entry => entry.quantity);
                Debug.Log($"{good.GoodName}: {totalQuantity} units in market inventory");
            }
            
            // Log company summaries using the new CompanyDisplayInfo class
            Debug.Log("=== COMPANY SUMMARIES ===");
            foreach (var company in companies)
            {
                var companyDisplayInfo = new CompanyDisplayInfo(company, testGoods);
                Debug.Log(companyDisplayInfo.GetFormattedInfo());
            }
        }
    }
}
