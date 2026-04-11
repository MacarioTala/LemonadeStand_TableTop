using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class TextBasedStoryHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textScroll;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Button EndTurnButton;
    [SerializeField] private TextMeshProUGUI PeriodText;
    [SerializeField] private int numberOfPopulations;
    [SerializeField] private int numberOfNPCFirms;
    [SerializeField] TextMeshProUGUI OrderPanelArrivingText;
    [SerializeField] TextMeshProUGUI SummaryPanelText;
    [SerializeField] TextMeshProUGUI SaleSignMaxCupsLabel;
    [SerializeField] TMP_InputField SaleSignCupsToSell;
    [SerializeField] TMP_InputField SaleSignLemonadePriceField;
    [SerializeField] NewsfeedController newsfeedController;
    private const string InitialMarketName = "Episode 1 Market";
    public static TextBasedStoryHandler Instance { get; private set; }
    public TheEconomy TheEconomyInstance;

#region Temporary
    private int lemonadeMadeThisTurn; //TODO: move this to EconAgent eventually
#endregion

#region Subscriptions
private SubscriptionToken deliveriesResolvedSubscription;
private SubscriptionToken goodsExpiredSubscription;
#endregion
    
#region Game Variables
    private bool isPeriodStart=false;
    private EconAgent PlayerCompany=null;
    private Market initialMarket;
    private UIStateEnum CurrentUIState = UIStateEnum.Idle;
    private bool areEventsSubscribed =false;
