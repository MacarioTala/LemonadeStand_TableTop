using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.UI;

public class Suitcase : MonoBehaviour
{
    [SerializeField] Button Lid;
    [SerializeField] GameObject ElementDelivery;
    public event Action<InventoryEntry> BuyRequested;
    private DeliverySlot[] slots;

    private void Awake()
    {
        slots = GetComponentsInChildren<DeliverySlot>(true);
        Lid.onClick.AddListener(Close);

        foreach(var slot in slots)
            slot.BuyRequested += OnBuyRequested;
    }

    public void BuyFailed(InventoryEntry entry)
    {
        
    }
    public void BuySucceeded(InventoryEntry entry)
        => FindSlot(entry).BuySucceeded();
    
    private DeliverySlot FindSlot(InventoryEntry entry)
        => slots.FirstOrDefault(x=>x._entry==entry);

    private void OnBuyRequested(InventoryEntry entry)
    {
        BuyRequested?.Invoke(entry);
    }

    public void Close() => ElementDelivery.SetActive(false);
    
    public void Display(List<InventoryEntry> entries)
    {
        for(var i=0;i<entries.Count;i++)
            slots[i].Display(entries[i]);
    }
}