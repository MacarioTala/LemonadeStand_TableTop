using System.Collections.Generic;
using UnityEngine;

public class StoryOrchestrator : MonoBehaviour
{
    //Subscriptions
    private SubscriptionToken turnSubscription;
    private readonly SubscriptionToken companyBankruptedSubscription;
    [SerializeField] List<StoryBeat> potentialStoryBeats=new();
    private readonly HashSet<string> playedBeats=new();

    GameState gameState;
    
    private void CheckForStoryBeatsInPeriod()
    {
        foreach(var beat in potentialStoryBeats)
        {
            if(playedBeats.Contains(beat.Id)) continue;
            if(!beat.ShouldPlay(gameState)) continue;

            beat.PlayBeat();
            GameRoot.Instance.Bus.Publish(new StoryBeatHappenedEvent(beat));
            playedBeats.Add(beat.Id);
        }
    }

    private void OnPeriodReached(PeriodHappenedEvent evt)
    {
        gameState.Period=evt.Period;
        CheckForStoryBeatsInPeriod();
    }

    
    private void Initialize()
    {
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        if(GameRoot.Instance !=null && GameRoot.Instance.Bus !=null)
        {
            turnSubscription = GameRoot.Instance.Bus.Subscribe<PeriodHappenedEvent>(OnPeriodReached,false);
        }
    }

    #region Unity builtins
    private void Start()
    {
        Initialize();
    }

    private void OnDestroy()
    {
        var bus = GameRoot.Instance != null ? GameRoot.Instance.Bus : null;
        if(bus == null) return;
        
        bus.Unsubscribe(turnSubscription);
        bus.Unsubscribe(companyBankruptedSubscription);
    }
    #endregion
}
