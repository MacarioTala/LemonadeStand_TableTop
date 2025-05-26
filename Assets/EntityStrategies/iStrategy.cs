public interface iStrategy
{
    void GenerateGoals(iCompany company);
    float GetAggressionLevel();
    LemonadeStandResultObject SetAggressionLevel(float aggressionLevel);
    public void PerformStrategy(ActionContext context);
    public void PerformStrategy(iCompany company, int period);
}
    