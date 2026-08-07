using System;
using UnityEngine;

[CreateAssetMenu(menuName ="LemonadeStandAssets/FixedCost")]
public class FixedCostTemplate : ScriptableObject
{
    public string Name;
    public string Description;
    public FixedCostEnum FixedCostType;
    public int Amount;
    [Tooltip("Occurs every this many periods")]public int Frequency;
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