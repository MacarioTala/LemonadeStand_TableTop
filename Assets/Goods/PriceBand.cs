using System;

[Serializable]
public class PriceBand{
    public int Min;
    public int Max;

    public PriceBand(int min, int max)=> (Min,Max) = (min,max);
}
