using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI countText;

    public void SetItem(Sprite icon, int count)
    {
        if (iconImage != null)
            iconImage.sprite = icon;

        if (countText != null)
            countText.text = count.ToString();
    }
}
