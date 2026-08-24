using System;
using System.Collections.Generic;
using UnityEngine;

public class Neighbourhood : ScriptableObject
{
    public readonly Guid NeighbourhoodId=new();
    public string Name;
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
    public int GetLocalDeliveryCostFrom(Neighbourhood otherNeighbourhood, DeliverySizeEnum size)
    {
        var nodeCount = GetDistanceToOrFrom(otherNeighbourhood);
        return nodeCount*(int)size;
    }

    public int GetDistanceToOrFrom(Neighbourhood otherNeighbourhood)
    {
        if(otherNeighbourhood == this)
            return 0;
        
        var visited = new HashSet<Neighbourhood>();
        var queue = new Queue<(Neighbourhood node,int distance)>();
        
        visited.Add(this);
        queue.Enqueue((this,0));

        while (queue.Count >0)
        {
            var (current,distance) = queue.Dequeue();

            foreach(var neighbour in current.Neighbours)
            {
                if(!visited.Add(neighbour))
                    continue;

                if(neighbour == otherNeighbourhood)
                    return distance+1;
                
                queue.Enqueue((neighbour, distance+1));
            }
        }
        return -1;
    }

    //Map Gen
    public readonly HashSet<Neighbourhood> Neighbours=new();
    public void ConnectTo(Neighbourhood neighbour)
    {
        Neighbours.Add(neighbour);
        neighbour.Neighbours.Add(this);
    }

    //Infrastructure
    public List<Good> CanProduce = new();
    public int InfrastructureLimit;
    public int CostToRaiseInfrastructureLimit;
    
}

public enum DeliverySizeEnum
{
    small=1,
    medium=2,
    large=3
}