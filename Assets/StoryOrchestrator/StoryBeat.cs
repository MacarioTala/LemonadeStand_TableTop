using System;
using UnityEngine;


public abstract class StoryBeat: ScriptableObject
{
    [SerializeField]private string id;
    public string Id=> id;
    public Sprite Image;
    public string FlavourText;
    
    [SerializeField]FixedCostTemplate fixedCostAssociatedWithBeat;

    public virtual BeatPropertyBag PlayBeat()
    {   var bag = new BeatPropertyBag();
        if(fixedCostAssociatedWithBeat!=null) bag.FixedCostAssociatedWithBeat=fixedCostAssociatedWithBeat;

        return bag;
    }
    public abstract bool ShouldPlay(GameState state);
    
    #region Unity Overrides
    void OnValidate()
    {
        if(string.IsNullOrEmpty(id))
            id=Guid.NewGuid().ToString();
    }
    #endregion
}

public struct GameState
{
    public int Period;
}

public class BeatPropertyBag
{
    public FixedCostTemplate FixedCostAssociatedWithBeat;
}
