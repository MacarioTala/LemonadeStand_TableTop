using System;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class DemographicHistoryAttribute : Attribute
{
    public string Label { get; set; }
    public DemographicHistoryAttribute(string label=null)
    {
        Label = label;
    }
}

public interface IDemographicObserver
{
    
}