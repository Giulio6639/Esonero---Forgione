using UnityEngine;

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
        if (requiresChurchKey && !GameFlow.hasChurchKey)
        {
            if (lockedDialogue != null)
            {
                Talk(lockedDialogue);
            }
            else
            {
                Debug.LogWarning("La porta è chiusa, ma non hai assegnato il DialogueText 'lockedDialogue' nell'Inspector!");
            }
            return;
        }

        DialogueText currentDialogue = firstDialogue;
        if (hasFinishedFirstDialogue && secondDialogue != null)
        {
            currentDialogue = secondDialogue;
        }

        Talk(currentDialogue);

        if (!hasFinishedFirstDialogue && dialogueController.gameObject.activeSelf)
        {
            if (givesItem && inventoryManager != null)
            {
                inventoryManager.AddItem(itemName, itemQuantity, itemSprite, itemDescription);
                givesItem = false;
            }

            hasFinishedFirstDialogue = true;
        }
    }

    public void Talk(DialogueText dialogueText)
    {
        if (isLevelExit)
        {
            dialogueController.SetSceneExit(sceneToLoad);
        }

        dialogueController.DisplayNextParagraph(dialogueText);
    }
}