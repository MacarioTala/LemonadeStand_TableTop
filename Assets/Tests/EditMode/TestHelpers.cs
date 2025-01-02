using System.Collections.Generic;
using System.Text;

public class TestHelpers
{
    #region Helper methods
public string ListToString(List<InventoryEntry> inventory)
{
    var sb = new StringBuilder();
    foreach (var item in inventory)
    {
        sb.AppendLine($"Good: {item.good.name}, Quantity: {item.quantity}, Price: {item.Cost}");
    }
    return sb.ToString();
}

public static ActionContext CreateActionContext(Order order, Market market,int period)
{
    return new ActionContext()
    {
        TradeToSubmit = order,
        MarketToSubmitTo = market,
        Period = period
    };
}
#endregion
}
