using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeliverySlot:MonoBehaviour
{
    private Image _elementImage;
    private TextMeshProUGUI _priceTag;

    private void Awake()
    {
        _elementImage = transform.Find("Element").GetComponent<Image>();
        _priceTag = transform.Find("PriceTagText").GetComponent<TextMeshProUGUI>();
    }

    public void Display(InventoryEntry entry)
    {
        _elementImage.sprite=entry.good.GoodSprite;
        _priceTag.text=GetPriceTagFromPriceAndQuantity(entry.PriceOfGood,entry.quantity);
    }

     private string GetPriceTagFromPriceAndQuantity(int priceOfGood, int quantity)
        => $"{quantity} for {priceOfGood*quantity}";
}