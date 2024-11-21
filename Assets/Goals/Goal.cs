using System;
using UnityEngine.UI;

public class Goal
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Func<iCompany, bool> IsGoalMet { get; set; }
    public bool IsAchieved=false;

    public Goal(string name, string description, Func<iCompany, bool> isGoalMet)
    {
        Name = name;
        Description = description;
        IsGoalMet = isGoalMet;
    }

    public override string ToString()
    {
        return Name;
    }

    public override bool Equals(object other)
    {
        if (other is Goal)
        {
            return Name == ((Goal)other).Name;
        }
        return false;
    }
    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}