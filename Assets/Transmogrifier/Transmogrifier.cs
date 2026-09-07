using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Transmogrifier:MonoBehaviour
{
    readonly Dictionary<string,Combination> combinations = new ();

    private void Awake()
    {
        LoadGoodCombinationsFromResources();
    }

    public string Make(bool hasRecipe,List<string> ingredients)
    {
        if(ingredients.Count<2) return string.Empty;

        var key = MakeKey(ingredients[0],ingredients[1]);

        if(!combinations.TryGetValue(key,out var combination))
            return string.Empty;

        if(hasRecipe)
            return combination.Product;
        
        return MathHelper.RollD(1,100)<combination.PercentageJunk
            ? combination.Junk
            : combination.Product;
    }

    private void LoadGoodCombinationsFromResources()
    {
        var assetPath = "GoodCombinations";
        var csv = Resources.Load<TextAsset>(assetPath);

        var lines = csv.text.Split("\n");
        foreach (var line in lines.Skip(1))
        {
            if(string.IsNullOrWhiteSpace(line)) continue;

            var columns = line.Trim().Split(",");

            var ingredient1 = columns[0].Trim();
            var ingredient2 = columns[1].Trim();
            var product = columns[2].Trim();
            var junk = columns[3].Trim();
            var percentageJunk = columns[4].Trim();

            if(string.IsNullOrWhiteSpace(product)) continue;

            var key = MakeKey(ingredient1,ingredient2);
            combinations.Add(key,new Combination()
                    {
                        Ingredient1=ingredient1,
                        Ingredient2=ingredient2,
                        Product=product,
                        Junk=junk,
                        PercentageJunk=int.TryParse(percentageJunk,out var percent)?percent:0
                        });
        }
    }

    private string MakeKey(string a, string b)
        => string.CompareOrdinal(a,b) < 0? $"{a}|{b}":$"{b}|{a}";
}

internal class Combination
{
    public string Ingredient1;
    public string Ingredient2;
    public string Product;
    public string Junk;
    public int PercentageJunk;

}