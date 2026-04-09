using UnityEngine;
using UnityEngine.EventSystems; // Fondamentale per i comandi del mouse sulla UI

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Suoni del Bottone")]
    [Tooltip("Il suono quando il mouse passa SOPRA il bottone")]
    public AudioClip hoverSound;

    [Tooltip("Il suono quando CLICCHI il bottone")]
    public AudioClip clickSound;

    // Questa funzione scatta in automatico quando il mouse entra nel rettangolo del bottone
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(hoverSound);
        }
    }

    // Questa funzione scatta in automatico quando clicchi
    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clickSound);
        }
    }
}