using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Country", menuName = "LemonadeStandAssets/Country", order = 1)]
public class Country : ScriptableObject
{
    public readonly Guid CountryId=new();
    [SerializeField]private int numberOfNeighbourhoods;
    readonly List<Neighbourhood> Neighbourhoods=new();
    public int TransportCapacity=0;
    private readonly Dictionary<Neighbourhood,Vector2Int> CountryMap;
    public void Initialize()
    {
        GenerateNeighbourhoods(numberOfNeighbourhoods);
        EndowNeighbourhoods();
    }
    public IReadOnlyList<Neighbourhood> GetNeighbourhoods() => Neighbourhoods;

    public void GenerateNeighbourhoods(int totalNeighbourhoods)
    {
        for(var i=0; i<totalNeighbourhoods;i++)
        {
            var maxResidents = MathHelper.RollD(1,6);
            var neighbourhood = new Neighbourhood(this,maxResidents)
            {
                Name = $"N{i}"
            };
            Neighbourhoods.Add(neighbourhood);
        }
        
        for(var i=1;i<Neighbourhoods.Count;i++)
        {
            var neighbourhood = Neighbourhoods[i];
            var randomNeighbour = Neighbourhoods[UnityEngine.Random.Range(0,i)];
            neighbourhood.ConnectTo(randomNeighbour);
        }
    }

    public void EndowNeighbourhoods()
    {
        foreach(var neighbourhood in Neighbourhoods)
            {
                foreach(var element in GameRoot.Instance.GetElements())
                {
                    if(neighbourhood.GetProduction().Count>=3) break;
                    Endow(element,neighbourhood);
                }
                
                neighbourhood.Produce();
            }
    }

    private void Endow(Good element, Neighbourhood neighbourhood)
    {
        var endowThis = element.GetRarity()==MathHelper.GetRarityFromPercentage(MathHelper.RollD(1,100));

        var producedQty = Math.Min(MathHelper.GetNumberProducedPerTurnFromRarity(element.GetRarity()),1);

        if(endowThis)
            neighbourhood.AddProduction(element,producedQty);
    }

    public void Dispose()
    {
        foreach(var neighbourhood in Neighbourhoods) neighbourhood.Dispose();
    }
}