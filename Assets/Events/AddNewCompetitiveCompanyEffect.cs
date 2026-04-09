using UnityEngine;

[CreateAssetMenu(menuName = "LemonadeStandAssets/AddNewCompetitiveCompanyEffect")]
public class AddNewCompetitiveCompanyEffect : MarketEffectSO
{
    public string Name;
    public int InitialCashInCents;
    public AgentLevelEnum Level;
    
    public override void Apply(Market market, ActiveMarketEvent activeMarketEvent)
    {
        var company = EconAgentBuilder
                    .ForBaseAgent()
                    .Named(CompanyNameGenerator.GenerateName())
                    .AtLevel(Level)
                    .WithInitialCash(InitialCashInCents*100)
                    .Build();
        
        market.RegisterMarketParticipant(company);
    }
}