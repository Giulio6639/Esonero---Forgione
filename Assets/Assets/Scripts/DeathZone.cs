using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Se a toccare il burrone è il Player...
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // ... richiamiamo la morte istantanea, ignorando armature o invincibilità!
                playerHealth.InstantDeath();
            }
        }

        // OPZIONALE: Se vuoi che anche i nemici muoiano quando cadono nel burrone!
        else if (collision.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                // I nemici non hanno gli iFrames complessi del player, 
                // quindi un danno gigantesco basta e avanza.
                enemyHealth.TakeDamage(9999, transform, false);
            }
        }
    }
}