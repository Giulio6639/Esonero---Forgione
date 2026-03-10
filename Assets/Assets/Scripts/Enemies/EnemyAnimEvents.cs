using UnityEngine;

public class EnemyAnimEvents : MonoBehaviour
{
    private SkeletonAI ai;

    void Start()
    {
        // Trova in automatico lo script sul genitore
        ai = GetComponentInParent<SkeletonAI>();
    }

    // Questa funzione verrà chiamata dall'animazione
    public void Hit()
    {
        if (ai != null)
            ai.TriggerAttackHit();
    }
}