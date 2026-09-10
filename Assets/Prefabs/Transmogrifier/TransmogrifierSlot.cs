using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TransmogrifierSlot : MonoBehaviour,
                                IDropHandler
{
    private Image _slotImage;
    private InventoryEntry _inventoryEntry;
    public bool IsEmpty => _inventoryEntry == null;

    private void Awake()
    {
        _slotImage = transform.Find("Image").GetComponent<Image>();
        _slotImage.gameObject.SetActive(false);
    }
    public void OnDrop(PointerEventData eventData)
    {
        var shelfItem = eventData.pointerDrag.GetComponent<ShelfItem>();

        if (shelfItem == null || shelfItem.IsEmpty) return;

        AddIngredient(shelfItem);
    }

    private void AddIngredient(ShelfItem shelfItem)
    {
        _inventoryEntry = shelfItem.GetItem();
        _slotImage.sprite = _inventoryEntry.good.GoodSprite;
        _slotImage.gameObject.SetActive(true);
    }

    public InventoryEntry GetInventoryEntry()
        => _inventoryEntry;
}
