using System;
using UnityEngine;

public class GoodEffect
{
    public string Name {get;internal set;}
    public string Description {get; internal set;}
    public MetricModifier<PopulationCompany> Effect{get; internal set;}

    public float Magnitude {get; internal set;}

    public void Apply(PopulationCompany company)
    {
        Effect?.Modify(company, Magnitude);
    }
}

public static class GoodEffectBuilder
{
    public static GoodEffect Create()
    {
        return new GoodEffect();
    }
    public static GoodEffect Named(this GoodEffect effect, string name)
    {
        effect.Name = name;
        return effect;
    }
    public static GoodEffect DescribedAs(this GoodEffect effect, string description)
    {
        effect.Description = description;
        return effect;
    }
    public static GoodEffect WithEffect(this GoodEffect effect, MetricModifier<PopulationCompany> func)
    {
        effect.Effect = func;
        return effect;
    }
    public static GoodEffect WithEffectMagnitude(this GoodEffect effect, float magnitude)
    {
        effect.Magnitude = magnitude;
        return effect;
    }
}