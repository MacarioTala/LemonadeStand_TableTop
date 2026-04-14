using System;

public class ElasticDemandComponent
{
    public ElasticDemandComponentEnum Type { get; set; }
    public float Elasticity { get; set; }// Note: Elasticity is negative almost always except for Veblen goods.
    public float MinPercentageChange { get; set; } = float.MinValue;
    public float MaxPercentageChange { get; set; } = float.MaxValue;// This is the maximum adjustment that this component can make to the demand
    public float BlackSwanToZeroLevel { get; set; } = float.MinValue;// At this level, this component can override all other components
                                                                     // and set demand to zero.
    public float BlackSwanToVerticalLevel { get; set; } = float.MinValue; // At this level, this component can override all other components
                                                                          // and set demand to vertical (infinite) level.
    public bool IsVeblenGood() => Elasticity > 0;
    private bool IsBlackSwanToZeroSet()
    {
        return BlackSwanToZeroLevel != float.MinValue;
    }
    private bool IsBlackSwanToVerticalLevelSet()
    {
        return BlackSwanToVerticalLevel != float.MinValue;
    }

    public int GetDemandAdjustment(float percentChangeInFactor, int currentDemand)
    {
        if (IsBlackSwanToZero(percentChangeInFactor))
        {
            return -currentDemand; // Demand collapses to zero at Black Swan levels of change
        }
        if (IsBlackSwanToVertical(percentChangeInFactor))
        {
            return int.MaxValue; // Demand goes to vertical level at Black Swan levels of change
        }

        var boundedPercentChange = Elasticity * percentChangeInFactor;

        if (Math.Abs(boundedPercentChange) > Math.Abs(MaxPercentageChange) && MaxPercentageChange != float.MaxValue)
        {
            boundedPercentChange = MaxPercentageChange; // Cap the percent change to the maximum adjustment
        }
        else if (Math.Abs(boundedPercentChange) < Math.Abs(MinPercentageChange) && MinPercentageChange != float.MinValue)
        {
            boundedPercentChange = MinPercentageChange; // Cap the percent change to the minimum adjustment
        }

        return (int)(boundedPercentChange * currentDemand);
    }

    public bool IsBlackSwanToZero(float percentChangeInFactor)
    {
        if (!IsBlackSwanToZeroSet())
        {
            return false; // Black Swan to zero level is not set for this component
        }
        if (IsVeblenGood())
        {
            if (percentChangeInFactor <= BlackSwanToZeroLevel)
            {
                return true;
            }
        }
        else
        {
            if (percentChangeInFactor >= BlackSwanToZeroLevel)
            {
                return true; // Collapse demand at Black Swan levels of change of this component
            }
        }
        return false;
    }
    public bool IsBlackSwanToVertical(float percentChangeInFactor)
    {
        if (!IsBlackSwanToVerticalLevelSet())
        {
            return false; // Black Swan to vertical level is not set for this component
        }
        if (IsVeblenGood())
        {
            if (percentChangeInFactor >= BlackSwanToVerticalLevel)
            {
                return true; // Demand goes to vertical level at Black Swan levels of change of this component
            }
        }
        else
        {
            if (percentChangeInFactor <= BlackSwanToVerticalLevel)
            {
                return true; // Demand goes to vertical level at Black Swan levels of change of this component
            }
        }
        return false;
    }
}


