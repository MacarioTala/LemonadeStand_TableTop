using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TextBasedGameManager : MonoBehaviour
{
 #region Serialized Fields
    [SerializeField] private GameObject ZorkView;
    [SerializeField] private GameObject SplashCanvas;
    [SerializeField] private float splashDuration = 3f;
    private readonly GameObject splashAnimation;
    private readonly GameObject mainMenu;
    [SerializeField] private GameObject splashTypewriterPrefab;
    [SerializeField] private AudioSource splashScreenAudioSource;
    [SerializeField] private AudioClip splashScreenSoundClip;
    [SerializeField] private TextMeshProUGUI typeWrittenText;
    [SerializeField] private TextMeshProUGUI cursor;
    [SerializeField] private float blinkSpeed = 0.5f;
    private readonly string gameSceneName = Scenes.ZorkView;
    [SerializeField] private GameState gameState=GameState.Splash;
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
   

    private void Start()
    {
        Initialize();
        typeWriterInstance.Initialize(typeWrittenText, splashScreenAudioSource, splashScreenSoundClip);
        //StartCoroutine(BlinkCursor()); //Fix this as soon as we have good game flow.
        typingCoroutine = StartCoroutine(typeWriterInstance.TypeText(TypeWriter.splashMessage));
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
            typeWrittenText.text = isCursorVisible ? "|" : "";
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
        Debug.Log("Starting New Game");
        gameState = GameState.Zork;
        SceneManager.LoadScene(gameSceneName);
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

    #region Overloads
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDestroy()
    {
        if(Instance == this) SceneManager.sceneLoaded -=OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name!=gameSceneName) return;
        
        TextBasedStoryHandler found =null;

        foreach(var root in scene.GetRootGameObjects())
        {
            found = root.GetComponentInChildren<TextBasedStoryHandler>(true);
            if(found != null) break;
        }    
            if(found == null)
            {
                Debug.LogError($"Story Handler not found in scene: "+scene.name);
                return;
            }

            found.StartTextBasedGame();
    }
    private void Update()
    {
        if(
            gameState == GameState.Splash &&
            Input.GetKeyDown(KeyCode.Space)) 
        {
            SkipTypeWriter();
        }
    }
    
    #endregion
}

public enum GameState{Splash,Zork}
public static class Scenes
{
    public const string Splash = "Splash";
    public const string ZorkView = "ZorkScene";
}