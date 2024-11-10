using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Good", menuName = "GameObjects/Good", order = 1)]
public class Good : ScriptableObject
{
    private float _price;
    private float Price{
                        get => _price;
                        set => _price = Mathf.Round(value * 100f) / 100f;}
    public string good_name;
    private float price_increment_rate;
    private Price_band price_band;

    private Rarity_enum rarity;
    
    public int price_increase_threshold; //Might not need this. Are there any good-specific price thresholds?
    public int price_decrease_threshold; //ibid

    [SerializeField] private List<Good> _substitute_goods = new();


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
        this.good_name = good_name;
        this.price_band = price_band;
        this.rarity = rarity;
        //Initial price will be determined based on price_band
        Price = Generate_initial_price();
        Set_initial_price_thresholds();
        Set_initial_price_increment_rate();
    }

    public Price_band Get_price_band()
    {
        return price_band;
    }

    public float Get_price()
    {
        return Price;
    }

    public Rarity_enum Get_rarity()
    {
        return rarity;
    }
    public float Get_price_increment_rate()
    {
        return price_increment_rate;
    }

    private float Generate_initial_price()
    {
        return Random.Range(price_band.min, price_band.max);
    }

    public void Add_substitute_good(Good good){
        _substitute_goods.Add(good);
    }

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
        if(rarity==Rarity_enum.Common){
            Set_price_thresholds(common_increase_threshold, common_decrease_threshold);
        }
        if(rarity==Rarity_enum.Uncommon){
            Set_price_thresholds(uncommon_increase_threshold, uncommon_decrease_threshold);
        }
        if(rarity==Rarity_enum.Rare){
            Set_price_thresholds(rare_increase_threshold, rare_decrease_threshold);
        }
        if(rarity==Rarity_enum.Very_Rare){
            Set_price_thresholds(very_rare_increase_threshold, very_rare_decrease_threshold);
        }
    }

    private void Set_initial_price_increment_rate()
    {
        const float common_price_increment_rate = 0.1f;
        const float uncommon_price_increment_rate = 0.15f;
        const float rare_price_increment_rate = 0.3f;
        const float very_rare_price_increment_rate = 0.4f;
        if(rarity==Rarity_enum.Common){
            price_increment_rate = common_price_increment_rate;
        }
        if(rarity==Rarity_enum.Uncommon){
            price_increment_rate = uncommon_price_increment_rate;
        }
        if(rarity==Rarity_enum.Rare){
            price_increment_rate = rare_price_increment_rate;
        }
        if(rarity==Rarity_enum.Very_Rare){
            price_increment_rate = very_rare_price_increment_rate;
        }
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
    public readonly float min;
    public readonly float max;

    public Price_band(float lower_bound=0, float upper_bound=0){
        min = lower_bound;
        max = upper_bound;
    }
}

