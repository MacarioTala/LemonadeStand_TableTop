using System;
using System.Collections.Generic;
using System.Linq;

public static class MathHelper
{
    public static float EstimatePeriodsToFinal(float initialValue, float targetValue, float rate)
    {
        if (rate <=0 || rate >= 1) return float.PositiveInfinity;
        if (targetValue <= 0) targetValue = 0.01f;
        return (float)(Math.Log(targetValue/initialValue) / Math.Log(1 - rate));
    }
    public static float GetSingleCoeffientCubicOutput(float ratio, float coefficient)
    {
        /// <summary>
        /// This function returns a cubic output based on a single coefficient
        /// It's meant to return a value that increases with the ratio up to a peak, then decreases
        /// </summary>
        var saddle = 1-coefficient;
        return -coefficient*(float)Math.Pow(ratio-1,3) + saddle;
    }
    public static float GetMetricPercentageChangeInPeriod<T>
        (
            Func<Guid, IEnumerable<T>> getHistoryFunc,
            Func<T, float> getMetricValueFunc,
            int currentPeriod,
            Guid marketId
        )
        where T : iHistorical
    {
        if(currentPeriod == 0) return 0;

        var history = getHistoryFunc(marketId);

        var metricValues = history
                            .Where(x=>
                                (x.Period == currentPeriod || x.Period == currentPeriod-1) 
                                &&
                                (x.Phase == TurnPhase.End)
                            )
                            .ToDictionary(x=>x.Period, x=>getMetricValueFunc(x));

        var currentMetricValue = metricValues.GetValueOrDefault(currentPeriod,0);
        var previousMetricValue = metricValues.GetValueOrDefault(currentPeriod-1,0);

        var valueToReturn = (currentMetricValue - previousMetricValue)
                            /(previousMetricValue==0?1:previousMetricValue);

        return valueToReturn;
    }
}