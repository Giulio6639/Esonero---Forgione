using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class FinalBossManager : MonoBehaviour, ITalkable
{
    [Header("Riferimenti Boss Samurai")]
    public GameObject bossGameObject;
    public SamuraiAI bossAI; // <-- Collegato al nuovo script del Samurai
    public EnemyHealth bossHealth;
    private Rigidbody2D bossRb;

    [Header("Riferimenti Dialoghi")]
    public DialogueController dialogueController;
    public DialogueText introDialogue;
    public DialogueText outroDialogue;

    [Header("Impostazioni Uscita")]
    public string creditsSceneName = "CreditsScene"; // <-- Inserisci qui il nome esatto della tua scena dei Titoli di Coda

    private enum RoomState { Dormant, IntroDialogue, Fighting, OutroDialogue, Dying }
    private RoomState currentState = RoomState.Dormant;

    private DialogueText currentActiveDialogue = null;

    void Start()
    {
        // Il boss inizia "dormiente"
        if (bossAI != null) bossAI.enabled = false;
        if (bossHealth != null) bossHealth.enabled = false;

        if (bossGameObject != null)
        {
            bossRb = bossGameObject.GetComponent<Rigidbody2D>();
            if (bossRb != null) bossRb.simulated = false; // Ferma la fisica (non cade)
        }
        else
        {
            Debug.LogError("Non hai trascinato il Samurai nello slot 'Boss Game Object' del FinalBossManager!");
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
        yield return new WaitForSeconds(0.5f); // Pausa scenica prima di parlare
        currentActiveDialogue = introDialogue;
        Talk(introDialogue);
    }

    void Update()
    {
        // --- GESTIONE DEI DIALOGHI ---
        if (currentActiveDialogue != null)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                Talk(currentActiveDialogue);
            }

            // Se il riquadro del dialogo si spegne, significa che abbiamo finito di parlare
            if (dialogueController != null && !dialogueController.gameObject.activeSelf)
            {
                currentActiveDialogue = null;

                if (currentState == RoomState.IntroDialogue)
                {
                    // INIZIA LA BATTAGLIA
                    currentState = RoomState.Fighting;
                    if (bossAI != null) bossAI.enabled = true;
                    if (bossHealth != null) bossHealth.enabled = true;
                    if (bossRb != null) bossRb.simulated = true;
                }
                else if (currentState == RoomState.OutroDialogue)
                {
                    // IL DIALOGO FINALE È FINITO, MUORE
                    currentState = RoomState.Dying;
                    StartCoroutine(FinalDeathSequence());
                }
            }
            return;
        }

        // --- CONTROLLO MORTE DEL BOSS ---
        if (currentState == RoomState.Fighting && bossHealth != null && bossHealth.currentHealth <= 0)
        {
            currentState = RoomState.OutroDialogue;
            Time.timeScale = 0f; // Congela l'azione per le ultime parole
            currentActiveDialogue = outroDialogue;
            Talk(outroDialogue);
        }
    }

    private IEnumerator FinalDeathSequence()
    {
        // IMPORTANTE: Rimettiamo il tempo normale, altrimenti la morte si congela a schermo!
        Time.timeScale = 1f;

        Animator bossAnim = null;
        if (bossGameObject != null) bossAnim = bossGameObject.GetComponentInChildren<Animator>();

        if (bossAnim != null)
        {
            // Richiama l'animazione di morte del Samurai
            bossAnim.SetBool("isDead", true);
        }

        // 1. Aspettiamo che cada a terra e ci godiamo la scena (es. 3 o 4 secondi)
        yield return new WaitForSeconds(3.5f);

        // 2. Carichiamo la scena dei Crediti!
        Debug.Log("Carico i Titoli di Coda...");
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