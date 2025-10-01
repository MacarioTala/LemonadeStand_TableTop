using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Sandbox
{
    public class CompanyView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Image moneyBarFill; // assign the green fill image in inspector
        [SerializeField] private InventoryPanelUI inventoryPanel; // assign the inventory panel in inspector
        [SerializeField] private GoodIconMapping iconMapping; // assign the good icon mapping in inspector
        [SerializeField] private GameObject moneyIndicatorPrefab; // assign the money indicator prefab in inspector

        private EconAgent company;
        private List<Good> availableGoods;
        private Market market;

        public void Init(EconAgent econAgent, List<Good> goods, GoodIconMapping mapping, GameObject moneyIndicator)
        {
            company = econAgent;
            availableGoods = goods;
            iconMapping = mapping;
            moneyIndicatorPrefab = moneyIndicator;
            nameText.text = company.Name;

            // Get reference to the market
            market = company.GetMarket();
            
            // Subscribe to the OrderFulfilled event
            if (market != null)
            {
                Debug.Log($"Subscribing to OrderFulfilled event for market: {market.Name}");
                market.OrderFulfilled += OnOrderFulfilled;
            }
            else
            {
                Debug.LogError("Market is null, cannot subscribe to OrderFulfilled event");
            }

            UpdateMoneyBar();
            UpdateInventoryPanel();
        }

        private void Update()
        {
            if (company != null)
            {
                UpdateMoneyBar();
                UpdateInventoryPanel();
            }
        }

        private void UpdateMoneyBar()
        {
            float maxMoney = 10000f;
            // Assume company has Money and MaxMoney
            float ratio = Mathf.Clamp01((float)company.GetCash() / maxMoney);
            moneyBarFill.fillAmount = ratio;
        }

        private void UpdateInventoryPanel()
        {
            if (inventoryPanel == null) return;

            // Get company inventory data
            var companyDisplayInfo = new CompanyDisplayInfo(company, availableGoods);
            var stock = companyDisplayInfo.Stock;

            // Convert to the format expected by InventoryPanelUI (Dictionary<Sprite, int>)
            var inventoryForUI = new Dictionary<Sprite, int>();
            foreach (var item in stock)
            {
                // Use the icon mapping to get the appropriate sprite for each good
                Sprite spriteToUse = null;
                if (iconMapping != null)
                {
                    spriteToUse = iconMapping.GetSpriteForGood(item.Key);
                }
                
                // If no sprite is found, use a default white texture
                if (spriteToUse == null)
                {
                    Debug.LogWarning($"No sprite found for good: {item.Key}. Using default sprite.");
                    spriteToUse = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
                }
                
                inventoryForUI[spriteToUse] = item.Value;
            }

            inventoryPanel.ShowInventory(inventoryForUI);
        }
        
        private void OnOrderFulfilled(OrderFulfilledEvent orderFulfilledEvent)
        {
            // Check if this company is involved in the order
            bool isCompanyInvolved = false;
            bool isBuyOrder = false;
            
            // Check primary order
            if (orderFulfilledEvent.PrimaryOrder.Buyer == company)
            {
                isCompanyInvolved = true;
                isBuyOrder = true;
            }
            else if (orderFulfilledEvent.PrimaryOrder.Seller == company)
            {
                isCompanyInvolved = true;
                isBuyOrder = false;
            }
            
            // Check counter party orders if company is not involved in primary order
            if (!isCompanyInvolved)
            {
                foreach (var counterPartyOrder in orderFulfilledEvent.CounterPartyOrders)
                {
                    if (counterPartyOrder.Buyer == company)
                    {
                        isCompanyInvolved = true;
                        isBuyOrder = true;
                        break;
                    }
                    else if (counterPartyOrder.Seller == company)
                    {
                        isCompanyInvolved = true;
                        isBuyOrder = false;
                        break;
                    }
                }
            }
            
            Debug.Log($"Order fulfilled: Company {company.Name} : {isCompanyInvolved}, Buy Order: {isBuyOrder}");
            // If company is involved, display the indicator
            if (isCompanyInvolved && moneyIndicatorPrefab != null)
            {
                DisplayMoneyIndicator(isBuyOrder);
            }
        }
        
        private void DisplayMoneyIndicator(bool isBuyOrder)
        {
            // Instantiate the money indicator prefab
            GameObject indicator = Instantiate(moneyIndicatorPrefab, transform.position, Quaternion.identity);
            
            // Set the color based on buy/sell order
            SpriteRenderer spriteRenderer = indicator.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = isBuyOrder ? Color.red : Color.green;
            }
            
            // Position the indicator above the company
            indicator.transform.position = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
            
            // Add a simple animation to fade out and move up
            StartCoroutine(FadeOutIndicator(indicator));
        }
        
        private System.Collections.IEnumerator FadeOutIndicator(GameObject indicator)
        {
            SpriteRenderer spriteRenderer = indicator.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Color originalColor = spriteRenderer.color;
                float duration = 1.0f;
                float elapsed = 0f;
                
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                    spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                    
                    // Move the indicator up slightly
                    indicator.transform.position += Vector3.up * Time.deltaTime * 0.5f;
                    
                    yield return null;
                }
            }
            
            // Destroy the indicator after fading out
            Destroy(indicator);
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from the event when the object is destroyed
            if (market != null)
            {
                market.OrderFulfilled -= OnOrderFulfilled;
            }
        }
    }
}
