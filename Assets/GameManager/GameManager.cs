using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public bool IsGameRunning { get; private set; }

    [SerializeField] private float splashDuration = 3f;
    [SerializeField] private readonly GameObject splashAnimation;
    [SerializeField] private readonly GameObject mainMenu;
    [SerializeField] private readonly GameObject gameScreen;


    ITradeLogger TradeLogger;

    int Period=0;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        IsGameRunning = true;

        if(TheEconomy.Instance == null)
        {
            Debug.Log("The Economy is not initialised. Add Economy to Scene.");
            return;
        }

        TradeLogger = new TradeLoggerV1();
        Period = TheEconomy.Instance.tradingPeriod;
        TheEconomy.Instance.Initialize(TradeLogger);
        Debug.Log("Game Manager Initialized");
    }
// Splash screen
    private void ShowSplashScreen()
    {
        splashAnimation.SetActive(true);
        mainMenu.SetActive(false);
        gameScreen.SetActive(false);

        StartCoroutine(TransitionToMainMenu());
    }

    private IEnumerator TransitionToMainMenu()
    {
        yield return new WaitForSeconds(splashDuration);
        splashAnimation.SetActive(false);
        mainMenu.SetActive(true);
    }

    // Game Flow
    public void StartTurn()
    {
        if(!IsGameRunning) return;

        Debug.Log($"Starting Turn {Period} ...");
        //UI code goes here
    }

    public void EndTurn()
    {
        if(!IsGameRunning) return;

        Debug.Log($"Ending Turn {Period} ...");
        TheEconomy.Instance.EndTradingPeriod();
        Period=TheEconomy.Instance.tradingPeriod;

        CheckGameStatus();
        //UI code goes here
    }

    private void CheckGameStatus()
    {
        //Win/Lose Conditions here
        throw new NotImplementedException();
    }

    public void TriggerEvent(string eventName)
    {
        Debug.Log($"Event Triggered : {eventName}");
    }

    public void EndGame(string message)
    {
        IsGameRunning = false;
        Debug.Log($"Game Over : {message}");
    }
}