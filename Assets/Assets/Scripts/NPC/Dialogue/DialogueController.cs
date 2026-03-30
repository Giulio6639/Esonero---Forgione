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

    public void SetSceneExit(string sceneName)
    {
        sceneToLoad = sceneName;
        shouldChangeScene = true;
    }

    public void DisplayNextParagraph(DialogueText dialogueText)
    {
        // Se stiamo aspettando il secondo di pausa, non fare nulla
        if (isWaitingBeforeType) return;

        if (paragraphs.Count == 0)
        {
            if(!conversationEnded)
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
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        // Time Stop
        Time.timeScale = 0f;

        // Update the speaker name
        NPCNameText.text = dialogueText.SpeakerName;

        // Add dialogue Text to the queue
        for (int i = 0; i < dialogueText.paragraphs.Length; i++)
        {
            paragraphs.Enqueue(dialogueText.paragraphs[i]);
        }
    }
    private void EndConversation()
    {
        conversationEnded = false;
        Time.timeScale = 1f;

        // --- CAMBIO SCENA ---
        if (shouldChangeScene && !string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            if (gameObject.activeSelf) gameObject.SetActive(false);
        }

        shouldChangeScene = false; // Reset per il prossimo NPC
    }

    private IEnumerator TypeDialogueText(string p)
    {
        isTyping = true;
        isWaitingBeforeType = true; // Blocca input durante l'attesa

        NPCDialogueText.text = "";

        // ASPETTA 1 SECONDO (Realtime perché Time.timeScale è 0)
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
