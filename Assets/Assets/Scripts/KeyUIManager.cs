using UnityEngine;
using UnityEngine.UI; // Fondamentale per interagire con le immagini della UI

public class KeyUIManager : MonoBehaviour
{
    [Header("UI Element")]
    public Image keyIcon; // L'immagine della chiave sulla Canvas

    void Update()
    {
        if (keyIcon != null)
        {
            // L'immagine sarà visibile SOLO se la variabile nel GameFlow è true
            keyIcon.enabled = GameFlow.hasChurchKey;
        }
    }
}