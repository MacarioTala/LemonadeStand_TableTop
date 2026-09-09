using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
[RequireComponent(typeof(Image))]

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class ShelfItem : MonoBehaviour,
                         IBeginDragHandler,
                         IDragHandler,
                         IEndDragHandler
{
    private Image _image;
    private InventoryEntry _entry;
    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    private ShelfSlot _slot;
    private RectTransform _rectTransform;
    private Vector2 _originalPosition;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _slot = GetComponentInParent<ShelfSlot>();

        _image = GetComponent<Image>();
        _image.gameObject.SetActive(false);
    }

    public InventoryEntry GetItem() =>_entry;
    public bool IsEmpty => _entry is null;

    public void ReplaceInventoryEntry (InventoryEntry entry)
    { 
        _image.sprite = entry.good.GoodSprite;
        _entry = entry;
        _image.gameObject.SetActive(true);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _originalPosition = _rectTransform.anchoredPosition;
        _canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    => _rectTransform.anchoredPosition += eventData.delta/_canvas.scaleFactor;

    public void OnEndDrag(PointerEventData eventData)
    {
        _rectTransform.anchoredPosition = _originalPosition;
        _canvasGroup.blocksRaycasts = true;
    }
}
