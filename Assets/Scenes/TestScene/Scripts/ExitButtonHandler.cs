using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExitButtonHandler : MonoBehaviour
{
    [SerializeField] private GameObject StandPOVView;
    [SerializeField] private GameObject NeighbourhoodGrid;
    [SerializeField] private GameObject ExitButton;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(ExitToNeighbourhood);
        Debug.Log($"ExitButton {ExitButton.gameObject.activeSelf} is active.");
    }

    private void ExitToNeighbourhood()
    {
        StandPOVView.SetActive(false);
        NeighbourhoodGrid.SetActive(true);
    }
}
