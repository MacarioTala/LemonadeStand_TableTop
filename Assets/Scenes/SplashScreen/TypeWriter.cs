using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class TypeWriter : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    private TextMeshProUGUI typeWrittenText;
    private AudioSource typewriterAudioSource;
    private AudioClip typewriterSoundClip;
    private float typeSpeed = 0.05f;

    public static event System.Action OnTypeWriterFinished;
    
    private readonly List<string> splashMessage = new()
    {
        " 2142 is when we stopped buying stuff.",
        " The economic system collapsing under the weight of numbers that were just too big.",
        " Triple leveraged derivatives that no one understood,",
        " . . . mortgages on arcologies that changed hands so many times no one knew who owned them.",
        " And with aging and hunger genetically edited away, we couldn't even die.",
        " When it was time for the missiles ... ", 
        " .... we were just too confused to end it all. ",
        " The world didn't end in a ball of fire.",
        " We just stopped .... DOING things. ",
        " And that's where you come in."
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
    
    public IEnumerator TypeText()
    {
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
         OnTypeWriterFinished?.Invoke();
        }
    }