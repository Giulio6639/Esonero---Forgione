using UnityEngine;

public class Door : NPC, ITalkable
{
    [SerializeField] private DialogueText dialogueText;
    [SerializeField] private DialogueController dialogueController;

    [Header("Configurazione Uscita")]
    [SerializeField] private bool isLevelExit = false;
    [SerializeField] private string sceneToLoad;

    public override void Interact()
    {
        Talk(dialogueText);
    }

    public void Talk(DialogueText dialogueText)
    {
        // Se è un'uscita, avvisa il controller prima di iniziare
        if (isLevelExit)
        {
            dialogueController.SetSceneExit(sceneToLoad);
        }

        dialogueController.DisplayNextParagraph(dialogueText);
    }
}