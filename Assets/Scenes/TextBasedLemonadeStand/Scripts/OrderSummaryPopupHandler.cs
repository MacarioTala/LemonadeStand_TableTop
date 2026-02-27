using System;
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
        LocalMarket = market;
        Debug.Log("Showing Order Summary"+OrderSummaryPanel.activeInHierarchy);
                var player = LocalMarket.GetMarketParticipants().FirstOrDefault(x=>x.Name=="Player1");
                var orders = LocalMarket.GetOrdersSentToMarket()
                    .Where(x=>x !=null && (Equals(player,x.Buyer) || Equals(player,x.Seller)));
                var panelText = OrderSummaryText.GetComponent<TextMeshProUGUI>();
                panelText.text = "";

                panelText.text += "Orders";
                panelText.text += "\n";
                panelText.text += "----------------";
                panelText.text += "\n";
                
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
                OrderSummaryPanel.SetActive(true);  
    }

    public void CloseOrderSummary()
    {
        OrderSummaryPanel.SetActive(false);
    }  

}
