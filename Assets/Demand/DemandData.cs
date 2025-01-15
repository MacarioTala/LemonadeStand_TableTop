public class DemandData
{
    public decimal Ask {get; set;}
    public int CurrentDemand{get; set;}
    public float FulfilmentRate{get; set;}
    public int MinDemand{get; set;}
    public int MaxDemand{get; set;}

    //Demand Curve
    public float Curvature{get; set;}
    public float Steepness{get; set;}
    public float Shift{get; set;}
}


