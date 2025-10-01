using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Sandbox; // Add this using directive

public class PopulationView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI populationText;
    [SerializeField] private TextMeshProUGUI ennuiText;
    [SerializeField] private InventoryPanelUI inventoryPanel; // assign the inventory panel in inspector
    [SerializeField] private GoodIconMapping iconMapping; // assign the good icon mapping in inspector

    private PopulationAgent populationAgent;
    private List<Good> availableGoods;
    private PopulationDisplayInfo populationDisplayInfo; // Add this field

    public void Init(PopulationAgent agent, List<Good> goods, GoodIconMapping mapping)
    {
        populationAgent = agent;
        availableGoods = goods;
        iconMapping = mapping;
        populationDisplayInfo = new PopulationDisplayInfo(populationAgent, availableGoods); // Initialize the display info
        
        UpdatePopulationDisplay();
        UpdateEnnuiDisplay();
        UpdateInventoryPanel();
    }

    private void Update()
    {
        if (populationAgent != null)
        {
            UpdatePopulationDisplay();
            UpdateEnnuiDisplay();
            UpdateInventoryPanel();
        }
    }

    private void UpdatePopulationDisplay()
    {
        if (populationText != null)
        {
            populationText.text = "Population: " + populationAgent.Population.ToString();
        }
    }

    private void UpdateEnnuiDisplay()
    {
        if (ennuiText != null)
        {
            // Display ennui as a percentage
            ennuiText.text = "Ennui: " + (populationAgent.Ennui * 100f).ToString("F1") + "%";
        }
    }

    private void UpdateInventoryPanel()
    {
        if (inventoryPanel == null) return;

        // Get population inventory data using PopulationDisplayInfo
        var stock = populationDisplayInfo.Stock;

        // Convert to the format expected by InventoryPanelUI (Dictionary<Sprite, int>)
        var inventoryForUI = new Dictionary<Sprite, int>();
        foreach (var item in stock)
        {
            // Use the icon mapping to get the appropriate sprite for each good
            Sprite spriteToUse = null;
            if (iconMapping != null)
            {
                spriteToUse = iconMapping.GetSpriteForGood(item.Key); // item.Key is the good name (string)
            }
            
            // If no sprite is found, use a default white texture
            if (spriteToUse == null)
            {
                Debug.LogWarning($"No sprite found for good: {item.Key}. Using default sprite.");
                spriteToUse = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
            }
            
            inventoryForUI[spriteToUse] = item.Value; // item.Value is the quantity
        }

        inventoryPanel.ShowInventory(inventoryForUI);
    }
}
