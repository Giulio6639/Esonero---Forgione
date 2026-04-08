using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class DialogueController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI NPCNameText;
    [SerializeField] private TextMeshProUGUI NPCDialogueText;
    [SerializeField] private float typeSpeed = 10;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip defaultVoiceSound; // Usato se il DialogueText non ha un suono specifico
    [Range(0.5f, 2f)][SerializeField] private float minPitch = 0.95f; // Variazione minima dell'intonazione
    [Range(0.5f, 2f)][SerializeField] private float maxPitch = 1.05f; // Variazione massima

    public static bool isDialogueActive = false;

    private Queue<string> paragraphs = new Queue<string>();

    private bool conversationEnded;
    private bool isTyping;

    private string p;

    private Coroutine typeDialogueCoroutine;

    private const string HTML_alpha = "<color=#00000000>";
    private const float MAX_TYPE_TIME = 0.01f;

    private bool isWaitingBeforeType = false;
    private string sceneToLoad;
    private bool shouldChangeScene = false;

    private AudioClip currentVoiceSound; // Memorizza la voce di chi sta parlando in questo momento

    public void SetSceneExit(string sceneName)
    {
        sceneToLoad = sceneName;
        shouldChangeScene = true;
    }

    public void DisplayNextParagraph(DialogueText dialogueText)
    {
        if (isWaitingBeforeType) return;

        if (paragraphs.Count == 0)
        {
            if (!conversationEnded)
            {
                StartConversation(dialogueText);
            }
            else if (conversationEnded && !isTyping)
            {
                EndConversation();
                return;
            }
        }

        if (!isTyping)
        {
            p = paragraphs.Dequeue();
            typeDialogueCoroutine = StartCoroutine(TypeDialogueText(p));
        }
        else
        {
            FinishParagraphEarly();
        }

        if (paragraphs.Count == 0) conversationEnded = true;
    }

    private void StartConversation(DialogueText dialogueText)
    {
        isDialogueActive = true;

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        Time.timeScale = 0f;

        NPCNameText.text = dialogueText.SpeakerName;

        // --- GESTIONE VOCE ---
        // Se il DialogueText ha un suono, usa quello, altrimenti usa quello di default
        currentVoiceSound = dialogueText.voiceSound != null ? dialogueText.voiceSound : defaultVoiceSound;

        paragraphs.Clear();
        conversationEnded = false;
        isTyping = false;
        isWaitingBeforeType = false;

        for (int i = 0; i < dialogueText.paragraphs.Length; i++)
        {
            paragraphs.Enqueue(dialogueText.paragraphs[i]);
        }
    }

    private void EndConversation()
    {
        isDialogueActive = false;
        conversationEnded = false;
        Time.timeScale = 1f;

        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }

        if (shouldChangeScene && !string.IsNullOrEmpty(sceneToLoad))
        {
            if (SceneChanger.instance != null)
            {
                SceneChanger.instance.ChangeLevelTo(sceneToLoad);
            }
            else
            {
                SceneManager.LoadScene(sceneToLoad);
            }
        }

        shouldChangeScene = false;
    }

    private IEnumerator TypeDialogueText(string p)
    {
        isTyping = true;
        isWaitingBeforeType = true;

        NPCDialogueText.text = "";

        yield return new WaitForSecondsRealtime(0.1f);

        isWaitingBeforeType = false;

        string originalText = p;
        int alphaIndex = 0;

        foreach (char c in p.ToCharArray())
        {
            alphaIndex++;
            NPCDialogueText.text = originalText;
            string displayedText = NPCDialogueText.text.Insert(alphaIndex, HTML_alpha);
            NPCDialogueText.text = displayedText;

            // --- RIPRODUZIONE EFFETTO SONORO (Stile Undertale) ---
            // Suoniamo solo se abbiamo una clip, un AudioSource, e se il carattere NON è uno spazio vuoto
            if (currentVoiceSound != null && audioSource != null && c != ' ')
            {
                // Variamo leggermente il pitch per renderlo meno "robotico"
                audioSource.pitch = Random.Range(minPitch, maxPitch);

                // PlayOneShot permette ai bip di sovrapporsi leggermente se il testo è molto veloce
                audioSource.PlayOneShot(currentVoiceSound);
            }

            yield return new WaitForSecondsRealtime(MAX_TYPE_TIME / typeSpeed);
        }

        isTyping = false;
    }

    private void FinishParagraphEarly()
    {
        StopCoroutine(typeDialogueCoroutine);
        NPCDialogueText.text = p;
        isTyping = false;
        isWaitingBeforeType = false;
    }
}