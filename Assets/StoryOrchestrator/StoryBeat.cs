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

/// <summary>
/// Note: This is for global game state: narration, etc. Query this object to learn what's currently in the story
/// </summary>
public struct GameState
{
    public int Period;
}

/// <summary>
/// Certain story beats mutate game state/rules. When you need to pass those mutations, use this object.
/// </summary>
public class BeatPropertyBag
{
    public FixedCostTemplate FixedCostAssociatedWithBeat;
}
