using System.Linq;

public class BasicTransactionManager : iTransactionManager
{
    private readonly DummyCompany dummyCorp = new();
    internal bool HasGood(Good good, int quantity,Inventory inventory)
    {
        var goodInInventory = inventory.GetInventoryEntriesByGood(good.good_name).FirstOrDefault();
        return goodInInventory != null && goodInInventory.quantity >= quantity;
    }

    public void ProcessTransaction(ActionContext context)
    {
        var buyer = context.TradeToSubmit.Buyer ?? dummyCorp;
        var seller = context.TradeToSubmit.Seller ?? dummyCorp;
        var good = context.TradeToSubmit.Good;
        var quantity = context.TradeToSubmit.IsPartiallyFilled
                            ?context.TradeToSubmit.FilledQuantity
                            :context.TradeToSubmit.Quantity;
        var price = context.TradeToSubmit.Price;

        var sellerInventory = seller.GetInventory();
        var buyerInventory = buyer.GetInventory();
        
        var totalCost = quantity * price;
        var tradingPeriod = context.Period;

        //Validate transaction
        if (buyer.GetCash() < totalCost)
        {
            throw new Company_InsufficientFundsException("Insufficient funds to buy good");
        }

        if (!HasGood(good, quantity, sellerInventory))
        {
            throw new Company_InventoryException("Company does not have enough of the good to sell");
        }
        //Adjust cash balances
        buyer.SetCash(buyer.GetCash() - totalCost);
        seller.SetCash(seller.GetCash() + totalCost);

        //Adjust inventories
        buyerInventory.AddGood(new InventoryEntry(good, quantity, price, tradingPeriod));
        sellerInventory.RemoveGood(good, quantity, price);

        //Record trade
        context.TradeToSubmit.FilledQuantity = quantity;
        RecordTrade(context.TradeToSubmit,context.MarketToSubmitTo, tradingPeriod);
    }

    private static void RecordTrade(Order tradeToRecord, Market marketToRecordIn, int tradingPeriod)
    {
       var executedTrade = new MarketTrade(tradeToRecord, tradingPeriod);
       marketToRecordIn.RecordTrade(executedTrade);
    }
}