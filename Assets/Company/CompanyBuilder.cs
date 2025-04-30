public static class CompanyBuilder
{
    public static Company WithFixedCostStrategy(this Company company, iFixedCostStrategy fixedCostStrategy)
    {
        company.FixedCostStrategy = fixedCostStrategy;
        return company;
    }
    public static Company WithBehaviourStrategy(this Company company, iStrategy behaviourStrategy)
    {
        company.SetStrategy(behaviourStrategy);
        return company;
    }
    public static Company WithInitialCash(this Company company, int initialCash)
    {
        company.SetCash(initialCash);
        return company;
    }
    public static Company WithActionsPerTurn(this Company company, int actionsPerTurn)
    {
        company.SetActionsPerCycle(actionsPerTurn);
        return company;
    }
    public static Company Named(this Company company, string name)
    {
        company.Name = name;
        return company;
    }

    public static Company AtLevel(this Company company, CompanyLevelEnum companyLevel)
    {
        company.companyLevel = companyLevel;
        return company;
    }
}