using UnityEngine;

public class UIHandlerScript : MonoBehaviour
{
    public GameObject ZorkView;
    public GameObject SplashCanvas;

    // Start is called before the first frame update
    void Start()
    {
        ZorkView.SetActive(false);
        SplashCanvas.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
