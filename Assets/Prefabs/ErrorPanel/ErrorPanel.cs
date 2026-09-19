using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ErrorPanel : MonoBehaviour
{
    private GameObject decisionBar;
    private Button answer;
    private Button ignore;
    private TextMeshProUGUI notificationText;
    private const int callWaitInterval=5;
    
    private void Awake()
    {
        decisionBar=transform.Find("Decision").gameObject;
        answer=transform.Find("Decision/Answer").GetComponent<Button>();
        ignore=transform.Find("Decision/Ignore").GetComponent<Button>();
        notificationText=transform.Find("ErrorText").GetComponent<TextMeshProUGUI>();
        notificationText.color = Constants.TextPanelGreen;
        notificationText.text=string.Empty;
        decisionBar.SetActive(false);
    }

    public IEnumerator IncomingCall(string text,System.Action onAnswer)
    {
        bool resolved = false;

        notificationText.text = text;
        decisionBar.SetActive(true);

        //Listeners
        answer.onClick.RemoveAllListeners();
        ignore.onClick.RemoveAllListeners();

        answer.onClick.AddListener(()=>{
            resolved=true;
            onAnswer?.Invoke();
            });

        ignore.onClick.AddListener(()=>resolved=true);

        float timeout = Time.time+callWaitInterval;
        float nextBlink = Time.time+.4f;

        yield return new WaitUntil(
            ()=> {
                    if(Time.time>=nextBlink)
                    {
                        notificationText.enabled = !notificationText.enabled;
                        nextBlink+=.4f;
                    }
                    return resolved|| Time.time >= timeout;
                }
            );

        notificationText.enabled=true;
        decisionBar.SetActive(false);
        notificationText.text = string.Empty;
    }
}
