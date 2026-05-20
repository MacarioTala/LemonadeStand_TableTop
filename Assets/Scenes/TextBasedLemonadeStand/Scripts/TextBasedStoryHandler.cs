using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections.Generic;

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
    [SerializeField] OrderPanelHandler orderPanelHandler;
    private const string InitialMarketName = "Episode 1 Market";
    public static TextBasedStoryHandler Instance { get; private set; }
    public TheEconomy TheEconomyInstance;

#region Turn Variables
    private int _lemonadeMadeThisTurn; //TODO: move this to EconAgent eventually
    private readonly List<(EconAgent Agent,int Period)> _bankruptcies= new();
    private int _cupsToSell;
    private bool _goodsSpoiled;
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
        if (Instance == null) Instance = this;
        else 
        {
            Destroy(gameObject);
            return;
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
            currentCash.text = PlayerCompany.GetCash().ToString();
            newsfeedController.SetHasNews(false);
            PeriodText.text = TheEconomyInstance.TradingPeriod.ToString();
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
            
            turnBasedSubscription = GameRoot.Instance.Bus
                    .Subscribe<StoryBeatHappenedEvent>(OnStoryBeatHappened,false);

            playerBankruptSubscription = GameRoot.Instance.Bus
                    .Subscribe<PlayerBankruptEvent>(OnPlayerBankrupted,false);
            
            otherAgentBankruptSubscription = GameRoot.Instance.Bus
                    .Subscribe<AgentBankruptEvent>(OnAgentBankrupted,false);

            SaleSignCupsToSell.onValueChanged.AddListener(OnCupsChanged);
            EndTurnButton.onClick.AddListener(EndTurn);
        }

        areEventsSubscribed = true;
    }
    #region Handlers
    private void OnAgentBankrupted(AgentBankruptEvent evt)=>_bankruptcies.Add(new(evt.Agent,evt.Period));

    private void OnPlayerBankrupted(PlayerBankruptEvent evt)=>_bankruptcies.Add(new(evt.Player,evt.Period));
    private void OnCupsChanged(string input)
    {
        var max = GetMaxCups();

        if(!int.TryParse(input,out var value))
        {
            value = 0;
        }
        
        value = Mathf.Clamp(value,0,max);
        _cupsToSell = value;

        var stringifiedValue = value.ToString();
        if (SaleSignCupsToSell.text != stringifiedValue)
        {
            SaleSignCupsToSell.SetTextWithoutNotify(stringifiedValue);
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
        
        LogMessage("The fridge smells funny");
        LogMessage("You see that");
        foreach(var item in evt.ExpiringItems)
            {
                var s = string.Empty;
                if(item.quantity>1)
                    s="s";
                LogMessage($"\n{item.quantity} {item.good.name}{s} spoiled overnight");
                LogMessage("\n");
            }
        _goodsSpoiled = true;
    }

    private void OnMarketEventFired(Market market, ActiveMarketEvent evt)
    {
        if(newsfeedController==null)
        {
            Debug.LogWarning("NewsfeedController missing, did you wire this in the inspector?");
            return;
        }

        if(!evt.IsContinuingEvent(market.CurrentPeriod))
        {
            newsfeedController.PlayBreakingNews(evt.EventDefinition);
            newsfeedController.SetHasNews(true);
        }      
    }

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
        
        var bankruptciesThisPeriod = _bankruptcies
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
        if(_lemonadeMadeThisTurn>0)
        {
            LogMessage($"You made {_lemonadeMadeThisTurn} cups of Lemonade.");
        }

        if(orders.Any())
        {
            //LogMessage($"You tried to sell {SaleSignCupsToSell.text} cups at {SaleSignLemonadePriceField.text}");
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
            }
        }

        if(_goodsSpoiled)
            LogMessage("\nSome goods spoiled");
       
        if(fixedCostsPaidThisPeriod.Count>0)
        {
            LogMessage($"You watch tokens disappear. You have: {PlayerCompany.GetCash():N2}");
            foreach(var fixedCost in fixedCostsPaidThisPeriod)
            {
                LogMessage(fixedCost.FixedCost.GetDescription());
            }
        }

        if(bankruptciesThisPeriod.Any())
        {
            if(bankruptciesThisPeriod.Any(x=>x.Agent.IsPlayer))
                GameOver();
            else
            {
                LogMessage("The following companies have gone bankrupt:");
                foreach(var (Agent, Period) in bankruptciesThisPeriod)
                {
                    LogMessage(Agent.Name);
                }
            }
        }

        LogMessage("You lock up and go home");
    }
    private void StartTurn()
    {
        isPeriodStart = true;
        ClearTextScroll();
        orderPanelHandler.RefreshOrderDropDown();

        GameRoot.Instance.Bus.Publish(new PeriodHappenedEvent(TheEconomyInstance.TradingPeriod),false);
        PlayStoryBeatsInPeriod();
        
        UpdateMaxLemonade();
        UpdateOrderPanelArrivingText(string.Empty);
        UpdateOrderSummaryArrivingText(string.Empty);
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
        newsfeedController.TurnTVOff();

        ProcessPlayerActions();

        yield return ShowMessageWithWait($"The day passes. Period {initialMarket.CurrentPeriod} ends", 1);
        
        TheEconomyInstance.ResolveTurn();

        PeriodText.text = TheEconomyInstance.TradingPeriod.ToString();

        yield return ShowTurnSummary();
        LogMessage("Hit <Space> to continue");
        yield return WaitForContinue();

        ClearSaleSign();
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
        SellLemonade();
    }

    private void SellLemonade()
    {
        var cupsToSell = _cupsToSell;
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
        
        ClearTextScroll();
        ClearUISelection();
        
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
        LogMessage($"It is Period : {TheEconomyInstance.TradingPeriod}.");
        LogMessage($"You have {currentCash.text} tokens.");
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
        LogMessage($"You have {currentCash.text} tokens");
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
                var maxQuantity = BasicLemonadeRecipe.GetMaxQuantityFromInventory(PlayerCompany
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
                    _lemonadeMadeThisTurn = maxQuantity;
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
    private void ClearSaleSign()
    {
        const string zero="0";
        SaleSignLemonadePriceField.SetTextWithoutNotify(zero);
        SaleSignCupsToSell.SetTextWithoutNotify(zero);
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
        _lemonadeMadeThisTurn=0;
        _cupsToSell=0;
        _goodsSpoiled = false;
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
        
        if(turnBasedSubscription.IsValid)
            turnBasedSubscription.Dispose();
    }
    #endregion
}

internal enum UIStateEnum
{
    Idle = 0,
    MainMenu,
    Reading
}