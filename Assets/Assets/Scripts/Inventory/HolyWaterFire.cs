using UnityEngine;
using System.Collections;

public class HolyFire : MonoBehaviour
{
    [Header("Impostazioni Fiamma")]
    public float duration = 1.5f;       // Dura 1.5 secondi
    public float damageRadius = 1.2f;   // Grandezza del fuoco
    public int damagePerTick = 10;      // Quanti danni fa ad ogni "bruciatura"
    public float tickRate = 0.3f;       // Ogni quanti secondi brucia? (0.3s significa più hit continui)
    public LayerMask enemyLayer;        // Assicurati di impostarlo su "Enemy" nell'Inspector

    void Start()
    {
        // Distrugge questo oggetto automaticamente dopo 1.5 secondi
        Destroy(gameObject, duration);

        // Avvia la routine che infligge danni ripetuti
        StartCoroutine(BurnRoutine());
    }

    private IEnumerator BurnRoutine()
    {
        while (true) // Continua finché l'oggetto non viene distrutto dallo Start
        {
            DamageEnemies();
            yield return new WaitForSeconds(tickRate); // Aspetta 0.3 secondi e poi brucia di nuovo
        }
    }

    private void DamageEnemies()
    {
        // Trova tutti i nemici nel raggio d'azione
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, damageRadius, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            // Nota: Se i tuoi nemici hanno script diversi (es. SkeletonAI invece di EnemyHealth), 
            // assicurati che ci sia un componente per la vita o cambialo qui sotto.
            // Uso EnemyHealth perché l'hai richiamato in AE_AttackHit() dell'HeroKnight
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health != null)
            {
                // Invia il danno. Passiamo 'null' o 'transform' come attaccante
                health.TakeDamage(damagePerTick, transform, false);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}