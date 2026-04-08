using UnityEngine;

public class GemmaPickup : MonoBehaviour
{
    [Header("Impostazioni Gemma")]
    public int valore = 1;

    [Header("Audio")]
    [Tooltip("L'effetto sonoro quando raccogli la gemma (es. un 'bling')")]
    public AudioClip pickupSound;
    [Tooltip("Il volume del suono (da 0 a 1)")]
    [Range(0f, 1f)] public float volume = 1f;

    // Cambiato da OnTriggerEnter2D a OnCollisionEnter2D
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<HeroKnight>() != null)
        {
            // 1. Aggiungiamo i soldi
            CurrencyManager.Instance.AggiungiGemme(valore);

            // 2. Facciamo suonare l'effetto sonoro indipendente dall'oggetto
            if (pickupSound != null)
            {
                // PlayClipAtPoint crea un riproduttore temporaneo che non viene tagliato dal Destroy
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, volume);
            }

            // 3. Distruggiamo la gemma
            Destroy(gameObject);
        }
    }
}