public class StrategyBuilder<T> where T : iStrategy, new()
{
    private T _strategyToReturn;

    public StrategyBuilder(T strategy) =>
        _strategyToReturn = strategy;
    
    public StrategyBuilder()
    {
        _strategyToReturn = new T();
    }
    public T Build()=> _strategyToReturn;
    
    public StrategyBuilder<T> WithAggressionLevel(float aggressionLevel)
    {
        _strategyToReturn.SetAggressionLevel(aggressionLevel);
        return this;
    }
}
public static class StrategyBuilder
{
    public static StrategyBuilder<T> For <T>() where T : iStrategy, new()
    {
        return new StrategyBuilder<T>();
    }
}