#endregion
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else 
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        StartTextBasedGame();
    }

    private void Update()
    {
        //return if player is entering text
        if(IsPlayerTyping()) return;
        
        switch(CurrentUIState)
        {
            case UIStateEnum.MainMenu:
                HandleMainMenu();
                break;
            case UIStateEnum.Reading:
                if(Input.GetKeyDown(KeyCode.Space))
                    CurrentUIState=UIStateEnum.Idle;
                else if(Input.GetKeyDown(KeyCode.Escape))
                    ShowMainMenu();
                break;
        }
    }
 
    private void ClearUISelection()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void StartTextBasedGame()
    {
        if (IsSceneOnly())
        {
            ClearTextScroll();
            LogMessage("Scene-only mode: Backend disabled");
            return;
        }
        Initialize();
        UpdateMaxLemonade();
        UpdateOrderPanelArrivingText(string.Empty);
        UpdateOrderSummaryArrivingText(string.Empty);
        StartCoroutine(PlayIntro());
    }

    private void Initialize()
    {
        if(GameRoot.Instance == null)
        {
            Debug.LogWarning("Gameroot is missing. Playing scene in scene-only mode.");
            return;
        }
        else
        {
            TheEconomyInstance = GameRoot.Instance.EconomyInstance;
            initialMarket = TheEconomyInstance.GetMarketByName(InitialMarketName);
            PlayerCompany = initialMarket.GetMarketParticipants()
                        .FirstOrDefault(x=>x.IsPlayer);
            newsfeedController.SetHasNews(false);
            PeriodText.text = TheEconomyInstance.tradingPeriod.ToString();
            SubscribeToEvents();
        }
    }

    private void SubscribeToEvents()
    {
        if(areEventsSubscribed) return;
        TheEconomyInstance.OnMarketEventFired += OnMarketEventFired;

        if(GameRoot.Instance!=null && GameRoot.Instance.Bus !=null)
        {
            deliveriesResolvedSubscription = GameRoot.Instance.Bus
                    .Subscribe<DeliveriesResolvedEvent>(OnDeliveriesResolved, replaySticky:false);
            
            goodsExpiredSubscription = GameRoot.Instance.Bus
                    .Subscribe<GoodsExpireEvent>(OnGoodsExpire, replaySticky:false);
            SaleSignCupsToSell.onValueChanged.AddListener(OnCupsChanged);
            EndTurnButton.onClick.AddListener(EndTurn);
        }

        areEventsSubscribed = true;
    }
    #region Handlers
    private void OnCupsChanged(string input)
    {
        if(string.IsNullOrWhiteSpace(input)) return;

        if(!int.TryParse(input,out var value))
        {
            SaleSignCupsToSell.text = "0";
            return;
        }
        var max = GetMaxCups();

        if (value > max)
        {
            SaleSignCupsToSell.text = max.ToString();
        }
    }
    private void OnDeliveriesResolved(DeliveriesResolvedEvent evt)
    {
        if(evt.Agent == null || evt.ArrivingItems == null || evt.ArrivingItems.Count==0)
            return;
        
        if(PlayerCompany == null || evt.Agent != PlayerCompany)
            return;

        UpdateOrderPanelArrivingText("Goods have arrived");
    }

    private void OnGoodsExpire(GoodsExpireEvent evt)
    {
        if(evt.Agent == null || evt.ExpiringItems==null||evt.ExpiringItems.Count==0)
            return;
        
        if(PlayerCompany == null || evt.Agent != PlayerCompany)
            return;
        
        LogMessage("!!Goods have expired!!");
        LogMessage("The following goods have expired");
        foreach(var item in evt.ExpiringItems)
            {
                var s = string.Empty;
                if(item.quantity>1)
                    s="s";
                LogMessage($"\n{item.quantity} {item.good.name}{s} spoiled overnight");
            }
    }

    private void OnMarketEventFired(Market market, MarketEventSO so)
    {
        if(newsfeedController!=null)
            {
                newsfeedController.PlayBreakingNews(so);
                newsfeedController.SetHasNews(true);
            }
        else
            Debug.LogWarning("NewsfeedController missing, did you wire this in the inspector?");
    }
    
    #endregion

    private IEnumerator ShowTurnSummary()
    {
        var summaryPeriod = initialMarket.CurrentPeriod==0?0:initialMarket.CurrentPeriod -1;

        var orders = initialMarket.GetOrdersExecutedInPeriod(summaryPeriod);
        var buys = orders
                    .Where(x=>ReferenceEquals(x.Order.Buyer,PlayerCompany))
                    .ToList();
        
        var sales = orders
                    .Where(x=>ReferenceEquals(x.Order.Seller,PlayerCompany))
                    .ToList();
        
        var fixedCostsPaidThisPeriod = PlayerCompany
                                     .FixedCostLedger
                                     .Where(x=>x.Period==summaryPeriod)
                                     .ToList();

        yield return ShowMessageWithWait($"Summary for period {summaryPeriod} ",1);

        if(buys.Count>0)
        {
            LogMessage("You bought");
            foreach(var line in buys)
            {
                var s = string.Empty;
                if(line.Order.Quantity>1) s="s";

                LogMessage($"\n{line.Order.Quantity} {line.Order.Good.GoodName}{s}");
            }
        }
        if(lemonadeMadeThisTurn>0)
        {
            LogMessage($"You made {lemonadeMadeThisTurn} cups of Lemonade.");
        }
        if(sales.Count>0)
        {
            LogMessage("You sold");
            foreach(var line in sales)
            {
                    var s = string.Empty;
                    if(line.Order.Quantity>1) s="s";

                    LogMessage($"\n{line.Order.Quantity} {line.Order.Good.GoodName}{s}");
            }
        }
       
        if(fixedCostsPaidThisPeriod.Count>0)
        {
            LogMessage($"You watch tokens disappear. You have: {PlayerCompany.GetCash():N2}");
            foreach(var fixedCost in fixedCostsPaidThisPeriod)
            {
                LogMessage(fixedCost.FixedCost.GetDescription());
            }
        }

        LogMessage("You lock up and go home");
    }
    private void StartTurn()
    {
        isPeriodStart = true;
        ClearTextScroll();

        lemonadeMadeThisTurn = 0;
        UpdateMaxLemonade();
        UpdateOrderPanelArrivingText(string.Empty);
        UpdateOrderSummaryArrivingText(string.Empty);
        ShowMainMenu();
    }
    private void EndTurn()
    {
        if(IsSceneOnly()) return;

        StartCoroutine(EndTurnFlow());
        Debug.Log("End Turn");
    }

    private IEnumerator EndTurnFlow()
    {
        CurrentUIState = UIStateEnum.Reading;
        ClearUISelection();
        ClearTextScroll();

        ProcessPlayerActions();

        yield return ShowMessageWithWait($"The day passes. Period {initialMarket.CurrentPeriod} ends", 1);
        
        TheEconomyInstance.ResolveTurn();

        PeriodText.text = TheEconomyInstance.tradingPeriod.ToString();

        yield return ShowTurnSummary();
        LogMessage("Hit <Space> to continue");
        yield return WaitForContinue();

        StartTurn();
    }

    private void ProcessPlayerActions()
    {
        /// other player actions here
        SellLemonade();
    }

    private void SellLemonade()
    {
        var cupsToSell = GetCupsToSell();
        if(cupsToSell <=0) return;

        var lemonadeEntry = PlayerCompany.GetInventory().GetInventoryEntries().FirstOrDefault(x=>x.good.GoodName=="Lemonade");
        if(lemonadeEntry == null) return;

        var lemonade = lemonadeEntry.good;
        var price = GetLemonadePrice();
        var sellOrder = new Order(null,PlayerCompany,lemonade,cupsToSell,price);
        var context = new ActionContextBuilder()
                    .ForMarket(initialMarket)
                    .ForPeriod(initialMarket.CurrentPeriod)
                    .WithTrade(sellOrder)
                    .Build();
        PlayerCompany.QueueOrder(context);
    }

    #region Menu stuff
    private void ChooseFromMainMenu(char choice)
    {   
        ClearTextScroll();

        switch (choice)
        {
            case 'C':
                DisplayInventory();
                break;
            case 'N':
                CheckNews();
                break;
            case 'M':
                MakeLemonade();
                break;
            default:
                LogMessage("Invalid choice. Please choose again.");
                ShowMainMenu();
                break;
        }
        LogMessage("\nPress <esc> to continue.");
    }
    private void HandleMainMenu()
    {
        if (Input.GetKeyDown(KeyCode.C)) ChooseFromMainMenu('C');
        if (Input.GetKeyDown(KeyCode.N)) ChooseFromMainMenu('N');
        if (Input.GetKeyDown(KeyCode.M)) ChooseFromMainMenu('M');
    }

    private void ShowMainMenu()
    {
        CurrentUIState = UIStateEnum.MainMenu;
        
        ClearUISelection();
        ClearTextScroll();
        
        LogMessage($"--- It is period {initialMarket.CurrentPeriod} ---");
        if(isPeriodStart) LogMessage($"You flip the sign open");
        
        LogMessage("What would you like to do?");
        LogMessage("(C)heck your supplies");
        LogMessage("(M)ake lemonade from recipe");
        LogMessage("Check (N)ews");
        LogMessage("\n");

        isPeriodStart = false;
    }
