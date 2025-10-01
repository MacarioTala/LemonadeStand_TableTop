using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sandbox
{
    /// <summary>
    /// A class dedicated to handling display information for the population in the simulation.
    /// This separates the display logic from the reporting logic.
    /// </summary>
    public class PopulationDisplayInfo
    {
        private readonly PopulationAgent population;
        private readonly List<Good> availableGoods;

        public PopulationDisplayInfo(PopulationAgent population, List<Good> availableGoods)
        {
            this.population = population;
            this.availableGoods = availableGoods ?? new List<Good>();
        }

        /// <summary>
        /// Gets the population count.
        /// </summary>
        public int PopulationCount => population.Population;

        /// <summary>
        /// Gets the population's ennui level.
        /// </summary>
        public float Ennui => population.Ennui;

        /// <summary>
        /// Gets the population's inventory as a dictionary of good names to quantities.
        /// </summary>
        public Dictionary<string, int> Stock
        {
            get
            {
                var stock = new Dictionary<string, int>();
                foreach (var good in availableGoods)
                {
                    var inventoryEntries = population.GetInventory().GetInventoryEntriesByGood(good.GoodName);
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
        /// Gets a formatted string representation of the population's information.
        /// </summary>
        /// <returns>Formatted string with population count, ennui, and stock information.</returns>
        public string GetFormattedInfo()
        {
            var info = $"Population: {PopulationCount}\n";
            info += $"  Ennui: {Ennui:P1}\n";
            
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
        /// Gets a simplified string representation of the population's key information.
        /// </summary>
        /// <returns>Simple string with population count, ennui, and stock count.</returns>
        public string GetSimpleInfo()
        {
            return $"Population: {PopulationCount} - Ennui: {Ennui:P1} - Stock Items: {Stock.Count}";
        }
    }
}
