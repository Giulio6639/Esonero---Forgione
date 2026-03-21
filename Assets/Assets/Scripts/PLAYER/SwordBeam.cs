using UnityEngine;
using System.Collections;

public class SwordBeam : MonoBehaviour
{
    [Header("Statistiche")]
    public int damage = 20;
    public float lifeTime = 1.5f;

    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D coll;

    private bool hasHit = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();

        // --- LA CINTURA DI SICUREZZA ---
        // Se per qualsiasi motivo l'animazione si blocca o ti dimentichi l'Event,
        // Unity distruggerà comunque questo oggetto dopo "lifeTime + 1" secondi.
        Destroy(gameObject, lifeTime + 1f);
        // -------------------------------

        StartCoroutine(AutoVanishRoutine());
    }

    private IEnumerator AutoVanishRoutine()
    {
        yield return new WaitForSeconds(lifeTime);
        if (!hasHit)
        {
            TriggerVanish();
        }
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Ignora se stesso, il giocatore, o altri trigger (come sensori visivi)
        if (hasHit || hitInfo.CompareTag("Player") || hitInfo.isTrigger) return;

        EnemyHealth enemy = hitInfo.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, transform, false);
        }

        TriggerVanish();
    }

    private void TriggerVanish()
    {
        hasHit = true;
        rb.linearVelocity = Vector2.zero;
        coll.enabled = false;

        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
        else
        {
            DestroyProjectile();
        }
    }

    public void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}