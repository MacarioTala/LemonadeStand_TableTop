using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Good", menuName = "LemonadeStandAssets/Good", order = 1)]
public class Good : ScriptableObject
{
    public string GoodName;
    public Sprite GoodSprite;
    public string Tooltip;
    private int _price;
    private int _minAskPrice;
    public long ExpiresAfterPeriods;
    public bool IsPerishable=true;
    public int DeliveryDelay;

    [SerializeField]private PriceBand PriceBand;

    public decimal GetCostOfGood(Recipe recipe)
    {
        if (!IsProducedGood) return Price;
        if (_minAskPrice == 0)
        {
            // If no minimum ask price is set, calculate it based on the recipe's ingredients
            _minAskPrice = recipe.GetIngredients()
                .Sum(ingredient => ingredient.Good.GetPrice() * ingredient.QuantityNeeded);
            return _minAskPrice;
        }
        return _minAskPrice;
    }
    private int Price
    {
        get => _price;
        set => _price = value;
    }

    //Demand
    public Dictionary<ElasticityTypeEnum, float> Elasticities = new();
    public void AddElasticity(ElasticityTypeEnum key, float value) => Elasticities.Add(key, value);
    public bool isDemandInelastic
    {
        get {
            return Elasticities.Count == 0
            ||
            Elasticities.Count(x=>x.Value!=0)==0
            ;
        }
    }


    public void SetExpiry(int periods)
        => ExpiresAfterPeriods = periods;
    [SerializeField]private RarityEnum _rarity;

    public bool IsProducedGood;

    [SerializeField] private readonly List<Good> _substituteGoods = new();
    #region Effects
    List<GoodEffect> _effects = new();

    public List<MetricEnum> AffectsMetrics()
    {
        return (from metric in _effects
                select metric.AffectsMetric).ToList();
    }

    public ReductionResult ReducesMetric(MetricEnum metric)
    {
        var isReducing = _effects.Any(effect => effect.AffectsMetric == metric && effect.IsReduce);
        var reductionAmount = _effects
            .Where(effect => effect.AffectsMetric == metric && effect.IsReduce)
            .FirstOrDefault()
            ?.Magnitude ?? 0f;
        return new ReductionResult(isReducing, reductionAmount);
    }

    public bool HasEffectOn(MetricEnum metric)
    {
        return _effects.Any(effect => effect.AffectsMetric == metric);
    }

    public void AddEffect(GoodEffect effect)
    {
        _effects.Add(effect);
    }
    public void RemoveEffect(GoodEffect effect)
    {
        _effects.Remove(effect);
    }
    public void ApplyEffects(PopulationAgent company, float percentageOfEffectToApply=1f)
    {
        foreach (var effect in _effects)
        {
            effect.Apply(company,percentageOfEffectToApply);
        }
    }
    #endregion

    public static Good CreateInstance(string good_name,
                                        PriceBand price_band = null,
                                        RarityEnum rarity = RarityEnum.Common)
    {
        var good = CreateInstance<Good>();
        good.Initialize(good_name, price_band, rarity);
        return good;
    }

    private void Initialize(string good_name,
                            PriceBand price_band,
                            RarityEnum rarity = RarityEnum.Common)
    {
        this.GoodName = good_name;
        PriceBand = price_band;
        _rarity = rarity;
        //Initial price will be determined based on price_band
        Price = Generate_initial_price();
    }

    public PriceBand GetPriceBand() => PriceBand;
    public void SetPriceBand(int minPrice,int maxPrice) => (PriceBand.Min,PriceBand.Max)=(minPrice,maxPrice);
    public int GetPrice() => Price;

    internal void SetPrice(int new_price) => Price = new_price;

    public RarityEnum GetRarity() => _rarity;
    public void SetRarity(RarityEnum rarity) => _rarity = rarity;

    private int Generate_initial_price()
    {
        var randomFloat = UnityEngine.Random.value;
        var price_range = PriceBand.Max - PriceBand.Min;
        return PriceBand.Min + (int)randomFloat * price_range;
    }

    public void AddSubstituteGood(Good good) => _substituteGoods.Add(good);

    
    #region Overrides
    public override bool Equals(object obj)
    {
        if (obj is Good other)
        {
            return GoodName == other.GoodName;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return GoodName?.GetHashCode() ?? 0;
    }

    public override string ToString()
    {
        return GoodName;
    }
    #endregion
}

public enum RarityEnum
{
    Common,
    Uncommon,
    Rare,
    VeryRare,
    Unique,
    Produced
}

[Serializable]
public class PriceBand{
    public int Min;
    public int Max;

    public PriceBand(int min, int max)=> (Min,Max) = (min,max);
}

public class ReductionResult
{
    public readonly bool IsReducing;
    public readonly float ReductionAmount;
    public ReductionResult(bool isReducing, float reductionAmount)
    {
        IsReducing = isReducing;
        ReductionAmount = reductionAmount;
    }

    public float By()=> IsReducing ? ReductionAmount : 0f;
}
