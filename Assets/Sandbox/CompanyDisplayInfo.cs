using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sandbox
{
    /// <summary>
    /// A class dedicated to handling display information for companies in the simulation.
    /// This separates the display logic from the reporting logic.
    /// </summary>
    public class CompanyDisplayInfo
    {
        private readonly EconAgent company;
        private readonly List<Good> availableGoods;

        public CompanyDisplayInfo(EconAgent company, List<Good> availableGoods)
        {
            this.company = company;
            this.availableGoods = availableGoods ?? new List<Good>();
        }

        /// <summary>
        /// Gets the company name.
        /// </summary>
        public string Name => company.Name;

        /// <summary>
        /// Gets the company's current cash balance.
        /// </summary>
        public decimal Cash => company.GetCash();

        /// <summary>
        /// Gets the company's inventory as a dictionary of good names to quantities.
        /// </summary>
        public Dictionary<string, int> Stock
        {
            get
            {
                var stock = new Dictionary<string, int>();
                foreach (var good in availableGoods)
                {
                    var inventoryEntries = company.GetInventory().GetInventoryEntriesByGood(good.GoodName);
                    var totalQuantity = inventoryEntries.Sum(entry => entry.quantity);
                    if (totalQuantity > 0)
                    {
                        stock[good.GoodName] = totalQuantity;
                    }
                }
                return stock;
            }
        }

        /// <summary>
        /// Gets a formatted string representation of the company's information.
        /// </summary>
        /// <returns>Formatted string with company name, cash, and stock information.</returns>
        public string GetFormattedInfo()
        {
            var info = $"Company: {Name}\n";
            info += $"  Cash: ${Cash}\n";
            
            var stock = Stock;
            if (stock.Count > 0)
            {
                foreach (var item in stock)
                {
                    info += $"  {item.Key}: {item.Value} units\n";
                }
            }
            else
            {
                info += "  No inventory\n";
            }
            
            return info;
        }

        /// <summary>
        /// Gets a simplified string representation of the company's key information.
        /// </summary>
        /// <returns>Simple string with company name, cash, and stock count.</returns>
        public string GetSimpleInfo()
        {
            return $"{Name} - Cash: ${Cash} - Stock Items: {Stock.Count}";
        }
    }
}
