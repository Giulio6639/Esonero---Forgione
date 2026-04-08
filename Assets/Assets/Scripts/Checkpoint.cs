using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Effetti Checkpoint")]
    [Tooltip("Suono quando attivi il checkpoint")]
    public AudioClip activationSound;

    private Animator animator;
    private bool isActivated = false;

    void Start()
    {
        // Cerca l'animator se vuoi fare un'animazione (es. fuoco che si accende)
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Se a toccarlo è il Player e non è già stato attivato
        if (!isActivated && collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // 1. Aggiorna il punto di respawn del player con la posizione di QUESTO checkpoint
                playerHealth.SetRespawnPoint(this.transform);

                // 2. Segnalo come attivato così non ripete il suono ogni volta che ci passi
                isActivated = true;

                // 3. Feedback Visivo/Sonoro
                if (animator != null) animator.SetTrigger("Activate"); // Assicurati di avere questo trigger nell'Animator
                if (activationSound != null) AudioSource.PlayClipAtPoint(activationSound, transform.position);

                Debug.Log("Checkpoint Salvato!");
            }
        }
    }
}