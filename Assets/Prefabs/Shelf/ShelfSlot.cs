using UnityEngine;
using UnityEngine.UI;

public class ShelfSlot : MonoBehaviour
{
    private Image _slotImage;
    private InventoryEntry _entry;

    private void Awake()
    {
        _slotImage = transform.Find("ShelfImage").GetComponent<Image>();
        _slotImage.gameObject.SetActive(false);
    }
    public InventoryEntry GetContents()
        => _entry;
    
    public bool IsEmpty()
        => _entry is null;

    public void ReplaceContents(InventoryEntry entry)
    {
        _entry = entry;
        _slotImage.sprite = entry.good.GoodSprite;
        _slotImage.gameObject.SetActive(true);
    }
}
