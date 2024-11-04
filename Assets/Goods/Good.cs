using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Good", menuName = "GameObjects/Good", order = 1)]
public class Good : ScriptableObject
{
    public float price;
    public string good_name;
    public float price_increment_rate;
    private int units_sold_in_period;//how many units of this good were sold in the last period
    private int price_increase_threshold; //if units_sold_in_period >= price_increase_threshold then increase price 
    private int price_decrease_threshold; //if units_sold_in_period <= price_decrease_threshold then decrease price

    public List<float> historical_prices = new();

    [SerializeField] private List<Good> _substitute_goods = new();


    public static Good CreateInstance(string good_name, float price, float price_increment_rate, int price_increase_threshold, int price_decrease_threshold)
    {
        var good = ScriptableObject.CreateInstance<Good>();
        good.Initialize(good_name, price, price_increment_rate, price_increase_threshold, price_decrease_threshold);
        return good;
    }

    private void Initialize(string good_name, float price, float price_increment_rate, int price_increase_threshold, int price_decrease_threshold) 
    {
        this.good_name = good_name;
        this.price = price;
        this.price_increment_rate = price_increment_rate;
        this.price_increase_threshold = price_increase_threshold;
        this.price_decrease_threshold = price_decrease_threshold;
    }
    public void Sell(int units_sold)
    {
        units_sold_in_period += units_sold;
    }

    public void Update_price_based_on_demand(){
        if(units_sold_in_period >= price_increase_threshold){
            price += price_increment_rate;
        }
        else if(units_sold_in_period <= price_decrease_threshold){
            price -= price_increment_rate;
        }

        historical_prices.Add(price);
        units_sold_in_period = 0;
    }

    public void Add_substitute_good(Good good){
        _substitute_goods.Add(good);
    }

}
