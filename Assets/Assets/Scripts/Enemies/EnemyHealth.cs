using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Impostazioni Salute")]
    public int maxHealth = 60; // RICORDA: nell'Inspector per il fungo, alza questo valore a 150 o 200!
    public int currentHealth;

    [Header("Danno da Contatto")]
    public bool dealsContactDamage = true;
    public int contactDamage = 10;

    [Header("Knockback")]
    public float knockbackForceX = 3f;
    public float knockbackForceY = 2f;
    public float knockbackDuration = 0.2f;

    [Header("Morte e Particelle")]
    public float disappearDelay = 2.5f;
    public GameObject deathParticles;

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

        // --- CONTROLLO SUPER ARMOR DEL FUNGO ---
        bool ignoreStun = false;
        FungusAI fungusAI = GetComponent<FungusAI>();
        if (fungusAI != null)
        {
            // Passa il danno all'IA per contare i colpi
            fungusAI.InterruptCombo();
            // Se il fungo si è appena arrabbiato o lo era già, diciamo a questo script di ignorare lo stordimento
            if (fungusAI.hasSuperArmor) ignoreStun = true;
            fungusAI.PlayFlashEffect();
        }
        // ----------------------------------------

        GoblinAI goblinAI = GetComponent<GoblinAI>();
        if (goblinAI != null) goblinAI.InterruptCombo();


        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Se ha la Super Armor, salta il knockback e l'animazione!
            if (ignoreStun)
            {
                // Il nemico prende danno, ma non si ferma!
                return;
            }

            animator.SetTrigger("Hurt");
            StartCoroutine(HitReaction(attacker, applyKnockback));
        }
    }

    private IEnumerator HitReaction(Transform attacker, bool applyKnockback)
    {
        // Disabilita lo script AI per lo stordimento (se non è un nemico che si gestisce da solo)
        if (aiScript != null && GetComponent<GoblinAI>() == null && GetComponent<FungusAI>() == null) aiScript.enabled = false;

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

        // Riabilita
        if (currentHealth > 0 && aiScript != null && GetComponent<GoblinAI>() == null && GetComponent<FungusAI>() == null)
        {
            aiScript.enabled = true;
        }
    }

    private void Die()
    {
        // 1. Torniamo a usare il Bool!
        if (animator != null) animator.SetBool("isDead", true);

        // ... resto del codice identico a prima ...
        GoblinAI goblin = GetComponent<GoblinAI>();
        if (goblin != null) goblin.enabled = false;

        SkeletonAI skeleton = GetComponent<SkeletonAI>();
        if (skeleton != null) skeleton.enabled = false;

        FungusAI fungus = GetComponent<FungusAI>();
        if (fungus != null) fungus.enabled = false;

        if (aiScript != null) aiScript.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        Collider2D[] allColliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in allColliders)
        {
            col.enabled = false;
        }

        StartCoroutine(DisappearRoutine());
    }

    private IEnumerator DisappearRoutine()
    {
        yield return new WaitForSeconds(disappearDelay);

        if (deathParticles != null)
        {
            Instantiate(deathParticles, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}