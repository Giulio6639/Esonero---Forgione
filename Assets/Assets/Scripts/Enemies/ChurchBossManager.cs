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

    [Header("Impostazioni Uscita")]
    public string sceneToLoadAfterBoss = "Level02";

    private enum RoomState { Dormant, IntroDialogue, Fighting, OutroDialogue, Dying }
    private RoomState currentState = RoomState.Dormant;

    private DialogueText currentActiveDialogue = null;

    void Start()
    {
        // Controlli anti-crash iniziali
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
        Debug.Log("Il Manager ha ricevuto il segnale! Stato attuale: " + currentState); // LOG 2

        if (currentState == RoomState.Dormant)
        {
            currentState = RoomState.IntroDialogue;
            StartCoroutine(StartIntroRoutine());
        }
    }

    private IEnumerator StartIntroRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        Debug.Log("Faccio partire il Dialogo Iniziale!"); // LOG 3

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
            // OPZIONE 1: Rimettiamo il Bool originale che avevi nel tuo primissimo script (non si sa mai!)
            bossAnim.SetBool("isDead", true);

            // OPZIONE 2: L'OPZIONE NUCLEARE. 
            // Forza la riproduzione ignorando le frecce dell'Animator.
            // ATTENZIONE: Devi scrivere il nome ESATTO del quadratino grigio dell'animazione di morte!
            // Se nell'Animator il quadratino si chiama "Wizard_Death", scrivi "Wizard_Death".
            bossAnim.Play("anim_Death_Wizard");
        }

        // 1. Aspettiamo che cada a terra
        yield return new WaitForSeconds(2.5f);

        // 2. Chiave ottenuta!
        GameFlow.hasChurchKey = true;
        Debug.Log("Chiave Ottenuta!");

        // 3. Pausa drammatica
        yield return new WaitForSeconds(2f);

        // 4. Caricamento scena
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