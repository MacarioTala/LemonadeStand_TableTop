using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using System.Linq;

public class TextBasedStoryHandler : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds1 = new(1);
    [SerializeField] private TextMeshProUGUI textScroll;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Button EndTurnButton;
    [SerializeField] private TextMeshProUGUI ActionPointsText;
    [SerializeField] private int numberOfPopulations;
    [SerializeField] private int numberOfNPCFirms;
    [SerializeField] TextMeshProUGUI OrderPanelArrivingText;
    [SerializeField] TextMeshProUGUI SummaryPanelText;
    [SerializeField] Recipe BasicLemonadeRecipe;
    private const string BasicLemonadeRecipeName = "Basic Lemonade";
    public static TextBasedStoryHandler Instance { get; private set; }
#region Game Variables
    private EconAgent PlayerCompany;
    private bool isWaitingForPlayerInput = false;
    private Market initialMarket;
    private int playerActionsRemaining;

    private MenuStateEnum CurrentMenuState = MenuStateEnum.Splash;
#endregion
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
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

    public void StartTextBasedGame()
    {
        InitializeMarket();
        InitializePlayer();
        ActionPointsText.text = TheEconomy.Instance.tradingPeriod.ToString();
        WireUpButtons();
        if (Instance == null)
        {
            Debug.Log("TextBasedStoryHandler is not initialized.");
            return;
        }
        UpdateOrderPanelArrivingText(string.Empty);
        UpdateOrderSummaryArrivingText(string.Empty);
        Instance.StartCoroutine(Instance.StartGameLoop());
    }
#region NPCs
    private void CreateNpcs()
    {
        CreatePopulations();
        CreateFirms();
    }

    private void CreateFirms()
    {
        var firmTemplates = Resources.LoadAll<EconAgent>("EconAgents/Firms");
        foreach(var firm in firmTemplates)
        {
            SpawnAgentFromTemplate(firm);
        }
    }

    private void SpawnAgentFromTemplate(EconAgent firm)
    {
        var instance = ScriptableObject.Instantiate(firm);
        EconAgentBuilder.Wrap(instance)
                        .Named($"{firm.Name}_{Guid.NewGuid().ToString("N")[..6]}")
                        .WithInitialCashFromTemplate()
                        .Build();
        initialMarket.RegisterMarketParticipant(instance);
    }

    private void CreatePopulations()
    {
        var populationTemplates = Resources.LoadAll<PopulationAgent>("EconAgents/Populations");
        foreach(var population in populationTemplates)
        {
            SpawnAgentFromTemplate(population);
        }
    }
#endregion

    #region Events
    public void CreateEvents()
    {
        var eventTemplates = Resources.LoadAll<MarketEventSO>("MarketEvents");
        foreach(var template in eventTemplates)
        {
            initialMarket.AddPotentialMarketEvent(template);
        }
    }
    #endregion
    private void WireUpButtons()
    {
        EndTurnButton.onClick.AddListener(EndTurn);
    }

    private void EndTurn()
    {
        TheEconomy.Instance.StartTradingPeriod();
        UpdateOrderPanelArrivingText("Goods have arrived");
        TheEconomy.Instance.EndTradingPeriod();
        ActionPointsText.text = TheEconomy.Instance.tradingPeriod.ToString();
        Debug.Log("End Turn");
    }

