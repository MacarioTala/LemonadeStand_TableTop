using System.Linq;
using TMPro;
using UnityEngine;

public class ShelfSlot : MonoBehaviour
{
    private ShelfItem item;
    private TextMeshProUGUI itemQuantity;
    private Inventory playerInventory;

    private void Awake()
    {
        item = transform.Find("ShelfItem").GetComponent<ShelfItem>();
        itemQuantity=transform.Find("ShelfQty").GetComponentInChildren<TextMeshProUGUI>();
    }
    public void Clear()
    =>item.Clear();

    public InventoryEntry GetContents()
        => item.GetItem();
    
    public bool IsEmpty()
        => item.IsEmpty;

    public void StoreItem(InventoryEntry entry)
    {
        item.PutItemInShelf(entry);
        UpdateInventory(entry.good);
    }

    public void Initialize(Inventory inventory)
        => playerInventory = inventory;
    
    private void UpdateInventory(Good good)
    {
        var resultingQuantity = playerInventory.GetInventoryEntries().Where(x=>x.good==good).Sum(x=> x.quantity).ToString();
        itemQuantity.text = resultingQuantity;
    }
    
}
