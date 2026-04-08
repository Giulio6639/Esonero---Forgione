using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class ChurchBossManager : MonoBehaviour, ITalkable
{
    [Header("Riferimenti Boss")]
    public GameObject bossGameObject;
    public WizardAI bossAI;
    public EnemyHealth bossHealth;
    private Rigidbody2D bossRb;

    [Header("Riferimenti Dialoghi")]
    public DialogueController dialogueController;
    public DialogueText introDialogue;
    public DialogueText outroDialogue;

    [Header("Audio (Boss Theme)")]
    [Tooltip("La musica epica che parte quando inizia il combattimento")]
    public AudioClip bossMusic; // <--- NUOVO

    [Header("Impostazioni Uscita")]
    public string sceneToLoadAfterBoss = "Level02";

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
        else
        {
            Debug.LogError("ERRORE FATALE: Non hai trascinato il Mago nello slot 'Boss Game Object' del Manager!");
        }
    }

    public void StartBossSequence()
    {
        Debug.Log("Il Manager ha ricevuto il segnale! Stato attuale: " + currentState);

        if (currentState == RoomState.Dormant)
        {
            currentState = RoomState.IntroDialogue;
            StartCoroutine(StartIntroRoutine());
        }
    }

    private IEnumerator StartIntroRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        Debug.Log("Faccio partire il Dialogo Iniziale!");

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
                    Debug.Log("IL MAGO SI SVEGLIA! BATTAGLIA INIZIATA!");

                    // --- CAMBIO MUSICA ---
                    // Chiamiamo l'AudioManager globale e gli diciamo di suonare la Boss Theme!
                    if (AudioManager.Instance != null && bossMusic != null)
                    {
                        AudioManager.Instance.PlayMusic(bossMusic);
                    }
                    // ---------------------
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
        Animator bossAnim = null;
        if (bossGameObject != null) bossAnim = bossGameObject.GetComponentInChildren<Animator>();

        if (bossAnim != null)
        {
            bossAnim.SetBool("isDead", true);
            bossAnim.Play("anim_Death_Wizard");
        }

        yield return new WaitForSeconds(2.5f);

        GameFlow.hasChurchKey = true;
        Debug.Log("Chiave Ottenuta!");

        yield return new WaitForSeconds(2f);

        GameFlow.targetSpawnPoint = "ChurchExitSpawn";
        SceneManager.LoadScene(sceneToLoadAfterBoss);
    }

    public void Talk(DialogueText dialogueText)
    {
        if (dialogueController != null)
        {
            dialogueController.DisplayNextParagraph(dialogueText);
        }
        else
        {
            Debug.LogError("ERRORE FATALE: Non hai trascinato il DialogueController nel Manager!");
        }
    }
}