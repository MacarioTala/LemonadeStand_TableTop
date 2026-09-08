using System;
using System.Linq;
using UnityEngine;

public class Shelf : MonoBehaviour
{
    private ShelfSlot[] _shelfSlots; 
    private void Awake()
    {
        _shelfSlots = GetComponentsInChildren<ShelfSlot>();
    }
    public void PlaceOnShelf(InventoryEntry entry)
    {
        var emptySlot = FindFirstEmptySlot();
        if(emptySlot != null)
            emptySlot.ReplaceContents(entry);
        else
            Debug.Log("Shelf is full!");
    }

    private ShelfSlot FindFirstEmptySlot()
        => _shelfSlots.FirstOrDefault(x=>x.IsEmpty());
}