#region Initialization
    private void InitializeMarket()
    {
        //Refactor this at some point. No need for 'the first market'
        var testMarket = TheEconomy.Instance.GetMarketByName("The First Market");
        TheEconomy.Instance.RemoveMarket(testMarket);

        //This might need a cleaner solution. Episode 1 market is a scriptable Object stored in Resources
        initialMarket = TheEconomy.Instance.GetMarketByName("Episode 1 Market");
        initialMarket.SetCash(initialMarket.InitialCashInCents / 100);
        initialMarket.WithMarketInteractionManager(new Episode1InteractionManager());
        initialMarket.WithTradeProcessor(new DefaultTradeProcessor());
        LoadGoods();
        LoadRecipes();
        CreateNpcs();
        CreateEvents();
    }

    private void LoadGoods()
    {
        var goods = Resources.LoadAll<Good>("Goods").Where(x=>x.IsProducedGood==false);
        var period = TheEconomy.Instance.tradingPeriod;
        var inventory = initialMarket.GetInventory();

        var Lemon = goods.FirstOrDefault(x=>x.GoodName == "Lemon");
        Lemon.SetPrice((decimal)UnityEngine.Random.Range(1.0f, 3.0f));
        Lemon.SetExpiry(5);

        var Sugar = goods.FirstOrDefault(x=>x.GoodName == "Sugar");
        Sugar.SetPrice((decimal)UnityEngine.Random.Range(1.0f, 3.0f));
        Lemon.SetExpiry(5);

        var Water = goods.FirstOrDefault(x=>x.GoodName == "Water");
        Water.SetPrice((decimal)UnityEngine.Random.Range(1.0f, 3.0f));
        Lemon.SetExpiry(5);

        var lemonEntry = inventory.AddGood(new InventoryEntry(Lemon, 1000, Lemon.GetPrice(), period));
        lemonEntry.SetPrice(Lemon.GetPrice());
        var sugarEntry = inventory.AddGood(new InventoryEntry(Sugar, 1000, Sugar.GetPrice(), period));
        sugarEntry.SetPrice(Sugar.GetPrice());
        var waterEntry = inventory.AddGood(new InventoryEntry(Water, 1000, Water.GetPrice(), period));
        waterEntry.SetPrice(Water.GetPrice());
    }

    private void LoadRecipes()
    {
        var recipes = Resources.LoadAll<Recipe>("Recipes");
        BasicLemonadeRecipe= recipes.FirstOrDefault(x=>x.RecipeName==BasicLemonadeRecipeName);
    }
#endregion
    private void ClearTextScroll()
    {
        if(textScroll!=null) textScroll.text = "";
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
   
    private void CheckNews()
    {
        ClearUISelection();
        LogMessage($"It is Period : {TheEconomy.Instance.tradingPeriod}.");
        LogMessage($"You have {PlayerCompany.GetCash()} credits.");
        LogMessage($"The people in your neighbourhood are {initialMarket.GetEnnuiLevel()}");
        if(PlayerCompany.Recipes.Count()>0)
        {
            LogMessage("You have a recipe for: " + PlayerCompany.Recipes.FirstOrDefault(x=>x.RecipeName=="Basic Lemonade"));
        }
        else
            LogMessage("You have not discovered any recipes");
        LogMessage("The news is not available yet.");
    }
    private void DisplayInventory()
    {
        ClearUISelection();
        var inventory = PlayerCompany.GetInventory().GetInventoryEntries();
        LogMessage("Inventory:");
        foreach (var item in inventory)
        {
            LogMessage($"Item: {item.good} Quantity: {item.quantity} Acquired at: {item.Cost}");
        }
    }
    private void InitializePlayer()
    {
        PlayerCompany= EconAgent.Factory.Create("Player1",AgentLevelEnum.Beginner);
        PlayerCompany.IsPlayer= true;
        playerActionsRemaining = PlayerCompany.GetActionsRemaining();
        initialMarket.RegisterMarketParticipant(PlayerCompany);
        if(BasicLemonadeRecipe != null)
            PlayerCompany.AddRecipe(BasicLemonadeRecipe);
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

    private void UpdateOrderPanelArrivingText(string message)
        => OrderPanelArrivingText.text = message;
    private void UpdateOrderSummaryArrivingText(string message)
        => SummaryPanelText.text = message;

    private void SetLemonadePrice()
    {
        ClearUISelection();
        LogMessage("Cannot set Lemonade Price yet");
    }

#region helpers
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