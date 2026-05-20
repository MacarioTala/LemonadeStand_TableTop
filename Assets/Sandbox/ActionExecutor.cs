using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sandbox
{
    public class ActionExecutor
    {
        private List<Good> testGoods;
        private Recipe lemonadeRecipe;
        private List<EconAgent> companies;
        private Market lemonadeMarket;
        
        public ActionExecutor(List<Good> goods, Recipe recipe, List<EconAgent> companiesList, Market market)
        {
            testGoods = goods;
            lemonadeRecipe = recipe;
            companies = companiesList;
            lemonadeMarket = market;
        }
        
        public void PerformRandomAction(int cycle)
        {
            // First, check if any companies have strategies and let them act
            var strategicCompanies = companies.Where(c => c.GetStrategy() != null).ToList();
            
            if (strategicCompanies.Count > 0)
            {
                // Let strategic companies act first
                foreach (var company in strategicCompanies)
                {
                    try
                    {
                        company.PerformStrategy();
                    }
                    catch (System.Exception e)
                    {
                        Debug.Log($"Error executing strategy for {company.Name}: {e.Message}");
                    }
                }
            }
            else
            {
                // Fall back to random actions if no companies have strategies
                var randomAction = Random.Range(0, 2);
                if (randomAction == 0)
                    PerformRandomTrade(cycle);
                else
                    PerformRandomProduction(cycle);
            }
        }
        
        private void PerformRandomTrade(int cycle)
        {
            var potentialSellers = new List<iEconAgent>(companies) { lemonadeMarket };
            var potentialBuyers = new List<iEconAgent>(companies) { lemonadeMarket };

            var buyer = potentialBuyers[Random.Range(0, potentialBuyers.Count)];
            var seller = potentialSellers[Random.Range(0, potentialSellers.Count)];

            if (buyer == seller) return;

            var goodToBuy = SelectRandomGood();
            if (goodToBuy == null) return;

            var doesSellerHaveRandomGood = seller.GetInventory().GetInventoryEntries().Exists(entry => entry.good == goodToBuy);
            if (!doesSellerHaveRandomGood) return;

            var quantity = Random.Range(1, 10);
            var price = goodToBuy.GetPrice();
            
            // Create orders with proper submitting companies
            Order trade;
            if (buyer.Equals(lemonadeMarket))
            {
                // Market is buying (this shouldn't happen in our current setup)
                trade = new Order(buyer, seller, goodToBuy, quantity, price);
                trade.SubmittingCompany = seller;
            }
            else
            {
                // Company is buying from market or another company
                trade = new Order(buyer, seller, goodToBuy, quantity, price);
                trade.SubmittingCompany = buyer;
            }

            var context = new ActionContext { 
                TradeToSubmit = trade, 
                MarketToSubmitTo = lemonadeMarket, 
                Period = cycle,
                SubmittingCompany = trade.SubmittingCompany
            };
            lemonadeMarket.QueueOrder(context);
            Debug.Log("Trade queued in cycle: " + cycle + " " + trade);
        }
        
        private void PerformRandomProduction(int cycle)
        {
            // Select a random company to perform production
            if (companies.Count == 0) return;
            
            var company = companies[Random.Range(0, companies.Count)];
            
            // Check if the company has the lemonade recipe
            if (!company.Recipes.Contains(lemonadeRecipe))
            {
                company.AddRecipe(lemonadeRecipe);
            }
            
            // Check if the company has enough ingredients to make lemonade
            var inventory = company.GetInventory().GetInventoryEntries();
            var maxQuantity = lemonadeRecipe.GetMaxQuantityFromInventory(inventory);
            
            if (maxQuantity > 0)
            {
                // Produce a random amount of lemonade (up to the maximum possible)
                var quantityToProduce = Random.Range(1, Mathf.Min(maxQuantity, 10));
                
                try
                {
                    var context = new ActionContext 
                    { 
                        Recipe = lemonadeRecipe, 
                        QuantityToMake = quantityToProduce, 
                        RecipeMaker = company,
                        Period = cycle
                    };
                    company.MakeRecipe(context);
                    Debug.Log($"{company.Name} produced {quantityToProduce} units of lemonade in cycle: {cycle}");
                }
                catch (RecipeException e)
                {
                    Debug.Log($"Production failed for {company.Name} in cycle: {cycle} - {e.Message}");
                }
            }
            else
            {
                Debug.Log($"{company.Name} has insufficient ingredients to produce lemonade in cycle: {cycle}");
            }
        }
        
        private Good SelectRandomGood()
        {
            return testGoods[Random.Range(0, testGoods.Count)];
        }
    }
}
