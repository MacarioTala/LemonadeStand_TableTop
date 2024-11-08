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
        sb.AppendLine($"Good: {item.good.name}, Quantity: {item.quantity}, Price: {item.acquisition_price}");
    }
    return sb.ToString();
}
#endregion
}
