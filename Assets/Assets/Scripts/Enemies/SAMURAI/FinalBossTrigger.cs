using UnityEngine;

public class FinalBossTrigger : MonoBehaviour
{
    public FinalBossManager manager;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (manager != null)
            {
                // Avvisa il Manager di far partire il dialogo
                manager.StartBossSequence();

                // Si spegne in modo che non si attivi più
                gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("Ricordati di trascinare il FinalBossManager nello slot del Trigger!");
            }
        }
    }
}