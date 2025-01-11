using System;

public class PopulationHistory : iHistorical
{
    public Guid MarketId;
    public int Period { get; set; }
    public int Population;

}