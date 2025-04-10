public class FulfillmentInfo
{
    public Good Good  {get;}
    public int TotalDemand {get;}
    public int TotalSupply {get;}
    public int FilledSupply {get;}
    public int FilledDemand {get;}
    public float FulfillmentRate=> TotalDemand==0?0:100*(TotalSupply / (float)TotalDemand);

    public float SupplyShortage=> TotalDemand<FilledDemand ?0: TotalDemand-FilledDemand;
    public float SupplyExcess=> TotalSupply<FilledSupply ?0: TotalSupply-FilledSupply;
    public float DemandShortage=> TotalSupply<FilledSupply ?0: TotalSupply-FilledSupply;
    public float DemandExcess=> TotalDemand<FilledDemand ?0: TotalDemand-FilledDemand;

    public float SupplyFulfillmentRate=> TotalSupply==0?0:100*(FilledSupply / (float)TotalSupply);
    public float DemandFulfillmentRate=> TotalDemand==0?0:100*(FilledDemand / (float)TotalDemand);

    public float SupplyExcessRate=> SupplyExcess==0?0:100*(SupplyExcess / (float)TotalDemand);
    public float DemandExcessRate=> DemandExcess==0?0:100*(DemandExcess / (float)TotalSupply);

    public float SupplyUnfilledRate=> 100-SupplyFulfillmentRate;
    public float DemandUnfilledRate=> 100-DemandFulfillmentRate;

    public FulfillmentInfo(Good good, int totalDemand, int totalSupply, int filledSupply, int filledDemand)
    {
        Good = good;
        TotalDemand = totalDemand;
        TotalSupply = totalSupply;
        FilledSupply = filledSupply;
        FilledDemand = filledDemand;
    }
};
