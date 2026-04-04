using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]

public class GameRoot : MonoBehaviour
{
    public GameObject TheEconomyGameObject;
    public TheEconomy EconomyInstance{get;private set;}
    public EventBus Bus{get; private set;}
    public static GameRoot Instance {get; private set;}
    private SubscriptionToken _loadSubscription;
    private ITradeLogger TradeLogger;
    [SerializeField] Recipe BasicLemonadeRecipe;
    private const string BasicLemonadeRecipeName = "Basic Lemonade";
    private const string InitialMarketName = "Episode 1 Market";
    private Market initialMarket = null;
    private EconAgent PlayerCompany=null;

    private void Start()
    {
        Debug.Log ("GameRoot starting. Economy Loaded. Loading Splash");
        Bus.Publish(new EconomyCoreReady(EconomyInstance),true);
        InitializeEconomy();
        InitializeContent();
        InitializePlayer();
        SceneManager.LoadScene(Scenes.Splash,LoadSceneMode.Single);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate GameRoot found, destroying...");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Bus = new EventBus();
        _loadSubscription = Bus.Subscribe<RequestLoadScene>(handler: req =>
        {
            Debug.Log($"Loading scene: {req.SceneName}");
            SceneManager.LoadScene(req.SceneName, LoadSceneMode.Single);
        }, replaySticky: false
        );
    }
#region Initialization
    private void InitializeEconomy()
    {
        TradeLogger = new TradeLoggerV1();
        EconomyInstance = TheEconomyGameObject.GetComponent<TheEconomy>();
        if (EconomyInstance == null)
            throw new Exception("TheEconomy component missing somehow, exiting.");
        EconomyInstance.Initialize(TradeLogger);
        EconomyInstance.InitializeEconomyForGame();
    }

    private void InitializeContent()
    {
        //This might need a cleaner solution. Episode 1 market is a scriptable Object stored in Resources
        initialMarket = EconomyInstance.GetMarketByName(InitialMarketName);
        initialMarket.SetCash(initialMarket.InitialCashInCents / 100);
        initialMarket.WithMarketInteractionManager(new Episode1InteractionManager());
        initialMarket.WithTradeProcessor(new DefaultTradeProcessor());
        LoadGoods();
        LoadRecipes();
        CreateNpcs();
        CreateMarketEvents();
     }
    private void LoadGoods()
    {
        var goods = Resources.LoadAll<Good>("Goods").Where(x=>x.IsProducedGood==false);
        var period = EconomyInstance.tradingPeriod;
        var inventory = initialMarket.GetInventory();

        foreach(var good in goods)
            inventory.AddGood(new InventoryEntry(good,1000,good.GetPrice(),period));

        foreach(var item in inventory.GetInventoryEntries())
            item.CalculatePriceFromBand();
    }

    private void LoadRecipes()
    {
        var recipes = Resources.LoadAll<Recipe>("Recipes");
        BasicLemonadeRecipe= recipes.FirstOrDefault(x=>x.RecipeName==BasicLemonadeRecipeName);
    }
    private void InitializePlayer()
    {
        var existingPlayer = EconomyInstance.EconomicAgents
                            .OfType<EconAgent>()
                            .FirstOrDefault(x=>x.IsPlayer);
        if(existingPlayer == null)
        {
            PlayerCompany= EconAgent.Factory.Create("Player1",AgentLevelEnum.Beginner);
            PlayerCompany.IsPlayer= true;
            initialMarket.RegisterMarketParticipant(PlayerCompany);
        }
        else 
        PlayerCompany = existingPlayer;
        
        if(BasicLemonadeRecipe != null)
            PlayerCompany.AddRecipe(BasicLemonadeRecipe);
    }
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
    private void CreatePopulations()
    {
        var populationTemplates = Resources.LoadAll<PopulationAgent>("EconAgents/Populations");
        foreach(var population in populationTemplates)
        {
            SpawnAgentFromTemplate(population);
        }
    }
    public void CreateMarketEvents()
    {
        var eventTemplates = Resources.LoadAll<MarketEventSO>("MarketEvents");
        foreach(var template in eventTemplates)
        {
            initialMarket.AddPotentialMarketEvent(template);
        }
    }

#endregion
#region Helpers
private void SpawnAgentFromTemplate(EconAgent firm)
    {
        var instance = ScriptableObject.Instantiate(firm);
        EconAgentBuilder.Wrap(instance)
                        .Named($"{firm.Name}_{Guid.NewGuid().ToString("N")[..6]}")
                        .WithInitialCashFromTemplate()
                        .Build();
        initialMarket.RegisterMarketParticipant(instance);
    }
#endregion
    void OnDestroy()
    {
        _loadSubscription.Dispose();
        Debug.Log($"[GR] OnDestroy id={GetInstanceID()} scene={gameObject.scene.name}", this);
    }
private void OnEnable() =>
    Debug.Log($"[GR] OnEnable id={GetInstanceID()} scene={gameObject.scene.name}", this);

private void OnApplicationQuit() =>
    Debug.Log($"[GR] OnApplicationQuit id={GetInstanceID()}", this);

private void LateUpdate()
{
    if (EconomyInstance == null)
        Debug.LogError($"[GR] EconomyInstance is NULL (unity-null) at frame {Time.frameCount}", this);
}
}
