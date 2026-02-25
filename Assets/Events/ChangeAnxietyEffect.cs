using UnityEngine;

[CreateAssetMenu(menuName ="LemonadeStandAssets/ChangeAnxietyEffect")]
public class ChangeAnxietyEffect : MarketEffectSO
{
    public float AnxietyChangePercentage;
    float _conversion => AnxietyChangePercentage / 100f;

    public ChangeAnxietyEffect(float anxietyChangePercentage)
    {
        AnxietyChangePercentage = anxietyChangePercentage;
    }

    public void ChangeEffectMultiplier(float multiplier)
    {
        AnxietyChangePercentage = multiplier / 100f;
    }

    public override void Apply(Market market)
    {
        var affectedAgents = market.GetMarketParticipants();

        foreach (var agent in affectedAgents)
        {
            var anxiety = agent.GetAnxiety();
            var delta = anxiety * (1 + _conversion);
            agent.SetAnxiety(delta);
        }
    }
   
}