using System.Collections.Generic;

public readonly struct EconomyCoreReadyEvent
{
    public readonly TheEconomy Economy;
    public EconomyCoreReadyEvent (TheEconomy economy)=> Economy=economy;
}
public readonly struct DeliveriesResolvedEvent
{
    public EconAgent Agent {get;}
    public IReadOnlyList<InventoryEntry> ArrivingItems{get;}
    public readonly int Period;

    public DeliveriesResolvedEvent(EconAgent agent, IReadOnlyList<InventoryEntry> arrivingItems,int period)
    {
        Agent = agent;
        ArrivingItems =arrivingItems;
        Period = period;
    }

    public bool HasArrivals => ArrivingItems !=null && ArrivingItems.Count>0;
}

public readonly struct GoodsExpireEvent
{
    public EconAgent Agent{get;}
    public IReadOnlyList<InventoryEntry> ExpiringItems{get;}
    public readonly int Period;
    
    public GoodsExpireEvent(EconAgent agent, IReadOnlyList<InventoryEntry> expiringItems, int period)
    {
        Agent = agent;
        ExpiringItems=expiringItems;
        Period= period;
    }

    public bool HasExpiringItems => ExpiringItems !=null && ExpiringItems.Count>0;
}

public readonly struct PeriodHappenedEvent
{
    public readonly int Period;
    public PeriodHappenedEvent(int period)=> Period=period;
}

public readonly struct StoryBeatHappenedEvent
{
    public readonly StoryBeat Beat;
    public StoryBeatHappenedEvent(StoryBeat beat) => Beat = beat;
}

public readonly struct RequestLoadSceneEvent
{
    public readonly string SceneName;
    public RequestLoadSceneEvent(string sceneName) => SceneName = sceneName;
}