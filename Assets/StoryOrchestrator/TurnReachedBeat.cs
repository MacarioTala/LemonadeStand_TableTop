using UnityEngine;

[CreateAssetMenu(menuName = "LemonadeStandAssets/StoryBeats/TurnReachedBeat")]
public class TurnReachedBeat : StoryBeat
{
    public int PeriodToPlayIn;
    public override void PlayBeat()
    {
        
    }
    public override bool ShouldPlay(GameState state)
    {
        return state.Period==PeriodToPlayIn;
    }
}