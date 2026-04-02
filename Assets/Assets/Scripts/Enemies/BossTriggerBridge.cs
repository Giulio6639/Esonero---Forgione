using UnityEngine;

public class BossTriggerBridge : MonoBehaviour
{
    public ChurchBossManager manager; // Trascina qui l'empty globale con il Manager

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            manager.StartBossSequence();
            // Disattiva il trigger dopo l'uso per evitare che riparta la scena
            gameObject.SetActive(false);
        }
    }
}