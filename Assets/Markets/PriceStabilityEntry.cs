using System;
using UnityEngine;

[Serializable]
public class PriceStabilityEntry
{
    public Good Good;
    [Range(0f,1f)]
     public float Flex;
}