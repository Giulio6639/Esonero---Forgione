using UnityEngine;

public class EnemyAnimEvents : MonoBehaviour
{
    private SkeletonAI skeletonAI;
    private GoblinAI goblinAI;
    private FungusAI fungusAI;
    private WizardAI wizardAI; // Aggiunto il Mago

    void Start()
    {
        skeletonAI = GetComponentInParent<SkeletonAI>();
        goblinAI = GetComponentInParent<GoblinAI>();
        fungusAI = GetComponentInParent<FungusAI>();
        wizardAI = GetComponentInParent<WizardAI>(); // Lo cerca all'avvio
    }

    // METODO VECCHIO (Per Scheletro, Goblin e Fungo) - NON TOCCARE
    public void Hit()
    {
        if (skeletonAI != null) skeletonAI.TriggerAttackHit();
        else if (goblinAI != null) goblinAI.TriggerAttackHit();
        else if (fungusAI != null) fungusAI.TriggerAttackHit();
    }

    // --- NUOVO METODO ESCLUSIVO PER IL MAGO ---
    // Unity ci permette di passare un "Int" (numero intero) direttamente dall'Animation Event!
    public void WizardHit(int attackIndex)
    {
        if (wizardAI != null)
        {
            wizardAI.TriggerAttackHit(attackIndex);
        }
    }
}