#endregion

#region Game Choices
    private void CheckNews()
    {
        CurrentUIState = UIStateEnum.Reading;

        ClearUISelection();
        LogMessage($"It is Period : {TheEconomyInstance.tradingPeriod}.");
        LogMessage($"You have {PlayerCompany.GetCash()} credits.");
        LogMessage($"The people in your neighbourhood are {initialMarket.GetEnnuiLevel()}");
        if(initialMarket.CurrentPeriod !=0)
        {
            LogMessage("The following trades happened yesterday");
            LogMessage("--------");
            foreach(var trade in initialMarket.GetExecutionsInPeriod(initialMarket.CurrentPeriod-1))
            {
                LogMessage(trade.ToString());
            }
        }
    }
    private void DisplayInventory()
    {
        CurrentUIState = UIStateEnum.Reading;

        ClearUISelection();
        var inventory = PlayerCompany.GetInventory().GetAvailableInventory();
        LogMessage("You open the fridge, you see:");
        if(inventory.Count>0)
        {
            foreach (var item in inventory)
            {
                var s = string.Empty;
                if(item.quantity>1)
                    s="s";
                LogMessage($"{item.quantity} {item.good} {s} you bought for {item.Cost}");
            }
        }
        else
        {
            LogMessage("... an empty fridge");
        }

        var orderedSupplies = PlayerCompany.GetInventory()
                                           .GetInventoryEntries()
                                           .Where(x=>x.RemainingDelay>0);

        if(orderedSupplies.Count()>0)
        {
            LogMessage("\nYou're still waiting for the following goods:");
            foreach(var item in orderedSupplies)
                LogMessage($"{item.good} Quantity: {item.quantity} Acquired at: {item.Cost} Arriving in {item.RemainingDelay}");
        }

        if(PlayerCompany.Recipes.Count()>0)
        {
            LogMessage("\nYou have recipes for: ");
            foreach(var recipe in PlayerCompany.Recipes)
                LogMessage($"- {recipe.RecipeName}");
        }
        else
            LogMessage("You have not discovered any recipes");
    }
    private void MakeLemonade()
    {
        CurrentUIState = UIStateEnum.Reading;
        ClearUISelection();
        if(PlayerCompany.Recipes.Count==0)
        {
            LogMessage("You have no recipes!");
        }
        else
        //Basic Lemonade for now
        {
            var BasicLemonadeRecipe = PlayerCompany.Recipes.FirstOrDefault(x=>x.RecipeName=="Basic Lemonade");
            if(BasicLemonadeRecipe)
            {
                var maxQuantity = BasicLemonadeRecipe.Get_max_quantity(PlayerCompany
                                        .GetInventory()
                                        .GetInventoryEntries());

                if(BasicLemonadeRecipe.CanRecipeBeMadeFrom(PlayerCompany.GetInventory().GetInventoryEntries()))
                {
                    var context = new ActionContext()
                    {
                        RecipeMaker=PlayerCompany,
                        Recipe=BasicLemonadeRecipe,
                        QuantityToMake=maxQuantity
                    };
                    PlayerCompany.MakeRecipe(context);
                    LogMessage("You made "+maxQuantity+" units of: "+BasicLemonadeRecipe.GetProduct());
                    UpdateMaxLemonade();
                    lemonadeMadeThisTurn = maxQuantity;
                }
                else
                {
                    LogMessage("You don't have enough ingredients to make:"+BasicLemonadeRecipe.GetProduct());
                }
            }
            else
            {
                LogMessage("You don't know how to make Basic Lemonade yet. Check the cupboard?");
            }
        }
    }
    #endregion

    private void UpdateOrderPanelArrivingText(string message)
        => OrderPanelArrivingText.text = message;
    private void UpdateOrderSummaryArrivingText(string message)
        => SummaryPanelText.text = message;
    private void UpdateMaxCupsText(string message)
        => SaleSignMaxCupsLabel.text = message;

   

