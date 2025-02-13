using UnityEngine;

public class TextBoxAnimator : MonoBehaviour
{
    private Animator textBoxAnimator;
    
    private void Start()
    {
        var stateInfo = textBoxAnimator.GetCurrentAnimatorStateInfo(0);
        textBoxAnimator.Play("Idle");
        Debug.Log($"Current Animator State is: {stateInfo.IsName("Idle")}");
    }

    private void Update()
    {
        var stateInfo = textBoxAnimator.GetCurrentAnimatorStateInfo(0).IsName("SlideLeft");
        if(stateInfo)
        {
            Debug.Log($"SlideLeft somehow being called");
        }
    }
    private void Awake()
    {
        textBoxAnimator = GetComponent<Animator>();
        textBoxAnimator.Play("Idle",0,0);
    }

    private void OnEnable()
    {
        TypeWriter.OnTypeWriterFinished += TextBoxSlideLeft;
    }

    private void OnDisable()
    {
        TypeWriter.OnTypeWriterFinished -= TextBoxSlideLeft;
    }
    private void TextBoxSlideLeft()
    {
        Debug.Log("Animation fired from Event");
        if (textBoxAnimator != null)
        {
            textBoxAnimator.SetTrigger("StartSlide");
        }
        
    }

    public void TriggerAnimation(string triggerName)
    {
        if (textBoxAnimator != null)
        {
            textBoxAnimator.SetBool(triggerName, true);
        }
    }
}
