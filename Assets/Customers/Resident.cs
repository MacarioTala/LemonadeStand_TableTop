using System;
using System.Collections.Generic;
using System.Linq;

public class Resident
{
    Neighbourhood homeNeighbourhood;
    int maxDistanceWillingToTravel=0;
    int maxNeighbourhoodsToVisit=0;
    bool isMoving=false;
    bool atHome=true;
    Neighbourhood currentLocation;
    Neighbourhood currentDestination;
    readonly HashSet<Neighbourhood> neighbourhoodsVisited=new();

    public Resident(Neighbourhood neighbourhood)
    {
        homeNeighbourhood=neighbourhood;
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
        homeNeighbourhood.GetDistanceToOrFrom(currentLocation);
    }

    public void EstablishTradeRoute(InventoryEntry entry, Neighbourhood neighbourhood)
    {
        throw new NotImplementedException();
    }
    private void Move()
    {
        isMoving=true;
        throw new NotImplementedException();
    }
    public void MoveToNextNeighbourhood()
    {
        var possibleMoves = currentLocation.Neighbours;
        
        var neighbourHoodToMoveTo = possibleMoves.ElementAt(MathHelper.RollD(1,possibleMoves.Count));
        
        currentLocation=neighbourHoodToMoveTo;
    }
    public void SetCurrentLocation(Neighbourhood neighbourhood) => currentLocation=neighbourhood;
}