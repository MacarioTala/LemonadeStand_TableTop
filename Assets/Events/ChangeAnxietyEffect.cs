public class ChangeAnxietyEffect : iMarketEffect
{
    iMarketEvent _parentEvent;
    public float AnxietyChangePercentage;
    float _originalAnxietyChangePercentage;
    float _conversion => AnxietyChangePercentage / 100f;

    public ChangeAnxietyEffect(float anxietyChangePercentage)
    {
        AnxietyChangePercentage = anxietyChangePercentage;
        _originalAnxietyChangePercentage = anxietyChangePercentage;
    }

    public void ChangeEffectMultiplier(float multiplier)
    {
        AnxietyChangePercentage = multiplier / 100f;
    }

    public void Apply(Market market)
    {
        var affectedAgents = market.GetMarketParticipants();

        foreach (var agent in affectedAgents)
        {
            var anxiety = agent.GetAnxiety();
            var delta = anxiety * (1 + _conversion);
            agent.SetAnxiety(delta);
        }
    }

    public void Reset()
    {
        AnxietyChangePercentage = _originalAnxietyChangePercentage;
    }

    public void SaveOriginalState()
    {
        //Noop: Original state saved in constructor
    }
    public iMarketEvent GetParentEvent()
    {
        return _parentEvent;
    }
    public void SetParentEvent(iMarketEvent marketEvent)
    {
        _parentEvent = marketEvent;
    }
}