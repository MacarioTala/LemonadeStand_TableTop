public class FulfillmentInfo
{
    public Good Good  {get;}
    public int Demand {get;}
    public int Supply {get;}
    public float FulfillmentRate=> Demand==0?0:100*(Supply / (float)Demand);
    public int SupplyShortage => Supply >= Demand?0 : Demand - Supply;
    public int SupplyExcess => Demand >= Supply?0 : Supply - Demand;
    public float SupplyShortageRate => 100*(SupplyShortage / (float)Demand);
    public float SupplyExcessRate => 100*(SupplyExcess / (float)Demand);

    public FulfillmentInfo(Good good, int demand, int supply)
    {
        Good = good;
        Demand = demand;
        Supply = supply;
    }
};
