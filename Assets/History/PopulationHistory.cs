using System;

public class PopulationHistory : iHistorical
{
    public Guid MarketId;
    public int Period { get; set; }
    public TurnPhase Phase { get; set; }

    public int Population;

}