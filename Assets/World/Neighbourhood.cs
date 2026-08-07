using System;
using System.Collections.Generic;
using UnityEngine;

public class Neighbourhood : ScriptableObject
{
    //Maslow
    public int Food;
    public int Security;
    public int Society;
    public int Validation;
    public int SelfActualization;

    //Geography
    public int Zone;
    private Country _country;
    public Country GetCountry() => _country;
    public void SetCountry(Country value) => _country=value;
    public int GetLocalDeliveryCostFrom(int zone, DeliverySizeEnum size)=> Math.Abs(Zone-zone)*(int)size;

    //Infrastructure
    public List<Good> CanProduce = new();
    public int InfrastructureLimit;
    
}

public enum DeliverySizeEnum
{
    small=1,
    medium=2,
    large=3
}