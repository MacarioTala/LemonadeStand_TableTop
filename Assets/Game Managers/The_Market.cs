using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class The_Market : MonoBehaviour
{
    public List<Good> goods = new();

    public List<InventoryEntry> goods_in_market = new();
    public List<Company> companies = new();
    private float _market_update_rate = 1f;
    private float _time_since_last_market_update = 0;

    public void Register_Company(Company company)
    {
        if(!companies.Any(x=>x.company_name == company.company_name))
        {
            companies.Add(company);
        }
        else
        {
            throw new CompanyException("Company already registered");
        }
        
    }
    private void Update()
    {
        _time_since_last_market_update += Time.deltaTime;

        if(_time_since_last_market_update >= _market_update_rate)
        {
            _time_since_last_market_update = 0;
            foreach(Good good in goods)
            {
                good.Update_price_based_on_demand();
            }
            _time_since_last_market_update = 0;
        }
    }

    public void Create_initial_goods(List<Good> goods)//move static data to DB in future
    {
        //generate random numbers for price bands
        var band1 = new Price_band(1f,5f);
        var band2 = new Price_band(6f,10f);
        var band3 = new Price_band(11f, 20f);

        //Limits for good quantities
        var common_range = UnityEngine.Random.Range(1, 1000);
        var uncommon_range = UnityEngine.Random.Range(1, 500);
        var rare_range = UnityEngine.Random.Range(1, 100);
        var very_rare_range = UnityEngine.Random.Range(1, 10);

        //create goods
        foreach(Good good in goods)
        {
            //Generate quantity based on rarity
            if(good.Get_rarity() == Rarity_enum.Common)
            {
                goods_in_market.Add(new InventoryEntry(good, common_range,good.Get_price()));
            }
            else if(good.Get_rarity() == Rarity_enum.Uncommon)
            {
                goods_in_market.Add(new InventoryEntry(good, uncommon_range,good.Get_price()));
            }
            else if(good.Get_rarity() == Rarity_enum.Rare)
            {
                goods_in_market.Add(new InventoryEntry(good, rare_range,good.Get_price()));
            }
            else if(good.Get_rarity() == Rarity_enum.Very_Rare)
            {
                goods_in_market.Add(new InventoryEntry(good, very_rare_range,good.Get_price()));
        }
        //in the future, have a concept of rarity driving the initial price
    }
}
}
[Serializable]
public class CompanyException : Exception
{
    public CompanyException(string message) : base(message)
    {
    }

}