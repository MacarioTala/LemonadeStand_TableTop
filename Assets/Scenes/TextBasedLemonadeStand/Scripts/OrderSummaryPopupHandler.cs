using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderSummaryPopupHandler : MonoBehaviour
{
    [SerializeField] private GameObject OrderSummaryPanel;
    [SerializeField] private GameObject OrderSummaryText;
    [SerializeField] private GameObject CloseButton;
    private Market LocalMarket;

    void Start()
    {
        CloseButton.GetComponent<Button>().onClick.AddListener(CloseOrderSummary);
    }

    public void ShowOrderSummary(Market market)
    {
        Debug.Log("Showing Order Summary"+OrderSummaryPanel.activeInHierarchy);
        
        LocalMarket = market;
        var panelText = OrderSummaryText.GetComponent<TextMeshProUGUI>();
                panelText.text = "";
                panelText.text += "Orders";
                panelText.text += "\n";
                panelText.text += "----------------";
                panelText.text += "\n";

        if(LocalMarket != null)
        {
            var player = LocalMarket.GetMarketParticipants().FirstOrDefault(x=>x.Name=="Player1");
            var orders = LocalMarket.GetOrdersSentToMarket()
                .Where(x=>x !=null && (Equals(player,x.Buyer) || Equals(player,x.Seller)));
            if(orders.Any())
                {
                    foreach(var order in orders)
                    {
                        var line = new OrderSummaryLineItem{
                            GoodName = order.Good.ToString(),
                            Quantity = order.Quantity,
                            Price = order.Price
                        };
                        panelText.text += line.ToString();
                        panelText.text += "\n";
                }
            }
        }
        else
        {
            panelText.text += "No backend detected";   
        }    
                OrderSummaryPanel.SetActive(true);  
    }

    public void CloseOrderSummary()
    {
        OrderSummaryPanel.SetActive(false);
    }  

}
