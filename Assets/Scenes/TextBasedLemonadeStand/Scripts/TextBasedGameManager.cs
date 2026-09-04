using TMPro;
using UnityEngine;

public class TextBasedGameManager : MonoBehaviour
{
 #region Serialized Fields
    [SerializeField] private GameObject SplashCanvas;
    [SerializeField] private GameObject splashTypewriterPrefab;
    [SerializeField] private AudioSource splashScreenAudioSource;
    [SerializeField] private AudioClip splashScreenSoundClip;
    [SerializeField] private TextMeshProUGUI typeWrittenText;
    [SerializeField] private TextMeshProUGUI cursor;
    private readonly string gameSceneName = Scenes.ZorkView;
    private readonly string craftingSceneName = Scenes.Crafting;
#endregion
    public static TextBasedGameManager Instance { get; private set; }
    private TypeWriter typeWriterInstance;
    public TheEconomy TheEconomyInstance{get; private set;}
#region Game Variables
    public bool IsGameRunning { get; private set; }
    private bool IsSceneOnly=true;
    int Period = 0;
    private Coroutine typingCoroutine;
#endregion
   

    private void Start()
    {
        Initialize();
        if(typeWriterInstance == null)
        {
            Debug.LogError("Typewriter instance missing. Did you forget to wire a prefab in the Inspector?");
            return;
        }
        else
        {
            typeWriterInstance.Initialize(typeWrittenText, splashScreenAudioSource, splashScreenSoundClip);
         }
        //StartCoroutine(BlinkCursor()); //Fix this as soon as we have good game flow.
        typingCoroutine = StartCoroutine(typeWriterInstance.TypeText(TypeWriter.splashMessage));
    }

    private void Initialize()
    {
        IsGameRunning = true;

        if(splashTypewriterPrefab != null)
        {
            var typeWriterInstanceObject = Instantiate(splashTypewriterPrefab);
            typeWriterInstance = typeWriterInstanceObject.GetComponent<TypeWriter>();
        }

        if(GameRoot.Instance == null)
        {
            Debug.LogWarning("Gameroot is missing. Playing in scene-only mode");
        }
        else
        {
            TheEconomyInstance = GameRoot.Instance.EconomyInstance;
            if(TheEconomyInstance == null)
            {
                Debug.LogError("TheEconomyInstance is null on GameRoot. Exiting");
                return;
            }
            Period = TheEconomyInstance.TradingPeriod;
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
#endregion
#region Main Menu
    public void StartNewGame()
    {   
        Debug.Log("Starting New Game");
        if(IsSceneOnly)
            Debug.Log("Running in Scene-only mode");
        else
            GameRoot.Instance.Bus.Publish(new RequestLoadSceneEvent(gameSceneName));
    }

    public void StartCrafting()
    {
        Debug.Log("Starting Crafting");
        GameRoot.Instance.Bus.Publish(new RequestLoadSceneEvent(craftingSceneName));
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
       Instance = this;
       if(GameRoot.Instance!=null)
        IsSceneOnly=false;
    }
   
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)) 
        {
            SkipTypeWriter();
        }
    }
    
    #endregion
}