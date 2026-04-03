using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Impostazioni Salute")]
    public int maxHealth = 60; // RICORDA: nell'Inspector per il fungo, alza questo valore a 150 o 200!
    public int currentHealth;

    [Header("Impostazioni Boss")]
    public bool isBoss = false; // NUOVO: Spunta questa casella nell'Inspector per il Mago!

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
            fungusAI.InterruptCombo();
            if (fungusAI.hasSuperArmor) ignoreStun = true;
            fungusAI.PlayFlashEffect();
        }
        // ----------------------------------------

        // Interrompi le combo degli altri nemici
        GoblinAI goblinAI = GetComponent<GoblinAI>();
        if (goblinAI != null) goblinAI.InterruptCombo();

        WizardAI wizardAI = GetComponent<WizardAI>();
        if (wizardAI != null)
        {
            // Se ha la super armor, non interrompere l'attacco, fallo solo lampeggiare
            if (wizardAI.hasSuperArmor)
            {
                ignoreStun = true;
                wizardAI.PlayFlashEffect();
            }
            else
            {
                // Se non sta attaccando, interrompi quello che stava facendo (es. camminare)
                wizardAI.InterruptCombo();
            }
        }
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (ignoreStun)
            {
                return; // Il nemico prende danno, ma non si ferma!
            }

            animator.SetTrigger("Hurt");
            StartCoroutine(HitReaction(attacker, applyKnockback));
        }
    }

    private IEnumerator HitReaction(Transform attacker, bool applyKnockback)
    {
        // Disabilita lo script AI per lo stordimento (ignorando quelli che si gestiscono da soli)
        if (aiScript != null && GetComponent<GoblinAI>() == null && GetComponent<FungusAI>() == null && GetComponent<WizardAI>() == null)
            aiScript.enabled = false;

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
        if (currentHealth > 0 && aiScript != null && GetComponent<GoblinAI>() == null && GetComponent<FungusAI>() == null && GetComponent<WizardAI>() == null)
        {
            aiScript.enabled = true;
        }
    }

    private void Die()
    {
        // --- LA MODIFICA CHIAVE ---
        if (animator != null)
        {
            if (isBoss)
            {
                // Se è il boss, subisce il colpo e aspetta il dialogo
                animator.SetTrigger("Hurt");
            }
            else
            {
                // Se è un nemico normale, muore subito
                animator.SetBool("isDead", true);
            }
        }

        // Spegni tutte le IA possibili
        GoblinAI goblin = GetComponent<GoblinAI>();
        if (goblin != null) goblin.enabled = false;

        SkeletonAI skeleton = GetComponent<SkeletonAI>();
        if (skeleton != null) skeleton.enabled = false;

        FungusAI fungus = GetComponent<FungusAI>();
        if (fungus != null) fungus.enabled = false;

        WizardAI wizard = GetComponent<WizardAI>();
        if (wizard != null) wizard.enabled = false;

        if (aiScript != null) aiScript.enabled = false;

        // Blocca il corpo
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Spegni i collider
        Collider2D[] allColliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in allColliders)
        {
            col.enabled = false;
        }

        // Se NON è un boss, sparisce dopo 2.5 secondi. Il boss invece rimane lì finché non lo decide il Manager!
        if (!isBoss)
        {
            StartCoroutine(DisappearRoutine());
        }
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