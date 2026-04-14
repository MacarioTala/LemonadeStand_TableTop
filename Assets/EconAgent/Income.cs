using System;
using UnityEngine;

[Serializable]
public class Income
{
    public int IncomeInCents;
    [Tooltip("Income will be generated once at period 0, then every this many periods after")]
    public int FrequencyInPeriods;
}