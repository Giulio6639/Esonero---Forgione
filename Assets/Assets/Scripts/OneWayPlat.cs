using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlatformEffector2D))]
public class OneWayPlat : MonoBehaviour
{
    private PlatformEffector2D effector;
    public float fallTime = 0.5f; // Quanto tempo rimane "aperta" la botola

    void Start()
    {
        // Prende l'effector in automatico
        effector = GetComponent<PlatformEffector2D>();
    }

    void Update()
    {
        // Se premi 'S' (o freccia giù)
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            StartCoroutine(FallThroughRoutine());
        }
    }

    private IEnumerator FallThroughRoutine()
    {
        // Ruota il "muro" dell'effector di 180 gradi, facendoti cadere giù
        effector.rotationalOffset = 180f;

        // Aspetta mezzo secondo per darti il tempo di attraversare la piattaforma
        yield return new WaitForSeconds(fallTime);

        // Ripristina l'effector alla normalità
        effector.rotationalOffset = 0f;
    }
}