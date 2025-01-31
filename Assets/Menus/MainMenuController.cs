using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button exitGameButton;
    [SerializeField] private Button settingsButton;
    Animator mainMenuAnimator;
    [SerializeField] GameObject mainMenu;

    private void Start()
    {
        newGameButton.onClick.AddListener(StartNewGame);
        loadGameButton.onClick.AddListener(LoadGame);
        exitGameButton.onClick.AddListener(ExitGame);
        settingsButton.onClick.AddListener(ShowSettings);

        var canvasGroup = mainMenu.GetComponent<CanvasGroup>();
        mainMenuAnimator = mainMenu.GetComponent<Animator>();

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        mainMenuAnimator.Play("Idle", 0, 0);
    }

    private void Awake()
    {
        TypeWriter.OnTypeWriterFinished += ShowMainMenu;
    }

    private void OnDestroy()
    {
        TypeWriter.OnTypeWriterFinished -= ShowMainMenu;
    }

    private void ShowSettings()
    {
        throw new NotImplementedException();
    }

    private void ExitGame()
    {
        Debug.Log("Exiting Game");
    }

    private void LoadGame()
    {
        Debug.Log("Loading Game");
    }

    private void StartNewGame()
    {
        Debug.Log("Starting New Game");
    }
    private void ShowMainMenu()
    {
        StartCoroutine(WinkInMainMenu());
    }

    private IEnumerator WinkInMainMenu()
    {
        var canvasGroup = mainMenu.GetComponent<CanvasGroup>();
        mainMenu.SetActive(true);
        Debug.Log($"Before WinkIn: {mainMenuAnimator.GetBool("WinkIn")}");
        mainMenuAnimator.SetBool("WinkIn", true);
        Debug.Log($"After WinkIn: {mainMenuAnimator.GetBool("WinkIn")}");

        const float duration =.5f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        mainMenuAnimator.SetBool("WinkIn", false);
    }
}
