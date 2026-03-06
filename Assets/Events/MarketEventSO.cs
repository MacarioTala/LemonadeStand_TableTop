using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
[CreateAssetMenu(menuName ="LemonadeStandAssets/MarketEvent")]
public class MarketEventSO : ScriptableObject, iMarketEvent,iTaggable
{
    public string EventName;

    [TextArea(3,10)]public string EventDescription;
    [Tooltip("The chance of this event happening in a given period. 0-100%")]
    public float EventChance; 
    [SerializeField,Tooltip("The duration of the event in periods. 0 = permanent")]
    int _eventDuration;
    [SerializeField]
    public Sprite EventSprite;
    public string FlavourText;
    [SerializeReference]
    List<MarketEffectSO> Effects = new();
    public IEnumerable<iMarketEffect> GetEffects()
    {
        return Effects;
    }
    public void AddEffect(MarketEffectSO effect)
    {
        Effects.Add(effect);
        effect.SetParentEvent(this);
    }
    
#region iTaggable
    [SerializeField]List<string> _tags = new();
    public List<string> Tags => _tags;
    List<string> iTaggable.Tags => _tags;

    public List<string> GetTags()
    {
        return Tags;
    }
    public void AddTag(string tag)
    {
        if (!Tags.Contains(tag))
        {
            Tags.Add(tag);
        }
    }
    public void RemoveTag(string tag)
    {
        if (Tags.Contains(tag))
        {
            Tags.Remove(tag);
        }
    }
    public bool HasTag(string tag)
    {
        return Tags.Contains(tag);
    }
    
#endregion
    public bool IsExpiredAt(int startPeriod, int currentPeriod)
    {
        return currentPeriod>= startPeriod + _eventDuration;
    }
    public int GetDuration()
    {
        return _eventDuration;
    }
    public void SetDuration(int duration)
    {
        _eventDuration = duration;
    }
    public float GetProbabilityOf()
    {
        return EventChance;
    }

    public void Invoke(Market market,ActiveMarketEvent activeEvent)
    {
        foreach (var effect in Effects)
        {
            effect.Apply(market,activeEvent);
        }

        foreach (var effect in activeEvent.ExtraEffects)
        {
            effect.Apply(market,activeEvent);
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
