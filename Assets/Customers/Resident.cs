using System;
using System.Linq;

public class Resident
{
    Neighbourhood homeNeighbourhood;
    int maxDistanceWillingToTravel=0;
    bool isMoving =false;

    public Neighbourhood CurrentLocation{get; private set;}
    Neighbourhood currentDestination;

    public Resident(Neighbourhood neighbourhood)
    {
        homeNeighbourhood=neighbourhood;
        CurrentLocation=homeNeighbourhood;
        homeNeighbourhood.GetMaslovianNeeds();
        maxDistanceWillingToTravel=MathHelper.RollD(1,2)+homeNeighbourhood.GetCountry().TransportCapacity;
    }

    public void Act()
    {
        if(isMoving) Move();
    }

    public void GoHome()
    {
        isMoving=true;
        homeNeighbourhood.GetDistanceToOrFrom(CurrentLocation);
        currentDestination=homeNeighbourhood;
    }
    
    public bool IsHome()=>CurrentLocation==homeNeighbourhood;

    public void EstablishTradeRoute(InventoryEntry entry, Neighbourhood neighbourhood)
    {
        throw new NotImplementedException();
    }
    private void Move()
    {
        isMoving=true;
        var from = CurrentLocation;
        var possibleMoves = from.Neighbours;
        
        Neighbourhood neighbourHoodToMoveTo;
        if(currentDestination != null)
            neighbourHoodToMoveTo = possibleMoves
                                    .OrderBy(x=>x.GetDistanceToOrFrom(currentDestination))
                                    .First();
        else
            neighbourHoodToMoveTo=possibleMoves.ElementAt(UnityEngine.Random.Range(0,possibleMoves.Count));
        
        CurrentLocation=neighbourHoodToMoveTo;

        GameRoot.Instance.Bus.Publish(new ResidentMovedEvent(this,from,CurrentLocation));
        
        if(CurrentLocation==currentDestination)
        {
            currentDestination=null;
            isMoving=false;
        }
    }
    
    public void SetCurrentLocation(Neighbourhood neighbourhood) => CurrentLocation=neighbourhood;
}