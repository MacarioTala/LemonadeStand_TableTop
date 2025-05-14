using System;

public class MetricModifier<T>
{
    readonly Func<T, float> _metricToModify;
    readonly Action<T, float> _modifier;
    public MetricModifier(Func<T, float> metricToModify, Action<T, float> modifier)
    {
        _metricToModify = metricToModify;
        _modifier = modifier;
    } 

    public void Modify(T target, float amount)
    {
        var currentValue = _metricToModify(target);
        var newValue = currentValue + amount;
        _modifier(target, newValue);
    }
}