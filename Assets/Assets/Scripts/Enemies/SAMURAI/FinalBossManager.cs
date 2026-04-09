using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class FinalBossManager : MonoBehaviour, ITalkable
{
    [Header("Riferimenti Boss Samurai")]
    public GameObject bossGameObject;
    public SamuraiAI bossAI;
    public EnemyHealth bossHealth;
    private Rigidbody2D bossRb;

    [Header("Riferimenti Dialoghi")]
    public DialogueController dialogueController;
    public DialogueText introDialogue;
    public DialogueText outroDialogue;

    [Header("Audio (Boss Theme)")]
    public AudioClip bossMusic;

    [Header("Impostazioni Uscita")]
    [Tooltip("Il nome esatto della scena dei titoli di coda")]
    public string creditsSceneName = "CreditsScene";

    private enum RoomState { Dormant, IntroDialogue, Fighting, OutroDialogue, Dying }
    private RoomState currentState = RoomState.Dormant;

    private DialogueText currentActiveDialogue = null;

    private void Awake()
    {
        dialogueController = FindFirstObjectByType<DialogueController>(FindObjectsInactive.Include);
    }

    void Start()
    {
        if (bossAI != null) bossAI.enabled = false;
        if (bossHealth != null) bossHealth.enabled = false;

        if (bossGameObject != null)
        {
            bossRb = bossGameObject.GetComponent<Rigidbody2D>();
            if (bossRb != null) bossRb.simulated = false;
        }
    }

    public void StartBossSequence()
    {
        if (currentState == RoomState.Dormant)
        {
            currentState = RoomState.IntroDialogue;
            StartCoroutine(StartIntroRoutine());
        }
    }

    private IEnumerator StartIntroRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        currentActiveDialogue = introDialogue;
        Talk(introDialogue);
    }

    void Update()
    {
        if (currentActiveDialogue != null)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                Talk(currentActiveDialogue);
            }

            if (dialogueController != null && !dialogueController.gameObject.activeSelf)
            {
                currentActiveDialogue = null;

                if (currentState == RoomState.IntroDialogue)
                {
                    currentState = RoomState.Fighting;
                    if (bossAI != null) bossAI.enabled = true;
                    if (bossHealth != null) bossHealth.enabled = true;
                    if (bossRb != null) bossRb.simulated = true;

                    if (AudioManager.Instance != null && bossMusic != null)
                    {
                        AudioManager.Instance.PlayMusic(bossMusic);
                    }
                }
                else if (currentState == RoomState.OutroDialogue)
                {
                    currentState = RoomState.Dying;
                    StartCoroutine(FinalDeathSequence());
                }
            }
            return;
        }

        if (currentState == RoomState.Fighting && bossHealth != null && bossHealth.currentHealth <= 0)
        {
            currentState = RoomState.OutroDialogue;
            Time.timeScale = 0f;
            currentActiveDialogue = outroDialogue;
            Talk(outroDialogue);
        }
    }

    private IEnumerator FinalDeathSequence()
    {
        Time.timeScale = 1f;

        Animator bossAnim = null;
        if (bossGameObject != null) bossAnim = bossGameObject.GetComponentInChildren<Animator>();

        if (bossAnim != null)
        {
            bossAnim.SetBool("isDead", true);
        }

        yield return new WaitForSeconds(3.5f);

        Debug.Log("Carico la Scena dei Titoli di Coda...");

        // Carica direttamente la scena statica
        SceneManager.LoadScene(creditsSceneName);
    }

    public void Talk(DialogueText dialogueText)
    {
        if (dialogueController != null)
        {
            dialogueController.DisplayNextParagraph(dialogueText);
        }
    }
}