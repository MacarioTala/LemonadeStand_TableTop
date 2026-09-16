using System.Linq;
using UnityEngine;

public class Shelf : MonoBehaviour
{
    private Inventory playerInventory;
    private ShelfSlot[] _shelfSlots; 
    
    public void Initialize(Inventory inventory)
    {
        _shelfSlots = GetComponentsInChildren<ShelfSlot>();
        playerInventory=inventory;
        foreach(var slot in _shelfSlots) 
            slot.Initialize(playerInventory);
    }
    public void PlaceOnShelf(InventoryEntry entry)
    {
        var slotToFill = DoesASlotAlreadyContainThis(entry.good.GoodName)?? FindFirstEmptySlot();

        if(slotToFill != null)
            slotToFill.StoreItem(entry);
        else
            Debug.Log("Shelf is full!");
    }

    private ShelfSlot DoesASlotAlreadyContainThis(string good)
        =>_shelfSlots.FirstOrDefault(x=> x.GetContents()?.good.GoodName==good);

    private ShelfSlot FindFirstEmptySlot()
        => _shelfSlots.FirstOrDefault(x=>x.IsEmpty());
}
