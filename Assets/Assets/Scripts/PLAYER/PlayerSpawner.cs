using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    void Start()
    {
        // 1. Controlla se abbiamo un "messaggio" nella memoria statica
        if (GameFlow.targetSpawnPoint != "")
        {
            // 2. Cerca nella scena l'oggetto con quel nome esatto (es. "ChurchExitSpawn")
            GameObject spawnPoint = GameObject.Find(GameFlow.targetSpawnPoint);

            if (spawnPoint != null)
            {
                // 3. Trova il giocatore e teletrasportalo LÌ!
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    player.transform.position = spawnPoint.transform.position;
                    Debug.Log("Teletrasportato con successo a: " + GameFlow.targetSpawnPoint);
                }
            }
            else
            {
                Debug.LogWarning("Non ho trovato il punto di spawn: " + GameFlow.targetSpawnPoint);
            }

            // 4. Svuota la memoria, così se ricarichiamo il livello normalmente partiamo dall'inizio
            GameFlow.targetSpawnPoint = "";
        }
    }
}