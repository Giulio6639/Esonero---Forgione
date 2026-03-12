using UnityEngine;

public class EnemyAnimEvents : MonoBehaviour
{
    private SkeletonAI skeletonAI;
    private GoblinAI goblinAI;
    private FungusAI fungusAI; // Aggiunto il Fungo

    void Start()
    {
        // Cerca lo script nel genitore, chiunque esso sia!
        skeletonAI = GetComponentInParent<SkeletonAI>();
        goblinAI = GetComponentInParent<GoblinAI>();
        fungusAI = GetComponentInParent<FungusAI>();
    }

    // Questa funzione verrà chiamata dall'animazione di QUALSIASI nemico
    public void Hit()
    {
        if (skeletonAI != null)
        {
            skeletonAI.TriggerAttackHit();
        }
        else if (goblinAI != null)
        {
            goblinAI.TriggerAttackHit();
        }
        else if (fungusAI != null)
        {
            fungusAI.TriggerAttackHit();
        }
    }
}