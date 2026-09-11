using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FridgeSlot : MonoBehaviour
{
   Image _image;
   FridgeItem _item;
    private void Awake()
    {
        _item = transform.Find("Item").GetComponent<FridgeItem>();
        _image = GetComponent<Image>();
        _image.enabled=false;
    }

    public bool IsEmpty()
        =>_item.IsEmpty();

    public InventoryEntry GetContents() => _item.GetItem();
    public void StoreItem(InventoryEntry entry)
    => _item.PlaceItem(entry);
}
