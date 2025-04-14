using System.Collections.Generic;
using UnityEngine;

public class MarketEventSO : ScriptableObject, iMarketEvent
{
    public string EventName;

    [TextArea(3,10)]public string EventDescription;
    [Tooltip("The chance of this event happening in a given period. 0-100%")]
    public float EventChance; 
    [Tooltip("The duration of the event in periods. 0 = permanent")]
    int _eventDuration;
    
    public List<iMarketEffect> Effects = new();

    public int GetDuration()
    {
        return _eventDuration;
    }
    public float GetProbabilityOf()
    {
        return EventChance;
    }

    public void Invoke(Market market)
    {
        foreach (var effect in Effects)
        {
            effect.Apply(market);
        }
    }

    public void Initialize(string eventName, string eventDescription, float eventChance, int eventDuration)
    {
        EventName = eventName;
        EventDescription = eventDescription;
        EventChance = eventChance;
        _eventDuration = eventDuration;
    }
}
