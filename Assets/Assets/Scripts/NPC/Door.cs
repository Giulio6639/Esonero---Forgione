using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : NPC, ITalkable
{
    [Header("Configurazione Chiave (Novità)")]
    [Tooltip("Spunta se questa porta richiede la chiave della chiesa per aprirsi")]
    [SerializeField] private bool requiresChurchKey = false;
    [Tooltip("Il dialogo che appare se non hai la chiave (es. 'La porta è chiusa a chiave.')")]
    [SerializeField] private DialogueText lockedDialogue;

    [Header("Dialoghi (Se Aperta)")]
    [SerializeField] private DialogueText firstDialogue;
    [Tooltip("Se lasciato vuoto, ripeterà sempre il primo dialogo")]
    [SerializeField] private DialogueText secondDialogue;

    [Header("Oggetto in Regalo (Opzionale)")]
    [SerializeField] private bool givesItem = false;
    [SerializeField] private string itemName;
    [SerializeField] private int itemQuantity;
    [SerializeField] private Sprite itemSprite;
    [TextArea][SerializeField] private string itemDescription;

    [Header("Configurazione Uscita")]
    [SerializeField] private bool isLevelExit = false;
    [SerializeField] private string sceneToLoad;

    private DialogueController dialogueController;
    private InventoryManager inventoryManager;
    private bool hasFinishedFirstDialogue = false;

    private void Awake()
    {
        dialogueController = Object.FindFirstObjectByType<DialogueController>(FindObjectsInactive.Include);
        inventoryManager = Object.FindFirstObjectByType<InventoryManager>(FindObjectsInactive.Include);
    }

    public override void Interact()
    {
        // 1. CONTROLLO CHIAVE
        if (requiresChurchKey && !GameFlow.hasChurchKey)
        {
            if (lockedDialogue != null)
            {
                // Parla, MA non permettere il cambio scena!
                Talk(lockedDialogue, false);
            }
            else
            {
                Debug.LogWarning("La porta è chiusa, ma non hai assegnato il DialogueText 'lockedDialogue' nell'Inspector!");
            }
            return;
        }

        // 2. LA PORTA È APERTA (o non richiede chiavi)
        DialogueText currentDialogue = firstDialogue;
        if (hasFinishedFirstDialogue && secondDialogue != null)
        {
            currentDialogue = secondDialogue;
        }

        // Se non c'è nessun dialogo e deve cambiare scena
        if (isLevelExit && currentDialogue == null)
        {
            Debug.Log("Nessun dialogo assegnato: Cambio scena istantaneo verso " + sceneToLoad);
            if (SceneChanger.instance != null)
            {
                SceneChanger.instance.ChangeLevelTo(sceneToLoad);
            }
            else
            {
                SceneManager.LoadScene(sceneToLoad);
            }
            return;
        }

        // Parla e PERMETTI il cambio scena (se questa porta è una via d'uscita)
        Talk(currentDialogue, isLevelExit);

        if (!hasFinishedFirstDialogue && dialogueController != null && dialogueController.gameObject.activeSelf)
        {
            if (givesItem && inventoryManager != null)
            {
                inventoryManager.AddItem(itemName, itemQuantity, itemSprite, itemDescription);
                givesItem = false;
            }
            hasFinishedFirstDialogue = true;
        }
    }

    // --- FUNZIONE AGGIORNATA ---
    // Abbiamo aggiunto il parametro "shouldChangeSceneAfterTalk"
    public void Talk(DialogueText dialogueText, bool shouldChangeSceneAfterTalk)
    {
        // Imposta l'uscita SOLO se questo specifico dialogo lo permette
        if (shouldChangeSceneAfterTalk)
        {
            dialogueController.SetSceneExit(sceneToLoad);
        }

        if (dialogueText != null)
        {
            dialogueController.DisplayNextParagraph(dialogueText);
        }
    }

    // Questa serve per mantenere compatibilità con l'interfaccia ITalkable
    public void Talk(DialogueText dialogueText)
    {
        Talk(dialogueText, false);
    }
}