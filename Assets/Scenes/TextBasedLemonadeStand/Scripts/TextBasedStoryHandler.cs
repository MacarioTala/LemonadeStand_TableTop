using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

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
    [SerializeField] NewsfeedController newsfeedController;
    readonly Recipe BasicLemonadeRecipe=null;
    private const string InitialMarketName = "Episode 1 Market";
    public static TextBasedStoryHandler Instance { get; private set; }
    public TheEconomy TheEconomyInstance;

    private bool isSceneOnly = true;
    private bool areEventsSubscribed =false;
    private bool isSceneRunning =false;
    private bool areButtonsWired=false;
    
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
        areEventsSubscribed = true;
    }

    #region Events
    
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
            TheEconomyInstance.StartTradingPeriod();
            UpdateOrderPanelArrivingText("Goods have arrived");
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
            case 'S':
                SetLemonadePrice();
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
        LogMessage("(S)et Lemonade Price");
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
            var maxQuantity = BasicLemonadeRecipe.Get_max_quantity(PlayerCompany.GetInventory().GetInventoryEntries());

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
            }
            else
            {
                LogMessage("Insufficient ingredients to make:"+BasicLemonadeRecipe.name);
            }
        }
    }
     private void SetLemonadePrice()
    {
        ClearUISelection();
        LogMessage("Cannot set Lemonade Price yet");
    }
    #endregion

    private void UpdateOrderPanelArrivingText(string message)
        => OrderPanelArrivingText.text = message;
    private void UpdateOrderSummaryArrivingText(string message)
        => SummaryPanelText.text = message;

   

#region helpers
    private void ClearTextScroll()
    {
        if(textScroll!=null) textScroll.text = "";
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

}