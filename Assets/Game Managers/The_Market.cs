using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Runtime.Serialization;

public class The_Market : MonoBehaviour
{
    public List<Good> goods = new();
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

    private void Create_initial_goods()
    {
        //generate random numbers for price bands
        var band1 = new Price_band(1f,5f);
        var band2 = new Price_band(6f,10f);
        var band3 = new Price_band(11f, 20f);

        //create goods
        
        
    }
}

[Serializable]
public class CompanyException : Exception
{
    public CompanyException(string message) : base(message)
    {
    }

}