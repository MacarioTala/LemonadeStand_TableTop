using System.Collections.Generic;
using UnityEngine;

public class InventoryPanelUI : MonoBehaviour
{
    [SerializeField] private Transform container;       // parent with VerticalLayoutGroup
    [SerializeField] private InventoryItemUI itemPrefab;

    private readonly List<InventoryItemUI> activeItems = new();

    public void ShowInventory(Dictionary<Sprite, int> inventory)
    {
    
        foreach (var item in activeItems)
            Destroy(item.gameObject);
        activeItems.Clear();

        // Spawn new
        foreach (var kvp in inventory)
        {
            var itemUI = Instantiate(itemPrefab, container);
            itemUI.SetItem(kvp.Key, kvp.Value);
            activeItems.Add(itemUI);
        }
    }
}
