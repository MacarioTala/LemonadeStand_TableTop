using System.Collections.Generic;
using System.Linq;

public class BasicTransactionManager : iTransactionManager
{
    internal bool HasGood(Good good, int quantity,Inventory inventory)
    {
        var goodInInventory = inventory.GetInventoryEntriesByGood(good.good_name).FirstOrDefault();
        return goodInInventory != null && goodInInventory.quantity >= quantity;
    }

    public void ProcessPairedOrders(ActionContext context)
    {
        var primaryOrder = context.TradeToSubmit;
        var counterPartyOrders = context.CounterPartyOrders;
        var _counterPartyOrdersToRecord = new List<Order>();
        foreach(var order in counterPartyOrders)
        {
            if (primaryOrder.IsFullyFilled)
            {
                break;
            }

            ProcessTransaction(primaryOrder,order,context.Period);
            _counterPartyOrdersToRecord.Add(order);
        }
        //Record trade
        RecordTrade(context.TradeToSubmit,context.MarketToSubmitTo, context.Period,_counterPartyOrdersToRecord);
    }

    internal LemonadeStandResultObject ProcessTransaction(Order primaryOrder, Order counterPartyOrder,int period)
    {
        var buyer= primaryOrder.Buyer;
        var seller = counterPartyOrder.Seller;

        var good = primaryOrder.Good;
        var price = counterPartyOrder.Price;

        var buyerInventory = buyer.GetInventory();
        var sellerInventory = seller.GetInventory();

        var quantity = counterPartyOrder.Quantity>=primaryOrder.RemainingQuantity
            ?primaryOrder.RemainingQuantity
            :counterPartyOrder.Quantity;

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
        primaryOrder.FilledQuantity = quantity;
        primaryOrder.OrderStatus = primaryOrder.IsFullyFilled
            ? LemonadeStandResultObject.Success()
            : LemonadeStandResultObject.Failure(
                ResultTypeEnum.PartialFill, "Order partially filled"
                                            );
    
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
                (ResultTypeEnum.InsufficientFunds,"Buyer does not have enough cash to complete the transaction");
            return primaryOrder.OrderStatus;
        }

        if (!HasGood(counterPartyOrder.Good, counterPartyOrder.Quantity, sellerInventory))
        {
            counterPartyOrder.OrderStatus = LemonadeStandResultObject.Failure
                (ResultTypeEnum.InsufficientGoods,"Seller does not have enough goods to complete the transaction");
            return counterPartyOrder.OrderStatus;
        }

        return LemonadeStandResultObject.Success();
    }
    public void ProcessTransaction(ActionContext context)
    {
      throw new System.NotImplementedException("Refactoring to ProcessPairedOrders!");   
    }

    internal static void RecordTrade(Order tradeToRecord, Market marketToRecordIn
            , int tradingPeriod,List<Order> counterPartyOrders)
    {
       var executedTrade = new MarketTransaction(tradeToRecord, tradingPeriod);
       foreach (var order in counterPartyOrders)
       {
           executedTrade.AddCounterPartyTrade(order);
       }
       marketToRecordIn.RecordTrade(executedTrade);
    }
}