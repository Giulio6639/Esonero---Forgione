using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ChurchBossManager : MonoBehaviour, ITalkable
{
    [Header("Riferimenti Boss")]
    public GameObject bossGameObject;
    public WizardAI bossAI;
    public EnemyHealth bossHealth;

    [Header("Riferimenti Dialoghi")]
    public DialogueController dialogueController;
    public DialogueText introDialogue;
    public DialogueText outroDialogue;

    [Header("Impostazioni Uscita")]
    public string sceneToLoadAfterBoss = "Level02";

    private bool isBossDead = false;
    private bool sequenceStarted = false;
    private DialogueText currentActiveDialogue = null;

    void Start()
    {
        // Setup iniziale: Boss congelato
        if (bossAI != null) bossAI.enabled = false;
        if (bossHealth != null) bossHealth.enabled = false;
        if (bossGameObject != null && bossGameObject.GetComponent<Rigidbody2D>())
            bossGameObject.GetComponent<Rigidbody2D>().simulated = false;
    }

    // Chiamata dallo script "Ponte"
    public void StartBossSequence()
    {
        if (!sequenceStarted)
        {
            sequenceStarted = true;
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
        // --- GESTIONE TASTO E PER DIALOGHI ---
        if (currentActiveDialogue != null)
        {
            // Keyboard.current funziona anche con Time.timeScale = 0
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                Debug.Log("Tasto E premuto nella cutscene!");
                Talk(currentActiveDialogue);
            }

            // Se il canvas del dialogo viene spento dal DialogueController, abbiamo finito
            if (dialogueController != null && !dialogueController.gameObject.activeSelf)
            {
                currentActiveDialogue = null;
            }
            return; // Blocca il resto finché parliamo
        }

        // --- ATTIVAZIONE BOSS ---
        if (sequenceStarted && bossAI != null && !bossAI.enabled && Time.timeScale == 1f && !isBossDead)
        {
            bossAI.enabled = true;
            bossHealth.enabled = true;
            if (bossGameObject.GetComponent<Rigidbody2D>())
                bossGameObject.GetComponent<Rigidbody2D>().simulated = true;
        }

        // --- CONTROLLO MORTE ---
        if (bossHealth != null && bossHealth.currentHealth <= 0 && !isBossDead)
        {
            isBossDead = true;
            StartCoroutine(BossDeathSequenceRoutine());
        }
    }

    private IEnumerator BossDeathSequenceRoutine()
    {
        yield return new WaitForSeconds(2.5f);

        if (dialogueController != null)
        {
            GameFlow.targetSpawnPoint = "ChurchExitSpawn";
            dialogueController.SetSceneExit(sceneToLoadAfterBoss);
        }

        currentActiveDialogue = outroDialogue;
        Talk(outroDialogue);
    }

    public void Talk(DialogueText dialogueText)
    {
        if (dialogueController != null)
        {
            dialogueController.DisplayNextParagraph(dialogueText);
        }
    }
}