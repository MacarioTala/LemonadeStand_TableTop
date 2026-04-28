using System;
using UnityEngine;


public abstract class StoryBeat: ScriptableObject
{
    [SerializeField]private string id;
    public string Id=> id;
    public Sprite Image;
    public string FlavourText;
    public abstract void PlayBeat();
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
