using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class TypeWriter : MonoBehaviour
{
    [SerializeField] private TextBasedGameManager gameManager;
    private TextMeshProUGUI typeWrittenText;
    private AudioSource typewriterAudioSource;
    private AudioClip typewriterSoundClip;
    private float typeSpeed = 0.05f;
    private bool isTyping = false;

    public static event System.Action OnTypeWriterFinished;
    
    public static readonly List<string> splashMessage = new()
    {
        " I apologise for not being here when you woke up, you seemed very tired so I let you sleep. ",
        " There are 100 tokens in the drawer next to the bed. Your palmprint should open the lemonade stand. ",
        " In case you've forgotten, it's the stand at the very end of the street. The recipe is taped to the fridge. ", 
        " Remember, the tiny lizard likes lemons raw, no need to add sugar. ",
        " -- Agatha, August 22, 2047, 0630 ",
        " You glance at the clock.", 
        " January 4, 2347, 0630. ",
        " There are sounds of activity outside. "
    };

    private void Awake()
    {
        if (typewriterAudioSource == null)
        {
            Debug.Log("Audio Source is not assigned to the Typewriter");
        }
    }

    private void Start()
    {
        if (typewriterAudioSource == null)
        {
            Debug.Log("Audio Source is not assigned to the Typewriter");
        }
    }

    public void StopTyping()
    {
        if(!isTyping) return;
        
        isTyping = false;
        if(typewriterAudioSource != null)
        {
            typewriterAudioSource.Stop();
        }
        OnTypeWriterFinished?.Invoke();
    }

    public void Initialize(TextMeshProUGUI textbox,AudioSource audioSource, AudioClip soundClip)
    {

        if (gameManager != null)
        {
            typeWrittenText = gameManager.GetTypeWrittenText();
            
        }

        typewriterAudioSource = audioSource;
        typewriterSoundClip = soundClip;
        typeWrittenText = textbox;
    }
    
    public IEnumerator TypeText(List<string> TextToType)
    {
        isTyping = true;
        var textRecTransform = typeWrittenText.GetComponent<RectTransform>();
        var boxWidth = textRecTransform.rect.width;
        foreach (var line in splashMessage)
        {
            var textSize = typeWrittenText.GetPreferredValues(line);

            if (textSize.x <= boxWidth)
            {
                typeWrittenText.alignment = TextAlignmentOptions.TopGeoAligned;
            }
            else
            {
                typeWrittenText.alignment = TextAlignmentOptions.TopLeft;
            }

            typeWrittenText.text = "";

            if (typewriterAudioSource != null && typewriterSoundClip != null)
                {
                  typewriterAudioSource.loop = true;
                  typewriterAudioSource.clip = typewriterSoundClip;
                  typewriterAudioSource.Play();
                }

            foreach (var letter in line)
            {
                typeWrittenText.text += letter;
                yield return new WaitForSeconds(typeSpeed);
            }

            if (typewriterAudioSource != null)
            {
                typewriterAudioSource.loop = false;
                typewriterAudioSource.Stop();
            }

            var elapsedTime = 0f;
            while (elapsedTime < 2f)
            {
                if (Input.anyKeyDown)
                {
                    break;
                }
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
         isTyping = false;
         OnTypeWriterFinished?.Invoke();
        }
    }