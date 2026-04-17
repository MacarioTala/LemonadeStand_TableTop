public static class OrderExtensions
{
    public static OrderSnapshot ToOrderSnapshot (this Order order)
    {
        var SnapshotToReturn = new OrderSnapshot(){
            OrderId = order.Id,
            BuyerName = order.Buyer?.Name,
            SellerName = order.Seller?.Name,
            GoodName = order.Good?.GoodName,
            Quantity = order.Quantity,
            Price = order.Price
    };
        return SnapshotToReturn;
    }
}