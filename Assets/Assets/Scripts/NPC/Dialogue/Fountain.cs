using UnityEngine;

public class Fountain : NPC, ITalkable
{
    [Header("Dialoghi")]
    [SerializeField] private DialogueText firstTimeDialogue;  // Usato solo la PRIMISSIMA volta
    [SerializeField] private DialogueText repeatDialogue;     // Usato per tutte le volte successive

    [Header("Oggetto in Regalo (Infinito)")]
    [SerializeField] private string itemName = "Red Potion";
    [SerializeField] private int itemQuantity = 3;
    [SerializeField] private Sprite itemSprite;
    [TextArea][SerializeField] private string itemDescription;

    // LA VARIABILE MAGICA: condivisa da tutte le fontane del gioco
    private static bool _anyFountainUsedGlobal = false;

    private DialogueController dialogueController;
    private InventoryManager inventoryManager;

    private void Awake()
    {
        // Troviamo i manager anche se sono spenti
        dialogueController = Object.FindFirstObjectByType<DialogueController>(FindObjectsInactive.Include);
        inventoryManager = Object.FindFirstObjectByType<InventoryManager>(FindObjectsInactive.Include);
    }

    public override void Interact()
    {
        bool isStartingConversation = false;
        if (dialogueController != null && !dialogueController.gameObject.activeSelf)
        {
            isStartingConversation = true;
        }

        // 1. Scegliamo il dialogo basandoci sulla variabile STATICA globale
        DialogueText dialogueToUse = _anyFountainUsedGlobal ? repeatDialogue : firstTimeDialogue;

        // --- IL CONTROLLO SALVA-VITA (Anti-Crash) ---
        if (dialogueToUse == null)
        {
            Debug.LogError("ATTENZIONE: Manca uno dei dialoghi nella Fontana! Controlla l'Inspector.");
            return;
        }

        // 2. Parliamo
        Talk(dialogueToUse);

        // 3. Se è il primo click della conversazione...
        if (isStartingConversation)
        {
            // Diamo gli item (il manager penserà da solo a fermarsi a 3)
            if (inventoryManager != null)
            {
                inventoryManager.AddItem(itemName, itemQuantity, itemSprite, itemDescription);
            }

            // Segniamo che la prima interazione globale è avvenuta
            _anyFountainUsedGlobal = true;
        }
    }

    public void Talk(DialogueText dialogueText)
    {
        if (dialogueController != null)
        {
            dialogueController.DisplayNextParagraph(dialogueText);
        }
    }
}