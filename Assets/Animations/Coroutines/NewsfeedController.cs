using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using System;

public class NewsfeedController : MonoBehaviour
{
    [SerializeField] private Image NewsImage;
    [SerializeField] private Sprite BreakingNewsSprite;
    [SerializeField] private Sprite NewsAnchorSprite;
    [SerializeField] private Image StaticOverlay;
    [SerializeField] private Image NewsFeed;
    [SerializeField] private GameObject EventSplash;
    [SerializeField] private GameObject CloseButton;

    [SerializeField] private float WinkDuration =.2f;
    [SerializeField] private float holdDuration = 2f;
    [SerializeField] private Image Screen;

    private Button newsbutton;
    private Button closebutton;

    private void Awake()
    {
        NewsImage.transform.localScale = Vector3.zero;
        Screen.transform.localScale = Vector3.zero;
        EventSplash.SetActive(false);
        newsbutton = NewsFeed.GetComponent<Button>();
        closebutton = CloseButton.GetComponent<Button>();
    }

    void Start()
    {
        NewsFeed.GetComponent<Button>().onClick.AddListener(ShowEventSplash);
        closebutton.onClick.AddListener(CloseEventSplash);
    }

    private void CloseEventSplash()
    {
        EventSplash.SetActive(false);
    }

    public void OnEventFired()
    {
        newsbutton.interactable = true; 
    }

    public void OnEventSplashClosed()
    {
        newsbutton.interactable = false;
    }
    private void ShowEventSplash()
    {
        EventSplash.SetActive(true);
    }

    public void PlayBreakingNews()
    {
        Screen.transform.localScale = Vector3.one;   
        StopAllCoroutines();
        StartCoroutine(BreakingNewsRoutine());
    }

    public IEnumerator BreakingNewsRoutine()
    {
        NewsImage.sprite = BreakingNewsSprite;
        
        //Wink On
        yield return StartCoroutine(WinkOn());
        //Hold Image
        yield return new WaitForSeconds(holdDuration);
        //Swap to News Anchor
        yield return StartCoroutine(GlitchIn(NewsAnchorSprite));
    }

    private IEnumerator GlitchIn(Sprite newsAnchor)
    {
        StaticOverlay.gameObject.SetActive(true);

        //flicker
        var glitchTime = .3f;
        var elapsed = 0f;

        while(elapsed < glitchTime)
        {
            elapsed += Time.deltaTime;

            //Horizontal jitter
            var x = UnityEngine.Random.Range(-8f,8f);
            var y = UnityEngine.Random.Range(-3f,3f);
            NewsImage.rectTransform.anchoredPosition = new Vector2(x,y);

            //Horizontal compression
            NewsImage.rectTransform.localScale = 
                new Vector3(1f, UnityEngine.Random.Range(0.8f, 1.1f), 1f);

            //Random Alpha flicker
            StaticOverlay.color = new Color(1,1,1, UnityEngine.Random.Range(.4f,1f));
            yield return null;
        }

        //Stabilise Image
        NewsImage.rectTransform.anchoredPosition = Vector2.zero;
        StaticOverlay.gameObject.SetActive(false);

        NewsImage.sprite = newsAnchor;
    }
    private IEnumerator WinkOn()
    {
        var elapsed = 0f;
        var start = Vector3.zero;
        var end = Vector3.one;

        while(elapsed < holdDuration)
        {
            elapsed+=Time.deltaTime;
            var t = elapsed/WinkDuration;
            
            //Ease out
            t = 1f - Mathf.Pow(1f-t,3f);

            NewsImage.transform.localScale = Vector3.Lerp(start,end,t);
            yield return null;
        }
        NewsImage.transform.localScale = Vector3.one;
    }
    
}