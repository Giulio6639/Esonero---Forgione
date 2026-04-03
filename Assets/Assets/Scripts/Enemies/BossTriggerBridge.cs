using UnityEngine;

public class BossTriggerBridge : MonoBehaviour
{
    public ChurchBossManager manager;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Il Player ha toccato il trigger!"); // LOG 1

            if (manager != null)
            {
                manager.StartBossSequence();
                gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("ERRORE FATALE: Non hai trascinato il ChurchBossManager nello slot del Trigger!");
            }
        }
    }
}