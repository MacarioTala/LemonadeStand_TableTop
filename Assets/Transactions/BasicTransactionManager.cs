using System.Linq;

public class BasicTransactionManager : iTransactionManager
{
    private readonly DummyCompany dummyCorp = new();
    internal bool HasGood(Good good, int quantity,Inventory inventory)
    {
        var goods = inventory.GetInventoryEntries();
        var good_in_inventory = goods.Find(item=> item.good.good_name == good.good_name);
        return good_in_inventory != null && good_in_inventory.quantity >= quantity;
    }

    public void ProcessTransaction(ActionContext context)
    {
        var buyer = context.Buyer??dummyCorp;
        var seller = context.Seller??dummyCorp;
        var sellerInventory = seller.GetInventory();
        var buyerInventory = buyer.GetInventory();
        var buyerCash = buyer.GetCash();
        var sellerCash = seller.GetCash();
        var good = context.GoodToBuy;
        var quantity = context.Quantity;
        var price = context.Price;
        var totalCost = context.Quantity * context.Price;
        var tradingPeriod = context.Period;

        if (!(buyer.GetCash()>=totalCost))
        {
            throw new Company_InsufficientFundsException("Insufficient funds to buy good");
        }

        if (!HasGood(good, quantity,sellerInventory))
        {
            throw new Company_InventoryException("Company does not have enough of the good to sell");
        }
        //Adjust cash balances
        buyerCash -= totalCost;
        sellerCash += totalCost;
        buyer.SetCash(buyerCash);
        seller.SetCash(sellerCash);

        //Adjust inventories
        if(seller is Market marketSeller)
        {
            var inventoryEntryForTrade = new InventoryEntry(good, quantity, price,tradingPeriod);
            var trade = new MarketTrade(inventoryEntryForTrade, tradingPeriod,TradeType.Sell);
            marketSeller.RecordTrade(trade);
        }
        sellerInventory.RemoveGood(good, quantity, price);
        
        if(buyer is Market)
        {   
            if(buyerInventory.GetInventoryEntriesByGood(good.good_name).Count > 0)
            {   
                var inventoryEntryToModify=buyerInventory.GetInventoryEntriesByGood(good.good_name).First();
                inventoryEntryToModify.quantity += quantity;
            }
            else
            {
                buyerInventory.AddGood(new InventoryEntry(good, quantity, price,tradingPeriod));
            }
            var inventoryEntry = new InventoryEntry(good, quantity, price,tradingPeriod); 
            var trade = new MarketTrade(inventoryEntry, tradingPeriod,TradeType.Buy);   
            (buyer as Market).RecordTrade(trade);
        }
        
    }
}