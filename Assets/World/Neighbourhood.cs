using System;
using System.Collections.Generic;

public class Neighbourhood 
{
    public readonly Guid NeighbourhoodId=new();
    public string Name;
    
    private int gold;
    private readonly MaslovianNeeds maslovianNeeds=new();
    private readonly List<Resident> residents;

    //Infrastructure
    private readonly Dictionary<Good,int> produces = new();
    private readonly Dictionary<Good,int> stockpile = new();
    public int InfrastructureLimit;
    public int CostToRaiseInfrastructureLimit;

    //Geography
    public readonly HashSet<Neighbourhood> Neighbours=new();
    public int Zone;
    private Country country;
    public Neighbourhood(int? maxResidents=null)
    {
        maxResidents ??= MathHelper.RollD(1,6);

        for(var i=0;i<maxResidents;i++)
            residents.Add(new Resident());
        
    }

    public void Act()
    {
        Produce();
    }

    #region Geography
    public Country GetCountry() => country;
    public void SetCountry(Country value) => country=value;
    public void ConnectTo(Neighbourhood neighbour)
    {
        Neighbours.Add(neighbour);
        neighbour.Neighbours.Add(this);
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
    #endregion

    #region Logistics
    public int GetLocalDeliveryCostFrom(Neighbourhood otherNeighbourhood, DeliverySizeEnum size)
    {
        var nodeCount = GetDistanceToOrFrom(otherNeighbourhood);
        return nodeCount*(int)size;
    }
    #endregion
    
    #region Economics
    public void AddProduction(Good good, int amountPerTurn) =>produces.Add(good,amountPerTurn);

    private void Produce()
    {
        foreach(var product in produces)
        {
            stockpile.TryGetValue(product.Key, out int currentStock);
            stockpile[product.Key]=currentStock+product.Value;
        }
    }
    public MaslovianNeeds GetMaslovianNeeds()=>maslovianNeeds;
    private void RankGoods()
    {
        throw new NotImplementedException();
    }
    #endregion
}

public class Preferences
{
    readonly List<Good> knownGoods=new();

    public void AddKnownGood(Good good) => knownGoods.Add(good);

    
}
public class Preference
{
    public Good Good;
    public int Rank;
}