using UnityEngine;
using UnityEngine.SceneManagement; // FONDAMENTALE PER CAMBIARE SCENA
using System.Collections;

public class BossDefeated : MonoBehaviour
{
    public string sceneToLoad = "Level02";
    public string spawnPointName = "ChurchExitSpawn"; // Il nome dell'oggetto vuoto nel Livello 2
    public float delayBeforeLoad = 3f; // Secondi per godersi l'animazione di morte del boss

    // Chiamerai questa funzione dallo script della Salute del Mago quando i suoi HP sono <= 0
    public void TriggerBossDeath()
    {
        StartCoroutine(EndSceneRoutine());
    }

    private IEnumerator EndSceneRoutine()
    {
        // 1. Aspetta che finisca l'animazione di morte
        yield return new WaitForSeconds(delayBeforeLoad);

        // 2. Scriviamo nella memoria globale DOVE vogliamo apparire
        GameFlow.targetSpawnPoint = spawnPointName;

        // 3. Carichiamo la scena Level02
        SceneManager.LoadScene(sceneToLoad);
    }
}