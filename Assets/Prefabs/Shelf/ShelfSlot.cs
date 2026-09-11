using UnityEngine;

public class ShelfSlot : MonoBehaviour
{
    private ShelfItem _item;

    private void Awake()
    {
        _item = transform.Find("ShelfItem").GetComponent<ShelfItem>();
    }
    public void Clear()
    =>_item.Clear();

    public InventoryEntry GetContents()
        => _item.GetItem();
    
    public bool IsEmpty()
        => _item.IsEmpty;

    public void StoreItem(InventoryEntry entry)
        =>_item.PutItemInShelf(entry);
}
