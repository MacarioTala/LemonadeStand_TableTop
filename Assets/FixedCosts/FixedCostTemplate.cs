using System;
using UnityEngine;

[CreateAssetMenu(menuName ="LemonadeStandAssets/FixedCost")]
public class FixedCostTemplate : ScriptableObject
{
    public string Name;
    public string Description;
    public FixedCostEnum FixedCostType;
    public int AmountInCents;
    [Tooltip("Occurs every this many periods")]public int Frequency;

    public decimal Amount { 
        get=>AmountInCents/100m; 
        set=>AmountInCents=(int)Math.Round(value*100m); 
        }
}

[Serializable]
public class FixedCostInstance
{
    public FixedCostTemplate Template;
    public int PeriodAcquired;
    public FixedCostInstance(FixedCostTemplate template,int periodAcquired)
    {
        Template = template;
        PeriodAcquired=periodAcquired;
    }

    public string GetDescription()
    {
        return string.Format(Template.Description,Template.Amount,Template.Frequency);
    }
}