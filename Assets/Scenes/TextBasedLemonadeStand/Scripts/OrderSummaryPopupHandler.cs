using System;
using TMPro;
using Unity.VisualScripting.YamlDotNet.Core;
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
        try{
                var orders = LocalMarket.GetOrdersSentToMarket();
                var panelText = OrderSummaryText.GetComponent<TextMeshProUGUI>();
                panelText.text = "";

                panelText.text += "Orders";
                panelText.text += "\n";
                panelText.text += "----------------";
                panelText.text += "\n";
                foreach(var order in orders){
                    var line = new OrderSummaryLineItem{
                        GoodName = order.Good.ToString(),
                        Quantity = order.Quantity,
                        Price = order.Price
                    };
                    panelText.text += line.ToString();
                    panelText.text += "\n";
                }
                OrderSummaryPanel.SetActive(true);
            }
        catch(Exception e){
            Debug.Log(e);
        }
        
    }

    public void CloseOrderSummary()
    {
        OrderSummaryPanel.SetActive(false);
    }  

}
