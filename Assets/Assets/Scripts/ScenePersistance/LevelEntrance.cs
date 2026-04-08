using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEntrance : MonoBehaviour
{
    [Header("Impostazioni Destinazione")]
    [SerializeField] private string nextLevelName;

    [Tooltip("Il nome dell'oggetto vuoto nel prossimo livello dove vuoi che il player appaia")]
    [SerializeField] private string targetSpawnPointName = "SpawnPoint";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Controlla se a toccare il trigger è stato il Player
        if (collision.GetComponent<HeroKnight>() != null)
        {
            // 1. Salviamo nella memoria globale DOVE vogliamo apparire nella prossima scena
            GameFlow.targetSpawnPoint = targetSpawnPointName;

            // 2. Cambiamo scena usando il tuo SceneChanger per fare la transizione fluida
            if (SceneChanger.instance != null)
            {
                SceneChanger.instance.ChangeLevelTo(nextLevelName);
            }
            else
            {
                // Fallback di sicurezza se lo SceneChanger non c'è
                SceneManager.LoadScene(nextLevelName);
            }
        }
    }
}