using System.Linq;
using UnityEngine;

public class Fridge : MonoBehaviour
{
    FridgeSlot[] _slots;
    private void Awake()
    {
        _slots = GetComponentsInChildren<FridgeSlot>(true);
    }

    public void PlaceInFridge(InventoryEntry entry)
    {
        var slotToFill = DoesASlotAlreadyContainThis(entry.good.GoodName)?? FindFirstEmptySlot();

        if(slotToFill != null)
            slotToFill.StoreItem(entry);
        else
            Debug.Log("Shelf is full!");
    }

    private FridgeSlot DoesASlotAlreadyContainThis(string goodName)
    => _slots.FirstOrDefault(x=>x.GetContents()?.good.GoodName==goodName);

    private FridgeSlot FindFirstEmptySlot() 
    {
        var retval=_slots.FirstOrDefault(x=>x.IsEmpty());
        return retval;
    }
}
