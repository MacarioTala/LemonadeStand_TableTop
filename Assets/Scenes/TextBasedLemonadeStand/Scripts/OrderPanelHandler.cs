using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderPanelHandler : MonoBehaviour
{
    [SerializeField] private GameObject ItemDropdown;
    [SerializeField] private GameObject QuantityInput;
    [SerializeField] private Button OrderButton;
    [SerializeField] private Button SummaryButton;
    [SerializeField] private GameObject ArrivingLabel;
    [SerializeField] private TextMeshProUGUI ValueLabel;
    [SerializeField] private GameObject TotalLabel;
    [SerializeField] private TextMeshProUGUI actionCounter;
    [SerializeField] private GameObject OrderSummaryPanel;
    [SerializeField] private GameObject OrderQueuedLabel;
    [SerializeField] private GameObject DetailedOrderPanel;
    [SerializeField] private Button ShowDetailedOrderButton;
    private EconAgent PlayerCompany;
    private Market LocalMarket;
    private List<InventoryEntry> MarketInventoryEntries;
    private TMP_Dropdown dropdown;
    private TextMeshProUGUI OrderConfirmationText;
#region UnityBuiltIns
    public void Start()
    {
        OrderConfirmationText = OrderQueuedLabel.GetComponent<TextMeshProUGUI>();
        OrderConfirmationText.alpha = 0;
        var summaryPanelHandler = OrderSummaryPanel.GetComponent<OrderSummaryPopupHandler>();
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        dropdown = ItemDropdown.GetComponent<TMP_Dropdown>();
        LocalMarket = GetMarket();
        OrderButton.onClick.AddListener(SubmitOrder);
        SummaryButton.onClick.AddListener(() => summaryPanelHandler.ShowOrderSummary(LocalMarket));
        QuantityInput.GetComponent<TMP_InputField>().onValueChanged.AddListener(value => HandleOrderQuantityChange(value));
        MarketInventoryEntries = LocalMarket.GetInventory().GetInventoryEntries();

        if(MarketInventoryEntries.Count>0)
            ValueLabel.text = MarketInventoryEntries[0].Price.ToString();

        InitializePlayer();
        InitializeOrderDropDown();
    }

    private void ShowOrderConfirmation()
    {
        ArrivingLabel.SetActive(true);
        var arrivingText = ArrivingLabel.GetComponent<TextMeshProUGUI>();
        arrivingText.text = "Goods are arriving next turn";
        ResetOrderPanel();
        StartCoroutine(FadeText("Order Queued"));
    }

    private void ResetOrderPanel()
    {
        QuantityInput.GetComponent<TMP_InputField>().text = 0.ToString();
        dropdown.value = 0;
        ValueLabel.text = MarketInventoryEntries[0].Price.ToString();
    }

    private IEnumerator FadeText(string message)
    {
        OrderConfirmationText.text = message;
        const float duration = .5f;
        const float holdTime = 1.5f;
        float elapsedTime = 0;

        while(elapsedTime < duration)
        {
            OrderConfirmationText.alpha = Mathf.Lerp(0, 1, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        OrderConfirmationText.alpha = 1;
        yield return new WaitForSeconds(holdTime);

        elapsedTime = 0;
        while(elapsedTime < duration)
        {
            OrderConfirmationText.alpha = Mathf.Lerp(1, 0, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        OrderConfirmationText.alpha = 0;
    }

    private void HandleOrderQuantityChange(string value)
    {
        int.TryParse(value, out int quantity);
        var selectedEntry = MarketInventoryEntries[dropdown.value];
        var price = selectedEntry.Price;
        var totalText = TotalLabel.GetComponent<TextMeshProUGUI>();
        totalText.text = (price * quantity).ToString();
    }

    private void InitializeOrderDropDown()
    {
        dropdown.onValueChanged.AddListener(HandleOrderSelection);
        dropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> options = new();
        foreach (var entry in MarketInventoryEntries)
        {
            options.Add(new TMP_Dropdown.OptionData(entry.good.GoodName));
        }
        dropdown.AddOptions(options);
        dropdown.RefreshShownValue();
    }

    private void HandleOrderSelection(int selectedIndex)
    {
        var selectedEntry = MarketInventoryEntries[selectedIndex];
        var price = selectedEntry.Price;
        int.TryParse(QuantityInput.GetComponent<TMP_InputField>().text, out int quantity);
        var totalText = TotalLabel.GetComponent<TextMeshProUGUI>();
        ValueLabel.text = price.ToString();
        totalText.text = (price * quantity).ToString();
    }

    private Market GetMarket()
    {
       if(TheEconomy.Instance.EconomicAgents.OfType<Market>().Count() == 1)
       {
           return TheEconomy.Instance.EconomicAgents.OfType<Market>().First();
       }
       else
       {
           throw new Exception("If you are seeing this message, congratulations! We have expanded and now it's your job to implement multiple markets.");
       }
    }

    #endregion

    public void InitializePlayer()
    {
        var companies = TheEconomy.Instance.EconomicAgents;
        var playerCompanies = companies.OfType<EconAgent>().Where(c => c.IsPlayer);
        if (playerCompanies.Count() == 1)
        {
            PlayerCompany = playerCompanies.First();
        }
        else if (playerCompanies.Count()>1)
        {
            throw new Exception("If you are seeing this message, congratulations! We have expanded and now it's your job to implement multipleplayer.");
        }
        else
        {
            throw new Exception("no players exist.");
        }
    }

    public void SubmitOrder()
    {
        var selectedGood = MarketInventoryEntries[dropdown.value].good;
        var totalText = TotalLabel.GetComponent<TextMeshProUGUI>();
        var totalprice = decimal.Parse(totalText.text);
        int.TryParse(QuantityInput.GetComponent<TMP_InputField>().text, out var quantity);
    
        iEconAgent seller = null; //Market Order

        var order = new Order(PlayerCompany,seller,selectedGood, quantity, totalprice);
        var orderContext = new ActionContext
        {
            TradeToSubmit = order,
            MarketToSubmitTo = LocalMarket,
            Period = LocalMarket.CurrentPeriod
        };
        var result = PlayerCompany.QueueOrder(orderContext);

        if (result == LemonadeStandResultObject.Success())
        {
            ShowOrderConfirmation();
        }
        else
        {
            Debug.Log(result.Message);
        }
    }
}
