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
                
                foreach(var order in orders){
                    panelText.text += order.ToString();
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
