using System.Collections.Generic;

public interface iTradeProcessor
{   
    public void AddOrderToSendToEconomy (Trade trade);
    /// <summary>  
    /// Called by entities that placed orders to get the results of their orders
    /// </summary>
    List<Trade> GetOrderResults (ActionContext context);
    List<Trade> GetOrders ();
    List<Trade> ProcessCompanyOrders(Market market);
    /// <summary>
    /// Public interface that lets an entity send an Order 
    /// to any entity that can Process Orders
    /// </summary>
    /// <param name="context"></param>
    void QueueOrder (ActionContext context);
}