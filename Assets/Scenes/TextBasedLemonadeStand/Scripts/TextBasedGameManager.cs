using System.Collections;
using TMPro;
using UnityEngine;

public class TextBasedGameManager : MonoBehaviour
{
 #region Serialized Fields
    [SerializeField] private GameObject ZorkView;
    [SerializeField] private GameObject SplashCanvas;
    [SerializeField] private float splashDuration = 3f;
    [SerializeField] private readonly GameObject splashAnimation;
    [SerializeField] private readonly GameObject mainMenu;
    [SerializeField] private readonly GameObject gameScreen;
    [SerializeField] private GameObject splashTypewriterPrefab;
    [SerializeField] private AudioSource splashScreenAudioSource;
    [SerializeField] private AudioClip splashScreenSoundClip;
    [SerializeField] private TextMeshProUGUI typeWrittenText;
    [SerializeField] private TextMeshProUGUI cursor;
    [SerializeField] private float blinkSpeed = 0.5f;
    TextBasedStoryHandler storyHandler;
#endregion
    public static TextBasedGameManager Instance { get; private set; }
    private TypeWriter typeWriterInstance;
    public TheEconomy TheEconomy;
    private ITradeLogger TradeLogger;
#region Game Variables
    public bool IsGameRunning { get; private set; }
    int Period = 0;
    private Coroutine typingCoroutine;
#endregion

    private bool isCursorVisible = true;
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
            return;
        }

        storyHandler = FindFirstObjectByType<TextBasedStoryHandler>();
        
        if (storyHandler == null)
        {
            Debug.Log("Story Handler not found. Cannot start game.");
            return;    
        }
        if(ZorkView != null) ZorkView.SetActive(false);
    }

    private void Start()
    {
        Initialize();
        typeWriterInstance.Initialize(typeWrittenText, splashScreenAudioSource, splashScreenSoundClip);
//        StartCoroutine(BlinkCursor()); Fix this as soon as we have good game flow.
        typingCoroutine = StartCoroutine(typeWriterInstance.TypeText(TypeWriter.splashMessage));
    }

    private void Update()
    {
        if(SplashCanvas.activeSelf && Input.GetKeyDown(KeyCode.Space)) 
        {
            SkipTypeWriter();
        }
    }

    private void Initialize()
    {
        IsGameRunning = true;

        if(TheEconomy == null)
        {
            try{
                TheEconomy = FindFirstObjectByType<TheEconomy>();
            }
            catch (System.Exception)
            {
                Debug.Log("The Economy is not initialised. Add Economy to Scene.");
                return;
            }
        }

        TradeLogger = new TradeLoggerV1();
        Period = TheEconomy.Instance.tradingPeriod;
        TheEconomy.Initialize(TradeLogger);

        if(splashTypewriterPrefab != null)
        {
            var typeWriterInstanceObject = Instantiate(splashTypewriterPrefab);
            typeWriterInstance = typeWriterInstanceObject.GetComponent<TypeWriter>();
        }

        Debug.Log("Text Based Game Manager Initialized");
    }
#region Splash Screen
    public TextMeshProUGUI GetTypeWrittenText()
    {
        return typeWrittenText;
    }

    private void SkipTypeWriter()
    {
        if(typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typeWrittenText.text = "";
            typeWriterInstance.StopTyping();
        }
    }

    private IEnumerator BlinkCursor()
    {
        while (true)
        {
            cursor.text = isCursorVisible ? "|" : "";
            isCursorVisible = !isCursorVisible;
            yield return new WaitForSeconds(blinkSpeed);
        }
    }
    private IEnumerator TransitionToMainMenu()
    {
        yield return new WaitForSeconds(splashDuration);
        splashAnimation.SetActive(false);
        mainMenu.SetActive(true);
    }
#endregion
#region Main Menu
    public void StartNewGame()
    {
        
        if(ZorkView == null)
        {
            Debug.Log("ZorkView not found. Cannot start new game.");
            return;
        }

        SplashCanvas.SetActive(false);
        ZorkView.SetActive(true);
        storyHandler.StartTextBasedGame();
        Debug.Log("Starting New Game");
    }
#endregion
#region Game Loop

    public void StartTurn()
    {
        if(!IsGameRunning)
        {
            Debug.Log("Game is not running. Cannot start turn.");
            return;
        }

        Debug.Log($"Starting Turn {Period}");
    }

    public void EndTurn()
    {
        if(!IsGameRunning)
        {
            Debug.Log("Game is not running. Cannot end turn.");
            return;
        }

        Debug.Log($"Ending Turn {Period}");
        TheEconomy.Instance.EndTradingPeriod();
        Period = TheEconomy.Instance.tradingPeriod;

        CheckGameStatus();
    }
    private void CheckGameStatus()
    {
        //Win/Lose Conditions here
        Debug.Log("Checking Game Status");
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
#endregion
}
