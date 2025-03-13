using UnityEngine;
using UnityEngine.UI;

public class OrderSummaryPopupHandler : MonoBehaviour
{
    [SerializeField] private GameObject OrderSummaryPanel;
    [SerializeField] private GameObject OrderSummaryText;
    [SerializeField] private GameObject CloseButton;
    // Start is called before the first frame update
    void Start()
    {
        OrderSummaryPanel.SetActive(false);
        CloseButton.GetComponent<Button>().onClick.AddListener(CloseOrderSummary);
    }

    public void ShowOrderSummary()
    {
        Debug.Log("Showing Order Summary"+OrderSummaryPanel.activeInHierarchy);
        OrderSummaryPanel.SetActive(true);
    }

    public void CloseOrderSummary()
    {
        OrderSummaryPanel.SetActive(false);
    }  

}
