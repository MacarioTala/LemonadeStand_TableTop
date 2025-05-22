using UnityEngine;

public class GoodBuilder
{
    private readonly Good goodToReturn;

    public GoodBuilder() => goodToReturn=ScriptableObject.CreateInstance<Good>();
    public GoodBuilder Named(string name)
    {
        goodToReturn.GoodName = name;
        return this;
    }
    public GoodBuilder WithRarity(RarityEnum rarity)
    {
        goodToReturn.SetRarity(rarity);
        return this;
    }

    public GoodBuilder Costing(decimal cost)
    {
        goodToReturn.SetPrice(cost);
        return this;
    }
    public GoodBuilder WhichIsProducedGood()
    {
        goodToReturn.IsProducedGood = true;
        return this;
    }

    public Good Build()
    {
        return goodToReturn;
    }
}