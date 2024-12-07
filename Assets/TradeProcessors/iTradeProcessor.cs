using System.Collections.Generic;

public interface iTradeProcessor
{   
    public void AddOrderToSendToEconomy (Trade trade);
    /// <summary>
    /// Returns all orders in the Market's queue
    /// </summary>
    public void SendTradesToEconomy ();
    List<Trade> GetOrders ();
    /// <summary>
    /// Processes all orders in the queue
    /// and sends them to TheEconomy
    /// </summary>
    void ProcessOrders ();
    /// <summary>
    /// Public interface that lets an entity send an Order 
    /// to any entity that can Process Orders
    /// </summary>
    /// <param name="context"></param>
    void QueueOrder (ActionContext context);
    /// <summary>  
    /// Called by entities that placed orders to get the results of their orders
    /// </summary>
    List<Trade> GetOrderResults (ActionContext context);
}