using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public class PlayerSceneSetup : MonoBehaviour
{
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(SetupAfterLoad(scene));
    }

    private IEnumerator SetupAfterLoad(Scene scene)
    {
        yield return null; // Aspetta che la scena sia pronta
        yield return null;

        if (rb != null) rb.linearVelocity = Vector2.zero;

        // --- LOGICA DI SPAWN DINAMICA ---
        string pointToFind = "SpawnPoint"; // Di default cerchiamo quello standard

        // Se il BossDefeated ha impostato un punto specifico, usiamo quello!
        if (!string.IsNullOrEmpty(GameFlow.targetSpawnPoint))
        {
            pointToFind = GameFlow.targetSpawnPoint;
        }

        GameObject selectedSpawn = GameObject.Find(pointToFind);

        if (selectedSpawn != null)
        {
            transform.position = selectedSpawn.transform.position;
            Debug.Log($"[Spawn] Player posizionato su: {pointToFind}");

            // RESET: Puliamo la memoria globale dopo l'uso!
            GameFlow.targetSpawnPoint = "";
        }
        else
        {
            // Fallback: se non trova il punto specifico, prova a cercare quello base
            GameObject defaultSpawn = GameObject.Find("SpawnPoint");
            if (defaultSpawn != null) transform.position = defaultSpawn.transform.position;
            Debug.LogWarning($"[Spawn] Punto {pointToFind} non trovato! Usato SpawnPoint base.");
        }

        // --- TELECAMERA ---
        CinemachineCamera vcam = FindFirstObjectByType<CinemachineCamera>();
        if (vcam != null)
        {
            vcam.Follow = this.transform;
            vcam.PreviousStateIsValid = false;
        }
    }
}