#region helpers
    private bool IsSceneOnly()
    {
        if(GameRoot.Instance==null) 
            return true;
        else
            return false;
    }
    private void ClearTextScroll()
    {
        if(textScroll!=null) textScroll.text = "";
    }
    private int GetCupsToSell()
    {
        int.TryParse(SaleSignCupsToSell.text, out var cups);
        return cups;
    }
    private decimal GetLemonadePrice()
    {
        decimal.TryParse(SaleSignLemonadePriceField.text, out var price);
        return price;
    }
    private int GetMaxCups()
    {
        var maxcups = PlayerCompany?
                        .GetInventory()?
                        .GetInventoryEntries()
                        .Where(x=>x.good.GoodName == "Lemonade")
                        .Sum(x=>x.quantity)??0;
        return maxcups;
    }

    private bool IsPlayerTyping()
    {
        var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        return selected != null && selected.GetComponent<TMP_InputField>() !=null;
    }
    public void LogMessage(string message)
    {
        if (textScroll != null)
        {
            textScroll.text += "\n" + message;
            textScroll.ForceMeshUpdate();

            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }
        else
        {
            Debug.Log("GameLogTMP is not assigned in the inspector.");
        }
    }
    
    private IEnumerator PlayIntro()
    {   
        CurrentUIState=UIStateEnum.Reading;
        ClearTextScroll();
        yield return ShowMessageWithWait("Welcome to Lemonade Stand!",1);
        yield return ShowMessageWithWait("Can you save Capitalism?",1);
        LogMessage("Let's find out!");
        LogMessage($"Hit <Space> to continue");
        yield return WaitForContinue();

        CurrentUIState = UIStateEnum.Idle;

        StartTurn();
    }

    private IEnumerator ShowMessageWithWait(string message,int waitForSeconds)
    {
        LogMessage(message);
        yield return new WaitForSeconds(waitForSeconds);
    }
    private void UpdateMaxLemonade()
    {
        var maxcups = GetMaxCups();
        UpdateMaxCupsText(maxcups.ToString());
    }

    private IEnumerator WaitForContinue()
    {
        CurrentUIState = UIStateEnum.Reading;
        while(CurrentUIState == UIStateEnum.Reading)
            yield return null;
    }

    #endregion
    #region Unity Stuff
    private void OnDisable()
        => CleanupSubscription();
    private void OnDestroy()
        => CleanupSubscription();

    private void CleanupSubscription()
    {
        if (areEventsSubscribed && TheEconomyInstance != null)
            {
                TheEconomyInstance.OnMarketEventFired -= OnMarketEventFired;
                areEventsSubscribed = false;
            }
        
        if(deliveriesResolvedSubscription.IsValid)
            deliveriesResolvedSubscription.Dispose();
        
        if(goodsExpiredSubscription.IsValid)
            goodsExpiredSubscription.Dispose();
    }
    #endregion
}

internal enum UIStateEnum
{
    Idle = 0,
    MainMenu,
    Reading
}