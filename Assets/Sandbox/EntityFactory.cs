using System.Collections.Generic;
using UnityEngine;

namespace Sandbox
{
    public class EntityFactory
    {
        private Good lemon;
        private Good water;
        private Good sugar;
        private Good lemonade;
        private Recipe lemonadeRecipe;
        private readonly List<Good> testGoods = new();
        
        public void CreateGoodsAndRecipes()
        {
            // Create goods with price bands and rarity
            lemon = Good.CreateInstance("Lemon", new PriceBand(1.0m, 3.0m), RarityEnum.Common);
            water = Good.CreateInstance("Water", new PriceBand(1.0m, 1.0m), RarityEnum.Common);
            sugar = Good.CreateInstance("Sugar", new PriceBand(1.0m, 2.0m), RarityEnum.Common);
            lemonade = Good.CreateInstance("Lemonade", new PriceBand(4.0m, 5.0m), RarityEnum.Uncommon);
            
            // Add elasticity values for Lemon
            lemon.AddElasticity(ElasticityTypeEnum.SaturationElasticity, -0.3f);
            lemon.AddElasticity(ElasticityTypeEnum.PriceElasticity, -0.5f);
            lemon.AddElasticity(ElasticityTypeEnum.PopulationElasticity, 0.8f);
            
            // Add elasticity values for Water
            water.AddElasticity(ElasticityTypeEnum.SaturationElasticity, -0.1f);
            water.AddElasticity(ElasticityTypeEnum.PriceElasticity, -0.2f);
            water.AddElasticity(ElasticityTypeEnum.PopulationElasticity, 0.9f);
            
            // Add elasticity values for Sugar
            sugar.AddElasticity(ElasticityTypeEnum.SaturationElasticity, -0.2f);
            sugar.AddElasticity(ElasticityTypeEnum.PriceElasticity, -0.4f);
            sugar.AddElasticity(ElasticityTypeEnum.PopulationElasticity, 0.7f);
            
            // Add elasticity values for Lemonade
            lemonade.AddElasticity(ElasticityTypeEnum.SaturationElasticity, -0.5f);
            lemonade.AddElasticity(ElasticityTypeEnum.PriceElasticity, -0.8f);
            lemonade.AddElasticity(ElasticityTypeEnum.PopulationElasticity, 1.0f);
            
            // Add ennui-reducing effect to Lemonade
            var reduceEnnuiEffect = new GoodEffect()
                .Named("Reduce Ennui")
                .DescribedAs("Reduces ennui when consumed")
                .Affecting(MetricEnum.Ennui)
                .WithEffectMagnitude(-0.1f) // Reduce ennui by 10% per unit consumed
                .WithEffect(new MetricModifier<PopulationAgent>(
                            c => c.Ennui,
                            (c, newValue) => c.Ennui = Mathf.Clamp01(newValue)));
            lemonade.AddEffect(reduceEnnuiEffect);
            
            // Add goods to the test goods list
            testGoods.Add(lemon);
            testGoods.Add(water);
            testGoods.Add(sugar);
            testGoods.Add(lemonade);

            // Create the lemonade recipe
            var lemonIngredient = new Ingredient(lemon, 1);
            var waterIngredient = new Ingredient(water, 5);
            var sugarIngredient = new Ingredient(sugar, 2);
            lemonadeRecipe = new Recipe("Basic Lemonade", lemonade, new List<Ingredient> { lemonIngredient, waterIngredient, sugarIngredient });
            
            Debug.Log("Goods and recipes created successfully.");
        }
        
        public List<Good> GetTestGoods() => testGoods;
        public Recipe GetLemonadeRecipe() => lemonadeRecipe;
        public Good GetLemon() => lemon;
        public Good GetWater() => water;
        public Good GetSugar() => sugar;
        public Good GetLemonade() => lemonade;
    }
}
