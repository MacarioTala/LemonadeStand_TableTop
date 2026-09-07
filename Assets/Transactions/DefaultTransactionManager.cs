using System.Collections.Generic;
using System.Linq;
using static HistoricalRecordHelper;

public class DefaultTransactionManager : iTransactionManager,iMarketAware
{
    Market _market;
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
                .AddInventoryEntry(new InventoryEntry(primaryOrder.Good, 
                    primaryOrder.RemainingQuantity, 
                    primaryOrder.Price,
                    context.Period));
        }
        //Record the orders
        RecordOrder(primaryOrder, context.MarketToSubmitTo, context.Period);
        foreach(var counterPartyOrder in counterPartyOrders)
        {
            RecordOrder(counterPartyOrder, context.MarketToSubmitTo, context.Period);
            ///remove this when we refactor iConsumptionManager to remove .FulfillDemand()
#region RemoveThisPartAfterFulfillDemandRefactor
            if(counterPartyOrder.SubmittingCompany is Market)
            {
                context.MarketToSubmitTo.LogOrder(counterPartyOrder, context.Period);
            }
#endregion
        }
        
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
        foreach(var counterPartyOrder in counterPartyOrders)
        {
            if (primaryOrder.IsFullyFilled)
            {
                break;
            }

            var processTransactionResult = ProcessTransactionPair(primaryOrder,counterPartyOrder,context.Period);
            if (!processTransactionResult.Equals(LemonadeStandResultObject.Success()))
            {
                primaryOrder.OrderStatus = processTransactionResult;
                return primaryOrder.OrderStatus;
            }
            _counterPartyOrdersToRecord.Add(counterPartyOrder);
            //Record CounterParty Order
            RecordOrder(counterPartyOrder, context.MarketToSubmitTo, context.Period);
        }
       
        //Record the primary order
        RecordOrder(primaryOrder, context.MarketToSubmitTo, context.Period);
        
        return LemonadeStandResultObject.Success();
    }

    internal LemonadeStandResultObject ProcessTransactionPair(Order primaryOrder, Order counterPartyOrder,int period)
    {
        // var buyer= primaryOrder.Buyer;
        // var seller = counterPartyOrder.Seller;

        var buyer = primaryOrder.Buyer ?? counterPartyOrder.Buyer;
        var seller = counterPartyOrder.Seller??primaryOrder.Seller;
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
        buyer.SetCash(buyer.GetCash() - costOfThisLeg);
        seller.SetCash(seller.GetCash() + costOfThisLeg);

        //Adjust inventories
        var adjustedPeriod = period;
        if(good.DeliveryDelay>0)
            adjustedPeriod+=good.DeliveryDelay;
        else
            adjustedPeriod+=good.DeliveryDelay+1;//Goods should count as being acquired when they arrive, which is always the next turn
        
        buyerInventory.AddInventoryEntry(new InventoryEntry(good, quantity, price, adjustedPeriod));
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
        //Log to journal
        var primaryRecord = CreateLocalHistoricalRecord(_market,primaryOrder);
        JournalSuccessEntry(_market,primaryOrder,primaryRecord);
        var counterPartyRecord = CreateLocalHistoricalRecord(_market,counterPartyOrder);
        JournalSuccessEntry(_market,counterPartyOrder,counterPartyRecord);
        

        //raise event
         _market.RaiseOrderFulfilledEvent(
            new OrderFulfilledEvent
            {
                Good = primaryOrder.Good,
                FulfilledQuantity = primaryOrder.FilledQuantity,
                OriginalQuantity = primaryOrder.Quantity,
                FillPrice = price,
                OrderMarket = _market,
                Period = _market.CurrentPeriod,
                PrimaryOrder = primaryOrder,
                CounterPartyOrders = new List<Order>{counterPartyOrder}
            }
        );

        return LemonadeStandResultObject.Success();
    }

    internal LemonadeStandResultObject ValidateTransaction(Order primaryOrder, Order counterPartyOrder,decimal costOfThisLeg)
    {
        var buyer= primaryOrder.Buyer??counterPartyOrder.Buyer;
        var seller = counterPartyOrder.Seller??primaryOrder.Seller;

        var sellerInventory = seller.GetInventory();
        var record = CreateLocalHistoricalRecord(_market,primaryOrder);

        //Validate transaction
        if (
            (primaryOrder.SubmittingCompany == counterPartyOrder.SubmittingCompany)
                ||
            (buyer == seller)
            )
        {
            var msg = "Company cannot trade with itself";
            primaryOrder.OrderStatus = LemonadeStandResultObject.Failure(ResultTypeEnum.SelfTrade,msg);
            JournalRejectEntry(_market,primaryOrder,record,msg,ResultTypeEnum.SelfTrade);
            return primaryOrder.OrderStatus;
        }

        if (buyer.GetCash() < costOfThisLeg)
        {
            var msg = "Buyer does not have enough cash to complete the transaction";
            primaryOrder.OrderStatus = LemonadeStandResultObject.Failure
                (ResultTypeEnum.InsufficientCash,msg);
            JournalRejectEntry(_market,primaryOrder,record,ResultTypeEnum.InsufficientCash.ToString(),ResultTypeEnum.InsufficientCash);
            return primaryOrder.OrderStatus;
        }

        if (!HasGood(counterPartyOrder.Good, sellerInventory))
        {
            var msg = $"Seller must have at least 1 quantity of {counterPartyOrder.Good} to trade";
            record = CreateLocalHistoricalRecord(_market,counterPartyOrder);
            counterPartyOrder.OrderStatus = LemonadeStandResultObject.Failure
                (ResultTypeEnum.InsufficientGoods,msg);
            JournalRejectEntry(_market,counterPartyOrder,record,msg,ResultTypeEnum.InsufficientGoods);
            return counterPartyOrder.OrderStatus;
        }

        return LemonadeStandResultObject.Success();
    }
    
    internal static LemonadeStandResultObject RecordOrder(Order orderToRecord, Market marketToRecordIn, int tradingPeriod)
    {
        return marketToRecordIn.RecordOrderInPeriod(orderToRecord, tradingPeriod);
    }

    internal static void RecordTransaction(Order orderToRecord, Market marketToRecordIn
            , int tradingPeriod,List<Order> counterPartyOrders)
    {  
       //Record Primary Order
       if ( counterPartyOrders.Count == 1)
         {
            var counterPartyOrder = counterPartyOrders[0];
            
            if (counterPartyOrder.IsSell())
            { orderToRecord.Seller = counterPartyOrder.Seller; }
            else
            { orderToRecord.Buyer = counterPartyOrder.Buyer; }
         }
        var executedOrder = new Execution(orderToRecord,orderToRecord.Buyer,orderToRecord.Seller,orderToRecord.FilledQuantity,orderToRecord.Price, tradingPeriod);
       //Add CounterParty Orders to Primary Order
       foreach (var counterPartyOrder in counterPartyOrders)
       {
           if (counterPartyOrder.IsSell())
           {
            counterPartyOrder.Buyer=orderToRecord.Buyer;
            executedOrder.Seller=counterPartyOrder.Seller;
           }
           else
           {
            counterPartyOrder.Seller=orderToRecord.Seller;
            executedOrder.Buyer=counterPartyOrder.Buyer;
           }
           executedOrder.AddCounterPartyTrade(counterPartyOrder);
       }
       marketToRecordIn.RecordExecution(executedOrder);
       //Record CounterParty Orders as their own Market Trades
       foreach (var order in counterPartyOrders)
        {
            var counterPartyExecution = new Execution(order, order.Buyer,order.Seller,order.FilledQuantity,order.Price, tradingPeriod);
            counterPartyExecution.AddCounterPartyTrade(orderToRecord);
            marketToRecordIn.RecordExecution(counterPartyExecution);
        }
    }

    public void SetMarket(Market market)=> _market = market;
}