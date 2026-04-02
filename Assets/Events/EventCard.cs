using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI EventTitle;
    [SerializeField] private TextMeshProUGUI EventText;
    [SerializeField] private TextMeshProUGUI FlavourText;
    [SerializeField] private Image Image;

    public void Display (MarketEventSO so)
    {
        EventTitle.text = so.EventName;
        EventText.text = so.EventDescription;
        FlavourText.text=so.FlavourText;
        Image.sprite=so.EventSprite;
    }

}