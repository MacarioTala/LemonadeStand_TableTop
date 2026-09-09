using UnityEngine;
using UnityEngine.UI;


public class BigRedButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _image;
    private void Awake()
    {
        _button.onClick.AddListener(Transmogrify);   
    }

    private void Transmogrify()
    {
        Debug.Log("Transmogrify!");
    }
}
