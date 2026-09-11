using System;
using System.Collections.Generic;
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
    private IEnumerable<FixedCostTemplate> fixedCostTemplates;
    private List<Good> _elements;
    private List<Good> _products;
    private readonly Dictionary<string,Combination> _combinations=new();

    private void Start()
    {
        Debug.Log ("GameRoot starting. Economy Loaded. Loading Splash");
        InitializeEconomy();
        InitializeContent();
        InitializePlayer();
          Bus.Publish(new EconomyCoreReadyEvent(EconomyInstance),true);
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
        _loadSubscription = Bus.Subscribe<RequestLoadSceneEvent>(handler: req =>
        {
            Debug.Log($"Loading scene: {req.SceneName}");
            SceneManager.LoadScene(req.SceneName, LoadSceneMode.Single);
        }, replaySticky: false
        );
    }
#region Public Methods
    public IReadOnlyList<Good> GetElements() => _elements;
    public IReadOnlyList<Good> GetProducts() => _products;
    public IReadOnlyDictionary<string,Combination> GetCombinations()=>_combinations;
#endregion
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
        LoadFixedCosts();
        CreateNpcs();
        CreateMarketEvents();
    }
    private void LoadFixedCosts()
    {
        fixedCostTemplates = Resources.LoadAll<FixedCostTemplate>("FixedCosts");
    }
    private void LoadGoods()
    {
        LoadElements();
        LoadProducts();
        LoadGoodCombinationsFromResources();
    }
private void LoadGoodCombinationsFromResources()
    {
        var assetPath = "GoodCombinations";
        var csv = Resources.Load<TextAsset>(assetPath);

        var lines = csv.text.Split("\n");
        foreach (var line in lines.Skip(1))
        {
            if(string.IsNullOrWhiteSpace(line)) continue;

            var columns = line.Trim().Split(",");

            var ingredient1 = columns[0].Trim();
            var ingredient2 = columns[1].Trim();
            var product = columns[2].Trim();
            var junk = columns[3].Trim();
            int.TryParse(columns[4].Trim(),out var percentageJunk);
            int.TryParse(columns[5].Trim(), out var recipeDiscoveryChance);
            int.TryParse(columns[6].Trim(),out var ingredient1Needed);
            int.TryParse(columns[7].Trim(),out var ingredient2Needed);
            int.TryParse(columns[8].Trim(),out var producesQty);
            int.TryParse(columns[9].Trim(),out var producesQtyJunk);
            int.TryParse(columns[10].Trim(),out var maxQtyWithoutRecipe);

            if(string.IsNullOrWhiteSpace(product)) continue;

            var key = ETLHelper.MakeKey(ingredient1,ingredient2);
            _combinations.Add(key,new Combination()
                    {
                        Ingredient1=ingredient1,
                        Ingredient2=ingredient2,
                        Product=product,
                        Junk=junk,
                        PercentageJunk=percentageJunk,
                        RecipeDiscoveryChance=recipeDiscoveryChance,
                        Ingredient1Needed=ingredient1Needed,
                        Ingredient2Needed=ingredient2Needed,
                        ProducesQty=producesQty,
                        ProducesQtyJunk=producesQtyJunk,
                        MaxQtyWithoutRecipe=maxQtyWithoutRecipe
                        });
        }
    }
    private void LoadElements()
    {
        _elements = Resources.LoadAll<Good>("Goods").Where(x=>x.IsProducedGood==false).ToList();
        const string elementsPath = "ElementaryGoodsList";
        var elementsCSV = Resources.Load<TextAsset>(elementsPath);
        var lines = elementsCSV.text.Split('\n');

        foreach(var line in lines.Skip(1))
        {
            var columns = line.Split(new[] {','},6);
            var goodName = columns[0].Trim();

            var currentGood = _elements.FirstOrDefault(x=>x.GoodName == goodName);

            //Set Element characteristics here
            var sprite = Resources.Load<Sprite>($"Art/{columns[1]}");
            Enum.TryParse<RarityEnum>(columns[2].Trim(),out var rarity);
            int.TryParse(columns[3].Trim(),out var minPrice);
            int.TryParse(columns[4].Trim(),out var maxPrice);
            var tooltip = columns[5].Trim();

            currentGood.GoodSprite=sprite;
            currentGood.SetRarity(rarity);
            currentGood.SetPriceBand(minPrice,maxPrice);
            currentGood.Tooltip = tooltip;
        }
    }
    private void LoadProducts()
    {
        _products = Resources.LoadAll<Good>("Goods").Where(x=>x.IsProducedGood).ToList();
        const string productsPath = "ProductAttributes";
        var productsCSV = Resources.Load<TextAsset>(productsPath);
        var lines=productsCSV.text.Split('\n');

        foreach(var line in lines.Skip(1))
        {
            var columns = line.Split(new[]{','});
            var goodName = columns[0].Trim();
            if(string.IsNullOrEmpty(goodName)) continue;

            var currentGood = _products.FirstOrDefault(x=>x.GoodName==goodName);

            //Set product characteristics here
            var sprite = Resources.Load<Sprite>($"Art/{goodName.ToLower()}");
            var rarity = RarityEnum.Produced;
            var tooltip = columns[7];

            if(sprite != null) currentGood.GoodSprite=sprite;

            currentGood.SetRarity(rarity);
        }
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
            var playerTemplate = Resources.Load<EconAgent>("EconAgents/Player/Player");
            PlayerCompany = SpawnAgentFromTemplate(playerTemplate);
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
private EconAgent SpawnAgentFromTemplate(EconAgent agent)
    {
        string agentName;
        if (agent.IsPlayer)
            agentName = agent.Name;
        else if (agent is PopulationAgent)
            agentName = $"{NameGenerator.GeneratePopulationName()}";
        else
            agentName = $"{NameGenerator.GenerateCompanyName()}_{Guid.NewGuid().ToString("N")[..6]}";

        var fixedCostStrategy = new BasicFixedCostStrategy();
        var instance = ScriptableObject.Instantiate(agent);
        EconAgentBuilder.Wrap(instance)
                        .Named(agentName)
                        .WithInitialCashFromTemplate()
                        .WithFixedCostStrategy(fixedCostStrategy)
                        .WithInitialRecipes()
                        .Build();
        
        instance.LoadFixedCostsFromTemplates(fixedCostTemplates ?? Enumerable.Empty<FixedCostTemplate>(),initialMarket.CurrentPeriod);

        initialMarket.RegisterMarketParticipant(instance);
        
        if(!instance.IsPlayer)
            {
                instance.SetStrategy();
            }

         
        return instance;
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
