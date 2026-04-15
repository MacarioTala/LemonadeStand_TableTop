using UnityEngine;

public abstract class StrategyFactory:ScriptableObject
{
    public abstract iStrategy Instantiate();
}

public abstract class StrategyFactory<T>: StrategyFactory 
where T:iStrategy,new()
{
    public override iStrategy Instantiate() => new T();

}