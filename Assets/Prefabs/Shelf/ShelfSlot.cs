using UnityEngine;
using UnityEngine.UI;

public class ShelfSlot : MonoBehaviour
{
    private ShelfItem _item;

    private void Awake()
    {
        _item = transform.Find("ShelfItem").GetComponent<ShelfItem>();
    }
    public void Clear()
    {
        _item.gameObject.SetActive(false);
        _item = null;
    }
    public InventoryEntry GetContents()
        => _item.GetItem();
    
    public bool IsEmpty()
        => _item.IsEmpty;

    public void ReplaceContents(InventoryEntry entry)
        =>_item.ReplaceInventoryEntry(entry);
}
