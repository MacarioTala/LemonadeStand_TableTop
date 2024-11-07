using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Good", menuName = "GameObjects/Good", order = 1)]
public class Good : ScriptableObject
{
    private float Price;
    public string good_name;
    public float price_increment_rate;
    private Price_band price_band;
    private int units_sold_in_period;//how many units of this good were sold in the last period
    public int price_increase_threshold; //if units_sold_in_period >= price_increase_threshold then increase price 
    public int price_decrease_threshold; //if units_sold_in_period <= price_decrease_threshold then decrease price

    public List<float> historical_prices = new();

    [SerializeField] private List<Good> _substitute_goods = new();


    public static Good CreateInstance(string good_name, float price_increment_rate, int price_increase_threshold, int price_decrease_threshold, Price_band price_band=null)
    {
        var good = ScriptableObject.CreateInstance<Good>();
        good.Initialize(good_name,  price_increment_rate, price_increase_threshold, price_decrease_threshold, price_band);
        return good;
    }

    private void Initialize(string good_name, float price_increment_rate, int price_increase_threshold, int price_decrease_threshold, Price_band price_band) 
    {
        this.good_name = good_name;
        this.price_increment_rate = price_increment_rate;
        this.price_increase_threshold = price_increase_threshold;
        this.price_decrease_threshold = price_decrease_threshold;
        this.price_band = price_band;
        //Initial price will be determined based on price_band
        Price = Generate_initial_price();
    }

    public Price_band Get_price_band()
    {
        return price_band;
    }

    public float Get_price()
    {
        return Price;
    }
    public void Sell(int units_sold)
    {
        units_sold_in_period += units_sold;
    }

    private float Generate_initial_price()
    {
        return Random.Range(price_band.min, price_band.max);
    }

    public void Update_price_based_on_demand(){
        if(units_sold_in_period >= price_increase_threshold){
            Price += price_increment_rate;
        }
        else if(units_sold_in_period <= price_decrease_threshold){
            Price -= price_increment_rate;
        }

        historical_prices.Add(Price);
        units_sold_in_period = 0;
    }

    public void Add_substitute_good(Good good){
        _substitute_goods.Add(good);
    }

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

