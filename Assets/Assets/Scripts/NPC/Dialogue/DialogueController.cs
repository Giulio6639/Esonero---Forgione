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
    [SerializeField] private AudioClip defaultVoiceSound;
    [Range(0.5f, 2f)][SerializeField] private float minPitch = 0.95f;
    [Range(0.5f, 2f)][SerializeField] private float maxPitch = 1.05f;

    // --- NUOVO: Freno per il motore audio ---
    [Tooltip("Tempo minimo tra un 'beep' e l'altro per non far crashare l'audio")]
    [SerializeField] private float soundCooldown = 0.04f;
    private float lastSoundTime = 0f;
    // ----------------------------------------

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

    private AudioClip currentVoiceSound;

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

            // --- NUOVA LOGICA AUDIO OTTIMIZZATA ---
            if (currentVoiceSound != null && audioSource != null && c != ' ')
            {
                // Controlla se è passato abbastanza tempo dal suono precedente usando Time.unscaledTime
                // (usiamo unscaledTime perché il gioco è in pausa durante i dialoghi: Time.timeScale = 0)
                if (Time.unscaledTime - lastSoundTime >= soundCooldown)
                {
                    audioSource.pitch = Random.Range(minPitch, maxPitch);
                    audioSource.PlayOneShot(currentVoiceSound);

                    // Salviamo il momento esatto in cui abbiamo suonato
                    lastSoundTime = Time.unscaledTime;
                }
            }
            // --------------------------------------

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