using System;
using UnityEngine;

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
        var newValue = (float)Math.Round(currentValue + amount,2);
        _modifier(target, newValue);
    }
}