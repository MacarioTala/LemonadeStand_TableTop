using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections.Generic;
using System;

public class CraftingStoryHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gameLog;
    [SerializeField] private ScrollRect textScroll;
    [SerializeField] private Button EndTurnButton;
    [SerializeField] private TextMeshProUGUI PeriodText;
    [SerializeField] private int numberOfPopulations; 
    [SerializeField] private int numberOfNPCFirms;
    [SerializeField] private DeliveryVan _currentVan;
    [SerializeField] private Suitcase ElementDelivery;
    [SerializeField] private Shelf _shelf;

    private const string InitialMarketName = "Episode 1 Market";
    public static CraftingStoryHandler Instance { get; private set; }
    public TheEconomy TheEconomyInstance;

#region Turn Variables
    [SerializeField]TextMeshProUGUI currentCash;
    #endregion
    #region Story Beats
    readonly List<StoryBeat> storyBeats=new();
    public void AddStoryBeat(StoryBeat beat) => storyBeats.Add(beat);
#endregion
#region Subscriptions
private SubscriptionToken deliveriesResolvedSubscription;
private SubscriptionToken goodsExpiredSubscription;
private SubscriptionToken turnBasedSubscription;
private SubscriptionToken playerBankruptSubscription;
private SubscriptionToken otherAgentBankruptSubscription;
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
        if (Instance == null) 
        {
            Instance = this;
            ElementDelivery.BuyRequested += BuyRequested;
        }
        else 
        {
            Destroy(gameObject);
            return;
        }
    }

    private void BuyRequested(InventoryEntry entry)
    {
        try
            {
                PlayerCompany.BuyGood(entry.good,entry.quantity,entry.PriceOfGood,TheEconomyInstance.TradingPeriod);
                var totalPrice = entry.PriceOfGood*entry.quantity;
                int.TryParse(currentCash.text,out var intCash);
                intCash -= totalPrice;
                currentCash.text=intCash.ToString();

                _shelf.PlaceOnShelf(entry);
                ElementDelivery.BuySucceeded(entry);    
            }
        catch(InsufficientFundsException)
        {
            Debug.Log("Insufficient funds");
            ElementDelivery.BuyFailed(entry);
        }
    }

    private void Start()
        => StartTextBasedGame();

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
            currentCash.text = PlayerCompany.GetCash().ToString();
            PeriodText.text = TheEconomyInstance.TradingPeriod.ToString();
            SubscribeToEvents();
        }
    }

    private void SubscribeToEvents()
    {
        if(GameRoot.Instance!=null && GameRoot.Instance.Bus !=null)
        {
            turnBasedSubscription = GameRoot.Instance.Bus
                    .Subscribe<StoryBeatHappenedEvent>(OnStoryBeatHappened,false);

            EndTurnButton.onClick.AddListener(EndTurn);
        }

        areEventsSubscribed = true;
    }
    #region Handlers

    private void OnStoryBeatHappened(StoryBeatHappenedEvent evt)
        {
            if(evt.Bag.FixedCostAssociatedWithBeat !=null)
                AddFixedCostsForPlayer(evt.Bag.FixedCostAssociatedWithBeat);

            AddStoryBeat(evt.Beat);
        }

    private void AddFixedCostsForPlayer(FixedCostTemplate template)
    {
        PlayerCompany.AddFixedCostInPeriod(template,initialMarket.CurrentPeriod);
    }
    
    #endregion

    private IEnumerator ShowTurnSummary()
    {
        var summaryPeriod = initialMarket.CurrentPeriod==0?0:initialMarket.CurrentPeriod -1;

        var orders = initialMarket.GetHistoricalRecordsInPeriod(summaryPeriod);
        var executions = initialMarket.GetOrdersExecutedInPeriod(summaryPeriod);
        var buys = executions
                    .Where(x=>ReferenceEquals(x.Order.Buyer,PlayerCompany))
                    .ToList();
        
        var salesAttempts = orders.Where(x=>x.OriginalOrderSnapshot.SellerName==PlayerCompany.Name).ToList();
        var sales = executions
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

                LogMessage($"{line.Order.Quantity} {line.Order.Good.GoodName}{s}");
            }
        }

        if(orders.Any())
        {
            LogMessage($"You tried to sell ");
            if(sales.Count==0) LogMessage("...but sold none.");
            foreach(var order in orders.Where(x=>x.OriginalOrderSnapshot.SellerName==PlayerCompany.Name))
                LogMessage(order.Message);
        }

        if(sales.Count>0)
        {
            LogMessage("You sold");
            foreach(var line in sales)
            {
                    var s = string.Empty;
                    if(line.Order.Quantity>1) s="cups";

                    LogMessage($"\n{line.Order.Quantity} {line.Order.Good.GoodName} {s} at {line.Order.Price}");
                    LogMessage($"\n{line.Order.Quantity*line.Order.Price} tokens have appeared in the cash register.");
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
     
        GameRoot.Instance.Bus.Publish(new PeriodHappenedEvent(TheEconomyInstance.TradingPeriod),false);
        PlayStoryBeatsInPeriod();
        
        ShowMainMenu();
    }

    private void PlayStoryBeatsInPeriod()
    {
        foreach(var beat in storyBeats)
            LogMessage(beat.FlavourText);
        storyBeats.Clear();
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

        PeriodText.text = TheEconomyInstance.TradingPeriod.ToString();

        yield return ShowTurnSummary();
        LogMessage("Hit <Space> to continue");
        yield return WaitForContinue();

        ResetTurnVariables();
        StartTurn();
    }

    public void GameOver()
    {
        LogMessage("Game Over");
        Quit();
    }

    private void ProcessPlayerActions()
    {
        /// other player actions here
        //SellGoods();
    }

    private void SellGoods()
    {
        throw new NotImplementedException();
        // var sellOrder = new Order(null,PlayerCompany,lemonade,cupsToSell,price);
        // var context = new ActionContextBuilder()
        //             .ForMarket(initialMarket)
        //             .ForPeriod(initialMarket.CurrentPeriod)
        //             .WithTrade(sellOrder)
        //             .Build();
        // PlayerCompany.QueueOrder(context);
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
            case 'S':
                BuyGoods(TheEconomyInstance.TradingPeriod);
                break;
            case 'M':
                MakeGood();
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
        if (Input.GetKeyDown(KeyCode.S)) ChooseFromMainMenu('S');
        if (Input.GetKeyDown(KeyCode.M)) ChooseFromMainMenu('M');
    }

    private void ShowMainMenu()
    {
        CurrentUIState = UIStateEnum.MainMenu;
        
        ClearTextScroll();
        ClearUISelection();
        
        LogMessage($"--- It is period {initialMarket.CurrentPeriod} ---");
        if(isPeriodStart) LogMessage($"You flip the sign open");
        
        LogMessage("What would you like to do?");
        LogMessage("(C)heck your supplies");
        LogMessage("(S)ee what Pete and Dmitri have for sale");
        LogMessage("(M)ake something");
        LogMessage("\n");

        isPeriodStart = false;
    }
#endregion

#region Game Choices
    private void BuyGoods(int tradingPeriod)
    {
        ElementDelivery.gameObject.SetActive(true);
        var availableGoods = _currentVan.GetCurrentDelivery(tradingPeriod);
       
       ElementDelivery.Display(availableGoods);
    }

   

    private void DisplayInventory()
    {
        CurrentUIState = UIStateEnum.Reading;

        ClearUISelection();
        var inventory = PlayerCompany.GetInventory().GetAvailableInventory();
        LogMessage($"You have {currentCash.text} gold");
        LogMessage("You open the fridge, you see:");
        if(inventory.Count>0)
        {
            foreach (var item in inventory.Where(x=>!x.good.IsProducedGood))
            {
                var s = string.Empty;
                if(item.quantity>1)
                    s="s";
                LogMessage($"{item.quantity} {item.good} {s} you bought for {item.Cost}");
            }
            if(inventory.Any(x=>x.good.IsProducedGood))
            {
                LogMessage("..and");
                foreach (var item in inventory.Where(x=>x.good.IsProducedGood))
                {
                    var s = string.Empty;
                    if(item.quantity>1)
                        s="s";
                    LogMessage($"{item.quantity} {item.good} {s} you made at {item.Cost} per {item.good}");
                }
            }
        }
        else
        {
            LogMessage("... an empty fridge");
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

    private void MakeGood()
    {
        CurrentUIState = UIStateEnum.Reading;
        ClearUISelection();
        if(PlayerCompany.Recipes.Count==0)
        {
            
        }
        else
        {
            //jury rigging
        }
    }
    #endregion
   

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
        if(gameLog!=null) gameLog.text = "";
    }
    private bool IsPlayerTyping()
    {
        var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        return selected != null && selected.GetComponent<TMP_InputField>() !=null;
    }
    public void LogMessage(string message)
    {
        if (gameLog != null)
        {
            gameLog.text += "\n" + message;
            gameLog.ForceMeshUpdate();

            if (textScroll != null)
            {
                Canvas.ForceUpdateCanvases();
                textScroll.verticalNormalizedPosition = 0f;
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
        yield return ShowMessageWithWait(" The market is crowded, but you find the lemonade stand. ",1);
        yield return ShowMessageWithWait(" People glance expectantly as you enter the stand ",1);
        yield return ShowMessageWithWait(" It is in pristine condition. ",1);
        LogMessage($"Hit <Space> to continue");
        yield return WaitForContinue();

        CurrentUIState = UIStateEnum.Idle;

        StartTurn();
    }

    private void Quit()
    {
        #if UNITY_STANDALONE
            Application.Quit();
        #endif
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
        #endif
    }
    public void ResetTurnVariables()
    {
        currentCash.text = PlayerCompany.GetCash().ToString();
    }

    private IEnumerator ShowMessageWithWait(string message,int waitForSeconds)
    {
        LogMessage(message);
        yield return new WaitForSeconds(waitForSeconds);
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
        if(deliveriesResolvedSubscription.IsValid)
            deliveriesResolvedSubscription.Dispose();
        
        if(goodsExpiredSubscription.IsValid)
            goodsExpiredSubscription.Dispose();
        
        if(turnBasedSubscription.IsValid)
            turnBasedSubscription.Dispose();
        
        if(playerBankruptSubscription.IsValid)
            playerBankruptSubscription.Dispose();
        
        if(otherAgentBankruptSubscription.IsValid)
            otherAgentBankruptSubscription.Dispose();
    }
    #endregion
}