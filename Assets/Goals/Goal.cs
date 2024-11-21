using System;
using System.Collections.Generic;

public class Goal
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Func<iCompany, bool> IsGoalMet { get; set; }
    public Action<Company,Goal> InitializeGoal { get; private set; }
    public bool IsAchieved=false;

    public Dictionary<string,object> OriginalValues {get;private set;} = new ();

    public Goal(string name, string description, Func<iCompany, bool> isGoalMet,Action<Company,Goal> initializeGoal=null)
    {
        Name = name;
        Description = description;
        IsGoalMet = isGoalMet;
        InitializeGoal = initializeGoal;
    }

    public T GetOriginalValue<T>(string key)
    {
        return OriginalValues.ContainsKey(key) ? (T)OriginalValues[key] : default;
    }

    public void SetOriginalValue(string key, object value)
    {
        if (OriginalValues.ContainsKey(key))
        {
            OriginalValues[key] = value;
        }
        else
        {
            OriginalValues.Add(key, value);
        }
    }
    public void Initialize(Company company)
    {
        InitializeGoal?.Invoke(company,this);
    }
    public override string ToString()
    {
        return Name;
    }

    public override bool Equals(object other)
    {
        if (other is Goal goal)
        {
            return Name == goal.Name;
        }
        return false;
    }
    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}