using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using System;
using System.Collections.Generic;

public class TextBasedStoryHandler : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds1 = new(1);
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
    Recipe BasicLemonadeRecipe=null;
    private const string InitialMarketName = "Episode 1 Market";
    public static TextBasedStoryHandler Instance { get; private set; }
    public TheEconomy TheEconomyInstance;

    private bool isSceneOnly = true;
    private bool areEventsSubscribed =false;
    private bool isSceneRunning =false;
    private bool areButtonsWired=false;
#region Subscriptions
private SubscriptionToken deliveriesResolvedSubscription;
private SubscriptionToken goodsExpiredSubscription;
#endregion
    
#region Game Variables
    private EconAgent PlayerCompany=null;
    private bool isWaitingForPlayerInput = false;
    private Market initialMarket;
    private MenuStateEnum CurrentMenuState = MenuStateEnum.Splash;
#endregion
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else 
        {
            Destroy(gameObject);
            return;
        }
        SaleSignCupsToSell.onValueChanged.AddListener(OnCupsChanged);
    }

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

    private void Start()
    {
        CheckForBackEnd();
        if(!isSceneOnly)
        {
            initialMarket = TheEconomyInstance.GetMarketByName(InitialMarketName);
            PlayerCompany = initialMarket.GetMarketParticipants()
                            .FirstOrDefault(x=>x.IsPlayer);
        }
        StartTextBasedGame();
    }

    private void CheckForBackEnd()
    {
        if(GameRoot.Instance==null) isSceneOnly=true;
        else
            {
                TheEconomyInstance=GameRoot.Instance.EconomyInstance;
                isSceneOnly = false;
            }
    }

    private void Update()
    {
        if(!isWaitingForPlayerInput) return;

        if(Input.GetKeyDown(KeyCode.Space))
        {
            ClearUISelection();
            isWaitingForPlayerInput = false;
            ClearTextScroll();
            DisplayChoices();
        }

        if(CurrentMenuState==MenuStateEnum.MainMenu) ChoicesMainMenu();
    }
 
    private void ClearUISelection()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void StartTextBasedGame()
    {
        if(isSceneRunning) return;
        isSceneRunning = true;
        Initialize();
        WireUpButtons();

        if (isSceneOnly)
        {
            LogMessage("Scene-only mode: Backend disabled");
            return;
        }
        UpdateMaxLemonade();
        UpdateOrderPanelArrivingText(string.Empty);
        UpdateOrderSummaryArrivingText(string.Empty);
        StartCoroutine(StartGameLoop());
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
            isSceneOnly = false;
            TheEconomyInstance = GameRoot.Instance.EconomyInstance;
            PeriodText.text = TheEconomyInstance.tradingPeriod.ToString();
            SubscribeToEvents();
        }
    }

    private void SubscribeToEvents()
    {
        if(areEventsSubscribed) return;
        TheEconomyInstance.OnMarketEventFired += OnMarketEvent;

        if(GameRoot.Instance!=null && GameRoot.Instance.Bus !=null)
        {
            deliveriesResolvedSubscription = GameRoot.Instance.Bus
                    .Subscribe<DeliveriesResolvedEvent>(OnDeliveriesResolved, replaySticky:false);
            
            goodsExpiredSubscription = GameRoot.Instance.Bus
                    .Subscribe<GoodsExpireEvent>(OnGoodsExpire, replaySticky:false);
        }

        areEventsSubscribed = true;
    }
    #region Events
    private void OnDeliveriesResolved(DeliveriesResolvedEvent evt)
    {
        if(evt.Agent == null || evt.ArrivingItems == null || evt.ArrivingItems.Count==0)
            return;
        
        if(PlayerCompany == null || !evt.Agent.Equals(PlayerCompany))
            return;

        UpdateOrderPanelArrivingText("Goods have arrived");
    }

    private void OnGoodsExpire(GoodsExpireEvent evt)
    {
        if(evt.Agent == null || evt.ExpiringItems==null||evt.ExpiringItems.Count==0)
            return;
        
        if(PlayerCompany == null || !evt.Agent.Equals(PlayerCompany))
            return;
        
        LogMessage("!!Goods have expired!!");
        LogMessage("The following goods have expired");
        foreach(var item in evt.ExpiringItems)
            {
                var s = string.Empty;
                if(item.quantity>1)
                    s="s";
                LogMessage($"\n{item.quantity} {item.good.name}{s}");
            }
    }

    private void OnMarketEvent(Market market, MarketEventSO so)
    {
        if(newsfeedController!=null)
            newsfeedController.PlayBreakingNews(so);
        else
            Debug.LogWarning("NewsfeedController missing, did you wire this in the inspector?");
    }
    
    #endregion
    private void WireUpButtons()
    {
        if(areButtonsWired) return;
        
        areButtonsWired=true;
        EndTurnButton.onClick.AddListener(EndTurn);

    }

    private void EndTurn()
    {
        if(!isSceneOnly)
        {
            ClearTextScroll();
            DisplayChoices();
            UpdateOrderPanelArrivingText(string.Empty);
            UpdateOrderSummaryArrivingText(string.Empty);
            TheEconomyInstance.StartTradingPeriod();
            TheEconomyInstance.EndTradingPeriod();
            PeriodText.text = TheEconomyInstance.tradingPeriod.ToString();
        }
        Debug.Log("End Turn");
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
                DisplayChoices();
                break;
        }
        isWaitingForPlayerInput = true;
        LogMessage("Press <space> to continue.");
    }
    private void ChoicesMainMenu()
    {
        if (Input.GetKeyDown(KeyCode.C)) ChooseFromMainMenu('C');
        if (Input.GetKeyDown(KeyCode.S)) ChooseFromMainMenu('S');
        if (Input.GetKeyDown(KeyCode.N)) ChooseFromMainMenu('N');
        if (Input.GetKeyDown(KeyCode.M)) ChooseFromMainMenu('M');
    }

    private void DisplayChoices()
    {
        ClearUISelection();
        isWaitingForPlayerInput = true;
        CurrentMenuState = MenuStateEnum.MainMenu;
        LogMessage("What would you like to do?");
        LogMessage("(C)heck Inventory");
        LogMessage("(M)ake Lemonade from recipe");
        LogMessage("Check (N)ews");
        LogMessage("\n");
    }
