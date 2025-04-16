using System;
using System.Collections.Generic;

public static class MathHelper
{
    public static float GetSingleCoeffientCubicOutput(float ratio, float coefficient)
    {
        /// <summary>
        /// This function returns a cubic output based on a single coefficient
        /// It's meant to return a value that increases with the ratio up to a peak, then decreases
        /// </summary>
        var saddle = 1-coefficient;
        return -coefficient*(float)Math.Pow(ratio-1,3) + saddle;
    }
}