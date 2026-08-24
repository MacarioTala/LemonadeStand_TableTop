using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Country : ScriptableObject
{
    public readonly Guid CountryId=new();

    readonly List<Neighbourhood> Neighbourhoods=new();
    private readonly Dictionary<Neighbourhood,Vector2Int> CountryMap;
    public void Initialize()
    {
    
    }
    public IReadOnlyList<Neighbourhood> GetNeighbourhoods() => Neighbourhoods;

    public void GenerateNeighbourhoods(int totalNeighbourhoods)
    {
        for(var i=0; i<totalNeighbourhoods;i++)
        {
            var neighbourhood =CreateInstance<Neighbourhood>();
            neighbourhood.Name = $"N{i}";
            neighbourhood.SetCountry(this);
            Neighbourhoods.Add(neighbourhood);
        }
        
        for(var i=1;i<Neighbourhoods.Count;i++)
        {
            var neighbourhood = Neighbourhoods[i];
            var randomNeighbour = Neighbourhoods[UnityEngine.Random.Range(0,i)];
            neighbourhood.ConnectTo(randomNeighbour);
        }
    //     GenerateCountryMap(
    //         Neighbourhoods[0],

    //         );
    }

    private int GetNeighbourhoodWithMaxDegree()
        => Neighbourhoods.Max(x=>x.Neighbours.Count);
    
    private void GenerateCountryMap(
        Neighbourhood current,
        Vector2Int location,
        Dictionary<Neighbourhood,Vector2Int> locations
    )
    {
        locations[current]=location;
        foreach(var neighbour in current.Neighbours)
        {
            if(locations.ContainsKey(neighbour))
                continue;
            
            
        }
    }
}