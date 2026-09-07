using System;
using System.Collections.Generic;
using System.Linq;

public static class MathHelper
{
    public static float EstimatePeriodsToFinal(float initialValue, float targetValue, float rate)
    {
        if (rate <= 0 || rate >= 1) return float.PositiveInfinity;
        if (targetValue <= 0) targetValue = 0.01f;
        return (float)(Math.Log(targetValue / initialValue) / Math.Log(1 - rate));
    }
    public static bool IsCoinFlipHeads()
    {
        var rnd = new Random();
        return rnd.Next(0,1)==0;
    }
    public static float GetSingleCoeffientCubicOutput(float ratio, float coefficient)
    {
        /// <summary>
        /// This function returns a cubic output based on a single coefficient
        /// It's meant to return a value that increases with the ratio up to a peak, then decreases
        /// </summary>
        var saddle = 1 - coefficient;
        return -coefficient * (float)Math.Pow(ratio - 1, 3) + saddle;
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
        if (currentPeriod == 0) return 0;

        var history = getHistoryFunc(marketId);

        var metricValues = history
                            .Where(x =>
                                (x.Period == currentPeriod || x.Period == currentPeriod - 1)
                                &&
                                (x.Phase == TurnPhase.End)
                            )
                            .ToDictionary(x => x.Period, x => getMetricValueFunc(x));

        var currentMetricValue = metricValues.GetValueOrDefault(currentPeriod, 0);
        var previousMetricValue = metricValues.GetValueOrDefault(currentPeriod - 1, 0);

        var valueToReturn = (currentMetricValue - previousMetricValue)
                            / (previousMetricValue == 0 ? 1 : previousMetricValue);

        return valueToReturn;
    }

    public static int GetNumAppearingFromRarity(RarityEnum rolledRarity)
    {
        return rolledRarity switch
        {
          RarityEnum.Common => RollD(1,10),  
          RarityEnum.Uncommon => RollD(1,5),
          RarityEnum.Rare => RollD(1,2),
          RarityEnum.VeryRare => 1,
          _ =>0
        };
    }
    public static RarityEnum GetRarityFromPercentage(int roll)
    {
        return roll switch
        {
            <= 10 =>  RarityEnum.VeryRare,
            <= 30 => RarityEnum.Rare,
            <= 50 => RarityEnum.Uncommon,
            _ => RarityEnum.Common
        };
    }

    public static decimal Median<T>(this IEnumerable<T> source, Func<T, decimal> selector)
    {
        var sorted = source
                    .Select(selector)
                    .OrderBy(x => x)
                    .ToList();

        int count = sorted.Count;

        if (count == 0)
        { throw new InvalidOperationException("Empty sets have no median"); }

        if (count % 2 == 0)
        {
            return (sorted[count / 2 - 1] + sorted[count / 2]) / 2;
        }
        else
        {
            return sorted[count / 2];
        }
    }

    public static int RollD(int numDice,int numSides)
    {
        var rng = new Random();
        int roll=0;
        for(var i=0;i<numDice;i++)
        {
            var currentRoll = rng.Next(1,numSides+1);
            roll+=currentRoll;
        }
        return roll;
    }
}