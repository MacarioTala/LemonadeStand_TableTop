using System.Collections.Generic;
using System.Linq;

public class BasicTransactionManager : iTransactionManager
{
    internal bool HasGood(Good good, Inventory inventory)
    {
        var goodInInventory = inventory.GetInventoryEntriesByGood(good.GoodName).FirstOrDefault();
        return goodInInventory != null && goodInInventory.quantity >= 1;
    }

    public LemonadeStandResultObject ProcessTransaction(ActionContext context)
    {
        var processOrderResult = ProcessPairedOrders(context);   
        if (!processOrderResult.Result.Equals(LemonadeStandResultObject.Success().Result))
            return processOrderResult;
        
        return LemonadeStandResultObject.Success();
    }

    public LemonadeStandResultObject ProcessMarketTransaction(ActionContext context)
    {
        //This is ugly.
        //We're just going to record the transaction sent to us by the consumption manager
        //Instead of processing it like we do company orders
        //Find a way to refactor this
        //Maybe this is ok in case we decide that there's more processing to be done for 
        //other implementations of iTransactionManager
        var primaryOrder = context.PrimaryOrder;
        var counterPartyOrders = context.CounterPartyOrders;
        if(primaryOrder.Buyer.Equals(context.MarketToSubmitTo))
        {
            context.MarketToSubmitTo.GetInventory()
                .AddGood(new InventoryEntry(primaryOrder.Good, 
                    primaryOrder.RemainingQuantity, 
                    primaryOrder.Price,
                    context.Period));
        }
        RecordTransaction(primaryOrder,context.MarketToSubmitTo, context.Period,counterPartyOrders);
        
        return LemonadeStandResultObject.Success();
    }

    internal LemonadeStandResultObject ProcessPairedOrders(ActionContext context)
    {
        var containsValidPairedOrders = context.ContainsValidPairedOrders().Result
            .Equals(LemonadeStandResultObject.Success().Result);
        if(!containsValidPairedOrders) 
            return LemonadeStandResultObject.Failure(ResultTypeEnum.InvalidTransaction,"Invalid Transaction");
                
        var primaryOrder = context.PrimaryOrder;
        var counterPartyOrders = context.CounterPartyOrders;
        var _counterPartyOrdersToRecord = new List<Order>();
        foreach(var order in counterPartyOrders)
        {
            if (primaryOrder.IsFullyFilled)
            {
                break;
            }

            var processTransactionResult = ProcessTransactionPair(primaryOrder,order,context.Period);
            if (!processTransactionResult.Equals(LemonadeStandResultObject.Success()))
            {
                primaryOrder.OrderStatus = processTransactionResult;
                break;
            }
            _counterPartyOrdersToRecord.Add(order);
        }
       
        //Record transaction
        RecordTransaction(context.PrimaryOrder,context.MarketToSubmitTo, context.Period,_counterPartyOrdersToRecord);
        
        return LemonadeStandResultObject.Success();
    }

