using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Impostazioni Salute")]
    public int maxHealth = 60;
    public int currentHealth;

    [Header("Danno da Contatto")]
    public bool dealsContactDamage = true;
    public int contactDamage = 10;

    [Header("Knockback")]
    public float knockbackForceX = 3f;
    public float knockbackForceY = 2f;
    public float knockbackDuration = 0.2f;

    // --- NUOVE VARIABILI PER LA MORTE ---
    [Header("Morte e Particelle")]
    public float disappearDelay = 2.5f;   // Quanti secondi il cadavere rimane a terra
    public GameObject deathParticles;     // Il prefabbricato delle particelle (es. fumo o sangue)

    [Header("Riferimenti")]
    public Behaviour aiScript;
    private Animator animator;
    private Rigidbody2D rb;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (currentHealth <= 0 || !dealsContactDamage) return;

        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage, transform);
        }
    }

    public void TakeDamage(int damageAmount, Transform attacker, bool applyKnockback = true)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("Hurt");
            StartCoroutine(HitReaction(attacker, applyKnockback));
        }
    }

    private IEnumerator HitReaction(Transform attacker, bool applyKnockback)
    {
        if (aiScript != null) aiScript.enabled = false;

        if (applyKnockback)
        {
            float direction = transform.position.x < attacker.position.x ? -1f : 1f;
            rb.linearVelocity = new Vector2(direction * knockbackForceX, knockbackForceY);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        yield return new WaitForSeconds(knockbackDuration);

        if (currentHealth > 0 && aiScript != null)
        {
            aiScript.enabled = true;
        }
    }

    private void Die()
    {
        animator.SetBool("isDead", true);
        if (aiScript != null) aiScript.enabled = false;

        // 1. Fermiamo ogni movimento
        rb.linearVelocity = Vector2.zero;

        // 2. Spegniamo la gravità (Kinematic) così non cade
        rb.bodyType = RigidbodyType2D.Kinematic;

        // 3. Spegniamo il collider così il giocatore ci passa attraverso
        GetComponent<Collider2D>().enabled = false;

        StartCoroutine(DisappearRoutine());
    }

    // --- LA COROUTINE CHE GESTISCE LA SPARIZIONE ---
    private IEnumerator DisappearRoutine()
    {
        // 1. Aspetta che il cadavere stia a terra per il tempo deciso
        yield return new WaitForSeconds(disappearDelay);

        // 2. Se hai assegnato un effetto particellare nell'Inspector, spawnano!
        if (deathParticles != null)
        {
            // Instanzia le particelle esattamente dove si trova il nemico ora
            Instantiate(deathParticles, transform.position, Quaternion.identity);
        }

        // 3. Elimina definitivamente l'oggetto "Scheletro" dalla memoria del gioco
        Destroy(gameObject);
    }
    private void OnDisable()
    {
        // Uccide all'istante qualsiasi Coroutine (timer) in corso per questo script
        StopAllCoroutines();
    }
}