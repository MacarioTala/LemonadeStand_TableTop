using System.Collections.Generic;
using UnityEngine;
using Sandbox;

public class Simulation : MonoBehaviour
{
    // Configuration
    public SimulationConfiguration config;
    
    // Prefabs
    [SerializeField] private GameObject companyRendererPrefab;
    [SerializeField] private GameObject moneyIndicatorPrefab;
    [SerializeField] private GameObject populationRendererPrefab;
    
    // Components
    private EconomyInitializer economyInitializer;
    private EntityFactory entityFactory;
    private MarketInitializer marketInitializer;
    private CompanyFactory companyFactory;
    private ActionExecutor actionExecutor;
    private SimulationReporter simulationReporter;
    private GoodIconMapping iconMapping;
    
    // Economy components
    private TheEconomy lemonadeEconomy;
    private Market lemonadeMarket;
    
    // Simulation state
    private int currentCycle = 0;
    private float lastCycleTime = 0f;
    private List<EconAgent> companies = new();
    private GameObject populationGO; // To keep track of the population view GameObject
    
    // Pause state
    private bool isPaused = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeSimulation();
    }

    private void InitializeSimulation()
    {
        // Initialize components
        economyInitializer = new EconomyInitializer();
        entityFactory = new EntityFactory();

        // Initialize economy
        lemonadeEconomy = economyInitializer.InitializeEconomy();

        // Create goods and recipes
        entityFactory.CreateGoodsAndRecipes();

        // Load the icon mapping from Resources, or create a new instance if not found
        iconMapping = Resources.Load<GoodIconMapping>("Goods/GoodIconMapping");
        if (iconMapping == null)
        {
            iconMapping = ScriptableObject.CreateInstance<GoodIconMapping>();
            Debug.LogWarning("GoodIconMapping asset not found in Resources. Using default instance.");
        }
        // Initialize market
        marketInitializer = new MarketInitializer(entityFactory.GetTestGoods(), lemonadeEconomy);
        lemonadeMarket = marketInitializer.InitializeMarket();

        // Create strategy for population agent
        var populationStrategy = StrategyBuilder.For<ReduceEnnuiStrategy>()
            .WithAggressionLevel(0.8m)
            .Build();

        // Create population agent and initialize demand for all goods
        var populationAgent = EconAgentBuilder.For<PopulationAgent>()
            .Named("Population")
            .WithPopulation(1000)
            .WithInitialCash(1000)
            .WithEnnui(0.9f)
            .WithBehaviourStrategy(populationStrategy)
            .WithFixedCostStrategy(new BasicFixedCostStrategy())
            .AssumingNewGoodsCost(1m)
            .Build();
        lemonadeMarket.RegisterMarketParticipant(populationAgent);

        // Initialize demand for all goods in the population agent
        foreach (var good in entityFactory.GetTestGoods())
        {
            populationAgent.InitializeDemandBasedOnPopulation(good, 0.8f, 1, 10000);
        }
        
        // Generate goals for the population agent after demand is initialized
        populationStrategy.GenerateGoals(populationAgent);

        // Create and initialize PopulationView
        if (populationRendererPrefab != null)
        {
            populationGO = GameObject.Instantiate(populationRendererPrefab, new Vector3(0, -3, 0), Quaternion.identity);
            populationGO.GetComponent<PopulationView>().Init(populationAgent, entityFactory.GetTestGoods(), iconMapping);
        }

        // Create companies
        companyFactory = new CompanyFactory(entityFactory.GetTestGoods(), companyRendererPrefab, iconMapping, moneyIndicatorPrefab, lemonadeMarket);
        companies = companyFactory.CreateTestCompanies(lemonadeEconomy);
        // Initialize action executor
        actionExecutor = new ActionExecutor(
            entityFactory.GetTestGoods(),
            entityFactory.GetLemonadeRecipe(),
            companies,
            lemonadeMarket
        );

        // Initialize reporter
        simulationReporter = new SimulationReporter(
            entityFactory.GetTestGoods(),
            companies,
            lemonadeMarket
        );
        
        simulationReporter.GenerateSummary(config.totalNumberOfCycles, config.tradesPerCycle);
    }

    // Update is called once per frame
    void Update()
    {
        // Check if simulation is paused
        if (isPaused) return;
        
        // Run the simulation loop with speed control
        if (currentCycle < config.totalNumberOfCycles)
        {
            if (Time.time - lastCycleTime >= config.simulationSpeed)
            {
                Debug.Log("Starting cycle:" + currentCycle);
                for (int i = 0; i < config.tradesPerCycle; i++) 
                {
                    actionExecutor.PerformRandomAction(currentCycle);
                }
                
                try
                {
                    lemonadeMarket.StartTradingPeriod(); // This triggers population agents to create buy orders
                    lemonadeMarket.ProcessCompanyOrders();
                    lemonadeMarket.UnleashMarketForces(currentCycle); // Add this line to process market forces including population consumption
                }
                catch (System.Exception e)
                {
                    Debug.Log("Error in cycle:" + currentCycle + " " + e.Message + " " + e.StackTrace);
                }
                
                Debug.Log("Cycle:" + currentCycle + " completed");
                currentCycle++;
                lastCycleTime = Time.time;
            }
        }
        else if (currentCycle == config.totalNumberOfCycles)
        {
            // Simulation completed
            Debug.Log("Simulation completed after " + config.totalNumberOfCycles + " cycles");
            simulationReporter.GenerateSummary(config.totalNumberOfCycles, config.tradesPerCycle);
            currentCycle++; // Prevent running again
        }
    }

    public void ResetSimulation()
    {
        // Reset simulation parameters
        currentCycle = 0;
        lastCycleTime = 0f;
        
        // Clear existing companies
        companies.Clear();
        
        // Destroy company prefabs if companyFactory exists
        if (companyFactory != null)
        {
            companyFactory.DestroyAllCompanyPrefabs();
        }
        
        // Destroy population view if it exists
        if (populationGO != null)
        {
            GameObject.Destroy(populationGO);
        }
        
        // Reinitialize the simulation
        InitializeSimulation();
        
        Debug.Log("Simulation reset completed.");
    }
    
    /// <summary>
    /// Toggles the pause state of the simulation
    /// </summary>
    public void TogglePause()
    {
        isPaused = !isPaused;
        Debug.Log("Simulation " + (isPaused ? "paused" : "resumed"));
    }
    
    /// <summary>
    /// Returns the current pause state of the simulation
    /// </summary>
    /// <returns>True if the simulation is paused, false otherwise</returns>
    public bool IsPaused()
    {
        return isPaused;
    }
}
