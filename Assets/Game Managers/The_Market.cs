using UnityEngine;
using System.Collections.Generic;

public class The_Market : MonoBehaviour
{
    public List<Good> goods = new();
    private float _market_update_rate = 1f;
    private float _time_since_last_market_update = 0;

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
}