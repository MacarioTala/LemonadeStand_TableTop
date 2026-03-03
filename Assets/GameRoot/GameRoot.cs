using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(TheEconomy))]
public class GameRoot : MonoBehaviour
{
    public TheEconomy EconomyInstance{get;private set;}
    public EventBus Bus{get; private set;}
    public static GameRoot Instance {get; private set;}
    private SubscriptionToken _loadSubscription;
    private ITradeLogger TradeLogger;


    private void Start()
    {
        Debug.Log ("GameRoot starting. Economy Loaded. Loading Splash");
        Bus.Publish(new EconomyCoreReady(EconomyInstance),true);
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
        InitializeEconomy();
    
        _loadSubscription = Bus.Subscribe<RequestLoadScene>(handler: req =>
        {
            Debug.Log($"Loading scene: {req.SceneName}");
            SceneManager.LoadScene(req.SceneName, LoadSceneMode.Single);
        }, replaySticky: false
        );
    }

    private void InitializeEconomy()
    {
        TradeLogger = new TradeLoggerV1();
        EconomyInstance = GetComponent<TheEconomy>();
        if (EconomyInstance == null)
            throw new Exception("TheEconomy component missing somehow, exiting.");
        EconomyInstance.Initialize(TradeLogger);
    }

    void OnDestroy()
    {
        _loadSubscription.Dispose();
    }

}
