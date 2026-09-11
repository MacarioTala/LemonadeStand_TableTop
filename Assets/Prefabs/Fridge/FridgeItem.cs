using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FridgeItem : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler
{
    private Image _image;
    private InventoryEntry _entry;
    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private Vector2 _originalPosition;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _image = GetComponent<Image>();
        _canvas = GetComponent<Canvas>();
        _canvasGroup= GetComponent<CanvasGroup>();

        _image.enabled=false;
    }
    public void Clear()
    {
        _entry = null;
        _image.enabled=false;
        _image.sprite=null;
    }

    public bool IsEmpty()=>_entry is null;
    public InventoryEntry GetItem() =>_entry;
    public void PlaceItem(InventoryEntry entry)
    {
        _image.sprite = entry.good.GoodSprite;
        
        if(_entry == null)
            _entry = entry;
        else
            _entry.quantity+=entry.quantity;

        _image.enabled=true;
    }
#region UI Stuff
    public void OnBeginDrag(PointerEventData eventData)
    {
        _originalPosition=_rectTransform.anchoredPosition;
        _canvasGroup.blocksRaycasts=false;
    }

    public void OnDrag(PointerEventData eventData)
        => _rectTransform.anchoredPosition += eventData.delta/_canvas.scaleFactor;

    public void OnEndDrag(PointerEventData eventData)
    {
        _rectTransform.anchoredPosition=_originalPosition;
        _canvasGroup.blocksRaycasts=true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        throw new NotImplementedException();
    }
#endregion
}