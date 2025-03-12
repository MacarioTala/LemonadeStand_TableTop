using System;
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
    [SerializeField] private GameObject TurnLabel;
    [SerializeField] private GameObject ValueLabel;
    [SerializeField] private GameObject TotalLabel;
    private Company PlayerCompany;
    private Market LocalMarket;
    private List<InventoryEntry> MarketInventoryEntries;
    private TMP_Dropdown dropdown;
#region UnityBuiltIns
    public void Start()
    {
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        dropdown = ItemDropdown.GetComponent<TMP_Dropdown>();
        LocalMarket = GetMarket();
        OrderButton.onClick.AddListener(SubmitOrder);
        QuantityInput.GetComponent<TMP_InputField>().onValueChanged.AddListener(value => HandleOrderQuantityChange(value));
        MarketInventoryEntries = LocalMarket.GetInventory().GetInventoryEntries();
        InitializeOrderDropDown();
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
        var selectedGood = selectedEntry.good;
        int.TryParse(QuantityInput.GetComponent<TMP_InputField>().text, out int quantity);
        var totalText = TotalLabel.GetComponent<TextMeshProUGUI>();
        ValueLabel.GetComponent<TextMeshProUGUI>().text = price.ToString();
        totalText.text = (price * quantity).ToString();
    }

    private Market GetMarket()
    {
       if(TheEconomy.Instance.companies.OfType<Market>().Count() == 1)
       {
           return TheEconomy.Instance.companies.OfType<Market>().First();
       }
       else
       {
           throw new Exception("If you are seeing this message, congratulations! We have expanded and now it's your job to implement multiple markets.");
       }
    }

    #endregion

    public void Initialize(Company playerCompany)
    {
        PlayerCompany = playerCompany;
    }

    public void SubmitOrder()
    {
        var selectedGood = MarketInventoryEntries[dropdown.value].good;
        int.TryParse(QuantityInput.GetComponent<InputField>().text, out var quantity);
    
        iCompany seller = null; //Market Order

        var order = new Order(PlayerCompany,seller,selectedGood, quantity, selectedGood.GetPrice());
        var orderContext = new ActionContext
        {
            TradeToSubmit = order,
            MarketToSubmitTo = null,
            Period = LocalMarket.CurrentPeriod
        };


        var result = PlayerCompany.QueueOrder(orderContext);
    }
}
