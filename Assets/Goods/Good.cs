using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Good", menuName = "GameObjects/Good", order = 1)]
public class Good : ScriptableObject
{
    public string GoodName;
    
    private decimal _price;
    private decimal Price{
                        get => _price;
                        set => _price = Math.Round(value,2);}
    
    //Demand
    public Dictionary<ElasticityTypeEnum, float> Elasticities = new();
    public void AddElasticity(ElasticityTypeEnum key, float value) => Elasticities.Add(key, value);
    public bool isDemandInelastic => Elasticities.Count == 0;

    private decimal price_increment_rate;
    private Price_band PriceBand;

    public int ExpiresAfterPeriods { get; set; } = int.MaxValue;
    private Rarity_enum Rarity;
    
    public bool IsProducedGood { get; set; } = false;
    public int price_increase_threshold; //Might not need this. Are there any good-specific price thresholds?
    public int price_decrease_threshold; //ibid

    [SerializeField] private readonly List<Good> _substitute_goods = new();


    public static Good CreateInstance(  string good_name, 
                                        Price_band price_band=null,
                                        Rarity_enum rarity=Rarity_enum.Common)
    {
        var good = ScriptableObject.CreateInstance<Good>();
        good.Initialize(good_name, price_band,rarity);
        return good;
    }

    private void Initialize(string good_name, 
                            Price_band price_band,
                            Rarity_enum rarity=Rarity_enum.Common) 
    {
        this.GoodName = good_name;
        PriceBand = price_band;
        Rarity = rarity;
        //Initial price will be determined based on price_band
        Price = Generate_initial_price();
        Set_initial_price_thresholds();
        Set_initial_price_increment_rate();
    }

    public Price_band Get_price_band() => PriceBand;
    public decimal GetPrice() => Price;
    
    internal void Set_price(decimal new_price) => Price = new_price;

    public Rarity_enum GetRarity() => Rarity;
    public decimal Get_price_increment_rate() => price_increment_rate;

    private decimal Generate_initial_price() 
    {
        var randomFloat = UnityEngine.Random.value;
        var price_range = PriceBand.max - PriceBand.min;
        return PriceBand.min + (decimal)randomFloat * price_range;
    }

    public void Add_substitute_good(Good good) => _substitute_goods.Add(good);

    private void Set_price_thresholds(int increase_threshold, int decrease_threshold){
        price_increase_threshold = increase_threshold;
        price_decrease_threshold = decrease_threshold;
    }
    private void Set_initial_price_thresholds()
    {
        const int common_increase_threshold = 500;
        const int common_decrease_threshold = 100;
        const int uncommon_increase_threshold = 250;
        const int uncommon_decrease_threshold = 50;
        const int rare_increase_threshold = 50;
        const int rare_decrease_threshold = 10;
        const int very_rare_increase_threshold = 5;
        const int very_rare_decrease_threshold = 1;
        if(Rarity==Rarity_enum.Common){
            Set_price_thresholds(common_increase_threshold, common_decrease_threshold);
        }
        if(Rarity==Rarity_enum.Uncommon){
            Set_price_thresholds(uncommon_increase_threshold, uncommon_decrease_threshold);
        }
        if(Rarity==Rarity_enum.Rare){
            Set_price_thresholds(rare_increase_threshold, rare_decrease_threshold);
        }
        if(Rarity==Rarity_enum.Very_Rare){
            Set_price_thresholds(very_rare_increase_threshold, very_rare_decrease_threshold);
        }
    }

    private void Set_initial_price_increment_rate()
    {
        const decimal common_price_increment_rate = 0.1m;
        const decimal uncommon_price_increment_rate = 0.15m;
        const decimal rare_price_increment_rate = 0.3m;
        const decimal very_rare_price_increment_rate = 0.4m;
        if(Rarity==Rarity_enum.Common){
            price_increment_rate = common_price_increment_rate;
        }
        if(Rarity==Rarity_enum.Uncommon){
            price_increment_rate = uncommon_price_increment_rate;
        }
        if(Rarity==Rarity_enum.Rare){
            price_increment_rate = rare_price_increment_rate;
        }
        if(Rarity==Rarity_enum.Very_Rare){
            price_increment_rate = very_rare_price_increment_rate;
        }
    }

    public override bool Equals(object obj)
    {
        if(obj is Good other)
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
}

public enum Rarity_enum
{
    Common,
    Uncommon,
    Rare,
    Very_Rare,
    Unique
}

[System.Serializable]
public class Price_band{
    public readonly decimal min;
    public readonly decimal max;

    public Price_band(decimal lower_bound=0, decimal upper_bound=0){
        min = lower_bound;
        max = upper_bound;
    }
}

