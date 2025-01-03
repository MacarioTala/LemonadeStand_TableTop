using System.Collections.Generic;
using System.Linq;

public class BasicTransactionManager : iTransactionManager
{
    internal bool HasGood(Good good, Inventory inventory)
    {
        var goodInInventory = inventory.GetInventoryEntriesByGood(good.good_name).FirstOrDefault();
        return goodInInventory != null && goodInInventory.quantity >= 1;
    }

    public LemonadeStandResultObject ProcessTransaction(ActionContext context)
    {
        var processOrderResult = ProcessPairedOrders(context);   
        if (!processOrderResult.Result.Equals(LemonadeStandResultObject.Success().Result))
            return processOrderResult;
        
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
        //You are here -- Once the Primary Order is filled, this exits
        //We should do something about the remaining unfilled orders
        //Record trade
        RecordTrade(context.PrimaryOrder,context.MarketToSubmitTo, context.Period,_counterPartyOrdersToRecord);
        foreach (var order in _counterPartyOrdersToRecord)
        {RecordTrade(order,context.MarketToSubmitTo, context.Period,new List<Order>{context.PrimaryOrder});}
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
        if(primaryOrder.FilledQuantity == 0)
            primaryOrder.FilledQuantity = quantity;
        else
            primaryOrder.FilledQuantity += quantity;

        primaryOrder.OrderStatus = primaryOrder.IsFullyFilled
            ? LemonadeStandResultObject.Success()
            : LemonadeStandResultObject.Failure(
                ResultTypeEnum.PartialFill, "Order partially filled"
                                            );
        if(counterPartyOrder.FilledQuantity == 0)
            counterPartyOrder.FilledQuantity = quantity;
        else
            counterPartyOrder.FilledQuantity += quantity;
    
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
    

    internal static void RecordTrade(Order tradeToRecord, Market marketToRecordIn
            , int tradingPeriod,List<Order> counterPartyOrders)
    {
       var executedTrade = new MarketTransaction(tradeToRecord, tradingPeriod);
       if ( counterPartyOrders.Count == 1)
         {
            var counterPartyOrder = counterPartyOrders[0];
            var counterPartyOrderIsSeller = counterPartyOrder.Seller != null 
                && counterPartyOrder.SubmittingCompany.Equals(counterPartyOrder.Seller);

            if (counterPartyOrderIsSeller)
            { tradeToRecord.Seller = counterPartyOrder.Seller; }
            else
            { tradeToRecord.Buyer = counterPartyOrder.Buyer; }
         }
       foreach (var order in counterPartyOrders)
       {
           executedTrade.AddCounterPartyTrade(order);
       }
       marketToRecordIn.RecordTrade(executedTrade);
    }
}