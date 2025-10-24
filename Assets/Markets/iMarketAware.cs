public interface iMarketAware
{
    // Refactor candidate -- can make this LemonadeStandResultObject SetMarket(Market market)
    // and have EconAgents just be iMarketAware, and not have multiple ways of setting markets.
    // Also, this is redundant with iMarketParticipant. Merge concepts in future code.
    void SetMarket(Market market);
}