#endregion

#region Game Choices
    private void CheckNews()
    {
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
        ClearUISelection();
        var inventory = PlayerCompany.GetInventory().GetAvailableInventory();
        LogMessage("Inventory:");
        foreach (var item in inventory)
            LogMessage($"{item.good} Quantity: {item.quantity} Acquired at: {item.Cost}");
        
        var orderedSupplies = PlayerCompany.GetInventory()
                                           .GetInventoryEntries()
                                           .Where(x=>x.RemainingDelay>0);

        LogMessage("\nThe following goods are yet to arrive:");
        foreach(var item in orderedSupplies)
            LogMessage($"{item.good} Quantity: {item.quantity} Acquired at: {item.Cost} Arriving in {item.RemainingDelay}");
    
        if(PlayerCompany.Recipes.Count()>0)
        {
            LogMessage("\n\nYou have a recipe for: " + PlayerCompany.Recipes.FirstOrDefault(x=>x.RecipeName=="Basic Lemonade"));
        }
        else
            LogMessage("You have not discovered any recipes");
    }
    private void MakeLemonade()
    {
        ClearUISelection();
        if(PlayerCompany.Recipes.Count==0)
        {
            LogMessage("You have no recipes!");
        }
        {
            BasicLemonadeRecipe = PlayerCompany.Recipes.FirstOrDefault(x=>x.RecipeName=="Basic Lemonade");
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
                LogMessage("Made "+maxQuantity+" units of: "+BasicLemonadeRecipe.GetProduct());
                UpdateMaxLemonade();
            }
            else
            {
                LogMessage("Insufficient ingredients to make:"+BasicLemonadeRecipe.name);
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
    private void ClearTextScroll()
    {
        if(textScroll!=null) textScroll.text = "";
    }

    private int GetMaxCups()
    {
        var maxcups = PlayerCompany?
                        .GetInventory()?
                        .GetInventoryEntries()
                        .Where(x=>x.good.GoodName == "Lemonade")
                        ?.Count()??0;
        return maxcups;
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
    private IEnumerator StartGameLoop()
    {
        textScroll.text = "";
        LogMessage("Welcome to Lemonade Stand!");
        yield return _waitForSeconds1;
        LogMessage("Can you save Capitalism?");
        yield return _waitForSeconds1;
        LogMessage("Let's find out!");
        yield return _waitForSeconds1;
        DisplayChoices();
    }
    private void UpdateMaxLemonade()
    {
        var maxcups = GetMaxCups();
        UpdateMaxCupsText(maxcups.ToString());
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
                TheEconomyInstance.OnMarketEventFired -= OnMarketEvent;
                areEventsSubscribed = false;
            }
        
        if(deliveriesResolvedSubscription.IsValid)
            deliveriesResolvedSubscription.Dispose();
        
        if(goodsExpiredSubscription.IsValid)
            goodsExpiredSubscription.Dispose();
    }
    #endregion
}

internal enum MenuStateEnum
{
    Splash = 0,
    MainMenu,
    CheckInventory,
    CheckNews,
    OrderSupplies,
    SetPrice,
    SetQuantity,

}