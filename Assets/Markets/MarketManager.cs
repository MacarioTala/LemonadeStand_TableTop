using UnityEngine;

public class MarketManager : MonoBehaviour
{
    [SerializeField] private Market currentMarket;
    private MarketGrid grid;

    private void Awake()
    {
        grid = GetComponent<MarketGrid>();
    }

    private void Start()
    {
        if(currentMarket != null && grid != null)
        {
            grid.InitializeGrid();
        }
    }
 
}
