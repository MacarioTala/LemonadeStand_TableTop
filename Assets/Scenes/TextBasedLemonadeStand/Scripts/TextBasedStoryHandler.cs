using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class TextBasedStoryHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textScroll;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Button EndTurnButton;
    public static TextBasedStoryHandler Instance { get; private set; }
#region Game Variables
    private Company PlayerCompany;
    private bool isWaitingForPlayerInput = false;
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
        if(isWaitingForPlayerInput && Input.GetKeyDown(KeyCode.Space))
        {
            isWaitingForPlayerInput = false;
            ClearTextScroll();
            DisplayChoices();
        }
        ChooseFromDailyMenu();
    }

    private void ChooseFromDailyMenu()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) Choose(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) Choose(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) Choose(3);
        if (Input.GetKeyDown(KeyCode.Alpha4)) Choose(4);
    }

    public void StartTextBasedGame()
    {
        InitializePlayer();
        if (Instance == null)
        {
            Debug.Log("TextBasedStoryHandler is not initialized.");
            return;
        }
        Instance.StartCoroutine(Instance.StartGameLoop());
    }

    private void InitializePlayer()
    {
        PlayerCompany= Company.Factory.Create("Player1",CompanyLevelEnum.Beginner);
    }

    private IEnumerator StartGameLoop()
    {
        textScroll.text = "";
        LogMessage("Welcome to Lemonade Stand!");
        yield return new WaitForSeconds(2);
        LogMessage("Can you save Capitalism?");
        yield return new WaitForSeconds(2);
        LogMessage("Let's find out!");
        yield return new WaitForSeconds(2);
        DisplayChoices();
    }

    private void DisplayChoices()
    {
        LogMessage($"You have {PlayerCompany.GetCash()} credits.");
        LogMessage("What would you like to do?");
        LogMessage("1. Check Inventory");
        LogMessage("2. Set Lemonade Price");
        LogMessage("3. Order Supplies");
        LogMessage("4. Check the news");
    }

    private void ClearTextScroll()
    {
        if(textScroll!=null) textScroll.text = "";
    }
    public void Choose(int choice)
    {
        switch (choice)
        {
            case 1:
                DisplayInventory();
                break;
            case 2:
                SetLemonadePrice();
                break;
            case 3:
                OrderSupplies();
                break;
            case 4:
                CheckNews();
                break;
            default:
                LogMessage("Invalid choice. Please choose again.");
                DisplayChoices();
                break;
        }
    }

    private void CheckNews()
    {
        LogMessage("The news is not available yet.");
        isWaitingForPlayerInput = true;
        LogMessage("Press Space to continue.");
    }
    private void DisplayInventory()
    {
        var inventory = PlayerCompany.GetInventory().GetInventoryEntries();
        LogMessage("Inventory:");
        foreach (var item in inventory)
        {
            LogMessage($"Item: {item.good} Quantity: {item.quantity} Acquired at: {item.Cost}");
        }
        isWaitingForPlayerInput = true;
        LogMessage("Press Space to continue.");
    }

    private void SetLemonadePrice()
    {
        LogMessage("Cannot set Lemonade Price yet");
        isWaitingForPlayerInput = true;
        LogMessage("Press Space to continue.");
    }

    private void OrderSupplies()
    {
        LogMessage("Cannot order supplies yet");
        isWaitingForPlayerInput = true;
        LogMessage("Press Space to continue.");
    }

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
    
}