    internal LemonadeStandResultObject ProcessTransactionPair(Order primaryOrder, Order counterPartyOrder,int period)
    {
        var buyer= primaryOrder.Buyer;
        var seller = counterPartyOrder.Seller;

        var good = primaryOrder.Good;
        var price = counterPartyOrder.Price;

        var buyerInventory = buyer.GetInventory();
        var sellerInventory = seller.GetInventory();

        var quantity = counterPartyOrder.RemainingQuantity>=primaryOrder.RemainingQuantity
            ?primaryOrder.RemainingQuantity
            :counterPartyOrder.RemainingQuantity;

        var costOfThisLeg = quantity * price;

        var result = ValidateTransaction(primaryOrder, counterPartyOrder,costOfThisLeg);
        if (!result.Equals(LemonadeStandResultObject.Success()))
        {
            primaryOrder.OrderStatus = result;
            return result;
        }
        
        //Adjust cash balances
        primaryOrder.Buyer.SetCash(primaryOrder.Buyer.GetCash() - costOfThisLeg);
        counterPartyOrder.Seller.SetCash(counterPartyOrder.Seller.GetCash() + costOfThisLeg);

        //Adjust inventories
        buyerInventory.AddGood(new InventoryEntry(good, quantity, price, period));
        sellerInventory.RemoveGood(good, quantity, price);

        //Set filled quantity and status
        primaryOrder.FilledQuantity += quantity;

        primaryOrder.OrderStatus = primaryOrder.IsFullyFilled
            ? LemonadeStandResultObject.Success()
            : LemonadeStandResultObject.Failure(
                ResultTypeEnum.PartialFill, "Order partially filled"
                                            );

        counterPartyOrder.FilledQuantity += quantity;

        //Add this execution to the Order(s)
        //Primary Order
        var primaryExecution = new Execution(    order       : primaryOrder
                                                ,buyer       : buyer
                                                ,seller      : seller
                                                ,quantity    : quantity
                                                ,price       : price 
                                                ,period      : period
                                            );
        primaryOrder.AddExecution(primaryExecution);
        //CounterParty Order
        var counterPartyExecution = new Execution(   order       : counterPartyOrder
                                                    ,buyer       : buyer
                                                    ,seller      : seller
                                                    ,quantity    : quantity
                                                    ,price       : price 
                                                    ,period      : period
                                                );
        counterPartyOrder.AddExecution(counterPartyExecution);
        return LemonadeStandResultObject.Success();
    }

    internal LemonadeStandResultObject ValidateTransaction(Order primaryOrder, Order counterPartyOrder,decimal costOfThisLeg)
    {
        var buyer= primaryOrder.Buyer;
        var seller = counterPartyOrder.Seller;
        var sellerInventory = seller.GetInventory();

        //Validate transaction
        if (
            (primaryOrder.SubmittingCompany == counterPartyOrder.SubmittingCompany)
                ||
            (buyer == seller)
            )
        {
            primaryOrder.OrderStatus = LemonadeStandResultObject.Failure(ResultTypeEnum.SelfTrade,"Company cannot trade with itself");
            return primaryOrder.OrderStatus;
        }

        if (buyer.GetCash() < costOfThisLeg)
        {
            primaryOrder.OrderStatus = LemonadeStandResultObject.Failure
                (ResultTypeEnum.InsufficientCash,"Buyer does not have enough cash to complete the transaction");
            return primaryOrder.OrderStatus;
        }

        if (!HasGood(counterPartyOrder.Good, sellerInventory))
        {
            counterPartyOrder.OrderStatus = LemonadeStandResultObject.Failure
                (ResultTypeEnum.InsufficientGoods,$"Seller must have at least 1 quantity of {counterPartyOrder.Good} to trade");
            return counterPartyOrder.OrderStatus;
        }

        return LemonadeStandResultObject.Success();
    }
    

    internal static void RecordTransaction(Order orderToRecord, Market marketToRecordIn
            , int tradingPeriod,List<Order> counterPartyOrders)
    {
       var executedOrder = new Execution(orderToRecord,orderToRecord.Buyer,orderToRecord.Seller,orderToRecord.FilledQuantity,orderToRecord.Price, tradingPeriod);
       //Record Primary Order
       if ( counterPartyOrders.Count == 1)
         {
            var counterPartyOrder = counterPartyOrders[0];
            
            if (counterPartyOrder.IsSell())
            { orderToRecord.Seller = counterPartyOrder.Seller; }
            else
            { orderToRecord.Buyer = counterPartyOrder.Buyer; }
         }
       //Add CounterParty Orders to Primary Order
       foreach (var order in counterPartyOrders)
       {
           if (order.IsSell())
           {order.Buyer=orderToRecord.Buyer;}
           else
           {order.Seller=orderToRecord.Seller;}
           executedOrder.AddCounterPartyTrade(order);
       }
       marketToRecordIn.RecordTrade(executedOrder);
       //Record CounterParty Orders as their own Market Trades
       foreach (var order in counterPartyOrders)
        {
            var counterPartyTrade = new Execution(order, order.Buyer,order.Seller,order.FilledQuantity,order.Price, tradingPeriod);
            counterPartyTrade.AddCounterPartyTrade(orderToRecord);
            marketToRecordIn.RecordTrade(counterPartyTrade);
        }
    }
}