//This is a wrapper for DemandData so that EconAgent.DemandData can be set in the inspector
using System;

[Serializable]
public class DemandEntry
{
    public Good Good;
    public DemandDataForAuthoring DemandData;
}
