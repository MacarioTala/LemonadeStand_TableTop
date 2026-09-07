using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Suitcase : MonoBehaviour
{
    [SerializeField] Button Lid;
    [SerializeField] GameObject ElementDelivery;
    private DeliverySlot[] slots;

    private void Awake()
    {
        slots = GetComponentsInChildren<DeliverySlot>(true);
        Lid.onClick.AddListener(Close);
    }

    public void Close() => ElementDelivery.SetActive(false);
    
    public void Display(List<InventoryEntry> entries)
    {
        for(var i=0;i<entries.Count;i++)
            slots[i].Display(entries[i]);
    }
}