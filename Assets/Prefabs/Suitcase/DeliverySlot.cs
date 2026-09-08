using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class DeliverySlot:  MonoBehaviour,
                            IPointerEnterHandler,
                            IPointerExitHandler
{
    public event Action<InventoryEntry> BuyRequested;
    private Image _elementImage;
    private TextMeshProUGUI _priceTag;
    public InventoryEntry _entry;

    [SerializeField] private Button _buyButton;
    [SerializeField] private GameObject _hoverPanel;
    [SerializeField] private TextMeshProUGUI _toolTip;

    private void Awake()
    {
        _elementImage = transform.Find("Element").GetComponent<Image>();
        
        _priceTag = transform.Find("PriceTagText").GetComponent<TextMeshProUGUI>();

        //Listeners
        _buyButton.onClick.AddListener(RequestBuy);
    }

    public void BuySucceeded()
        => gameObject.SetActive(false);
    public void RequestBuy()
        =>BuyRequested?.Invoke(_entry);

    public void Display(InventoryEntry entry)
    {
        _entry = entry;
        _elementImage.sprite=entry.good.GoodSprite;
        _toolTip.text=entry.good.Tooltip;
        _priceTag.text=GetPriceTagFromPriceAndQuantity(entry.PriceOfGood,entry.quantity);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hoverPanel.transform.SetAsLastSibling();
        _hoverPanel.SetActive(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        _hoverPanel.SetActive(false);
    }
     private string GetPriceTagFromPriceAndQuantity(int priceOfGood, int quantity)
        => $"{quantity} for {priceOfGood*quantity}";
}